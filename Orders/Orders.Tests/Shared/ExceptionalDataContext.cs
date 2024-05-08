using Microsoft.EntityFrameworkCore;
using Orders.Backend.Data;

namespace Orders.Tests.Shared
{
    public class ExceptionalDataContext : DataContext
    {
        public ExceptionalDataContext(DbContextOptions<DataContext> options)
            : base(options)
        { }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Test Exception");
        }
    }
}