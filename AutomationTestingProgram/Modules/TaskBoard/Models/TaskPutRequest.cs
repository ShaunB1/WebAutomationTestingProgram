namespace AutomationTestingProgram.Modules.DBConnector.Models;

public class TaskPutRequest
{
    public string DraggableId { get; set; }
    public string DestinationDroppableId { get; set; }
    public string StartDate { get; set; }
}