using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions
{
    public class PageManager
    {   



        /// <summary>
        /// The Browser Context that holds all the pages and executes the test.
        /// </summary>
        private IBrowserContext context;


        public PageManager(
            IBrowserContext context)
        {
            this.context = context;
        }
    }
}
