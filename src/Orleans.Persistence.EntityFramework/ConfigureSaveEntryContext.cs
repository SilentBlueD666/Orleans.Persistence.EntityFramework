namespace Orleans.Persistence.EntityFramework;

public class ConfigureSaveEntryContext<TContext, TEntity>(TContext dbContext, TEntity entity)
{
    public TContext DbContext { get; } = dbContext;

    public TEntity Entity { get; } = entity;

    public bool IsPersisted { get; set; }
}