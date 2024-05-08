using Microsoft.EntityFrameworkCore;
using Orders.Backend.Data;

namespace Orders.Tests.Shared
{
    public class ExceptionalDBUpdateDataContext : DataContext
    {
        public ExceptionalDBUpdateDataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new DbUpdateException("Test Exception");
        }
    }
}