using Domain.Imports;
using Microsoft.EntityFrameworkCore;

namespace Application
{
    public interface IApplicationDbContext
    {
        public DbSet<ImportRecord> ImportRecords { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
