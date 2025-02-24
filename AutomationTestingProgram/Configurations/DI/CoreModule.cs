using Autofac;
using AutomationTestingProgram.Core;
using AutomationTestingProgram.Core.Services;
using AutomationTestingProgram.Core.Services.ApplicationLifetime;

namespace AutomationTestingProgram.Configurations.DI;

public class CoreModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(componentContext => new CustomLoggerProvider(LogManager.GetRunFolderPath()))
        .As<ICustomLoggerProvider>()
        .SingleInstance();

        builder.RegisterType<ShutDownService>().SingleInstance();
        builder.RegisterType<RequestHandler>().SingleInstance();
    }
}