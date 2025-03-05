using System.Text;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

namespace WebAutomationTestingProgram.Core.Helpers;

public static class LogMessageHelper
{
    public static string GenerateTestExecutionLog(TestStep testStep)
    {
        return new StringBuilder()
            .AppendLine("========================================================")
            .AppendLine("                TEST EXECUTION LOG                      ")
            .AppendLine("========================================================")
            .AppendLine($"TEST CASE:     {testStep.TestCaseName,-40}")
            .AppendLine($"DESCRIPTION:   {testStep.TestDescription,-40}")
            .AppendLine("--------------------------------------------------------")
            .AppendLine($"ACTION:        {testStep.ActionOnObject,-40}")
            .AppendLine($"OBJECT:        {testStep.Object,-40}")
            .AppendLine($"VALUE:         {testStep.Value,-40}")
            .AppendLine($"COMMENTS:         {testStep.Comments,-40}")
            .AppendLine("--------------------------------------------------------")
            .AppendLine($"EXECUTING...")
            .AppendLine("========================================================")
            .ToString();
    }

    public static string GenerateTestStepStatusLog(TestStep testStep)
    {
        return new StringBuilder()
            .AppendLine($"STATUS: {testStep.RunSuccessful}")
            .AppendLine()
            .ToString();
    }

    public static string GenerateCustomLogMessage(string message)
    {
        return new StringBuilder()
            .AppendLine($"Failed to find the element. Retrying...")
            .AppendLine()
            .ToString();
    }
}