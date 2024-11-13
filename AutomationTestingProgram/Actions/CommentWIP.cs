using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace AutomationTestingProgram.Actions
{
    /// <summary>
    /// This test step is used to log a comment in the test execution.
    /// </summary>
    public class Comment : IWebAction
    {
        public string Name { get; set; } = "Comment";

        public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration)
        {
            // Fetch the comment from the step Arguments or Value if available
            /*var comment = step.Arguments.ContainsKey("comment") ? step.Arguments["comment"] : step.Value;

            // If comment is empty or not provided, log a warning
            if (string.IsNullOrEmpty(comment))
            {
                Console.WriteLine("Warning: No comment provided in the TestStep.");
                return false;
            }

            // Log the comment to the console
            Console.WriteLine($"Comment: {comment}");*/

            // Mark the step as successful (no action to perform in this case)
            /*step.TestStepStatus.RunSuccessful = true;
            step.TestStepStatus.Actual = "Comment logged successfully";*/

            return true;
        }
    }
}
