﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Orleans.Runtime;

namespace Orleans.Persistence.EntityFramework.Storage;

public abstract class GrainStorageOptions
{
    internal string KeyPropertyName { get; set; }

    internal string KeyExtPropertyName { get; set; }

    internal string ETagPropertyName { get; set; }

    internal string PersistenceCheckPropertyName { get; set; }

    internal IProperty ETagProperty { get; set; }

    internal bool CheckForETag { get; set; }

    internal Func<object, string> ConvertETagObjectToStringFunc { get; set; }

    internal Type ETagType { get; set; }

    public bool ShouldUseETag { get; set; }

    internal bool IsConfigured { get; set; }

    internal bool PreCompileReadQuery { get; set; } = true;
}

public class GrainStorageOptions<TContext, TGrain, TEntity> : GrainStorageOptions
    where TContext : DbContext
    where TEntity : class
{
    internal Func<TContext, IQueryable<TEntity>> DbSetAccessor { get; set; }

    internal Func<TEntity, bool> IsPersistedFunc { get; set; }

    internal Func<TEntity, string> GetETagFunc { get; set; }

    internal Expression<Func<TEntity, Guid>> GuidKeySelector { get; set; }

    internal Expression<Func<TEntity, string>> KeyExtSelector { get; set; }

    internal Func<TEntity, long> LongKeySelector { get; set; }

    internal Func<TContext, GrainId, Task<TEntity>> ReadStateAsync { get; set; }

    internal Action<IGrainState<TEntity>, TEntity> SetEntity { get; set; }

    internal Func<IGrainState<TEntity>, TEntity> GetEntity { get; set; }
}