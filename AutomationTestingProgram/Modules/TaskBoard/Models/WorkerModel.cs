using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AutomationTestingProgram.Modules.DBConnector.Models;

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