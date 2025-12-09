namespace Drachma.Base.Tests.Extensions;

//public static class RepositoryExtensions
//{
//    public static Mock<PT> SetupReadOnlyRepository<T, PT>(this Mock<PT> repository, List<T> source)
//        where T : class, IEntity<Guid>, IAggregateRoot
//        where PT : class, IReadOnlyRepository<T, Guid>
//    {
//        repository
//            .Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
//            .ReturnsAsync((Guid key) => source.FirstOrDefault(e => e.Id.Equals(key)));

//        repository
//            .Setup(repo => repo.GetAllAsync())
//            .ReturnsAsync(() => source.ToList());

//        repository
//            .Setup(ur => ur.Where(It.IsAny<Expression<Func<T, bool>>>()))
//            .ReturnsAsync((Expression<Func<T, bool>> predicate) =>
//                predicate == null ? source.AsReadOnly() : source.AsQueryable().Where(predicate).ToList().AsReadOnly());

//        return repository;
//    }

//    public static Mock<PT> SetupRepository<T, PT>(this Mock<PT> repository, List<T> source)
//        where T : class, IEntity<Guid>, IAggregateRoot
//        where PT : class, IRepository<T, Guid>
//    {
//        repository
//            .Setup(repo => repo.AddAsync(It.IsAny<T>()))
//            .Callback<T>(source.Add);

//        repository
//            .Setup(r => r.DeleteAsync(It.IsAny<Guid>()))
//            .Callback((Guid id) => source.RemoveAll(entity => entity.Id.Equals(id)))
//            .Returns(Task.CompletedTask);

//        repository
//            .Setup(repo => repo.SaveChangesAsync())
//            .Returns(Task.FromResult(1));

//        return repository.SetupReadOnlyRepository(source);
//    }
//}
