using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain;

namespace TaskFlow.Infra;

public sealed class TaskRepository : ITaskRepository
{
    private readonly TaskFlowDbContext _dbContext;

    public TaskRepository(TaskFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaskItem?> FindByIdAsync(int id) =>
        await _dbContext.Tasks.FirstOrDefaultAsync(task => task.Id == id);

    public async Task<IReadOnlyList<TaskItem>> ListAllAsync() =>
        await _dbContext.Tasks.AsNoTracking().OrderBy(task => task.Id).ToListAsync();

    public async Task AddAsync(TaskItem task) =>
        await _dbContext.Tasks.AddAsync(task);

    public Task RemoveAsync(TaskItem task)
    {
        _dbContext.Tasks.Remove(task);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => _dbContext.SaveChangesAsync();
}
