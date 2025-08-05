using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using Blazor.BrowserExtension.Authentication.Microsoft.Interop;
using Blazor.BrowserExtension.Authentication.Microsoft.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebExtensions.Net;
using WebExtensions.Net.Identity;
using WebExtensions.Net.Manifest;

namespace Blazor.BrowserExtension.Authentication.Microsoft.Services;

internal class BrowserExtensionMicrosoftAuthenticator : IBrowserExtensionMicrosoftAuthenticator
{
    private readonly ILogger<BrowserExtensionMicrosoftAuthenticator> _logger;
    private readonly IIHttpClientFactory _httpClientFactory;
    private readonly IWebExtensionsApi _webExtensionsApi;

    private readonly string _clientId;
    private readonly string _scopes;
    private readonly string _authUrl;
    private readonly string _tokenUrl;

    private string? _pkceCodeVerifier;
    private string? _redirectUrl;

    public BrowserExtensionMicrosoftAuthenticator(
        ILogger<BrowserExtensionMicrosoftAuthenticator> logger,
        IConfiguration config,
        IIHttpClientFactory httpClientFactory,
        IWebExtensionsApi webExtensionsApi)
    {
        _clientId = config.GetSection("AzureAd:ClientId").Get<string>() ?? throw new ArgumentException("ClientId is not configured.");

        var scopes = config.GetSection("AzureAd:Scopes").Get<string[]>() ?? throw new ArgumentException("Scopes are not configured.");
        _scopes = string.Join(' ', scopes);

        var authority = config.GetSection("AzureAd:Authority").Get<string>() ?? throw new ArgumentException("Authority is not configured.");
        _authUrl = $"{authority}/oauth2/v2.0/authorize";
        _tokenUrl = $"{authority}/oauth2/v2.0/token";

        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _webExtensionsApi = webExtensionsApi;
    }

    public async Task SignOutAsync()
    {
        _logger.LogInformation("Signing out from Microsoft OAuth flow");
        await _webExtensionsApi.Storage.Local.RemoveTokenAsync();

        _pkceCodeVerifier = null;
        _redirectUrl = null;
    }

    public async Task<TokenResponse> AuthenticateAsync()
    {
        _logger.LogInformation("Starting Microsoft OAuth flow");

        await _webExtensionsApi.Storage.Local.RemoveTokenAsync();

        _redirectUrl = _webExtensionsApi.Identity.GetRedirectURL();

        var (codeVerifier, codeChallenge) = GeneratePkceParams();
        _pkceCodeVerifier = codeVerifier;

        var authUri = new UriBuilder(_authUrl)
        {
            Query = $"client_id={_clientId}" +
                    $"&response_type=code" +
                    $"&redirect_uri={HttpUtility.UrlEncode(_redirectUrl)}" +
                    $"&scope={HttpUtility.UrlEncode(_scopes)}" +
                    $"&response_mode=query" +
                    $"&state={GenerateRandomState()}" +
                    $"&code_challenge={codeChallenge}" +
                    $"&code_challenge_method=S256"
        }.Uri;

        var responseUrl = await _webExtensionsApi.Identity.LaunchWebAuthFlow(new LaunchWebAuthFlowDetails
        {
            Url = new HttpUrl(authUri.ToString()),
            Interactive = true // Always use interactive mode for authentication
        });
        if (responseUrl == null)
        {
            throw new AuthenticationException("Identity.LaunchWebAuthFlow did not return a valid response Url.");
        }

        _logger.LogDebug("ResponseUrl: {ResponseUrl}", responseUrl);

        var responseUri = new Uri(responseUrl);
        var code = HttpUtility.ParseQueryString(responseUri.Query).Get("code");
        var error = HttpUtility.ParseQueryString(responseUri.Query).Get("error");

        if (!string.IsNullOrEmpty(error))
        {
            throw new AuthenticationException($"OAuth error: {error}");
        }

        if (string.IsNullOrEmpty(code))
        {
            throw new AuthenticationException("No authorization code received.");
        }

        var token = await ExchangeCodeForTokensAsync(code);

        if (!string.IsNullOrWhiteSpace(token.IdToken))
        {
            token.UserProfile = ParseIdToken(token.IdToken);
        }

        await _webExtensionsApi.Storage.Local.StoreTokenAsync(token);

        return token;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        return !string.IsNullOrWhiteSpace(await GetAccessTokenAsync());
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var accessToken = await _webExtensionsApi.Storage.Local.GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            _logger.LogInformation("Access token is not available.");
            return null;
        }

        var expiry = await _webExtensionsApi.Storage.Local.GetLongAsync(nameof(TokenResponse.ExpiresIn));
        if (expiry == null || expiry <= TimeProvider.System.GetUtcNow().ToUnixTimeMilliseconds())
        {
            _logger.LogInformation("Access token has expired or is invalid. Expiry: {ExpiresIn}", expiry);
            return null;
        }

        return accessToken;
    }

    public async Task<UserProfile?> GetCurrentUserAsync()
    {
        if (string.IsNullOrWhiteSpace(await GetAccessTokenAsync()))
        {
            await AuthenticateAsync();
        }

        try
        {
            return await _webExtensionsApi.Storage.Local.GetPropertyAsync<UserProfile>(nameof(TokenResponse.UserProfile));
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Error getting current user");
            return null;
        }
    }

    private async Task<TokenResponse> ExchangeCodeForTokensAsync(string code)
    {
        using var client = _httpClientFactory.CreateClient(nameof(BrowserExtensionMicrosoftAuthenticator));

        var content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("scope", _scopes),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", _redirectUrl!),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code_verifier", _pkceCodeVerifier!)
        ]);

        var response = await client.PostAsync(_tokenUrl, content);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var error = JsonSerializer.Deserialize<OAuthError>(json);
            throw new Exception($"Token exchange failed: {error?.ErrorDescription ?? error?.Error}");
        }

        return JsonSerializer.Deserialize<TokenResponse>(json)!;
    }

    private static string GenerateRandomState()
    {
        var bytes = new byte[16];
        RandomNumberGenerator.Fill(bytes);
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }

    private static (string CodeVerifier, string CodeChallenge) GeneratePkceParams()
    {
        var codeVerifierBytes = new byte[32];
        RandomNumberGenerator.Fill(codeVerifierBytes);

        var codeVerifier = Base64UrlEncode(codeVerifierBytes);
        using var sha256 = SHA256.Create();
        var challengeBytes = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
        var codeChallenge = Base64UrlEncode(challengeBytes);

        return (codeVerifier, codeChallenge);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static UserProfile ParseIdToken(string idToken)
    {
        var parts = idToken.Split('.');
        if (parts.Length != 3)
        {
            throw new Exception("Invalid ID token");
        }

        var payload = parts[1];
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/')));
        var claims = JsonSerializer.Deserialize<IdTokenJwtPayload>(json)!;

        return new UserProfile
        {
            Name = claims.Name,
            Email = claims.Email,
            UserPrincipalName = claims.Upn ?? claims.PreferredUsername ?? claims.Email
        };
    }
}