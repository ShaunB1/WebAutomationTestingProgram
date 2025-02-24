﻿using AutomationTestingProgram.Modules.DBConnector.Repository;
using AutomationTestingProgram.Modules.DBConnector.Services;

namespace AutomationTestingProgram.Modules.TaskBoard;

public static class TaskBoardModule
{
    public static void AddDbConnectorModule(this IServiceCollection services)
    {
        services.AddScoped<ITasksRepository, TasksRepository>();
        services.AddScoped<ITasksService, TasksService>();

        services.AddScoped<ICompletedTasksRepository, CompletedTasksRepository>();
        services.AddScoped<ICompletedTasksService, CompletedTasksService>();

        services.AddScoped<IWorkersRepository, WorkersRepository>();
        services.AddScoped<IWorkersService, WorkersService>();
    }
}