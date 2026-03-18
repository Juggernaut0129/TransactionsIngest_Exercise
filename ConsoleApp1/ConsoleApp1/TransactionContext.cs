using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

public class TransactionContext : DbContext
{
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionUpdate> TransactionUpdates { get; set; }

    public string DbPath { get; }
    public TransactionContext(IConfiguration config)
    {
        DbPath = System.IO.Path.Join(config.GetConnectionString("DefaultConnection"), "transactions.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite($"Data Source={DbPath}");
}