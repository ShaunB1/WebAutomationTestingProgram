using System.Text;

namespace AutomationTestingProgram.Services;

public class Helpers
{
    public static string GenerateStepsXml(List<(string action, string expectedResult)> steps)
    {
        var sb = new StringBuilder();
        var stepId = 1;
        
        sb.AppendLine("<steps>");

        foreach (var step in steps)
        {
            sb.Append($@"
            <step id='{stepId}' type='ActionStep'>
               <parameterizedString isformatted='true'>{step.action}</parameterizedString>
               <parameterizedString isformatted='true'>{step.expectedResult}</parameterizedString>
            </step>
         ");
            stepId++;
        }
        
        sb.AppendLine("</steps>");
        
        return sb.ToString();
    }
}