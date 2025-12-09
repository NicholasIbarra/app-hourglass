using Shared.Entities;
using Shared.Repositories;

namespace Scheduler.Domain.Repositories;

public interface ISchedulerRepository<T, TId> : IRepository<T, TId> where T : class, IEntity<TId>, IAggregateRoot
{
}
