using System;
using Blazor.BrowserExtension.Authentication.Microsoft.Interop;
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
     
        return services
            .AddJsBind()
            .AddSingleton<IChromeStorageLocal, ChromeStorageLocal>()
            .AddSingleton<IChromeIdentity, ChromeIdentity>()
            .AddSingleton<IBrowserExtensionMicrosoftAuthenticator, BrowserExtensionMicrosoftAuthenticator>();
    }
}