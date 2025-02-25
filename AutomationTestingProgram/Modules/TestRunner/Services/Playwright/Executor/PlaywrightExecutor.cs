using System.Text;
using System.Text.RegularExpressions;
using Autofac;
using AutomationTestingProgram.Actions;
using AutomationTestingProgram.Core;
using AutomationTestingProgram.Core.Hubs;
using AutomationTestingProgram.Core.Settings;
using AutomationTestingProgram.Modules.TestRunner.Backend.Requests.TestController;
using AutomationTestingProgram.Modules.TestRunner.Services.Playwright.Objects;
using AutomationTestingProgram.Modules.TestRunnerModule;
using AutomationTestingProgram.Modules.TestRunnerModule.Services.Playwright.Objects;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace AutomationTestingProgram.Modules.TestRunner.Services.Playwright.Executor
{
    /// <summary>
    /// Class for executing Playwright Actions
    /// </summary>
    public class PlaywrightExecutor : IPlaywrightExecutor
    {   
        // STATIC VARIABLES *********************************************************
        private static Dictionary<string, WebAction> actions = new Dictionary<string, WebAction>();
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
            { "runprsqlscriptrevert", "runprsqlscriptrevert" },
            { "runprsqlscriptdelete", "runprsqlscriptdelete" },
            { "runsqlscript", "runsqlscript" },

            // Upload actions
            { "uploaddatafile", "uploadfile" },
            
            // Other
            { "gotopage", "navigatetourl" },
            { "closebrowser", "closewindow" },
            { "loadfile", "uploadfile" },
            
            // Not implemented
            { "brokenlinks", "notimplementedaction" },
            { "compareemail", "notimplementedaction" },
            { "getemail", "notimplementedaction" },
            { "getwebelementtext", "notimplementedaction" },
            { "launchbrowser", "notimplementedaction" },
            { "mapemailfolder", "notimplementedaction" },
            { "skiptoline", "notimplementedaction" },
            { "sqltocsv", "notimplementedaction" },
            { "verifyemail", "notimplementedaction" },
            { "verifyexcelfile", "notimplementedaction" },
            { "verifysqlquery", "notimplementedaction" },
            
            // Obsolete actions
            { "comparepdf", "obsoleteaction" },
            { "modifytxtfile", "obsoleteaction" },
            { "switchtoiframe", "obsoleteaction" },
            { "enteriframe", "obsoleteaction" },
            { "exitiframe", "obsoleteaction" },
            { "logchecker", "obsoleteaction" },
            { "runtestcase", "obsoleteaction" },
            { "selectddlmultivalues", "obsoleteaction" },
            { "clicksaveasie", "obsoleteaction" },
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
        
        public static void InitializeStaticVariables(IComponentContext componentContext)
        {
            string actionsFilePath = AppConfiguration.GetSection<PathSettings>("Paths").ActionsPath;

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
                        throw new InvalidOperationException($"Action type '{kvp.Value}' not found or is not a WebAction.");
                    }

                    WebAction? webAction = null;
                    try
                    {
                        webAction = Activator.CreateInstance(actionType) as WebAction;
                    }
                    catch (Exception)
                    {
                        webAction = componentContext.Resolve(actionType) as WebAction;
                    }

                    if (webAction == null)
                    {
                        throw new InvalidOperationException($"Failed to create an instance of '{kvp.Value}'.");
                    }

                    return new KeyValuePair<string, WebAction>(kvp.Key, webAction);
                })
                .Where(item => item.Value != null)
                .ToDictionary(
                    item => item.Key,
                    item => item.Value
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
        }

        public async Task ExecuteTestFileAsync(Page page)
        {
            double.TryParse(_envVars["delay"], out double delay);

            TestRun testRun = _reader.TestRun;
            testRun.StartedDate = DateTime.Now;

            // Request starts processing
            StringBuilder logMessage = new StringBuilder()
                          .AppendLine("                                                        ")
                          .AppendLine("========================================================")
                          .AppendLine("                REQUEST INFORMATION                     ")
                          .AppendLine("========================================================")
                          .AppendLine($"ID:               {_request.Id,-40}")
                          .AppendLine($"BROWSER TYPE:     {_request.BrowserType,-40}")
                          .AppendLine($"BROWSER VERSION:  {_request.BrowserVersion,-40}")
                          .AppendLine("--------------------------------------------------------")
                          .AppendLine($"ENVIRONMENT:      {_request.Environment,-40}")
                          .AppendLine($"DELAY:            {_request.Delay,-40}")
                          .AppendLine("========================================================");
;
            await page.LogInfo(logMessage.ToString());

            TestCase? testCase = null;
            TestStep step;

            try
            {   // Test Run starts
                logMessage.Clear()
                         .AppendLine("                                                        ")
                         .AppendLine("========================================================")
                         .AppendLine("          TEST EXECUTION LOG - TEST RUN START           ")
                         .AppendLine("========================================================")
                         .AppendLine($"TEST RUN:     {testRun.Name,-40}")
                         .AppendLine($"NUM OF CASES: {testRun.TestCaseNum,-40}")
                         .AppendLine($"START DATE:   {testRun.StartedDate,-40}")
                         .AppendLine("--------------------------------------------------------")
                         .AppendLine($"EXECUTING...")
                         .AppendLine("========================================================")
                         .AppendLine("                                                        ");

                await page.LogInfo(logMessage.ToString());


                while (!_reader.isComplete)
                {
                    _request.IsCancellationRequested();
                    await _request.IsPauseRequested(page.LogInfo);
                    
                    await Task.Delay(TimeSpan.FromSeconds(delay));

                    var data = _reader.GetNextTestStep();

                    // No previous test case
                    if (testCase == null || testCase.Name != data.TestCase.Name)
                    {
                        testCase = data.TestCase;
                        testCase.StartedDate = DateTime.Now;

                        // New Test Case Starts
                        logMessage.Clear()
                         .AppendLine("                                                        ")
                         .AppendLine("========================================================")
                         .AppendLine("          TEST EXECUTION LOG - TEST CASE START          ")
                         .AppendLine("========================================================")
                         .AppendLine($"TEST CASE:     {testCase.Name,-40}")
                         .AppendLine($"NUM OF STEPS:  {testCase.TestStepNum,-40}")
                         .AppendLine($"START DATE:    {testCase.StartedDate,-40}")
                         .AppendLine("--------------------------------------------------------")
                         .AppendLine($"EXECUTING...")
                         .AppendLine("========================================================")
                         .AppendLine("                                                        ");

                        await page.LogInfo(logMessage.ToString());
                    }

                    step = testCase.TestSteps[data.TestStepIndex];


                    try
                    {
                        await ExecuteTestStep(page, step);

                        // If Step Completes

                        // If last test step in test case
                        if (data.TestStepIndex + 1 >= testCase.TestSteps.Count)
                        {
                            testCase.Result = Result.Successful;
                            testCase.CompletedDate = DateTime.Now;
                            
                            logMessage.Clear()
                                .AppendLine("                                                        ")
                                .AppendLine("========================================================")
                                .AppendLine("         TEST EXECUTION LOG - TEST CASE COMPLETE        ")
                                .AppendLine("========================================================")
                                .AppendLine($"TEST CASE:     {testCase.Name,-40}")
                                .AppendLine($"NUM OF STEPS:  {testCase.TestStepNum,-40}")
                                .AppendLine($"START DATE:    {testCase.StartedDate,-40}")
                                .AppendLine($"END DATE:      {testCase.CompletedDate,-40}")
                                .AppendLine("--------------------------------------------------------")
                                .AppendLine($"RESULT:        {testCase.Result,-40}")
                                .AppendLine($"STEP FAILURES: {testCase.FailureCounter,-40}")
                                .AppendLine("========================================================")
                                .AppendLine("                                                        ");

                            await page.LogInfo(logMessage.ToString());

                        }
                    }
                    catch (OperationCanceledException)
                    {   // If Request Cancelled within Step
                        throw;
                    }
                    catch (Exception) // If Step Fails (Error logged in Test Step Logs)
                    {   

                        testCase.FailureCounter++;

                        // Either 5 failed steps, or over 33% failed steps -> TEST CASE FAILS
                        if (testCase.FailureCounter >= 5 ||
                            testCase.FailureCounter / (double)testCase.TestStepNum >= 0.33)
                        {
                            testCase.Result = Result.Failed;
                            testCase.CompletedDate = DateTime.Now;
                            testRun.FailureCounter++;

                            logMessage.Clear()
                                .AppendLine("                                                        ")
                                .AppendLine("========================================================")
                                .AppendLine("          TEST EXECUTION LOG - TEST CASE FAILURE        ")
                                .AppendLine("========================================================")
                                .AppendLine($"TEST CASE:     {testCase.Name,-40}")
                                .AppendLine($"NUM OF STEPS:  {testCase.TestStepNum,-40}")
                                .AppendLine($"START DATE:    {testCase.StartedDate,-40}")
                                .AppendLine($"END DATE:      {testCase.CompletedDate,-40}")
                                .AppendLine("--------------------------------------------------------")
                                .AppendLine($"RESULT:        {testCase.Result,-40}")
                                .AppendLine($"STEP FAILURES: {testCase.FailureCounter,-40}")
                                .AppendLine("========================================================")
                                .AppendLine("                                                        ");

                            await page.LogError(logMessage.ToString());
                        }

                        // Either 5 failed cases, or over 33% failed cases -> TEST RUN FAILS
                        if (testRun.FailureCounter >= 5 ||
                            testRun.FailureCounter / (double)testRun.TestCaseNum >= 0.33)
                        {
                            testRun.CompletedDate = DateTime.Now;
                            testRun.Result = Result.Failed;

                            logMessage.Clear()
                             .AppendLine("                                                        ")
                             .AppendLine("========================================================")
                             .AppendLine("         TEST EXECUTION LOG - TEST RUN FAILED           ")
                             .AppendLine("========================================================")
                             .AppendLine($"TEST RUN:      {testRun.Name,-40}")
                             .AppendLine($"NUM OF CASES:  {testRun.TestCaseNum,-40}")
                             .AppendLine($"START DATE:    {testRun.StartedDate,-40}")
                             .AppendLine($"END DATE:      {testRun.CompletedDate,-40}")
                             .AppendLine("--------------------------------------------------------")
                             .AppendLine($"RESULT:        {testRun.Result,-40}")
                             .AppendLine($"CASE FAILURES: {testRun.FailureCounter,-40}")
                             .AppendLine("========================================================")
                             .AppendLine("                                                        ");

                            await page.LogError(logMessage.ToString());

                            throw;
                        }
                    }
                }
            }
            catch (OperationCanceledException e) // Request Cancelled (cancelled Test Run)
            {

                testRun.CompletedDate = DateTime.Now;
                testRun.Result = Result.Uncomplete;

                logMessage.Clear()
                 .AppendLine("                                                        ")
                 .AppendLine("========================================================")
                 .AppendLine("        TEST EXECUTION LOG - TEST RUN CANCELLED         ")
                 .AppendLine("========================================================")
                 .AppendLine($"TEST RUN:      {testRun.Name,-40}")
                 .AppendLine($"NUM OF CASES:  {testRun.TestCaseNum,-40}")
                 .AppendLine($"START DATE:    {testRun.StartedDate,-40}")
                 .AppendLine($"END DATE:      {testRun.CompletedDate,-40}")
                 .AppendLine("--------------------------------------------------------")
                 .AppendLine($"RESULT:        {testRun.Result,-40}")
                 .AppendLine($"CASE FAILURES: {testRun.FailureCounter,-40}")
                 .AppendLine("========================================================")
                 .AppendLine("                                                        ");

                await page.LogError(logMessage.ToString());



                logMessage.Clear()
                          .AppendLine("                                                        ")
                          .AppendLine("========================================================")
                          .AppendLine("                REQUEST CANCELLED                       ")
                          .AppendLine("========================================================")
                          .AppendLine($"TEST EXECUTION CANCELLED => {e.Message, -40}           ")
                          .AppendLine("--------------------------------------------------------")
                          .AppendLine($"ID:               {_request.Id,-40}")
                          .AppendLine($"BROWSER TYPE:     {_request.BrowserType,-40}")
                          .AppendLine($"BROWSER VERSION:  {_request.BrowserVersion,-40}")
                          .AppendLine("--------------------------------------------------------")
                          .AppendLine($"ENVIRONMENT:      {_request.Environment,-40}")
                          .AppendLine($"DELAY:            {_request.Delay,-40}")
                          .AppendLine("========================================================");

                await page.LogError(logMessage.ToString());

                throw;
            }
            catch (Exception e) // Test Run Failed
            {
                logMessage.Clear()
                          .AppendLine("                                                        ")
                          .AppendLine("========================================================")
                          .AppendLine("                REQUEST FAILED                          ")
                          .AppendLine("========================================================")
                          .AppendLine($"TEST EXECUTION FAILED                                  ")
                          .AppendLine("--------------------------------------------------------")
                          .AppendLine($"ID:               {_request.Id,-40}")
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

                await page.LogError(logMessage.ToString());

                throw;
            }

            // Test Run Complete
            testRun.CompletedDate = DateTime.Now;
            testRun.Result = Result.Successful;

            logMessage.Clear()
             .AppendLine("                                                        ")
             .AppendLine("========================================================")
             .AppendLine("        TEST EXECUTION LOG - TEST RUN COMPLETE          ")
             .AppendLine("========================================================")
             .AppendLine($"TEST RUN:      {testRun.Name,-40}")
             .AppendLine($"NUM OF CASES:  {testRun.TestCaseNum,-40}")
             .AppendLine($"START DATE:    {testRun.StartedDate,-40}")
             .AppendLine($"END DATE:      {testRun.CompletedDate,-40}")
             .AppendLine("--------------------------------------------------------")
             .AppendLine($"RESULT:        {testRun.Result,-40}")
             .AppendLine($"CASE FAILURES: {testRun.FailureCounter,-40}")
             .AppendLine("========================================================")
             .AppendLine("                                                        ");

            await page.LogInfo(logMessage.ToString());

            logMessage.Clear()
                          .AppendLine("                                                        ")
                          .AppendLine("========================================================")
                          .AppendLine("                 REQUEST COMPLETE                       ")
                          .AppendLine("========================================================")
                          .AppendLine($"TEST EXECUTION COMPLETE                                ")
                          .AppendLine("--------------------------------------------------------")
                          .AppendLine($"ID:               {_request.Id,-40}")
                          .AppendLine($"BROWSER TYPE:     {_request.BrowserType,-40}")
                          .AppendLine($"BROWSER VERSION:  {_request.BrowserVersion,-40}")
                          .AppendLine("--------------------------------------------------------")
                          .AppendLine($"ENVIRONMENT:      {_request.Environment,-40}")
                          .AppendLine($"DELAY:            {_request.Delay,-40}")
                          .AppendLine("========================================================");

            await page.LogInfo(logMessage.ToString());

        }

        /// <summary>
        /// Executes a singular test step.
        /// This includes re-attempts;
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public async Task ExecuteTestStep(Page page, TestStep step)
        {
            _request.IsCancellationRequested();
            await _request.IsPauseRequested(page.LogInfo);

            // Sanitizing

            StringBuilder logMessage = new StringBuilder();

            string ActionOnObject = GetAlias(step.ActionOnObject.Replace(" ", "").ToLower());
            step.Object = InsertParams(step.Object);
            step.Value = InsertParams(step.Value);

            if (step.LocalAttempts <= 0 || step.LocalAttempts >= 11)
            {
                step.LocalAttempts = 3;
            }

            if (step.LocalTimeout <= 1 || step.LocalTimeout >= 500)
            {
                step.LocalTimeout = 30;
            }

            if (!new[] { 1, 2, 3, 4, 5 }.Contains(step.TestStepType))
            {
                step.TestStepType = 2;
            }

            if (step.Control.Equals("#"))
            {
                logMessage.Clear()
                        .AppendLine("                                                        ")
                        .AppendLine("========================================================")
                        .AppendLine("         TEST EXECUTION LOG - TEST STEP SKIPPED        ")
                        .AppendLine("========================================================")
                        .AppendLine($"TEST CASE:     {step.TestCaseName,-40}")
                        .AppendLine($"DESCRIPTION:   {step.TestDescription,-40}")
                        .AppendLine($"STEP NUM:      {step.StepNum,-40}")
                        .AppendLine("--------------------------------------------------------")
                        .AppendLine($"CONTROL:       {step.Control,-40}")
                        .AppendLine($"SKIPPED")
                        .AppendLine("========================================================");

                await page.LogInfo(logMessage.ToString());

                return;
            }

            step.StartedDate = DateTime.Now;
            logMessage.Clear()
                            .AppendLine("                                                        ")
                            .AppendLine("========================================================")
                            .AppendLine("          TEST EXECUTION LOG - TEST STEP START          ")
                            .AppendLine("========================================================")
                            .AppendLine($"TEST CASE:     {step.TestCaseName,-40}")
                            .AppendLine($"DESCRIPTION:   {step.TestDescription,-40}")
                            .AppendLine($"STEP NUM:      {step.StepNum,-40}")
                            .AppendLine($"ATTEMPTS:      {step.LocalAttempts,-40}")
                            .AppendLine($"START DATE:    {step.StartedDate,-40}")
                            .AppendLine("--------------------------------------------------------")
                            .AppendLine($"ACTION:        {ActionOnObject,-40}")
                            .AppendLine($"OBJECT:        {step.Object,-40}")
                            .AppendLine($"VALUE:         {step.Value,-40}")
                            .AppendLine($"COMMENT:       {step.Comments,-40}")
                            .AppendLine("--------------------------------------------------------")
                            .AppendLine($"EXECUTING...")
                            .AppendLine("========================================================");

            await page.LogInfo(logMessage.ToString());

            // Allows for re-attempts;
            while (true)
            {
                try
                {
                    _request.IsCancellationRequested();
                    await _request.IsPauseRequested(page.LogInfo);

                    if (actions.TryGetValue(ActionOnObject, out var action))
                    {
                        await action.ExecuteAsync(page, _request.Id, step, _envVars, _parameters);
                    }
                    else
                    {
                        step.LocalAttempts = 1;
                        throw new Exception($"Unknown Action provided: {ActionOnObject}");
                    }

                    // STEP Passes
                    step.CompletedDate = DateTime.Now;
                    step.Result = Result.Successful;

                    logMessage.Clear()
                            .AppendLine("                                                        ")
                            .AppendLine("========================================================")
                            .AppendLine("         TEST EXECUTION LOG - TEST STEP COMPLETE        ")
                            .AppendLine("========================================================")
                            .AppendLine($"STEP NUM:      {step.StepNum,-40}")
                            .AppendLine($"START DATE:    {step.StartedDate,-40}")
                            .AppendLine($"END DATE:      {step.CompletedDate,-40}")
                            .AppendLine($"ATTEMPTS USED: {step.FailureCounter + 1,-40}")
                            .AppendLine($"RESULT:        {step.Result,-40}")
                            .AppendLine("========================================================");


                    await page.LogInfo(logMessage.ToString());
                    break;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    step.FailureCounter++;

                    logMessage.Clear()
                            .AppendLine("                                                        ")
                            .AppendLine("--------------------------------------------------------")
                            .AppendLine("                       FAILURE                          ")
                            .AppendLine("--------------------------------------------------------")
                            .AppendLine($"ERROR:         {e.Message,-40}")
                            .AppendLine($"STACKTRACE:    {e.StackTrace,-40}")
                            .AppendLine($"ATTEMPT:       {step.FailureCounter + 1,-40}")
                            .AppendLine("--------------------------------------------------------");

                    if (step.FailureCounter >= step.LocalAttempts)
                    {   
                        step.CompletedDate = DateTime.Now;
                        step.Result = Result.Failed;

                        // No more re-attempts -> Failure
                        logMessage
                            .AppendLine("========================================================")
                            .AppendLine("          TEST EXECUTION LOG - TEST STEP FAILURE        ")
                            .AppendLine("========================================================")
                            .AppendLine($"TEST CASE:     {step.TestCaseName,-40}")
                            .AppendLine($"DESCRIPTION:   {step.TestDescription,-40}")
                            .AppendLine($"STEP NUM:      {step.StepNum,-40}")
                            .AppendLine($"START DATE:    {step.StartedDate,-40}")
                            .AppendLine($"END DATE:      {step.CompletedDate,-40}")
                            .AppendLine($"RESULT:        {step.Result,-40}")
                            .AppendLine("--------------------------------------------------------")
                            .AppendLine($"ACTION:        {ActionOnObject,-40}")
                            .AppendLine($"OBJECT:        {step.Object,-40}")
                            .AppendLine($"VALUE:         {step.Value,-40}")
                            .AppendLine($"COMMENT:       {step.Comments,-40}")
                            .AppendLine("========================================================");

                        await page.LogError(logMessage.ToString());

                        throw;
                    }
                    else // Re-attempt
                    {
                        logMessage
                                .AppendLine("--------------------------------------------------------")
                                .AppendLine("                       RE-ATTEMPTING                    ")
                                .AppendLine("--------------------------------------------------------")
                                .AppendLine($"WAITING 30 SECONDS FOR RE-ATTEMPT")
                                .AppendLine("--------------------------------------------------------");

                        await page.LogInfo(logMessage.ToString());
                        await Task.Delay(TimeSpan.FromSeconds(30));
                    }
                }
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
