using Api.Common.Repository.Contracts.Core.Entities;
using Api.Common.Repository.Contracts.Core.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Api.Common.Repository.EFCore
{
    public class EfCoreRepository<TEntity> : IRepositoryAsync<TEntity> where TEntity : DomainEntity
    {
        protected readonly DbContext context;
        protected readonly DbSet<TEntity> dbSet;

        public void Dispose()
        {
            // Cleanup
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            ////Commit
            //await context.SaveChangesAsync();

            // Cleanup
            context.Dispose();
        }

        public EfCoreRepository(DbContext context)
        {
            this.context = context;
            this.context.ChangeTracker.AutoDetectChangesEnabled = false;
            this.context.ChangeTracker.LazyLoadingEnabled = false;
            this.context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            dbSet = this.context.Set<TEntity>();
        }

        public IEnumerable<TEntity> All()
        {
            return dbSet.ToArray();
        }

        public async Task Delete(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                await DeleteInstance(id);
            }

            await context.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            await DeleteInstance(id);
            await context.SaveChangesAsync();
        }

        public async Task Delete(Expression<Func<TEntity, bool>> expression)
        {
            var instances = dbSet.Where(expression).ToArray();
            foreach (var instance in instances)
            {
                DeleteInstance(instance);
            }

            if (instances.Any())
            {
                await context.SaveChangesAsync();
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

        public async Task Insert(TEntity instance)
        {
            instance.Id = Guid.NewGuid();
            instance.CreateDate = DateTime.UtcNow;

            dbSet.Add(instance);
            await context.SaveChangesAsync();
        }

        public async Task Insert(IEnumerable<TEntity> instances)
        {
            foreach (var instance in instances)
            {
                instance.Id = Guid.NewGuid();
                instance.CreateDate = DateTime.UtcNow;

                dbSet.Add(instance);
            }

            await context.SaveChangesAsync();
        }

        public async Task Update(TEntity instance)
        {
            UpdateInstance(instance);
            await context.SaveChangesAsync();
        }

        public async Task Update(IEnumerable<TEntity> instances)
        {
            foreach (var instance in instances)
            {
                UpdateInstance(instance);
            }

            await context.SaveChangesAsync();
        }

        private async Task DeleteInstance(Guid id)
        {
            var instance = FindById(id);

            if (instance != null)
                DeleteInstance(instance);
        }

        private void DeleteInstance(TEntity instance)
        {
            dbSet.Remove(instance);
        }

        private void UpdateInstance(TEntity instance)
        {
            instance.ModifiedDate = DateTime.UtcNow;
            dbSet.Update(instance);
        }
    }
}