using Blazor.BrowserExtension.Authentication.Microsoft.Services;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Blazor.BrowserExtension.Authentication.Microsoft.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Microsoft Authenticator Service
    /// </summary>
    public static IServiceCollection AddMicrosoftAuthentication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpClient(nameof(BrowserExtensionMicrosoftAuthenticator));

        return services
            .AddWebExtensions()
            .AddSingleton<IIHttpClientFactory, HttpClientFactoryService>()
            .AddSingleton<IBrowserExtensionMicrosoftAuthenticator, BrowserExtensionMicrosoftAuthenticator>();
    }
}