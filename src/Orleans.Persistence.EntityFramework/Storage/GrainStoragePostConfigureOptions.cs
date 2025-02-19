﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Persistence.EntityFramework.Conventions;
using Orleans.Persistence.EntityFramework.Exceptions;
using Orleans.Runtime;

namespace Orleans.Persistence.EntityFramework.Storage
{
    public class GrainStoragePostConfigureOptions<TContext, TGrain, TGrainState, TEntity>
        : IPostConfigureOptions<GrainStorageOptions<TContext, TGrain, TEntity>>
        where TContext : DbContext
        where TGrain : Grain<TGrainState>
        where TGrainState : new()
        where TEntity : class
    {
        public IGrainStorageConvention<TContext, TGrain, TEntity> Convention { get; }
        public IGrainStorageConvention DefaultConvention { get; }

        public GrainStoragePostConfigureOptions(IServiceProvider serviceProvider)
        {
            DefaultConvention =
                (IGrainStorageConvention)serviceProvider.GetRequiredService(typeof(IGrainStorageConvention));
            Convention = (IGrainStorageConvention<TContext, TGrain, TEntity>)
                serviceProvider.GetService(typeof(IGrainStorageConvention<TContext, TGrain, TEntity>));
        }

        public void PostConfigure(string name, GrainStorageOptions<TContext, TGrain, TEntity> options)
        {
            if (!string.Equals(name, typeof(TGrain).FullName))
                throw new Exception("Post configure on wrong grain type.");

            if (options.IsPersistedFunc == null)
                options.IsPersistedFunc =
                    DefaultConvention.CreateIsPersistedFunc<TEntity>(options);

            // Configure ETag
            if (options.ShouldUseETag)
            {
                if (!string.IsNullOrWhiteSpace(options.ETagPropertyName))
                    DefaultConvention.ConfigureETag(options.ETagPropertyName, options);
            }

            if (options.ReadStateAsync == null)
            {
                if (options.DbSetAccessor == null)
                    options.DbSetAccessor = Convention?.CreateDefaultDbSetAccessorFunc()
                                        ?? DefaultConvention.CreateDefaultDbSetAccessorFunc<TContext, TEntity>();

                if (Convention != null)
                    Convention.SetDefaultKeySelector(options);
                else
                    DefaultConvention.SetDefaultKeySelectors(options);

                if (options.PreCompileReadQuery)
                {
                    options.ReadStateAsync
                        = Convention?.CreatePreCompiledDefaultReadStateFunc(options)
                          ?? DefaultConvention
                              .CreatePreCompiledDefaultReadStateFunc(options);
                }
                else
                {
                    options.ReadStateAsync
                        = Convention?.CreateDefaultReadStateFunc()
                          ?? DefaultConvention
                              .CreateDefaultReadStateFunc(options);
                }
            }

            if (options.SetEntity == null)
                options.SetEntity =
                    Convention?.GetSetterFunc()
                    ?? DefaultConvention.GetSetterFunc<TGrainState, TEntity>();

            if (options.GetEntity == null)
                options.GetEntity =
                    Convention?.GetGetterFunc()
                    ?? DefaultConvention.GetGetterFunc<TGrainState, TEntity>();

            DefaultConvention.FindAndConfigureETag(options, options.ShouldUseETag);

            // todo: Validate options

            options.IsConfigured = true;
        }
    }
}