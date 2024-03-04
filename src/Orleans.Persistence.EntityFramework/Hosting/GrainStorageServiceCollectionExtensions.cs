using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orleans.Persistence.EntityFramework.Conventions;
using Orleans.Persistence.EntityFramework.Storage;
using Orleans.Providers;
using Orleans.Storage;

namespace Orleans.Persistence.EntityFramework.Hosting;

public static class GrainStorageServiceCollectionExtensions
{
    public static IServiceCollection ConfigureGrainStorageOptions<TContext, TGrain, TEntity>(
        this IServiceCollection services,
        Action<GrainStorageOptions<TContext, TGrain, TEntity>> configureOptions = null)
        where TContext : DbContext
        where TGrain : Grain<TEntity>
        where TEntity : class, new()
    {
        return services
            .AddSingleton<IPostConfigureOptions<GrainStorageOptions<TContext, TGrain, TEntity>>,
                GrainStoragePostConfigureOptions<TContext, TGrain, TEntity, TEntity>>()
            .Configure<GrainStorageOptions<TContext, TGrain, TEntity>>(typeof(TGrain).FullName, options =>
            {
                configureOptions?.Invoke(options);
            });
    }

    public static IServiceCollection ConfigureGrainStorageOptions<TContext, TGrain, TGrainState, TEntity>(
        this IServiceCollection services,
        Action<GrainStorageOptions<TContext, TGrain, TEntity>> configureOptions = null)
        where TContext : DbContext
        where TGrain : Grain<TGrainState>
        where TGrainState : new()
        where TEntity : class
    {
        return services
            .AddSingleton<IPostConfigureOptions<GrainStorageOptions<TContext, TGrain, TEntity>>,
                GrainStoragePostConfigureOptions<TContext, TGrain, TGrainState, TEntity>>()
            .Configure<GrainStorageOptions<TContext, TGrain, TEntity>>(typeof(TGrain).FullName, options =>
            {
                configureOptions?.Invoke(options);
            });
    }

    /// <summary>
    /// Add Entity Framework storage provider services.
    /// </summary>
    /// <typeparam name="TContext">The type of the Entity Framework <see cref="DbContext"/>.</typeparam>
    /// <param name="services"></param>
    /// <param name="providerName">The storage provider name.</param>
    public static IServiceCollection AddEntityFrameworkGrainStorage<TContext>(
        this IServiceCollection services,
        string providerName = ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)
        where TContext : DbContext
    {
        if (string.IsNullOrWhiteSpace(providerName))
            throw new ArgumentException("The storage provider name cannot be null or empty.", nameof(providerName));

        services.TryAddSingleton(typeof(IEntityTypeResolver), typeof(EntityTypeResolver));
        services.TryAddSingleton(typeof(IGrainStorageConvention), typeof(GrainStorageConvention));
        services.TryAddSingleton(typeof(IGrainStateEntryConfigurator<,,>), typeof(DefaultGrainStateEntryConfigurator<,,>));
        services.AddSingleton(typeof(EntityFrameworkGrainStorage<TContext>));

        services.TryAddSingleton(sp =>
            sp.GetKeyedService<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));

        services.AddKeyedSingleton<IGrainStorage>(providerName,
            (sp, name) => sp.GetRequiredService<EntityFrameworkGrainStorage<TContext>>());

        return services;
    }
}