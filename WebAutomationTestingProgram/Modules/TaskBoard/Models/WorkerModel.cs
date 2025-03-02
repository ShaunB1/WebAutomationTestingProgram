using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebAutomationTestingProgram.Modules.TaskBoard.Models;

public class WorkerModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    public string Name { get; set; }
    [Column("droppable_id")]
    [JsonPropertyName("droppable_id")]
    public string DroppableId { get; set; }
}