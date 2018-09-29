using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Feature.LoyaltyPoints.Website.Controllers;
using Feature.LoyaltyPoints.Website.Managers;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;

namespace Feature.LoyaltyPoints.Website.Configurators
{
    public class RegisterDependencies:IServicesConfigurator
    {
        public void Configure(Microsoft.Extensions.DependencyInjection.IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<UnusedCouponsController>();
            serviceCollection.AddSingleton<CouponsServiceProvider>();

        }
    }
}