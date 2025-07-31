using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Blazor.BrowserExtension.Authentication.Microsoft.Interop;
using Blazor.BrowserExtension.Authentication.Microsoft.Models;
using Microsoft.Extensions.Configuration;

namespace Blazor.BrowserExtension.Authentication.Microsoft.Services;

internal class BrowserExtensionMicrosoftAuthenticator : IBrowserExtensionMicrosoftAuthenticator
{
    private readonly ChromeIdentity _chromeIdentity;
    private readonly ChromeStorageLocal _storage;

    private readonly string _clientId;
    private readonly string _scopes;
    private readonly string _authUrl;
    private readonly string _tokenUrl;

    private string? _pkceCodeVerifier;
    private string? _redirectUrl;

    public BrowserExtensionMicrosoftAuthenticator(IConfiguration config, ChromeIdentity chromeIdentity, ChromeStorageLocal storage)
    {
        _clientId = config.GetSection("AzureAd:ClientId").Get<string>() ?? throw new ArgumentException("ClientId is not configured.");

        var scopes = config.GetSection("AzureAd:Scopes").Get<string[]>() ?? throw new ArgumentException("Scopes are not configured.");
        _scopes = string.Join(' ', scopes);

        var authority = config.GetSection("AzureAd:Authority").Get<string>() ?? throw new ArgumentException("Authority is not configured.");
        _authUrl = $"{authority}/oauth2/v2.0/authorize";
        _tokenUrl = $"{authority}/oauth2/v2.0/token";

        _chromeIdentity = chromeIdentity;
        _storage = storage;
    }

    public async Task<TokenResponse> AuthenticateAsync()
    {
        Console.WriteLine("Starting Microsoft OAuth flow...");

        _redirectUrl = await _chromeIdentity.GetRedirectUrlAsync();

        var (codeVerifier, codeChallenge) = GeneratePkceParams();
        _pkceCodeVerifier = codeVerifier;

        var authUri = new UriBuilder(_authUrl)
        {
            Query = $"client_id={_clientId}" +
                    $"&response_type=code" +
                    $"&redirect_uri={HttpUtility.UrlEncode(_redirectUrl)}" +
                    $"&scope={HttpUtility.UrlEncode(string.Join(' ', _scopes))}" +
                    $"&response_mode=query" +
                    $"&state={GenerateRandomState()}" +
                    $"&code_challenge={codeChallenge}" +
                    $"&code_challenge_method=S256"
        }.Uri;

        Console.WriteLine($"Authorization URL: {authUri}");

        var responseUrl = (await _chromeIdentity.LaunchInteractiveWebAuthFlowAsync(authUri.ToString()))!;

        Console.WriteLine($"ResponseUrl: {responseUrl}");

        var code = HttpUtility.ParseQueryString(new Uri(responseUrl).Query).Get("code");
        var error = HttpUtility.ParseQueryString(new Uri(responseUrl).Query).Get("error");

        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception($"OAuth error: {error}");
        }

        if (string.IsNullOrEmpty(code))
        {
            throw new Exception("No authorization code received.");
        }

        Console.WriteLine($"Authorization code received:': {code}");

        var token = await ExchangeCodeForTokensAsync(code);

        Console.WriteLine($"token: {JsonSerializer.Serialize(token)}");

        if (!string.IsNullOrWhiteSpace(token.IdToken))
        {
            token.UserProfile = ParseIdToken(token.IdToken);
        }

        await _storage.SetAsync(new Dictionary<string, object>
        {
            { "tokenType", token.TokenType },
            { "accessToken", token.AccessToken },
            { "refreshToken", token.RefreshToken },
            { "tokenExpiry", (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + token.ExpiresIn * 1000).ToString() },
            { "idToken", token.IdToken }
        });

        return token;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var accessToken = await _storage.GetAccessTokenAsync();
            var expiryStr = await _storage.GetSingleStringAsync("tokenExpiry");

            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(expiryStr))
            {
                return false;
            }

            if (!long.TryParse(expiryStr, out var expiry))
            {
                return false;
            }

            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return expiry > now;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error checking authentication: " + ex.Message);
            return false;
        }
    }

    public async Task<UserProfile?> GetCurrentUserAsync()
    {
        try
        {
            var accessToken = await _storage.GetAccessTokenAsync();
            var expiryStr = await _storage.GetSingleStringAsync("tokenExpiry");

            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(expiryStr))
            {
                return null;
            }

            if (!long.TryParse(expiryStr, out var expiry) || expiry <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            {
                return null;
            }

            var userJson = await _storage.GetSingleStringAsync("userProfile");
            if (string.IsNullOrWhiteSpace(userJson))
            {
                return null;
            }

            var userProfile = JsonSerializer.Deserialize<UserProfile>(userJson);
            return userProfile;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error getting current user: " + ex.Message);
            return null;
        }
    }

    private async Task<TokenResponse> ExchangeCodeForTokensAsync(string code)
    {
        using var client = new HttpClient();

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
        Console.WriteLine($"json: {json}");
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
        var claims = JsonSerializer.Deserialize<JwtPayload>(json)!;

        return new UserProfile
        {
            DisplayName = claims.Name,
            Name = claims.Name,
            Email = claims.Email,
            UserPrincipalName = claims.Upn
        };
    }
}