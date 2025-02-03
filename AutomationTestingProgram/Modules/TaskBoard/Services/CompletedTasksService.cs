

using AutomationTestingProgram.Modules.DBConnector.Models;
using AutomationTestingProgram.Modules.DBConnector.Repository;

namespace AutomationTestingProgram.Modules.DBConnector.Services;

public interface ICompletedTasksService
{
    Task<List<CompletedTaskModel>> GetAllCompletedTasksAsync();
    Task<CompletedTaskModel> AddCompletedTaskAsync(CompletedTaskModel completedTask);
}

public class CompletedTasksService : ICompletedTasksService
{
    private readonly ICompletedTasksRepository _repository;

    public CompletedTasksService(ICompletedTasksRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<List<CompletedTaskModel>> GetAllCompletedTasksAsync()
    {
        return await _repository.GetCompletedTasksAsync();
    }

    public async Task<CompletedTaskModel> AddCompletedTaskAsync(CompletedTaskModel completedTask)
    {
        return await _repository.AddCompletedTaskAsync(completedTask);
    }
}