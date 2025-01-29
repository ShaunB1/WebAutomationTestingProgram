using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.IO;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using NPOI.SS.Formula.Functions;
using AutomationTestingProgram.Core;

namespace AutomationTestingProgram.Modules.TestRunnerModule;

/// <summary>
/// Reader class for Excel Files
/// </summary>
public class ExcelReader : IReader
{
    public bool isComplete { get; private set; }

    /// <summary>
    /// The run associated with this excel reader.
    /// </summary>
    public TestRun TestRun { get; }

    /// <summary>
    /// The current list of all loaded test cases.
    /// </summary>
    private IList<TestCase> _testCases;

    /// <summary>
    /// The current list of all loaded test steps.
    /// </summary>
    private IList<TestStep> _testSteps;

    /// <summary>
    /// The index of the current TestCase in the TestCases list
    /// </summary>
    private int _currentTestCase;

    /// <summary>
    /// The index of the current TestStep in the TestSteps list
    /// </summary>
    private int _currentTestStep;

    /// <summary>
    /// The current row in the Excel File
    /// </summary>
    private int _currentFileRow;

    /// <summary>
    /// Holds the last row in the Excel File.
    /// </summary>
    private int _lastRow;

    /// <summary>
    /// We currently only read 30 rows at a time. Should be settable in appsettings.
    /// </summary>
    private const int _numOfRows = 50;

    /// <summary>
    /// The path to the excel file
    /// </summary>
    private string _filePath;

    /* 
     * FOR NOW:
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
        isComplete = false;
        
        _filePath = FilePath;
        if (!File.Exists(FilePath))
        {
            throw new Exception("File Reader initialized failed. File path invalid");
        }

        TestRun = new TestRun(Path.GetFileNameWithoutExtension(_filePath));
        _testCases = new List<TestCase>();
        _testSteps = new List<TestStep>();
        _currentTestCase = 0;
        _currentTestStep = 0;

        _currentFileRow = 1; // skip first row

        using (var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
        {
            var workbook = new XSSFWorkbook(fs); // Only for .xlsx for now
            var sheet = workbook.GetSheetAt(0); // Gets the first sheet

            _lastRow = GetLastNonEmptyRowNumber(sheet);

            if (_lastRow == 0)
            {
                throw new Exception("File is empty");
            }

            for (int rowIndex = 1; rowIndex <= _lastRow; rowIndex++)
            {
                IRow row = sheet.GetRow(rowIndex);
                if (IsRowNullOrEmpty(row))
                    continue;

                string testCaseName = GetCellString(row.GetCell(1));
                if (_testCases.Count == 0 || _testCases.Last().Name != testCaseName)
                {
                    TestCase testCase = new TestCase(TestRun, testCaseName, rowIndex);
                    _testCases.Add(testCase);
                }

                _testCases.Last().TestStepNum++;
            }
        }

        TestRun.TestCaseNum = _testCases.Count;
        TestRun.StartedDate = DateTime.Now;
    }

    public TestCase GetCurrentTestCase()
    {
        return _testCases[_currentTestCase];
    }

    public async Task<TestStep> GetTestStepAsync()
    {
        if (isComplete)
            throw new Exception("No more steps to read");

        TestStep testStep;

        if (_currentTestStep < _testSteps.Count)
        {
            testStep = _testSteps[_currentTestStep++];
            if (testStep.TestCase.Name != _testCases[_currentTestCase].Name)
                _currentTestCase++;

            return testStep;
        }

        // Else, must fetch next batch
        _testSteps.Clear();
        _currentTestStep = 0;

        await IOManager.TryAquireSlotAsync();
        try
        {
            ReadNextChunck();
        }
        finally
        {
            IOManager.ReleaseSlot();
        }

        testStep = _testSteps[_currentTestStep++];
        if (testStep.TestCase.Name != _testCases[_currentTestCase].Name)
            _currentTestCase++;

        return testStep;

    }

    /// <summary>
    /// Reads the next 50 lines, populating all valid files, starting from _currentFileRow
    /// </summary>
    private void ReadNextChunck()
    {
        using (var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
        {
            var workbook = new XSSFWorkbook(fs); // Only for .xlsx for now
            var sheet = workbook.GetSheetAt(0); // Gets the first sheet

            int endRow = Math.Min(_currentFileRow + _numOfRows, _lastRow);

            int amount = endRow - _currentFileRow + 1;

            int TestCaseIndex = _currentTestCase;
            int stepNum = 0;

            while (amount > 0)
            {
                IRow row = sheet.GetRow(_currentFileRow++);
                if (IsRowNullOrEmpty(row))
                {
                    continue;
                };

                string TestCaseName = row.GetCell(1).ToString()!.Trim() ?? string.Empty;

                if (TestCaseName != _testCases[TestCaseIndex].Name)
                {
                    TestCaseIndex++;
                    stepNum = 0;
                }

                _testSteps.Add(new TestStep(
                    _testCases[TestCaseIndex],
                    GetCellString(row.GetCell(2)),   // testDescription
                    stepNum++,                       // stepNum
                    GetCellString(row.GetCell(4)),   // actionOnObject
                    GetCellString(row.GetCell(5)),   // objectName
                    GetCellString(row.GetCell(6)),   // value
                    GetCellString(row.GetCell(7)),   // comments
                    GetCellString(row.GetCell(8)),   // release
                    GetCellInt(row.GetCell(9)),      // localAttempts
                    GetCellInt(row.GetCell(10)),     // localTimeout
                    GetCellString(row.GetCell(11)),  // control
                    GetCellString(row.GetCell(12)),  // collection
                    GetCellString(row.GetCell(13)),  // testStepType
                    GetCellInt(row.GetCell(14))      // goToStep
                ));
                _testCases[_currentTestCase].TestSteps.Add(_testSteps.Last());
                amount--;
            }
        }

        if (_currentFileRow == _lastRow)
            isComplete = true;
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
