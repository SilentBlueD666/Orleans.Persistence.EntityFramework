using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Orleans.Storage;
using System;
using System.Threading.Tasks;

namespace Orleans.Persistence.EntityFramework.Storage;

internal sealed class GrainStorage<TContext, TGrain, TGrainState, TEntity> : IGrainStorage
    where TContext : DbContext
    where TGrain : Grain<TGrainState>
    where TGrainState : class, new()
    where TEntity : class
{
    private readonly GrainStorageOptions<TContext, TGrain, TEntity> _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GrainStorage<TContext, TGrain, TGrainState, TEntity>> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IGrainStateEntryConfigurator<TContext, TGrain, TEntity> _entryConfigurator;

    public GrainStorage(string stateName, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(stateName);

        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        _entryConfigurator = (IGrainStateEntryConfigurator<TContext, TGrain, TEntity>)serviceProvider.GetRequiredService(typeof(IGrainStateEntryConfigurator<TContext, TGrain, TEntity>));

        var loggerFactory = _serviceProvider.GetService<ILoggerFactory>();

        _logger = loggerFactory?.CreateLogger<GrainStorage<TContext, TGrain, TGrainState, TEntity>>()
            ?? NullLogger<GrainStorage<TContext, TGrain, TGrainState, TEntity>>.Instance;

        _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        _options = GetOrCreateDefaultOptions(stateName);
    }

    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        using var scope = _scopeFactory.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<TContext>();

        TEntity entity = await _options.ReadStateAsync(context, grainId).ConfigureAwait(false);

        _options.SetEntity((IGrainState<TEntity>)grainState, entity);

        if (entity is not null && _options.CheckForETag)
            grainState.ETag = _options.GetETagFunc(entity);
    }

    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        TEntity entity = _options.GetEntity((IGrainState<TEntity>)grainState);

        using IServiceScope scope = _scopeFactory.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<TContext>();

        if (GrainStorageContext<TEntity>.IsConfigured)
        {
            EntityEntry<TEntity> entry = context.Entry(entity);
            GrainStorageContext<TEntity>.ConfigureStateDelegate(entry);
        }
        else
        {
            var isPersisted = _options.IsPersistedFunc(entity);

            _entryConfigurator.ConfigureSaveEntry(
                new ConfigureSaveEntryContext<TContext, TEntity>(
                    context, entity)
                {
                    IsPersisted = isPersisted
                });
        }

        try
        {
            await context.SaveChangesAsync()
                .ConfigureAwait(false);

            if (_options.CheckForETag)
                grainState.ETag = _options.GetETagFunc(entity);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!_options.CheckForETag)
                throw new InconsistentStateException(ex.Message, ex);

            var storedETag = ex.Entries[0].OriginalValues[_options.ETagProperty];
            throw new InconsistentStateException(
                errorMsg: ex.Message,
                storedEtag: _options.ConvertETagObjectToStringFunc(storedETag),
                currentEtag: grainState.ETag,
                storageException: ex);
        }
    }

    public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        TEntity entity = _options.GetEntity((IGrainState<TEntity>)grainState);
        using IServiceScope scope = _scopeFactory.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<TContext>();

        context.Remove(entity);
        await context.SaveChangesAsync()
            .ConfigureAwait(false);
    }


    private GrainStorageOptions<TContext, TGrain, TEntity> GetOrCreateDefaultOptions(string stateName)
    {
        var options = _serviceProvider.GetOptionsByName<GrainStorageOptions<TContext, TGrain, TEntity>>(stateName);

        if (options.IsConfigured)
            return options;

        // Try generating a default options for the grain

        Type optionsType = typeof(GrainStoragePostConfigureOptions<,,,>)
            .MakeGenericType(
                typeof(TContext),
                typeof(TGrain),
                typeof(TGrainState),
                typeof(TEntity));

        var postConfigure = (IPostConfigureOptions<GrainStorageOptions<TContext, TGrain, TEntity>>)Activator.CreateInstance(optionsType, _serviceProvider) 
            ?? throw new InvalidOperationException($"Failed to create post configure options instance {nameof(IPostConfigureOptions<GrainStorageOptions<TContext, TGrain, TEntity>>)}.");

        postConfigure.PostConfigure(stateName, options);

        _logger.LogWarning($"GrainStorageOptions is not configured for grain state {stateName} " +
                               "and default options will be used. If default configuration is not desired, " +
                               "consider configuring options for grain using " +
                               "using IServiceCollection.ConfigureGrainStorageOptions<TContext, TGrain, TState> extension method.");

        return options;
    }
}