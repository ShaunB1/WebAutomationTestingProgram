using System.Text;

namespace AutomationTestingProgram.Modules.TestRunnerModule.Services.DevOpsReporting;

public class Helpers
{
    public static string GenerateStepsXml(IList<TestStepObject> steps)
    {
        var sb = new StringBuilder();
        var stepId = 1;

        sb.AppendLine("<steps>");

        foreach (var step in steps)
        {
            sb.Append($@"
            <step id='{stepId}' type='ActionStep' stepIdentifier='{stepId}'>
               <parameterizedString isformatted='true'>{step.TestDescription}</parameterizedString>
               <parameterizedString isformatted='true'>ACTION: {step.ActionOnObject}, OBJECT: {step.Object}, COMMENTS: {step.Comments}, VALUE: {step.Value}</parameterizedString>
            </step>
            ");
            stepId++;
        }

        sb.AppendLine("</steps>");

        return sb.ToString();
    }
}