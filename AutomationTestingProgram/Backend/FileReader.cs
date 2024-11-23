using AutomationTestingProgram.Models.Backend;
using AutomationTestingProgram.Services.Logging;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Backend
{
    public class FileReader
    {   
        public static async Task ExecutePage(Page pageObject)
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
            PageLogger.LogInformation("Netflix complete");
        }

        public void ValidateTestFile(IFormFile file)
        {
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
                default:
                    throw new InvalidOperationException("Unsupported file type.");

            }
        }

        private string SaveFile(IFormFile file)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, file.FileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return filePath;
        }

        /*private List<TestStep> ValidateExcel(IFormFile file)
        {
            var testSteps = new List<TestStep>();

            string filePath = SaveFile(file);

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var workbook = new XSSFWorkbook(fs))  // NPOI for .xlsx, use HSSFWorkbook for .xls
            {
                var sheet = workbook.GetSheetAt(0);  // Get first sheet
                var rowEnumerator = sheet.GetRowEnumerator();

                bool headerProcessed = false;
                while (rowEnumerator.MoveNext())
                {
                    var row = (IRow)rowEnumerator.Current;

                    if (!headerProcessed)
                    {
                        // Validate headers
                        ValidateExcelHeaders(row);
                        headerProcessed = true;
                        continue;
                    }

                    // Process the row
                    var testStep = new TestStep
                    {
                        TestCaseName = GetCellValue(row, 0),
                        TestDescription = GetCellValue(row, 1),
                        StepNum = GetCellValue(row, 2).ToInt(),
                        ActionOnObject = GetCellValue(row, 3),
                        Object = GetCellValue(row, 4),
                        Value = GetCellValue(row, 5),
                        Comments = GetCellValue(row, 6),
                        Release = GetCellValue(row, 7),
                        LocalAttempts = GetCellValue(row, 8).ToInt(),
                        LocalTimeout = GetCellValue(row, 9).ToInt(),
                        Control = GetCellValue(row, 10),
                        Collection = GetCellValue(row, 11),
                        TestStepType = GetCellValue(row, 12),
                        GoToStep = GetCellValue(row, 13).ToInt(),
                        Data = GetCellValue(row, 14),
                    };

                    testSteps.Add(testStep);
                }
            }
            return testSteps;
        }

        private void ValidateExcelHeaders(IRow row)
        {
            // Ensure the required columns exist
            var requiredHeaders = new[] { "TestCaseName", "TestDescription", "StepNum", "ActionOnObject", "Object", "Value", "Comments", "Release", "LocalAttempts", "LocalTimeout", "Control", "Collection", "TestStepType", "GoToStep", "Data" };

            for (int i = 0; i < requiredHeaders.Length; i++)
            {
                if (GetCellValue(row, i) != requiredHeaders[i])
                {
                    throw new InvalidOperationException($"Invalid header at column {i + 1}: Expected '{requiredHeaders[i]}' but found '{GetCellValue(row, i)}'");
                }
            }
        }

        private string GetCellValue(IRow row, int columnIndex)
        {
            var cell = row.GetCell(columnIndex);
            return cell != null ? cell.ToString().Trim() : string.Empty;
        }

        private List<TestStep> ValidateCSV(IFormFile file)
        {
            var testSteps = new List<TestStep>();
            string filePath = SaveFile(file);

            foreach (var line in File.ReadLines(filePath))
            {
                var columns = line.Split(',');

                if (columns.Length < 15)
                {
                    throw new InvalidOperationException("Invalid row in CSV: Not enough columns.");
                }

                var testStep = new TestStep
                {
                    TestCaseName = columns[0],
                    TestDescription = columns[1],
                    StepNum = columns[2].ToInt(),
                    ActionOnObject = columns[3],
                    Object = columns[4],
                    Value = columns[5],
                    Comments = columns[6],
                    Release = columns[7],
                    LocalAttempts = columns[8].ToInt(),
                    LocalTimeout = columns[9].ToInt(),
                    Control = columns[10],
                    Collection = columns[11],
                    TestStepType = columns[12],
                    GoToStep = columns[13].ToInt(),
                    Data = columns[14]
                };

                testSteps.Add(testStep);
            }

            return testSteps;
        }

        private List<TestStep> ValidateTXT(IFormFile file)
        {
            var testSteps = new List<TestStep>();
            string filePath = SaveFile(file);

            foreach (var line in File.ReadLines(filePath))
            {
                // Assuming you need to split by some delimiter or have specific format to validate
                var columns = line.Split('\t');  // Or use another delimiter depending on the file format

                if (columns.Length < 15)
                {
                    throw new InvalidOperationException("Invalid row in TXT: Not enough columns.");
                }

                var testStep = new TestStep
                {
                    TestCaseName = columns[0],
                    TestDescription = columns[1],
                    StepNum = columns[2].ToInt(),
                    ActionOnObject = columns[3],
                    Object = columns[4],
                    Value = columns[5],
                    Comments = columns[6],
                    Release = columns[7],
                    LocalAttempts = columns[8].ToInt(),
                    LocalTimeout = columns[9].ToInt(),
                    Control = columns[10],
                    Collection = columns[11],
                    TestStepType = columns[12],
                    GoToStep = columns[13].ToInt(),
                    Data = columns[14]
                };

                testSteps.Add(testStep);
            }

            return testSteps;
        }



        private void ValidateExcel(IFormFile file, bool xls = false)
        {

        }

        private void ValidateCSV(IFormFile file)
        {

        }

        private void ValidateTXT(IFormFile file)
        {

        }

        public List<TestStep> ReadTestSteps(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new FileNotFoundException("File not found");
            }

            var extension = Path.GetExtension(file.FileName);
            if (extension != ".xlsx" && extension != ".xls")
            {
                throw new ArgumentException("Invalid file extension. Only .xlsx and .xls are accepted.");
            }

            var testSteps = new List<TestStep>();

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                stream.Position = 0;

                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet?.RangeUsed()?.RowsUsed().Skip(1);

                if (rows == null) return testSteps;

                testSteps.AddRange(rows.Select(row => new TestStep()
                {
                    TestCaseName = row.Cell(1).TryGetValue(out string testCaseName) ? testCaseName : string.Empty,
                    TestDescription = row.Cell(2).TryGetValue(out string testDescription) ? testDescription : string.Empty,
                    StepNum = row.Cell(3).TryGetValue(out int stepNum) ? stepNum : 0,
                    ActionOnObject = row.Cell(4).TryGetValue(out string actionOnObject) ? actionOnObject : string.Empty,
                    Object = row.Cell(5).TryGetValue(out string objectName) ? objectName : string.Empty,
                    Value = row.Cell(6).TryGetValue(out string value) ? value : string.Empty,
                    Comments = row.Cell(7).TryGetValue(out string comments) ? comments : string.Empty,
                    Release = row.Cell(8).TryGetValue(out string release) ? release : string.Empty,
                    LocalAttempts = row.Cell(9).TryGetValue(out int localAttempts) ? localAttempts : 0,
                    LocalTimeout = row.Cell(10).TryGetValue(out int localTimeout) ? localTimeout : 0,
                    Control = row.Cell(11).TryGetValue(out string control) ? control : string.Empty,
                    Collection = row.Cell(12).TryGetValue(out string collection) ? collection : string.Empty,
                    TestStepType = row.Cell(13).TryGetValue(out string testStepType) ? testStepType : string.Empty,
                    GoToStep = row.Cell(14).TryGetValue(out int goToStep) ? goToStep : 0,
                    Data = row.Cell(15).TryGetValue(out string data) ? data : string.Empty,
                }));
            }

            return testSteps;
        }*/
    }
}
