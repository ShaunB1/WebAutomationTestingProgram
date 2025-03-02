using WebAutomationTestingProgram.Modules.TaskBoard;
using Microsoft.EntityFrameworkCore;
using WebAutomationTestingProgram.Infrastructure.Database;

namespace WebAutomationTestingProgram.Configurations;

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