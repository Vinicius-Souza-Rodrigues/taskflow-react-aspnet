using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Infra;

public class TaskFlowDbContext : DbContext
{
    public TaskFlowDbContext(DbContextOptions<TaskFlowDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(task => task.Id);

            entity.Property(task => task.Title)
                .HasConversion(title => title.Value, value => TaskTitle.Restore(value))
                .IsRequired()
                .HasMaxLength(TaskTitle.MaxLength);

            entity.Property(task => task.Description)
                .HasConversion(description => description!.Value, value => TaskDescription.Restore(value))
                .HasMaxLength(TaskDescription.MaxLength)
                .IsRequired(false);

            entity.Property(task => task.Status).HasConversion<string>().IsRequired();
            entity.Property(task => task.CreatedAt).IsRequired();
            entity.Property(task => task.UpdatedAt).IsRequired();
        });
    }
}
