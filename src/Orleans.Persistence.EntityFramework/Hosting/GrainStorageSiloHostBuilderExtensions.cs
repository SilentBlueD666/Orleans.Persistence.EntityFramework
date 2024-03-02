using System;
using Microsoft.EntityFrameworkCore;
using Orleans.Hosting;
using Orleans.Providers;

namespace Orleans.Persistence.EntityFramework.Hosting;

public static class GrainStorageSiloHostBuilderExtensions
{
    public static ISiloBuilder AddEntityFrameworkGrainStorageAsDefault<TContext>(this ISiloBuilder builder)
        where TContext : DbContext
    {
        throw new NotSupportedException();
        return builder.AddEntityFrameworkGrainStorage<TContext>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);
    }

    public static ISiloBuilder AddEntityFrameworkGrainStorage<TContext>(this ISiloBuilder builder,
        string providerName)
        where TContext : DbContext
    {
        return builder
            .ConfigureServices(services 
                => services.AddEntityFrameworkGrainStorage<TContext>(providerName));
    }
}
