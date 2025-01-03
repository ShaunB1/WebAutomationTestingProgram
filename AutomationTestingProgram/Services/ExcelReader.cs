using ClosedXML.Excel;

public class ExcelReader
{
    public List<TestStepV1> ReadTestSteps(IFormFile file)
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
        
        var testSteps = new List<TestStepV1>();

        using (var stream = new MemoryStream())
        {
            file.CopyTo(stream);
            stream.Position = 0;
            
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet?.RangeUsed()?.RowsUsed().Skip(1);

            if (rows == null) return testSteps;
            
            testSteps.AddRange(rows.Select(row => new TestStepV1()
            {
                TestCaseName = row.Cell(1).TryGetValue(out string testCaseName) ? testCaseName.Trim() : string.Empty,
                TestDescription = row.Cell(2).TryGetValue(out string testDescription) ? testDescription.Trim() : string.Empty,
                StepNum = row.Cell(3).TryGetValue(out int stepNum) ? stepNum : 0,
                ActionOnObject = row.Cell(4).TryGetValue(out string actionOnObject) ? actionOnObject.Trim() : string.Empty,
                Object = row.Cell(5).TryGetValue(out string objectName) ? objectName.Trim() : string.Empty,
                Value = row.Cell(6).TryGetValue(out string value) ? value.Trim() : string.Empty,
                Comments = row.Cell(7).TryGetValue(out string comments) ? comments.Trim() : string.Empty,
                Release = row.Cell(8).TryGetValue(out string release) ? release.Trim() : string.Empty,
                LocalAttempts = row.Cell(9).TryGetValue(out int localAttempts) ? localAttempts : 0,
                LocalTimeout = row.Cell(10).TryGetValue(out int localTimeout) ? localTimeout : 0,
                Control = row.Cell(11).TryGetValue(out string control) ? control.Trim() : string.Empty,
                Collection = row.Cell(12).TryGetValue(out string collection) ? collection.Trim() : string.Empty,
                TestStepType = row.Cell(13).TryGetValue(out string testStepType) ? testStepType.Trim() : string.Empty,
                GoToStep = row.Cell(14).TryGetValue(out int goToStep) ? goToStep : 0,
                Cycle = row.Cell(15).TryGetValue(out string cycle) ? cycle.Trim() : string.Empty,
                Data = row.Cell(16).TryGetValue(out string data) ? data.Trim() : string.Empty,
                CycleData = row.Cell(17).TryGetValue(out string cycleData) ? cycleData.Trim() : string.Empty,
            }));
        }
        
        return testSteps;
    }
}
