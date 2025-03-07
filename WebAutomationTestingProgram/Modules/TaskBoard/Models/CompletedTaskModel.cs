﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebAutomationTestingProgram.Modules.TaskBoard.Models;

public class CompletedTaskModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("task")]
    [JsonPropertyName("task")]
    public string Task { get; set; }
    [Column("start_date")]
    [JsonPropertyName("start_date")]
    public string StartDate { get; set; }
    [Column("end_date")]
    [JsonPropertyName("end_date")]
    public string EndDate { get; set; }
    [Column("worker")]
    public string Worker { get; set; }
    [Column("description")]
    public string? Description { get; set; }
    [Column("priority")]
    public int Priority { get; set; }
}