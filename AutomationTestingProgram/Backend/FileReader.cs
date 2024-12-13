using AutomationTestingProgram.Models.Backend;
using AutomationTestingProgram.Services.Logging;
using ClosedXML.Excel;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System.Management.Automation;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace AutomationTestingProgram.Backend
{   

    /// <summary>
    /// Handles FileReading:
    /// -> Validating Files
    /// -> Creating a FileBreakpoint list that is used to split the file into chunck
    /// -> Retrieves chunks from a given file
    /// 
    /// Uses a queing mechanism to ensure no more than x (custom defined) amount of files are opened at a time.
    /// Queing mechasim similar to Browser Manager -> Queues multiple request for the same file. Only processes
    /// one at a time due to concurrency issues with NPOI
    /// 
    /// NOTE:
    ///     - NPOI is not inherently thread-safe, even for read operations
    ///     - Only one operation to a specific file will be performed at a time.
    /// </summary>
    public class FileReader
    {

        private static readonly SemaphoreSlim FileSemaphore = new SemaphoreSlim(15); // 15 files max at a time

        /// <summary>
        /// Validates the request: User permissions, file.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IList<FileBreakpoint>> ValidateRequestAsync(Request request)
        {
            ValidatePermission(request);
        }

        /// <summary>
        /// Prepares the request for processing by creating all needed data in DevOps
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task PrepareRequestAsync(Request request)
        {

        }

        public async Task<IList<TestStep>> GetNextStepsAsync(Request request)
        {

        }

        /// <summary>
        /// Verifies that the user has permission to perform the request. Throws an error otherwise
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private void ValidatePermission(Request request)
        {
            request.SetStatus(RequestState.Validating, "Validating User Permissions");
            
            // Throw an error if user does not have permission
            // ex: request.SetStatus(RequestState.ValidationFailure, "Invalid User Permissions");
            return;
        }

        /// <summary>
        /// Validates the given test file.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private async Task<IList<FileBreakpoint>> ValidateTestFileAsync(Request request)
        {

            request.SetStatus(RequestState.Validating, "Validating Test File");

            try
            {
                IFormFile file = request.File;
                if (file == null)
                {
                    throw new FileNotFoundException("Null/Invalid file provided!");
                }
                else if (file.Length == 0)
                {
                    throw new FileNotFoundException("Empty file provided!");
                }

                var extension = Path.GetExtension(file.FileName);
                switch (extension)
                {
                    case ".xlsx":
                        break;
                    case ".xlsm":
                        break;
                    case ".xls":
                        break;
                    case ".csv":
                        break;
                    case ".txt":
                        break;
                    case ".json":
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported file type.");

                }

                return new List<FileBreakpoint>();
            }
            catch (Exception e)
            {
                request.SetStatus(RequestState.ValidationFailure, "Test File Validation Failed", e);
                throw;
            }
        }

        /* SUMMARY:
         * 
         * There are a lot of comments here. When a comment is resolved, remove it,
         * Steps:
         * 1. Work on concurrent file system. Similar to Browser Manager
         * 2. Work on validate file -> Use normal validation.
         *          -> Create breakpoints as per cycles (logic below)
         *          -> Create temporary Test Case objects (logic below)
         * 3. Validation file
         *      -> If fail: throw error, remove all temporary objects
         *      -> If pass, create test plan, suite, cases using test case and set objects.
         *         then return list of breakpoints as well as first chunck of steps.
         * 4. Retrieve from file:
         *      -> Retrieves a chunck of steps. Test Set (thread safe) must know what chunck to grab next
         * 
         */














        /* FILE LIFECYCLE
         * 
         * 1. Request is received -> Creates a new context in a browser
         * 2. Context -> Validate file
         *       While validating, create a list of test cases. In case of cycles, create multiple instances of
         *       test cases (ex: Test Case A 1, Test Case B 1 followed by A 2, B 2, followed by etc.
         *       If validation fails -> Throw error, and empty list (not needed)
         *       If validation succeeds -> do step 3.
         * 3. First, create a new test plan in DevOps called 'Temp #' (something to always be unique)
         *    Then, create a test suite under test plan
         *    Inside test suite, create all test cases. All test cases above contain a # detailing # of steps per case.
         *    Finally, return a list of breakpoints, which is used to get a chunck of test steps from the file at a time.
         *    This is used to limit memory usage.
         * 4. Perform operations. Whenever a step finishes, report to the test case. Concurrency shouldn't be an issue
         *    as only one thread reports to a specific case at a time (though will have to have a queue mechanism as
         *    you cannot edit the same test plan concurrently)
         *    In case of parallels (cycles), each parallel reports to their own iteration test cases. Ex: A1 B1 
         *    is one thread, while A2 B2 is another. They do not conflict.
         *    
         * This way, there is no dynamic creation of test cases, and they are only created in DevOps is file validation passes.
         * 
         * How will this work in parralel test runs?
         * Each context (file) gets its own test plan, test suite, etc.
         * Each context will have to manage concurrency wihtin their own reporting (simple lock)
         * Once a context completes, the test set suite will be moved under a 'specific' test plan.
         * If the test set already exists, we will overwrite its test cases with these new ones, but leave the
         * history to see all test case runs.
         * Semaphores will be used to make sure the move can only happen one at a time to this plan.
         * 
         * The idea is to have all runs under one test plan. But if we report there from the getgo,
         * it will be much slow reporting (in case of a lot of concurrency with multiple files).
         * 
         * 
         */




        /*public static async Task ExecutePage(Page pageObject)
        {
            CustomLoggerProvider provider = new CustomLoggerProvider(pageObject.FolderPath);
            ILogger<FileReader> PageLogger = provider.CreateLogger<FileReader>()!;

            IPage page = pageObject.Instance!;

            PageLogger.LogInformation("Starting");
            await page.GotoAsync("https://www.google.com");
            await Task.Delay(10000);
            PageLogger.LogInformation("Google complete");
            await page.GotoAsync("https://example.com");
            await Task.Delay(10000);
            PageLogger.LogInformation("Example complete");
            await page.GotoAsync("https://www.bing.com");
            await Task.Delay(10000);
            PageLogger.LogInformation("Bing complete");
            await page.GotoAsync("https://www.yahoo.com");
            await Task.Delay(10000);
            PageLogger.LogInformation("Yahoo complete");
            await page.GotoAsync("https://www.wikipedia.org");
            await Task.Delay(10000);
            PageLogger.LogInformation("Wikipedia complete");
            await page.GotoAsync("https://www.reddit.com");
            await Task.Delay(10000);
            PageLogger.LogInformation("Reddit complete");
            await page.GotoAsync("https://www.microsoft.com");
            await Task.Delay(10000);
            PageLogger.LogInformation("Microsoft complete");
            await page.GotoAsync("https://www.apple.com");
            await Task.Delay(10000);
            PageLogger.LogInformation("Apple complete");
            await page.GotoAsync("https://www.amazon.com");
            await Task.Delay(10000);
            PageLogger.LogInformation("Amazon complete");
            await page.GotoAsync("https://www.netflix.com");
            await Task.Delay(10000);
            PageLogger.LogInformation("Ne1. tflix complete");
        }*/

        public async Task<IList<FileBreakpoint>> ValidateRequestAsync(Request request)
        {
            /* TODO:
             * 1. Verify current paralellization:
             *      TestController -> BrowserManager -> Browser -> ContextManager -> Context
             *    for both logic and logs
             * 2. At the context level, create the request verification.
             *    Boilderplate function for permissions
             *    Verify file function for files -> Use VerifyExcelFile for proper code
             * 3. If an error found, it is thrown. If too many errors found, the first few are outputted, with
             *    a msg 'Please look at error file for more errors', and we output a separate file that includes
             *    all the errors found
             *    ** Take into account multiple sheets. If multiple sheets, log a warning, and use first!!
             * 4. Create a breakpoint list. All cycles start/ends should have a breakpoint. If too many actions
             *    occur between two breakpoints after the initial search, add breakpoints in between. 
             *    Max of 50 actions between breakpoints. If 51+, add a new breakpoint in the middle.
             *    Should be recursive. For ex: If you have 400 actions between two breakpoints, you add a new one
             *    in the middle, but now its 200 actions between breakpoints, add again, etc.
             * 5. Using breakpoint list, have the context read all actions until a breakpoint, and give assign them
             *    to a page. When page finishes and reports back, we know what breakpoint they ended at, whether its
             *    a cycle and/or parallel. If not a cycle or parallel, just get the next actions, and assign them to the
             *    page again. If a cycle, get the next set of actions from the start of the cycle.
             *    If a parallel, same as cycle, but deal with multiple pages request this at once (Interlocked increment
             *    for cycle iteration index)
             * 6. All pages should have an active weight as described below. This weight ensures whether the test succeeded
             *    or failed. 
             *    If more paralells created -> try to create from parallel with least weight.
             *    If fewer -> try to keep parallels with less weight
             *    ** Weight are copied into parallels (everything, whole state, pages, etc.)
             * 7. As pages run tasks, we need to finish the driver. The pages would then use the driver to run tasks.
             * 8. All the important features would be complete at this point. This is just testing and making sure everything works.
             *    Then can push, and work on below
             *    
             *    
             * 9. WebSocket connections! As all the test work, update the UI to dynamically create more logging windows,
             *    and create websocket connections to report to. As we update the file (or rather, add to the string
             *    for real time logging), we also log using the websocket connection to a specific screen area.
             * 10. In the UI, there should be a list of all test executions. Logs context and page level are shown.
             *     When you click an item on the list, it opens a page that shows the cmd for context level, and
             *     allows you to select from a ddl what to view in the second cmd for page level.
             *     By default, we select the context (only one) and the first page.
             * 11. Another list, with a ddl to select between the two, for browser and run level??
             * 
             * Cycles use iterations
 
So when we report, we report TestCase A1, then A2, then A3
 
And each parallel/cycle has their own iteration #, and reports there
 
As if you run it 3 times concurrently, it would report to A1, A2 and A3 all at the same time
 
Only thing that we'd need to make sure of is if a cycle uses multiple test cases, when reporting: It should report like this
 
A1
B1
A2
B2
A3
B3
 
instead of what will happen if we dont handle it:
 
A1
A2
A3
B1
B2
B3
 
             * 
             * 
             * 
             */


            /* 
             * Only Test Step Types 1-5 will be implemented:
             * 
             * System will use a weight system for failures to end a test.
             * If fail -> more weight, closer to failure.
             * If success -> less weight, further from failure
             * Weight added/removed is based on type.
             * Parrales created will use same weight as original page.
             * If multiple pages exist, and more added (parralellization to parralellization), 
             * we copy from the page with the smallest weight.
             * 
             * If too many steps in a row (or a short duration fail), whole test fails
             * 1 (Mandatory)    -> If failure, end immediatelly.
             *                  -> 100 % weight (if failed, end immediately)
             * 2 (Default)      -> If failure, log as failure and continue
             *                  -> 20% weight. If 5 fail in a row for example, end test
             * 3 (Optional)     -> If failure, log as pass and continue (but still marked that it failed in the pass: error msg)
             *                  -> 5% weight. If 20 fail in a row for example, end test
             * 4 (Conditional)  -> Should be 4, x, y, z*
             *                  -> If failure, go to x
             *                  -> If success, go to y
             *                  -> z is optional parameter => # of loops. Default: 5
             *                  -> Weight: 0% while on first z loops. 
             *                  -> Weight: 20% after first z loops
             *   Ex: If 4, 1, 2, 2 provided, and we fail in the first pass
             *       we go back to step 1, and continue. If we fail again in this step
             *       we go back again to step 1. BEcause z is 2, this failure doesnt add weight
             *       If we fail again, because its the 3rd time and 3 > 2, 20% weight added
             * 5 (Inverted)     -> If failure, its considered a pass
             *                  -> If pass, its consiered a failure
             *                  -> 20% weight (same as type 2, but pass <-> failure)
             *  
             *  
             */


            // Will validate file. Either comples or throws an error

            /*
             * 5 steps to validate. 
             * ** NOTE: We validate whole file, sending error list of all errors at once! Not only first error found!!
             * 
             * 1. Does user have permission to perform request?
             *  -> Requests will include environment to run on. Must make sure environment select is correct (pen testing)
             *  -> User must have permission to use all users included within the test
             *     - For login, which automatically gets the password and login, must check if user can use given email
             *     - Email allowed: @ontarioemail based on permissions, @ontario only for self
             *     - Can bypass by sending a hardcoded password
             * 2. Are all cycles correct?
             *  -> Verify that all cycles start at the beginning of a test case. If not, throw error
             *  -> Verify that all cycles end at the end of a test case. If no, throw error
             *  -> Verify that all innercycles end within their outercycle. (Use a stack)
             *      Ex: Correct         Incorrect
             *          Start1          Start1
             *          Start2          Start2
             *          Start3          Start3
             *          End3            End1
             *          End2            End3
             *          End1            End2
             *  -> Verify that all cycles have an appropriate table.
             *  -> Verify that all columns in table exist for a given cycle. By this: If a cycle has two populatetextbox,
             *     it should have two columns with both. This includes all actions that populate/write anything.
             *     We will need to make a list
             *  -> Verify that for any cycle with an innercycle, it does not include actions (columns) for anything inside
             *     the innercycle. The innercycle table will include these columns
             *  -> If logins within a cycle, make sure that when restarting the cycle, you are with the same user account as when you started
             *     the first iteration. If not, throw a error (or should it be warning??):
             *     Ex: Correct                          Incorrect
             *     Start 1    Login                                 Login
             *                Populate                  Start 1     Populate
             *                Login                                 Login
             *                Populate                              Populate
             *                Click                                 Click
             *     End 1      Click                     End 1       Click
             *     
             *  -> Based on all actions in the list made for columns in the cycle tables, all cycles must include
             *     at least one of these actions. If not, then it will be an empty table.
             *     If an empty table exists, must check that a manual input for # of cycle is given. Else, give a warning
             *     Ex:  Correct                 Incorrect           Correct (as an example)
             *     Start 1  Click               Start 1 Click       Start 1 (5)     Click 
             *              Click                       Click                       Click
             *              Populate                    Click                       Click
             *     End 1    Click               End 1   Click       End 1           Click   
             *     
             * 3. Are all rows correct?
             *  -> Check that all rows are correct, one cell at a time.
             *      ** Every column that we remove must still exist here for backwards compatability **
             *      -> Throw a warning if a depreciated column exists, but still process
             *      - TestCaseName: Exists (not empty): Throw error if failed validation
             *      - TestDescription: Exists (not empty): Throw warning if failed validation
             *      - StepNum: we remove in future? Only issue is test step type 4
             *      - ActionOnObject: a valid actionOnObject provided: Throw error if failed validation
             *      - Comments: depends on actionOnObject selected: Throw error if failed validation
             *          -> As any locator type can be included (even non-default ones), if an action accepts locators,
             *             we just make sure whatever is provided is not-empty and a string
             *      - Value: based on ActionOnObject + Comments
             *          -> If an action that uses locator, we look at comments for specific type.
             *             Ex: XPath -> should start with //
             *                 Html Id -> a string (not empty)
             *                 etc.
             *             If a non-default locator provided, like 'code' or 'role', just make sure a string is provided.
             *             Throw a warning if not??
             *          -> Throws errors or warnings depends on what we find 'broken'
             *      - Release: we remove in future?
             *      - Local Attempts: A valid int >= 1 and smaller than 10 (some limit of # of attempts is needed to not continously run something)
             *      - Local Timeout: A valid int >= 1 and smaller than 500 (some limit is needed)
             *      - Test Step Type: A valid type (1-4) (or is it 1-6? are we adding more types?)
             *      - GoToStep: removed. Combining into test step type. 
             *      ** Cycles have their own column, and tested in 2. **
             *      ** Should be able process no matter the order of columns. We just start from column A to the rightmost
             *         non-empty column. If an empty column in the middle, throw an error? **
             *  
             * 
             */

            // Validating request
            await ValidatePermission(request);
            // Validating request file
            // Breakpoint list is ordered. If empty, no breakpoints (no cycles)
            IList<FileBreakpoint> breakpoints = await ValidateTestFileAsync(request);

            return breakpoints;
        }

        /*private async Task ValidatePermission(Request request)
        {

            request.SetStatus(RequestResult.Validating, "Validating request");

            *//*
             * For sql scripts, check for concurrency issues. 
             * Running requests on the same DB with same users can be an issue??
             * 
             * Add a test step 5 -> expect to fail for a step
             * All other steps except 4 are automatically expect to pass
             * 
             * 
             *//*
        }*/

        

        private IList<FileBreakpoint> ReadExcelFile(string filePath)
        {
            IWorkbook workbook;
            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                string fileExtension = Path.GetExtension(filePath).ToLower();

                if (fileExtension == ".xls")
                {
                    workbook = new HSSFWorkbook(file);
                }
                else if (fileExtension == ".xlsx" || fileExtension == ".xlsm")
                {
                    workbook = new XSSFWorkbook(file);
                }
                else
                {
                    throw new InvalidOperationException("Unsupported file format. Please provide an .xls, .xlsx, or .xlsm file.");
                }

                // Maybe logic to read from multiple sheets in the future?
                ISheet sheet = workbook.GetSheetAt(0);

                int startRow = -1;

                for (int i = 0; i < Math.Min(10, sheet.PhysicalNumberOfRows); i++)
                {
                    if (sheet.GetRow(i) != null && !IsRowEmpty(sheet.GetRow(i)))
                    {
                        startRow = i;
                        break;
                    }
                }

                if (startRow == -1)
                {
                    throw new InvalidOperationException("First non-empty row not found within the first 10 rows.");
                }

                for (int rowIndex = startRow; rowIndex < sheet.PhysicalNumberOfRows; rowIndex++)
                {
                    IRow row = sheet.GetRow(rowIndex);
                    if (row == null || IsRowEmpty(row))
                    {
                        break;
                    }

                    // Non-empty row -> Process data
                    foreach (ICell cell in row.Cells)
                    {

                        DOSOMETHING();
                    }
                }
            }
        }

        private bool IsRowEmpty(IRow row)
        {
            foreach (ICell cell in row.Cells)
            {
                if (cell != null && !string.IsNullOrEmpty(cell.ToString()))
                {
                    return false;
                }
            }
            return true;
        }

        private void ReadCsvFile(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                string line;
                bool foundNonEmptyRow = false; // Used to determine when processing starts
                int curRow = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    curRow += 1;
                    if (string.IsNullOrWhiteSpace(line))
                    {   
                        // If we previousouly found a non-empty row, and now an empty row -> break
                        if (foundNonEmptyRow)
                        {
                            break;
                        }
                        else if (curRow > 10)
                        {
                            throw new InvalidOperationException("First non-empty row not found within the first 10 rows.");
                        }
                    }
                    else
                    {
                        foundNonEmptyRow = true;
                        DOSOMETHING();
                    }
                }
            }
        }


    }
}
