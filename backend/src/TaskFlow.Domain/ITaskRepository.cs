namespace TaskFlow.Domain;

public interface ITaskRepository
{
    Task<TaskItem?> FindByIdAsync(int id);
    Task<IReadOnlyList<TaskItem>> ListAllAsync();
    Task AddAsync(TaskItem task);
    Task RemoveAsync(TaskItem task);
    Task SaveChangesAsync();
}
