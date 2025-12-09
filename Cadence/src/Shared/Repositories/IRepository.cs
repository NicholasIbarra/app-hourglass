using Shared.Entities;

namespace Shared.Repositories;

public interface IRepository<T, in TId> : IReadOnlyRepository<T, TId> where T : class, IEntity<TId>, IAggregateRoot
{
    Task AddAsync(T entity);

    Task DeleteAsync(TId id);

    Task<int> SaveChangesAsync();
}
