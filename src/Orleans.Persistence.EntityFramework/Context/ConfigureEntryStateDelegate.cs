using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Orleans.Persistence.EntityFramework
{
    public delegate void ConfigureEntryStateDelegate<TGrainState>(EntityEntry<TGrainState> entry)
        where TGrainState : class;
}