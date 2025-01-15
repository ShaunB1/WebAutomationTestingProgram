/*using AutomationTestingProgram.Modules.TestRunnerModule.Backend.Playwright.Managers;
using AutomationTestingProgram.Services.Logging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Playwright;
using Microsoft.TeamFoundation.Build.WebApi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutomationTestingProgram.Modules.TestRunnerModule.Backend.Playwright.Objects
{
    /// <summary>
    /// Represents an instance of Playwright.
    /// </summary>
    public class PlaywrightObject
    {
        /// <summary>
        /// The IPlaywright Instance linked with this object
        /// </summary>
        public IPlaywright Instance { get; }

        /// <summary>
        /// Manages browser instances created by this object
        /// </summary>
        public BrowserManager BrowserManager { get; private set; }

        /// <summary>
        /// Keeps track of the next unique identifier for browser instances created by this object
        /// </summary>
        private int NextBrowserID;

        *//* INFO:
         * - Requests have unique IDs
         * - Playwright, Browser, Contexts, Pages have unique ID's within their parent
         *   This means that its possible for two Pages to have ID 1, but originate form different parents.
         *   Therefore, unique ID of objects per run are:
         *      Browser -> Browser ID within a run
         *      Context -> Parent (Browser ID), Context ID within parent
         *      Page -> Grandparent (Browser ID), Parent (Context ID), Page ID within parent
         * - Note: Requests and Context folders will link. Therefore, unique ID is more important request side.
         *//*

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaywrightObject"/> class.
        /// Instance is associated with a <see cref="Backend.Managers.BrowserManager"/> class
        /// to manage <see cref="Browser"/> instances.
        /// </summary>
        public PlaywrightObject()
        {
            Instance = Playwright.CreateAsync().GetAwaiter().GetResult();
            BrowserManager = new BrowserManager(this);
            NextBrowserID = 0;
        }

        /// <summary>
        /// Retrieves the next unique browser ID for browser instances.
        /// </summary>
        /// <returns>The next unique Browser ID</returns>
        public int GetNextBrowserID()
        {
            return Interlocked.Increment(ref NextBrowserID);
        }
    }
}
*/