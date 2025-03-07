﻿using System.ComponentModel.DataAnnotations;

namespace WebAutomationTestingProgram.Core.Models.Requests
{
    /// <summary>
    /// Used by API Requests to create a CancellationRequest
    /// </summary>
    public class CancellationRequestModel
    {
        [Required(ErrorMessage = "The ID is required")]
        [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", ErrorMessage = "ID must be in correct format.")]
        public string ID { get; set; }
    }
}
