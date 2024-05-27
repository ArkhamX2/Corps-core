using Microsoft.EntityFrameworkCore;

namespace Corps.Server.Data.Factory
{
    public interface IContextFactory<out TContext> where TContext : DbContext
    {
        DbContext CreateDataContext();
    }
}
