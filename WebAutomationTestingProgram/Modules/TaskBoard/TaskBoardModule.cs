using WebAutomationTestingProgram.Modules.TaskBoard.Repository;
using WebAutomationTestingProgram.Modules.TaskBoard.Services;

namespace WebAutomationTestingProgram.Modules.TaskBoard;

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