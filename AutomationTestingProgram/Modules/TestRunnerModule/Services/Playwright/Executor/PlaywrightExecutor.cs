using AutomationTestingProgram.Actions;
using AutomationTestingProgram.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NPOI.HPSF;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    /// <summary>
    /// Class for executing Playwright Actions
    /// </summary>
    public class PlaywrightExecutor
    {   
        // STATIC VARIABLES *********************************************************

        private static readonly Dictionary<string, WebAction> actions = new Dictionary<string, WebAction>();
        private static Dictionary<string, string> actionAliases = new Dictionary<string, string>
        {
            // Populate actions
            { "populatetextbox", "populatewebelement" },
            { "populatehtmleditor", "populatewebelement" },
            { "populateframe", "populatewebelement" },
            { "populatetextboxddl", "populatewebelement" },

            // Verify state actions
            { "verifylinkavailability", "verifywebelementavailability" },
            { "verifybuttonavailability", "verifywebelementavailability" },
            { "verifytextboxavailability", "verifywebelementavailability" },
            { "verifywebradiogroupavailability", "verifywebelementavailability" },
            { "verifycheckboxstatus", "verifywebelementavailability" },
            { "verifycheckboxavailablity", "verifywebelementavailability" },
            { "verifyddlavailability", "verifywebelementavailability" },
            { "verifyhtmleditoravailability", "verifywebelementavailability" },
            { "verifyimageavailability", "verifywebelementavailability" },
            { "verifywebtableavailability", "verifywebelementavailability" },
            
            // Verify content actions
            { "verifyddlcontent", "verifywebelementcontent" },
            { "verifyhtmleditorcontent", "verifywebelementcontent" },
            { "verifyimagecontent", "verifywebelementcontent" },
            { "verifytextboxcontent", "verifywebelementcontent" },

            // Click actions
            { "clickbutton", "clickwebelement" },
            { "clicklink", "clickwebelement" },
            { "clickimage", "clickwebelement" },
            { "clicktablelink", "clickwebelement" },
            { "selectlookup", "clickwebelement" },
            { "selectwebradiogroup", "clickwebelement" },

            // Login actions
            { "enteraadcredentials", "login" },

            // SQL actions
            { "runprsqlscriptrevert", "runsqlscript" },
            { "runprsqlscriptdelete", "runsqlscript" },

            // Upload actions
            { "uploaddatafile", "uploadfile" },
            
            // Other
            { "gotopage", "navigatetourl" },
        };

        // INSTANCE VARIABLES **********************************************************

        /// <summary>
        /// The context linked with this executor
        /// </summary>
        private Context _context { get; }

        /// <summary>
        /// The request linked with the context
        /// </summary>
        private ProcessRequest _request { get; }

        /// <summary>
        /// Dictionary of all environment variables used for the current test run
        /// 
        /// NOTE: LOWERCASE ALWAYS
        /// </summary>
        private Dictionary<string, string> _envVars;

        /// <summary>
        /// Dictionary of all currently saved parameters in the test run.
        /// Note: {UNIQUE_IDENTIFIER} by default exists, and is defined at initialization.
        ///       Other parameters must be defined with SetParameter ActionOnObject
        ///       
        /// NOTE: UPPERCASE ALWAYS
        /// </summary>
        private Dictionary<string, string> _parameters;

        /// <summary>
        /// Token used for cancellations purposes.
        /// Will be periodically checked, and an error will be throw if request is cancelled.
        /// </summary>
        private CancellationToken _cancellationToken { get; }

        /// <summary>
        /// Reader used to retrieve Test Steps from file.
        /// </summary>
        private IReader _reader;

        /// <summary>
        /// Hub used to broadcast logs to front-end with SignalR
        /// </summary>
        private readonly IHubContext<TestHub> _hubContext;


        static PlaywrightExecutor()
        {
            string actionsFilePath = AppConfiguration.GetSection<PathSettings>("PATHS").ActionsPath;

            if (!File.Exists(actionsFilePath))
            {
                throw new FileNotFoundException("Actions file not found.", actionsFilePath);
            }

            string jsonContent = File.ReadAllText(actionsFilePath);

            var actionDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);


            actions = actionDict
                .Select(kvp =>
                {
                    var actionType = Type.GetType(kvp.Value, throwOnError: false);

                    if (actionType == null || !typeof(WebAction).IsAssignableFrom(actionType))
                    {
                        throw new InvalidOperationException($"Action type '{kvp.Value}' not found.");
                    }

                    var webAction = (WebAction?)Activator.CreateInstance(actionType);
                    return new { Key = kvp.Key, Action = webAction };
                })
                .Where(item => item != null)
                .ToDictionary(
                    item => item!.Key,
                    item => item!.Action!
                );
        }

        /// <summary>
        /// The playwrighr Executor instance for running tests
        /// </summary>
        /// <param name="context">The associated Context object.</param>
        public PlaywrightExecutor(Context context, IReaderFactory readerFactory, IHubContext<TestHub> hubContext)
        {
            _context = context;

            _request = context.Request;

            _envVars = new Dictionary<string, string>();
            _envVars["environment"] = _request.Environment;
            _envVars["delay"] = _request.Delay.ToString();

            _parameters = new Dictionary<string, string>
            {
                {  "UNIQUE_IDENTIFIER", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff") }
            };

            _cancellationToken = _request.CancelToken;

            _reader = readerFactory.CreateReader(Path.Combine(_request.FolderPath, _request.FileName));
            _hubContext = hubContext;
        }

        public async Task ExecuteTestFileAsync(Page page)
        {
            double.TryParse(_envVars["delay"], out double delay);

            TestRun testRun = _reader.TestRun;

            TestCase testCase = _reader.GetCurrentTestCase();
            testCase.StartedDate = DateTime.Now;

            StringBuilder logMessage = new StringBuilder()
                          .AppendLine("========================================================")
                          .AppendLine("                REQUEST INFORMATION                     ")
                          .AppendLine("========================================================")
                          .AppendLine($"ID:               {_request.ID,-40}")
                          .AppendLine($"BROWSER TYPE:     {_request.BrowserType,-40}")
                          .AppendLine($"BROWSER VERSION:  {_request.BrowserVersion,-40}")
                          .AppendLine("--------------------------------------------------------")
                          .AppendLine($"ENVIRONMENT:      {_request.Environment,-40}")
                          .AppendLine($"DELAY:            {_request.Delay,-40}")
                          .AppendLine("========================================================");
;
            page.LogInfo(logMessage.ToString());
            await _hubContext.Clients.Group(_request.ID).SendAsync("BroadcastLog", _request.ID, logMessage.ToString());

            try
            {
                while (!_reader.isComplete)
                {
                    _request.IsCancellationRequested();
                    await Task.Delay(TimeSpan.FromSeconds(delay));

                    TestStep step = await _reader.GetTestStepAsync();

                    if (!testCase.Equals(_reader.GetCurrentTestCase()))
                    {
                        testCase = _reader.GetCurrentTestCase();
                        testCase.StartedDate = DateTime.Now;
                    }

                    try
                    {
                        await ExecuteTestStep(step);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        testCase.FailureCounter++;

                        // Either 5 failed steps, or over 33% failed steps -> TEST CASE FAILS
                        if (testCase.FailureCounter >= 5 ||
                            testCase.FailureCounter / (double)testCase.TestStepNum >= 0.33)
                        {
                            testCase.Result = Result.Failed;
                            testCase.CompletedDate = DateTime.Now;
                            testRun.FailureCounter++;
                        }
                    }
                    

                    

                    // Either 5 failed cases, or over 33% failed cases -> TEST RUN FAILS
                    if (testRun.FailureCounter >= 5 ||
                        testRun.FailureCounter / (double)testRun.TestCaseNum >= 0.33)
                    {
                        testRun.Result = Result.Failed;
                        testRun.CompletedDate = DateTime.Now;
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                logMessage.Clear()
                          .AppendLine("========================================================")
                          .AppendLine("                REQUEST CANCELLED                       ")
                          .AppendLine("========================================================")
                          .AppendLine($"TEST EXECUTION CANCELLED                               ")
                          .AppendLine("--------------------------------------------------------")
                          .AppendLine($"ID:               {_request.ID,-40}")
                          .AppendLine($"BROWSER TYPE:     {_request.BrowserType,-40}")
                          .AppendLine($"BROWSER VERSION:  {_request.BrowserVersion,-40}")
                          .AppendLine("--------------------------------------------------------")
                          .AppendLine($"ENVIRONMENT:      {_request.Environment,-40}")
                          .AppendLine($"DELAY:            {_request.Delay,-40}")
                          .AppendLine("========================================================");

                page.LogInfo(logMessage.ToString());
                await _hubContext.Clients.Group(_request.ID).SendAsync("BroadcastLog", _request.ID, logMessage.ToString());

                throw;
            }
            catch (Exception e)
            {
                logMessage.Clear()
                          .AppendLine("========================================================")
                          .AppendLine("                REQUEST FAILED                          ")
                          .AppendLine("========================================================")
                          .AppendLine($"TEST EXECUTION FAILED                                  ")
                          .AppendLine("--------------------------------------------------------")
                          .AppendLine($"ID:               {_request.ID,-40}")
                          .AppendLine($"BROWSER TYPE:     {_request.BrowserType,-40}")
                          .AppendLine($"BROWSER VERSION:  {_request.BrowserVersion,-40}")
                          .AppendLine("--------------------------------------------------------")
                          .AppendLine($"ENVIRONMENT:      {_request.Environment,-40}")
                          .AppendLine($"DELAY:            {_request.Delay,-40}")
                          .AppendLine("--------------------------------------------------------")
                          .AppendLine($"ERROR MESSAGE:    {e.Message,-40}")
                          .AppendLine($"STACK TRACE:      {e.StackTrace,-40}");
                if (e.InnerException != null)
                {
                    logMessage.AppendLine($"INNER EXCEPTION:  {e.InnerException,-40}");
                }                          
                logMessage.AppendLine("========================================================");

                page.LogInfo(logMessage.ToString());
                await _hubContext.Clients.Group(_request.ID).SendAsync("BroadcastLog", _request.ID, logMessage.ToString());

                throw;
            }
        }

        /// <summary>
        /// Executes a singular test step.
        /// This includes re-attempts;
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public async Task ExecuteTestStep(TestStep step)
        {
            string ActionOnObject = GetAlias(step.ActionOnObject.Replace(" ", "").ToLower());
            step.Object = InsertParams(step.Object);
            step.Value = InsertParams(step.Value);

            logMessage.Clear()
                      .AppendLine("========================================================")
                      .AppendLine("                TEST EXECUTION LOG                      ")
                      .AppendLine("========================================================")
                      .AppendLine($"TEST CASE:     {testCase.Name,-40}")
                      .AppendLine($"DESCRIPTION:   {step.TestDescription,-40}")
                      .AppendLine($"STEP NUM:      {step.StepNum,-40}")
                      .AppendLine("--------------------------------------------------------")
                      .AppendLine($"ACTION:        {ActionOnObject,-40}")
                      .AppendLine($"OBJECT:        {step.Object,-40}")
                      .AppendLine($"VALUE:         {step.Value,-40}")
                      .AppendLine($"COMMENT:       {step.Comments,-40}")
                      .AppendLine("--------------------------------------------------------")
                      .AppendLine($"EXECUTING...")
                      .AppendLine("========================================================");

            page.LogInfo(logMessage.ToString());
            await _hubContext.Clients.Group(_request.ID).SendAsync("BroadcastLog", _request.ID, logMessage.ToString());



            if (actions.TryGetValue(ActionOnObject, out var action))
            {

            }
            else
            {
                step.Result = Result.Failed;
                step.CompletedDate = DateTime.Now;
                step.Message = new StringBuilder()
                          .AppendLine($"STATUS: {step.Result.ToString()}")
                          .AppendLine($"Unknown action: {ActionOnObject,-40}")
                          .AppendLine()
                          .ToString();
                await _hubContext.Clients.Group(_request.ID).SendAsync("BroadcastLog", _request.ID, step.Message);
                testCase.FailureCounter++;
            }
        }

        /// <summary>
        /// Checks if provided string action is an alias for another action.
        /// If so, returns alias. If not, returns input value.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public string GetAlias(string action)
        {
            if (actionAliases.TryGetValue(action, out var alias))
            {
                return alias;
            }

            return action;
        }

        /// <summary>
        /// Replace parameters between "{" and "}" with the Saved Parameters. 
        /// To be used with Object or Value string. 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string InsertParams(string input)
        {
            string pattern = @"\{([^}]+)\}";

            var matches = Regex.Matches(input, pattern);

            foreach (Match match in matches)
            {
                string key = match.Groups[1].Value;
                if (_parameters.ContainsKey(key))
                {
                    input = input.Replace(match.Value, _parameters[key]);
                }
            }
            return input;
        }



    }
}
