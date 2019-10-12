using Api.Common.Repository.Contracts.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Api.Common.Repository.Contracts.Core.Repository
{
    public interface IQueryServiceAsync<TEntity> where TEntity : IDomainEntity
    {
        Task<TEntity> FindById(Guid id);

        Task<TEntity> Find(Expression<Func<TEntity, bool>> expression);

        Task<IEnumerable<TEntity>> All();

        Task<IEnumerable<TEntity>> FindList(Expression<Func<TEntity, bool>> expression);
    }
}