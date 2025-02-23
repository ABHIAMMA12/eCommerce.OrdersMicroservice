using BusinessLogicLayer.Mappers;
using BusinessLogicLayer.ServiceContracts;
using BusinessLogicLayer.Services;
using BusinessLogicLayer.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();

            services.AddAutoMapper(typeof(OrderAddRequestToOrderMappingProfile).Assembly);
            services.AddScoped<IOrderService, OrderService>();

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = $"my-redis:6379";
            });
            return services;
        }
    }
}
