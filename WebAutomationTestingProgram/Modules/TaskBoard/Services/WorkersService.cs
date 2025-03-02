using WebAutomationTestingProgram.Modules.TaskBoard.Models;
using WebAutomationTestingProgram.Modules.TaskBoard.Repository;

namespace WebAutomationTestingProgram.Modules.TaskBoard.Services;

public interface IWorkersService
{
    Task<List<WorkerModel>> GetAllWorkersAsync();
    Task<WorkerModel> AddWorkerAsync(WorkerModel worker);
    Task<WorkerModel> DeleteWorkerAsync(WorkerDeleteRequest request);
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

    public async Task<WorkerModel> DeleteWorkerAsync(WorkerDeleteRequest request)
    {
        return await _repository.DeleteWorkerAsync(request);
    }
}