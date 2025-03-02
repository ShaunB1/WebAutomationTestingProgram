using Autofac;
using WebAutomationTestingProgram.Core;
using WebAutomationTestingProgram.Core.Services;
using WebAutomationTestingProgram.Core.Services.ApplicationLifetime;
using WebAutomationTestingProgram.Core.Services.Logging;

namespace WebAutomationTestingProgram.Configurations.DI;

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