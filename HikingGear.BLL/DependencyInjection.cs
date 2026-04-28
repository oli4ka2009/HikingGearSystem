using HikingGear.BLL.Interfaces;
using HikingGear.BLL.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HikingGear.BLL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
        {
            services.AddHttpClient<IWeatherService, WeatherService>();
            services.AddHttpClient<IGearGenerationService, GearGenerationService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITripService, TripService>();
            services.AddScoped<IGearItemService, GearItemService>();

            return services;
        }
    }
}
