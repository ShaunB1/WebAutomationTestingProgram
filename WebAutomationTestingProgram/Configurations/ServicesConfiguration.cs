using Autofac;
using WebAutomationTestingProgram.Configurations.DI;

namespace WebAutomationTestingProgram.Configurations;

public static class ServicesConfiguration
{
    public static void Configure(WebApplicationBuilder builder)
    {
        RegisterDependencies(builder);
    }

    private static void RegisterDependencies(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
        {
            containerBuilder.RegisterModule(new CoreModule());
            containerBuilder.RegisterModule(new ActionsModule());
            containerBuilder.RegisterModule(new TestRunnerModule());
        });
    }
}