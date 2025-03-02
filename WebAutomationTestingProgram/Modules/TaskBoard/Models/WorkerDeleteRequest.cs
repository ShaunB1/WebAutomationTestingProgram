using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebAutomationTestingProgram.Modules.TaskBoard.Models;

public class WorkerDeleteRequest
{
    [Key]
    [Column("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }
}