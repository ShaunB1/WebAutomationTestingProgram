using Microsoft.Playwright;
using WebAutomationTestingProgram.Modules.TestRunnerV1.Models;

namespace WebAutomationTestingProgram.Actions;

public class VerifyTxtFile : WebAction
{
    private const string Seperator = "];[";
    public override async Task<bool> ExecuteAsync(IPage page, TestStep step,
        Dictionary<string, string> envVars, Dictionary<string, string> saveParams,
        Dictionary<string, List<Dictionary<string, string>>> cycleGroups, int currentIteration, string cycleGroupName)
    {
        string option = step.Comments;
        switch (option.ToLower())
        {
            case "":
            case "0":
            case "againsttextfile":
                return VerifyAgainstTextFile(step);
            // Can implement these other options later if needed
            //case "1":
            //case "againststring":
            //    return Task.FromResult(VerifyAgainstString(step));
            //    break;
            //case "2":
            //case "findstring":
            //    this.FindString();
            //    break;
            //case "3":
            //case "againstfilecontents":
            //    this.VerifyAgainstFileContents(false);
            //    break;
            //case "4":
            //case "againstfilecontentsbyline":
            //    this.VerifyAgainstFileContents(true);
            //    break;
            default:
                Console.WriteLine($"{option} is not an option for verify txt file");
                return false;
        }
    }

    private bool VerifyAgainstTextFile(TestStep step)
    {
        try {
            string actual = step.Object;
            string expected = step.Value;

            if (!File.Exists(actual))
            {
                throw new FileNotFoundException("Actual txt file does not exist");
            }
            if (!File.Exists(expected))
            {
                throw new FileNotFoundException("Expected txt file does not exist");
            }

            string actualContent = File.ReadAllText(actual);
            string expectedContent = File.ReadAllText(expected);

            string actualContentNoWhitespace = new string(actualContent.Where(c => !char.IsWhiteSpace(c)).ToArray());
            string expectedContentNoWhitespace = new string(expectedContent.Where(c => !char.IsWhiteSpace(c)).ToArray());

            Console.WriteLine(actualContentNoWhitespace);
            Console.WriteLine(expectedContentNoWhitespace);
            return actualContentNoWhitespace == expectedContentNoWhitespace;
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
            return false;
        }
    }

    //private bool VerifyAgainstString(TestStep step)
    //{
    //    string value = step.Value;
    //    string filepath = step.Object;
    //    if (!File.Exists(filepath))
    //    {
    //        throw new FileNotFoundException("Txt file does not exist");
    //    }

    //    if (value.Contains(Seperator))
    //    {
    //        int lineNumber = int.Parse(value.Substring(0, value.IndexOf(Seperator)));
    //        string expectedString = value.Substring(value.IndexOf(Seperator) + 3);

            
    //    }
    //    else
    //    {
    //        Console.WriteLine("Specified to verify against string. However the value that was passed in did not fit the syntax.");
    //        return false;
    //    }
    //}
}