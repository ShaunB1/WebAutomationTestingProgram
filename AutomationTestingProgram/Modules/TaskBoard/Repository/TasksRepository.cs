using AutomationTestingProgram.Infrastructure.Database;
using AutomationTestingProgram.Modules.DBConnector.Models;
using Microsoft.EntityFrameworkCore;

namespace AutomationTestingProgram.Modules.DBConnector.Repository;

public interface ITasksRepository
{
    Task<List<TaskModel>> GetAllTasksAsync();
    Task<TaskModel> AddTaskAsync(TaskModel task);
    Task<TaskModel?> PutTaskAsync(string draggableId, string destinationDroppableId, string startDate);
    Task<TaskModel?> DeleteTaskAsync(string draggableId);
    Task<TaskModel?> PatchTaskDetailsAsync(string draggableId, string name, string description, int? priority);
}

public class TasksRepository : ITasksRepository
{
    private readonly AtpDbContext _context;

    public TasksRepository(AtpDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskModel>> GetAllTasksAsync()
    {
        return await _context.Tasks.ToListAsync();
    }

    public async Task<TaskModel> AddTaskAsync(TaskModel task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return task;
    }

    public async Task<TaskModel?> PutTaskAsync(string draggableId, string destinationDroppableId, string startDate)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.DraggableId == draggableId);

        if (task == null)
        {
            return null;
        }
        
        task.DroppableId = destinationDroppableId;
        task.StartDate = startDate;

        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<TaskModel?> DeleteTaskAsync(string draggableId)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.DraggableId == draggableId);

        if (task == null)
        {
            return null;
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return task;
    }

    public async Task<TaskModel?> PatchTaskDetailsAsync(string draggableId, string name, string description, int? priority)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.DraggableId == draggableId);

        if (task == null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(name))
        {
            task.Name = name;
        }

        if (!string.IsNullOrEmpty(description))
        {
            task.Description = description;
        }

        if (priority.HasValue)
        {
            task.Priority = priority.Value;
        }
        
        await _context.SaveChangesAsync();

        return task;
    }
}