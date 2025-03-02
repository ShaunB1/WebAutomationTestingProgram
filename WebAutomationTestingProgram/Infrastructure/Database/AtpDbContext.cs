using Microsoft.EntityFrameworkCore;
using WebAutomationTestingProgram.Modules.TaskBoard.Models;

namespace WebAutomationTestingProgram.Infrastructure.Database;

public class AtpDbContext : DbContext
{
    public AtpDbContext(DbContextOptions<AtpDbContext> options) : base(options) { }
    
    public DbSet<TaskModel> Tasks { get; set; }
    public DbSet<CompletedTaskModel> CompletedTasks { get; set; }
    public DbSet<WorkerModel> Workers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
         
        modelBuilder.Entity<TaskModel>().ToTable("tasks");
        modelBuilder.Entity<CompletedTaskModel>().ToTable("completed_tasks");
        modelBuilder.Entity<WorkerModel>().ToTable("workers");
    }
}