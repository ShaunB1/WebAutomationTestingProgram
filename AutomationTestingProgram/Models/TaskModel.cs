namespace AutomationTestingProgram.Models;

public class TaskModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsComplete { get; set; }
}