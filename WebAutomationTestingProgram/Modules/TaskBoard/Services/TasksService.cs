using WebAutomationTestingProgram.Modules.TaskBoard.Models;
using WebAutomationTestingProgram.Modules.TaskBoard.Repository;

namespace WebAutomationTestingProgram.Modules.TaskBoard.Services;

public interface ITasksService
{
    Task<List<TaskModel>> GetAllTasksAsync();
    Task<TaskModel?> AddTaskAsync(TaskModel task);
    Task<TaskModel?> PutTaskAsync(string draggableId, string destinationDroppableId, string startDate);
    Task<TaskModel?> DeleteTaskAsync(string draggableId);
    Task<TaskModel?> PatchTaskAsync(string draggableId, string name, string description, int? priority);
}

public class TasksService : ITasksService
{
    private readonly ITasksRepository _repository;

    public TasksService(ITasksRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TaskModel>> GetAllTasksAsync()
    {
        return await _repository.GetAllTasksAsync();
    }

    public async Task<TaskModel> AddTaskAsync(TaskModel task)
    {
        return await _repository.AddTaskAsync(task);
    }

    public async Task<TaskModel?> PutTaskAsync(string draggableId, string destinationDroppableId, string startDate)
    {
        return await _repository.PutTaskAsync(draggableId, destinationDroppableId, startDate);
    }

    public async Task<TaskModel?> DeleteTaskAsync(string draggableId)
    {
        return await _repository.DeleteTaskAsync(draggableId);
    }

    public async Task<TaskModel?> PatchTaskAsync(string draggableId, string name, string description, int? priority)
    {
        return await _repository.PatchTaskDetailsAsync(draggableId, name, description, priority);
    }
}