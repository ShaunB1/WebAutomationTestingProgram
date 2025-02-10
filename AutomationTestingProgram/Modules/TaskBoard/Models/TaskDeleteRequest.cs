﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AutomationTestingProgram.Modules.DBConnector.Models;

public class TaskDeleteRequest
{
    [Key]
    [Column("draggable_id")]
    [JsonPropertyName("draggable_id")]
    public string DraggableId { get; set; }
}