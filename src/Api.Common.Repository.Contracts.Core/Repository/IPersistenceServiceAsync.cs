using Api.Common.Repository.Contracts.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Api.Common.Repository.Contracts.Core.Repository
{
    public interface IPersistenceServiceAsync<TEntity> where TEntity : IDomainEntity
    {
        Task Insert(TEntity instance);

        Task Insert(IEnumerable<TEntity> instances);

        Task Delete(IEnumerable<Guid> ids);

        Task Delete(Expression<Func<TEntity, bool>> expression);

        Task Delete(Guid id);

        Task Update(TEntity instance);

        Task Update(IEnumerable<TEntity> instances);
    }
}