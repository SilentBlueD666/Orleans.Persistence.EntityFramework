using System;

namespace Orleans.Persistence.EntityFramework;

public class EntityTypeResolver : IEntityTypeResolver
{
    public virtual Type ResolveEntityType<T>(string stateName, IGrainState<T> grainState) 
        => ResolveStateType(stateName, grainState);

    public virtual Type ResolveStateType<T>(string stateName, IGrainState<T> grainState) 
        => grainState.GetType().IsGenericType
            ? grainState.GetType().GenericTypeArguments[0]
            : grainState.State.GetType();
}