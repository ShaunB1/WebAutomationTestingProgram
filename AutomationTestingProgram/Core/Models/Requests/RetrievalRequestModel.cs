using System.ComponentModel.DataAnnotations;

namespace AutomationTestingProgram.Core
{   
    /// <summary>
    /// Used by API requests to create a RetrievalRequest
    /// </summary>
    public class RetrievalRequestModel
    {
        [Required(ErrorMessage = "The Type is required")]
        [AllowedFilterType()]
        public string FilterType;

        [Required(ErrorMessage = "The Value is required")]
        [AllowedFilterValue("FilterType")]
        public string FilterValue;
    }
}
