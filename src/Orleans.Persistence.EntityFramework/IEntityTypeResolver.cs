using System;

namespace Orleans.Persistence.EntityFramework;

public interface IEntityTypeResolver
{
    Type ResolveEntityType<T>(string grainType, IGrainState<T> grainState);
    Type ResolveStateType<T>(string grainType, IGrainState<T> grainState);
}