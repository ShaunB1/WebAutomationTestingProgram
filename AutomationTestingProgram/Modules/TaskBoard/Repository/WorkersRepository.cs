using AutomationTestingProgram.Infrastructure.Database;
using AutomationTestingProgram.Modules.DBConnector.Models;
using Microsoft.EntityFrameworkCore;

namespace AutomationTestingProgram.Modules.DBConnector.Repository;

public interface IWorkersRepository
{
    Task<List<WorkerModel>> GetAllWorkersAsync();
    Task<WorkerModel> AddWorkerAsync(WorkerModel worker);
    Task<WorkerModel> DeleteWorkerAsync(string name);
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

    public async Task<WorkerModel> DeleteWorkerAsync(string name)
    {
        var worker = await _context.Workers.FirstOrDefaultAsync(x => x.Name == name);

        if (worker == null)
        {
            return null;
        }
        
        _context.Workers.Remove(worker);
        await _context.SaveChangesAsync();

        return worker;
    }
}