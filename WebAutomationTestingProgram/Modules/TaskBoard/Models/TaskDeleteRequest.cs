using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebAutomationTestingProgram.Modules.TaskBoard.Models;

public class TaskDeleteRequest
{
    [Key]
    [Column("draggable_id")]
    [JsonPropertyName("draggable_id")]
    public string DraggableId { get; set; }
}