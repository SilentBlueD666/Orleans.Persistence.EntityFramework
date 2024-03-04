using Microsoft.EntityFrameworkCore;
using Orleans.Persistence.EntityFramework.Exceptions;
using Orleans.Runtime;
using Orleans.Storage;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Orleans.Persistence.EntityFramework.Storage;

internal sealed class EntityFrameworkGrainStorage<TContext>(
    IServiceProvider serviceProvider,
    IEntityTypeResolver entityTypeResolver) : IGrainStorage
    where TContext : DbContext
{
    private readonly ConcurrentDictionary<string, IGrainStorage> _storage = new();

    public Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        if (!_storage.TryGetValue(stateName, out IGrainStorage storage))
            storage = CreateStorage(stateName, grainId, grainState);

        return storage.ReadStateAsync(stateName, grainId, grainState);
    }

    public Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        if (!_storage.TryGetValue(stateName, out IGrainStorage storage))
            storage = CreateStorage(stateName, grainId, grainState);

        return storage.WriteStateAsync(stateName, grainId, grainState);
    }

    public Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        if (!_storage.TryGetValue(stateName, out IGrainStorage storage))
            storage = CreateStorage(stateName, grainId, grainState);

        return storage.ClearStateAsync(stateName, grainId, grainState);
    }

    private IGrainStorage CreateStorage<T>(string stateName,
        GrainId grainId,
        IGrainState<T> grainState)
    {
        //Type grainImplType = grainId.Type.;
        Type stateType = entityTypeResolver.ResolveStateType(stateName, grainState);
        Type entityType = entityTypeResolver.ResolveEntityType(stateName, grainState);

        Type storageType = typeof(GrainStorage<,,,>)
            .MakeGenericType(typeof(TContext),
                grainImplType, stateType, entityType);

        IGrainStorage storage;

        try
        {
            storage = (IGrainStorage)Activator.CreateInstance(storageType, stateName, serviceProvider);
        }
        catch (Exception e) when (e.InnerException is GrainStorageConfigurationException)
        {
            throw e.InnerException;
        }


        return _storage.TryAdd(stateName, storage)
            ? storage
            : throw new InvalidOperationException($"Duplicated state name found {stateName}, state names must be unquie");
    }
}