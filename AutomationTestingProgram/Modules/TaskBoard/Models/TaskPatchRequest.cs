namespace AutomationTestingProgram.Modules.DBConnector.Models;

public class TaskPatchRequest
{
    public string DraggableId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int? Priority { get; set; }
}