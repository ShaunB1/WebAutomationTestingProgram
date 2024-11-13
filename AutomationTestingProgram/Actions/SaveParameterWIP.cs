/*using AutomationTestingProgram.TestingData;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutomationTestingProgram.Actions
{
    /// <summary>
    /// This test step saves a parameter value into the run parameters dictionary.
    /// </summary>
    public class SaveParameter : IWebAction
    {
        public string Name { get; set; } = "SaveParameter";

        // Assuming InformationObject.RunParameters is a Dictionary or similar type that you want to use to store the data.
        private readonly Dictionary<string, string> _runParameters;

        public SaveParameter()
        {
            // Assuming InformationObject.RunParameters is a dictionary or similar structure
            _runParameters = InformationObject.RunParameters ?? new Dictionary<string, string>();
        }

        public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration)
        {
            // Extract arguments from the TestStep (Arguments should be a dictionary)
            var value = step.Arguments.ContainsKey("value") ? step.Arguments["value"] : string.Empty;
            var objectValue = step.Arguments.ContainsKey("object") ? step.Arguments["object"] : string.Empty;
            var comment = step.Arguments.ContainsKey("comment") ? step.Arguments["comment"].ToLower() : string.Empty;

            // Validation for required arguments
            if (string.IsNullOrEmpty(value))
            {
                Console.Error.WriteLine("Error: Wrong use of ActionOnObject, 'value' should be filled in the Arguments.");
                step.TestStepStatus.RunSuccessful = false;
                return false;
            }

            if (string.IsNullOrEmpty(objectValue))
            {
                Console.Error.WriteLine("Error: Wrong use of ActionOnObject, 'object' should be filled in the Arguments.");
                step.TestStepStatus.RunSuccessful = false;
                return false;
            }

            try
            {
                // If the parameter exists and 'comment' is "y", update the parameter
                if (_runParameters.ContainsKey(objectValue))
                {
                    if (comment == "y")
                    {
                        _runParameters[objectValue] = value; // Overwrite the existing value
                        Console.WriteLine($"Successfully updated RunParameter {objectValue} to {value}");
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Parameter already exists, not overwriting. Key: {objectValue} with value {_runParameters[objectValue]}");
                    }
                }
                else
                {
                    _runParameters.Add(objectValue, value); // Add new parameter if it doesn't exist
                    Console.WriteLine($"Successfully set RunParameter {objectValue} to {value}");
                }

                // Update the test step status
                step.TestStepStatus.RunSuccessful = true;
                step.TestStepStatus.Actual = "Successfully set dictionary value";
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: Failed to save parameter. Exception: {ex.Message}");
                step.TestStepStatus.RunSuccessful = false;
                step.TestStepStatus.Actual = "Error occurred while saving parameter";
                return false;
            }

            return true;
        }
    }
}
*/