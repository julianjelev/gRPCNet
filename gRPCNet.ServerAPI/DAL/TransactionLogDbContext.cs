using Microsoft.EntityFrameworkCore;
using gRPCNet.ServerAPI.Models.Domain.Logs;

namespace gRPCNet.ServerAPI.DAL
{
    public class TransactionLogDbContext : DbContext
    {
        public TransactionLogDbContext(DbContextOptions<TransactionLogDbContext> options)
            : base(options)
        {
            Database.SetCommandTimeout(300);
        }

        public DbSet<TransactionLog> TransactionLogs { get; set; }
        public DbSet<ActionLog> ActionLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
