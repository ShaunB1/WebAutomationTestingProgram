using Autofac;
using AutomationTestingProgram.Core.Services;
using AutomationTestingProgram.Modules.TestRunner.Models.Factories;
using AutomationTestingProgram.Modules.TestRunnerModule;

namespace AutomationTestingProgram.Configurations.DI;

public class TestRunnerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AzureKeyVaultService>().SingleInstance();
        builder.RegisterType<PasswordResetService>().SingleInstance();
        builder.RegisterType<CsvEnvironmentGetter>().SingleInstance();
        
        RegisterPlaywrightServices(builder);
        RegisterFactories(builder);
    }

    private void RegisterPlaywrightServices(ContainerBuilder builder)
    {
        builder.RegisterType<PlaywrightObject>().SingleInstance();
    }

    private void RegisterFactories(ContainerBuilder builder)
    {
        builder.RegisterType<BrowserFactory>().As<IBrowserFactory>().SingleInstance();
        builder.RegisterType<ContextFactory>().As<IContextFactory>().SingleInstance();
        builder.RegisterType<PageFactory>().As<IPageFactory>().SingleInstance();
        builder.RegisterType<ReaderFactory>().As<IReaderFactory>().SingleInstance();
        builder.RegisterType<ExecutorFactory>().As<IPlaywrightExecutorFactory>().SingleInstance();
    }
}