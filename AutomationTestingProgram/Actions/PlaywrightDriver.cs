using Microsoft.Playwright;
using Microsoft.TeamFoundation.Framework.Common;

namespace AutomationTestingProgram.Actions
{
    /// <summary>
    /// The Driver class for Playwright. This is defined per Browser Context.
    /// </summary>
    public class PlaywrightDriver
    {

        public PlaywrightDriver(
            IBrowserContext context,
            string url = "",
            int timeout = 30,
            string loadingSpinner = "loadingspinner",
            string errorContainer = "",
            bool highlight = false,
            string highlightColour = "red")
        {
            /*this.context = context;
            this.url = url;
            this.timeout = TimeSpan.FromSeconds(timeout);
            this.loadingSpinner = loadingSpinner;
            this.errorContainer = errorContainer;
            this.highlight = highlight;
            this.highlightColour = highlightColour;*/
        }

        /// <summary>
        /// Gets or sets the loading spinner that appears on the website.
        /// </summary>
        public string loadingSpinner { get; set; }

        /// <summary>
        /// Gets or sets the error container to check if any errors have appeared on the UI.
        /// </summary>
        public string errorContainer { get; set; }

    }
}
