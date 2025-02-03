using AutomationTestingProgram.Modules.DBConnector.Models;
using AutomationTestingProgram.Modules.DBConnector.Repository;

namespace AutomationTestingProgram.Modules.DBConnector.Services;

public interface IWorkersService
{
    Task<List<WorkerModel>> GetAllWorkersAsync();
    Task<WorkerModel> AddWorkerAsync(WorkerModel worker);
    Task<WorkerModel> DeleteWorkerAsync(string name);
}

public class WorkersService : IWorkersService
{
    private readonly IWorkersRepository _repository;

    public WorkersService(IWorkersRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<List<WorkerModel>> GetAllWorkersAsync()
    {
        return await _repository.GetAllWorkersAsync();
    }

    public async Task<WorkerModel> AddWorkerAsync(WorkerModel worker)
    {
        return await _repository.AddWorkerAsync(worker);
    }

    public async Task<WorkerModel> DeleteWorkerAsync(string name)
    {
        return await _repository.DeleteWorkerAsync(name);
    }
}