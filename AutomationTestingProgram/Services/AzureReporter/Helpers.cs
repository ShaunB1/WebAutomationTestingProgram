using System.Text;

namespace AutomationTestingProgram.Services;

public class Helpers
{
    public static string GenerateStepsXml(List<TestStep> steps)
    {
        var sb = new StringBuilder();
        var stepId = 1;
        
        sb.AppendLine("<steps>");

        foreach (var step in steps)
        {
            sb.Append($@"
            <step id='{stepId}' type='ActionStep'>
               <parameterizedString isformatted='true'>{step.TestDescription}</parameterizedString>
               <parameterizedString isformatted='true'>ACTION: {step.ActionOnObject}, OBJECT: {step.Object}, VALUE: {step.Value}</parameterizedString>
            </step>
         ");
            stepId++;
        }
        
        sb.AppendLine("</steps>");
        
        return sb.ToString();
    }
}