namespace AutomationTestingProgram.Models
{
    /// <summary>
    /// Used by API Requests to create a ProcessRequest
    /// </summary>
    public class ProcessRequestModel
    {
        public IFormFile File { get; set; }
        public string Type { get; set; }
        public string Version { get; set; }
        public string Environment { get; set; }
    }
}
