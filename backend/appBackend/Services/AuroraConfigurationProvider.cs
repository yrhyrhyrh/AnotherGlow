using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace appBackend.Services;
public static class AuroraConfigurationProvider
{
    public class DbKeyReferenceConfig
    {
        public string PartialConnStrKey { get; set; } = string.Empty;
        public string UsrNameConfigKey { get; set; } = string.Empty;
        public string CredValueConfigKey { get; set; } = string.Empty;
    }

    public static IServiceCollection EstablishAuroraConnection<TDbContext>(this IServiceCollection services, IConfiguration configuration, Action<DbKeyReferenceConfig>? configAction = null) where TDbContext : DbContext
    {
        DbKeyReferenceConfig dbKeyReferenceConfig = new DbKeyReferenceConfig
        {
            PartialConnStrKey = "DB",
            UsrNameConfigKey = "ConnectionStrings:DB:username",
            CredValueConfigKey = "ConnectionStrings:DB:password"
        };
        configAction?.Invoke(dbKeyReferenceConfig);
        string arg = configuration[dbKeyReferenceConfig.PartialConnStrKey]!;
        string arg2 = configuration[dbKeyReferenceConfig.UsrNameConfigKey]!;
        string arg3 = configuration[dbKeyReferenceConfig.CredValueConfigKey]!;
        string connectionString = $"{arg}User Id ={arg2};Password={arg3};";
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<TDbContext>(delegate (DbContextOptionsBuilder options)
            {
                options.UseNpgsql(connectionString).EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: false).AddXRayInterceptor();
            });
        }

        return services;
    }

    //public static IServiceCollection EstablishCodeTable<TCodeDbContext, TCodeEntity>(this IServiceCollection services) where TCodeDbContext : class, IDromCodeDbContext<TCodeEntity> where TCodeEntity : class, ICodeEntity
    //{
    //    services.AddScoped<ICodeTableRepository<TCodeEntity>, CodeTableRepository<TCodeEntity>>();
    //    services.AddScoped<IDromCodeDbContext<TCodeEntity>, TCodeDbContext>();
    //    return services;
    //}

    //public static IServiceCollection EstablishConfigurationTable<TConfigurationDbContext, TConfigurationEntity>(this IServiceCollection services) where TConfigurationDbContext : class, IDromConfigDbContext<TConfigurationEntity> where TConfigurationEntity : class, IConfigurationEntity
    //{
    //    services.AddScoped<IConfigTableRepository<TConfigurationEntity>, ConfigTableRepository<TConfigurationEntity>>();
    //    services.AddScoped<IDromConfigDbContext<TConfigurationEntity>, TConfigurationDbContext>();
    //    return services;
    //}
}