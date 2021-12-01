using Galebra.Security.Headers.Csp.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace Galebra.Security.Headers.Csp
{ 

public static class CspServiceCollectionExtensions
{
    /// <summary>
    /// Adds the content security policy singleton service
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configurationSection"></param>
    /// <returns></returns>
    public static IServiceCollection
        AddContentSecurityPolicy(this IServiceCollection services,
        IConfigurationSection configurationSection)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configurationSection is null)
        {
            throw new ArgumentNullException(nameof(configurationSection));
        }

        return services.AddContentSecurityPolicy(c => configurationSection.Bind(c));
    }

    /// <summary>
    /// Adds the content security policy singleton service
    /// </summary>
    /// <param name="services"></param>
    /// <param name="cspOptionsBuilderAction"></param>
    /// <returns></returns>
    public static IServiceCollection
        AddContentSecurityPolicy(this IServiceCollection services,
        Action<ICspOptionsBuilder> cspOptionsBuilderAction)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (cspOptionsBuilderAction is null)
        {
            throw new ArgumentNullException(nameof(cspOptionsBuilderAction));
        }

        var cspOptions = new CspOptions();
        cspOptionsBuilderAction(cspOptions);

        //Check and build the user's Csp data
        cspOptions.Build();

        //Get all groups having a nonceable, either in csp or report-only, or both
        var nonceableGroups = cspOptions.PolicyGroups
            .Where(k => k.Value.RequiresNonceForAtLeastOne)
            //.Where(k => k.Value.Csp.RequiresNonce || k.Value.CspReportOnly.RequiresNonce)
            .ToDictionary(k => k.Key, v => v.Value);

            //This is better than (new CcpNonceService()) as the latter is not disposed.
            //The func allows for automatic disposal. See DI guidlines disposing services
            //https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-registration-methods
            //TryAdd does not have this overload
            //services.AddScoped<ICspNonce, CspNonce>(s => new CspNonce(nonceableGroups));
            var oCspNonce = new CspNonce(nonceableGroups);
            oCspNonce.GenerateNonces();
            services.TryAddScoped<ICspNonce>(sp => oCspNonce);

            //Cannot be scoped unless you change the middleware ctor
            services.TryAddSingleton<ICspOptions>(sp => cspOptions);
        return services;
    }
}

}