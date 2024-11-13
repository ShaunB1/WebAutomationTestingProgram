using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions
{
    public class PageObject
    {
        /// <summary>
        /// PRIMARY PAGE:
        /// The currently 'active' page within a browser context for performing actions.
        /// </summary>
        public IPage CurrentPage { get; set; }

        /// <summary>
        /// SECONDARY PAGE:
        /// The list of all 'inactive' pages within a browser context, attached to a primary page.
        /// This list is ordered to keep track of page creation time.
        /// </summary>
        public List<IPage> Pages { get; set; }

        /// <summary>
        /// The url of the currently 'active' page (primary page).
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Local timeout (in seconds) for performing any action.
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Whether highlighting is turned on/off.
        /// </summary>
        private bool Highlight;

        /// <summary>
        /// The colour of highlighting.
        /// </summary>
        private string HighlightColour;

        /// <summary>
        /// List containing all the xpaths of iframes in the current page (deals with nested iframes)
        /// </summary>
        public LinkedList<string> IFrameXPath { get; set; }

        public PageObject(
            IPage currentPage,
            string url = "",
            int timeout = 30,
            bool highlight = false,
            string highlightColour = "red")
        {
            this.CurrentPage = currentPage;
            this.Url = url;
            this.Timeout = TimeSpan.FromSeconds(timeout);
            this.Highlight = highlight;
            this.HighlightColour = highlightColour;

            this.Pages = new List<IPage>();
            this.IFrameXPath = new LinkedList<string>();
        }
    }
}
