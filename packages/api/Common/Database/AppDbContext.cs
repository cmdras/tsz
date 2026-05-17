using Api.Common.Counters;
using Api.Modules.Contracts;
using Api.Modules.Customers;
using Api.Modules.LeaveTypes;
using Api.Modules.Users;
using Microsoft.EntityFrameworkCore;

namespace Api.Common.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractTask> ContractTasks => Set<ContractTask>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<Counter> Counters => Set<Counter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ContractConfiguration());
        modelBuilder.ApplyConfiguration(new ContractTaskConfiguration());
        modelBuilder.ApplyConfiguration(new CounterConfiguration());
        modelBuilder.ApplyConfiguration(new LeaveTypeConfiguration());
    }
}
