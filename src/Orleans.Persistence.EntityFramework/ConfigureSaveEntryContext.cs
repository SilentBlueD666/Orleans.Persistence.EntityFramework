namespace Orleans.Persistence.EntityFramework
{
    public class ConfigureSaveEntryContext<TContext, TEntity>
    {
        public ConfigureSaveEntryContext(TContext dbContext, TEntity entity)
        {
            DbContext = dbContext;
            Entity = entity;
        }

        public TContext DbContext { get; }

        public TEntity Entity { get; }

        public bool IsPersisted { get; set; }
    }
}