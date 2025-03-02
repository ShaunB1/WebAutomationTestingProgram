using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebAutomationTestingProgram.Modules.TaskBoard.Models;

public class TaskPatchRequest
{
    [Key]
    [Column("draggable_id")]
    [JsonPropertyName("draggable_id")]
    public string DraggableId { get; set; }
    [Column("name")]
    public string Name { get; set; }
    [Column("description")]
    public string Description { get; set; }
    [Column("priority")]
    public int? Priority { get; set; }
}