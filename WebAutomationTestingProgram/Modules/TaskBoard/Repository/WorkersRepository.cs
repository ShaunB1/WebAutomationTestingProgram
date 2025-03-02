using Microsoft.EntityFrameworkCore;
using WebAutomationTestingProgram.Infrastructure.Database;
using WebAutomationTestingProgram.Modules.TaskBoard.Models;

namespace WebAutomationTestingProgram.Modules.TaskBoard.Repository;

public interface IWorkersRepository
{
    Task<List<WorkerModel>> GetAllWorkersAsync();
    Task<WorkerModel> AddWorkerAsync(WorkerModel worker);
    Task<WorkerModel> DeleteWorkerAsync(WorkerDeleteRequest request);
}

public class WorkersRepository : IWorkersRepository
{
    private readonly AtpDbContext _context;

    public WorkersRepository(AtpDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<WorkerModel>> GetAllWorkersAsync()
    {
        return await _context.Workers.ToListAsync();
    }

    public async Task<WorkerModel> AddWorkerAsync(WorkerModel worker)
    {
        _context.Workers.Add(worker);
        await _context.SaveChangesAsync();

        return worker;
    }

    public async Task<WorkerModel> DeleteWorkerAsync(WorkerDeleteRequest request)
    {
        var worker = await _context.Workers.FirstOrDefaultAsync(x => x.Name == request.Name);

        if (worker == null)
        {
            return null;
        }
        
        _context.Workers.Remove(worker);
        await _context.SaveChangesAsync();

        return worker;
    }
}