using ClosedXML.Excel;

public class ExcelReader
{
    public List<TestStep> ReadTestSteps(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found");
        }
        
        var extension = Path.GetExtension(filePath);
        if (extension != ".xlsx" && extension != ".xls")
        {
            throw new ArgumentException("Invalid file extension. Only .xlsx and .xls are accepted.");
        }
        
        var testSteps = new List<TestStep>();
        using var workbook = new XLWorkbook(filePath);
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
        }));

        return testSteps;
    }
}
