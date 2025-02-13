using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.IO;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using NPOI.SS.Formula.Functions;
using AutomationTestingProgram.Core;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;

namespace AutomationTestingProgram.Modules.TestRunnerModule;

/// <summary>
/// Reader class for Excel Files
/// </summary>
public class ExcelReader : IReader
{
    public bool isComplete { get; private set; } = false;

    /// <summary>
    /// The run associated with this excel reader.
    /// </summary>
    public TestRunObject TestRun { get; }

    /// <summary>
    /// The index of the current TestCase in the TestCases list
    /// </summary>
    private int _currentTestCase = 0;

    /// <summary>
    /// The index of the current TestStep in the TestSteps list
    /// </summary>
    private int _currentTestStep = 0;

    /// <summary>
    /// The path to the excel file
    /// </summary>
    private string _filePath;

    /* 
     * FOR NOW:
     * 
     * - Everything is saved in memory
     *      -> While not very memory efficient, the data shouldn't take up too much memory.
     *      Can work on adding a more memory efficient reader in the future.
     * 
     * 
     * - Cycles are ignored
     * - GoToStep is ignored
     * 
     * Requires refactoring to implement.
     * Do after DevOpsReporting 
     * 
     */

    /// <summary>
    /// Initializes an instance of the reader class.
    /// </summary>
    public ExcelReader(string FilePath)
    {
        
        _filePath = FilePath;
        if (!File.Exists(FilePath))
        {
            throw new Exception("File Reader initialized failed. File path invalid");
        }

        TestRun = new TestRunObject(Path.GetFileNameWithoutExtension(_filePath));

        using (var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
        {
            var workbook = new XSSFWorkbook(fs); // Only for .xlsx for now
            var sheet = workbook.GetSheetAt(0); // Gets the first sheet

            int lastRow = GetLastNonEmptyRowNumber(sheet);

            if (lastRow == 0)
            {
                throw new Exception("File is empty");
            }

            IList<TestCaseObject> testCases = TestRun.TestCases;
            TestCaseObject? currentTestCase = null;
            int stepNum = 1;

            for (int rowIndex = 1; rowIndex <= lastRow; rowIndex++)
            {
                IRow row = sheet.GetRow(rowIndex);
                if (IsRowNullOrEmpty(row))
                    continue;

                string testCaseName = GetCellString(row.GetCell(0));
                if (string.IsNullOrEmpty(testCaseName) && currentTestCase != null)
                    testCaseName = currentTestCase.Name;

                if (currentTestCase == null || currentTestCase.Name != testCaseName)
                {
                    currentTestCase = new TestCaseObject(testCaseName);
                    testCases.Add(currentTestCase);
                    stepNum = 1;
                }

                currentTestCase.TestSteps.Add(new TestStepObject(
                    currentTestCase.Name,            // testCaseName
                    GetCellString(row.GetCell(1)),   // testDescription
                    stepNum++,                       // stepNum
                    GetCellString(row.GetCell(3)),   // actionOnObject
                    GetCellString(row.GetCell(4)),   // objectName
                    GetCellString(row.GetCell(5)),   // value
                    GetCellString(row.GetCell(6)),   // comments
                    GetCellString(row.GetCell(7)),   // release
                    GetCellInt(row.GetCell(8)),      // localAttempts
                    GetCellInt(row.GetCell(9)),     // localTimeout
                    GetCellString(row.GetCell(10)),  // control
                    GetCellString(row.GetCell(11)),  // collection
                    GetCellInt(row.GetCell(12)),  // testStepType
                    GetCellString(row.GetCell(13))   // goToStep
                ));
            }
        }
    }

    public (TestCaseObject TestCase, int TestStepIndex) GetNextTestStep()
    {
        if (isComplete)
            throw new Exception("No more steps to read");


        // If no more steps in test case, move on to next test case
        if (_currentTestStep >= TestRun.TestCases[_currentTestCase].TestSteps.Count)
        {
            _currentTestCase++;
            _currentTestStep = 0;
        }

        // Test Case failed early, move on to next test case
        if (TestRun.TestCases[_currentTestCase].Result == Result.Failed)
        {
            _currentTestCase++;
            _currentTestStep = 0;
        }

        // If last _testCase && last step, set isComplete to true
        if (_currentTestCase + 1 >= TestRun.TestCases.Count && _currentTestStep + 1 >= TestRun.TestCases[_currentTestCase].TestSteps.Count)
        {
            isComplete = true;
        }

        return (TestRun.TestCases[_currentTestCase], _currentTestStep++);
    }

    private int GetCellInt(ICell cell)
    {
        if (cell == null) return 0;

        if (cell.CellType == CellType.Numeric)
        {
            return (int)cell.NumericCellValue;
        }
        else if (cell.CellType == CellType.String && int.TryParse(cell.StringCellValue.Trim(), out int result))
        {
            return result;
        }

        return 0;
    }

    private string GetCellString(ICell cell)
    {
        if (cell == null) return string.Empty;

        return cell.ToString()?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Gets the last non-null or empty row in the sheet.
    /// </summary>
    /// <param name="sheet">The sheet to find this row in.</param>
    /// <returns>The index of the found row.</returns>
    private int GetLastNonEmptyRowNumber(ISheet sheet)
    {
        int lastRowNum = sheet.LastRowNum;
        int lastNonEmptyRowNum = -1;

        for (int i = lastRowNum; i >= 0; i--)
        {
            IRow row = sheet.GetRow(i);
            if (!this.IsRowNullOrEmpty(row))
            {
                lastNonEmptyRowNum = i;
                break;
            }
        }

        return lastNonEmptyRowNum + 1;
    }

    /// <summary>
    /// Check if an IRow is empty.
    /// </summary>
    /// <param name="row">The IRow of an ISheet</param>
    private bool IsRowNullOrEmpty(IRow row)
    {
        if (row == null)
            return true;

        foreach (ICell cell in row)
        {
            if (cell.CellType != CellType.Blank)
                return false;
        }

        return true;
    }
}
