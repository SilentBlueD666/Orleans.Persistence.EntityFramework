using System;

namespace Orleans.Persistence.EntityFramework
{
    public interface IEntityTypeResolver
    {
        Type ResolveEntityType(string grainType, IGrainState grainState);
        Type ResolveStateType(string grainType, IGrainState grainState);
    }
}