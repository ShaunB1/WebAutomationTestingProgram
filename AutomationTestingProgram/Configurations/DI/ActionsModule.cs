using Autofac;
using AutomationTestingProgram.Actions;

namespace AutomationTestingProgram.Configurations.DI;

public class ActionsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<Login>().InstancePerDependency();
        builder.RegisterType<RunPrSQLScriptDelete>().InstancePerDependency();
        builder.RegisterType<RunPrSQLScriptRevert>().InstancePerDependency();
    }
}