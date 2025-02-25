using System.ComponentModel.DataAnnotations;
using AutomationTestingProgram.Core.Models.Attributes;

namespace AutomationTestingProgram.Core.Models.Requests
{   
    /// <summary>
    /// Used by API requests to create a RetrievalRequest
    /// </summary>
    public class RetrievalRequestModel
    {
        [Required(ErrorMessage = "The Type is required")]
        [AllowedFilterType()]
        public string FilterType { get; set; }

        [AllowedFilterValue("FilterType")]
        public string FilterValue { get; set; }
    }
}
