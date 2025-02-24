using AutomationTestingProgram.Infrastructure.Database;
using AutomationTestingProgram.Modules.TaskBoard;
using Microsoft.EntityFrameworkCore;

namespace AutomationTestingProgram.Configurations;

public static class DatabaseConfiguration
{
    public static void Configure(WebApplicationBuilder builder)
    {
        ConfigureDatabase(builder);
    }

    private static void ConfigureDatabase(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AtpDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDB")));
        builder.Services.AddDbConnectorModule();
    }
}