using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebAutomationTestingProgram.Modules.TaskBoard.Models;

public class TaskPutRequest
{
    [Key]
    [Column("draggable_id")]
    [JsonPropertyName("draggable_id")]
    public string DraggableId { get; set; }
    [Column("destination_droppable_id")]
    [JsonPropertyName("destination_droppable_id")]
    public string DestinationDroppableId { get; set; }
    [Column("start_date")]
    [JsonPropertyName("start_date")]
    public string StartDate { get; set; }
}