using Microsoft.EntityFrameworkCore;
using Orders.Backend.Data;

namespace Orders.Tests.Shared
{
    public class ExceptionalDBUpdateDataContextWithInnerException : DataContext
    {
        public ExceptionalDBUpdateDataContextWithInnerException(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var innerException = new Exception("duplicate record");
            throw new DbUpdateException("Test Exception", innerException);
        }
    }
}