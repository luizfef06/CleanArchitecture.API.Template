﻿using Api.Common.Repository.Contracts.Core.Entities;
using Api.Common.Repository.Contracts.Core.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Api.Common.Repository.EFCore
{
    public class EfCoreRepositorySoftDelete<TEntity> : IRepository<TEntity> where TEntity : DomainEntity
    {
        protected readonly DbContext context;
        protected readonly DbSet<TEntity> dbSet;

        public void Dispose()
        {
            // Cleanup
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
            context.Dispose();
        }

        public EfCoreRepositorySoftDelete(DbContext context)
        {
            this.context = context;
            this.context.ChangeTracker.AutoDetectChangesEnabled = false;
            this.context.ChangeTracker.LazyLoadingEnabled = false;
            this.context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            dbSet = this.context.Set<TEntity>();
        }

        public IEnumerable<TEntity> All()
        {
            return dbSet.AsEnumerable();
        }

        public async void Delete(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                DeleteInstance(id);
            }

            await context.SaveChangesAsync();;
        }

        public async void Delete(Guid id)
        {
            DeleteInstance(id);
            await context.SaveChangesAsync();;
        }

        public async void Delete(Expression<Func<TEntity, bool>> expression)
        {
            var query = dbSet.Where(expression);
            var instances = query.Where(x => x.IsActive).AsEnumerable();

            foreach (var instance in instances)
            {
                DeleteInstance(instance);
            }

            if (instances.Any())
            {
                await context.SaveChangesAsync();;
            }
        }

        public TEntity Find(Expression<Func<TEntity, bool>> expression)
        {
            var query = dbSet.Where(expression);
            return query.FirstOrDefault(x => x.IsActive);
        }

        public TEntity FindById(Guid id)
        {
            return Find(x => x.Id == id);
        }

        public IEnumerable<TEntity> FindList(Expression<Func<TEntity, bool>> expression)
        {
            var query = dbSet.Where(expression);
            return query.Where(x => x.IsActive).AsEnumerable();
        }

        public async void Insert(TEntity instance)
        {
            instance.Id = Guid.NewGuid();
            instance.CreateDate = DateTime.UtcNow;
            instance.IsActive = true;

            dbSet.Add(instance);
            await context.SaveChangesAsync();;
        }

        public async void Insert(IEnumerable<TEntity> instances)
        {
            foreach (var instance in instances)
            {
                instance.Id = Guid.NewGuid();
                instance.CreateDate = DateTime.UtcNow;
                instance.IsActive = true;

                dbSet.Add(instance);
            }

            await context.SaveChangesAsync();;
        }

        public async void Update(TEntity instance)
        {
            instance.ModifiedDate = DateTime.UtcNow;
            UpdateInstance(instance);
            await context.SaveChangesAsync();;
        }

        public async void Update(IEnumerable<TEntity> instances)
        {
            foreach (var instance in instances)
            {
                UpdateInstance(instance);
            }

            await context.SaveChangesAsync();;
        }

        private void DeleteInstance(Guid id)
        {
            DeleteInstance(FindById(id));
        }

        private void DeleteInstance(TEntity instance)
        {
            instance.IsActive = false;
            UpdateInstance(instance);
        }

        private void UpdateInstance(TEntity instance)
        {
            instance.ModifiedDate = DateTime.UtcNow;
            dbSet.Update(instance);
        }
    }
}