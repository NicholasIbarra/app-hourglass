using Shared.Entities;
using System.Linq.Expressions;

namespace Shared.Repositories;

public interface IReadOnlyRepository<T, in TId> where T : IEntity<TId>
{
    Task<T?> GetByIdAsync(TId id);

    Task<IEnumerable<T>> GetAllAsync();

    Task<IEnumerable<T>> Where(Expression<Func<T, bool>> predicate);

    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
}