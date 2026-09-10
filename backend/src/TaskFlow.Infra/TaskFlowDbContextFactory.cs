using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskFlow.Infra;

// Used by `dotnet ef` at design time (e.g. generating migrations) — does not open a
// real connection, so the fallback connection string only needs to be syntactically valid.
public class TaskFlowDbContextFactory : IDesignTimeDbContextFactory<TaskFlowDbContext>
{
    public TaskFlowDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Host=localhost;Port=5432;Database=taskflow;Username=taskflow;Password=taskflow";

        var optionsBuilder = new DbContextOptionsBuilder<TaskFlowDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new TaskFlowDbContext(optionsBuilder.Options);
    }
}
