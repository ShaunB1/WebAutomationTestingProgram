﻿using Microsoft.EntityFrameworkCore;
using WebAutomationTestingProgram.Infrastructure.Database;
using WebAutomationTestingProgram.Modules.TaskBoard.Models;

namespace WebAutomationTestingProgram.Modules.TaskBoard.Repository;

public interface ICompletedTasksRepository
{
    Task<List<CompletedTaskModel>> GetCompletedTasksAsync();
    Task<CompletedTaskModel> AddCompletedTaskAsync(CompletedTaskModel completedTask);
}

public class CompletedTasksRepository : ICompletedTasksRepository
{
    private readonly AtpDbContext _context;

    public CompletedTasksRepository(AtpDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<CompletedTaskModel>> GetCompletedTasksAsync()
    {
        return await _context.CompletedTasks.ToListAsync();
    }

    public async Task<CompletedTaskModel> AddCompletedTaskAsync(CompletedTaskModel completedTask)
    {
        _context.CompletedTasks.Add(completedTask);
        await _context.SaveChangesAsync();
        
        return completedTask;
    }
}