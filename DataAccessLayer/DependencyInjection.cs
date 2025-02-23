using DataAccessLayer.Repositories;
using DataAccessLayer.RepositoryContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataAccessLayer(this IServiceCollection services,IConfiguration configuration)
        {
            string connectionStringTemplate = configuration.GetConnectionString("MongoConnection")!;
            string connectionString = connectionStringTemplate
                .Replace("$MONGO_HOST", Environment.GetEnvironmentVariable("MONGO_HOST"))
                .Replace("$MONGO_PORT", Environment.GetEnvironmentVariable("MONGO_PORT"));
            //rhis mongoclient shouldnt be used to inject instad we use mongo database
            services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
            services.AddScoped<IMongoDatabase>(provider =>
            {
                IMongoClient client = provider.GetRequiredService<IMongoClient>();
                return client.GetDatabase(Environment.GetEnvironmentVariable("MONGODB_DATABASE"));
            });
            services.AddScoped<IOrdersRepository, OrderRepository>();
            return services;
        }
    }
}
