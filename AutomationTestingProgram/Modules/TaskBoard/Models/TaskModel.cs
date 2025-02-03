using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AutomationTestingProgram.Modules.DBConnector.Models;

public class TaskModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    public string Name { get; set; }
    [Column("draggable_id")]
    [JsonPropertyName("draggable_id")]
    public string DraggableId { get; set; }
    [Column("droppable_id")]
    [JsonPropertyName("droppable_id")]
    public string DroppableId { get; set; }
    [Column("start_date")]
    [JsonPropertyName("start_date")]
    public string StartDate { get; set; }
    [Column("description")]
    public string Description { get; set; }
    [Column("priority")]
    public int Priority { get; set; }
}