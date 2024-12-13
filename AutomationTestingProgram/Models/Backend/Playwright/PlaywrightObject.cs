using AutomationTestingProgram.Backend;
using AutomationTestingProgram.Backend.Managers;
using AutomationTestingProgram.Services.Logging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Playwright;
using Microsoft.TeamFoundation.Build.WebApi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AutomationTestingProgram.Models.Backend
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
        public BrowserManager? BrowserManager { get; private set; }

        /// <summary>
        /// Keeps track of the next unique identifier for browser instances created by this object
        /// </summary>
        private int NextBrowserID;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaywrightObject"/> class.
        /// Instance is associated with a <see cref="AutomationTestingProgram.Backend.Managers.BrowserManager"/> class
        /// to manage <see cref="Browser"/> instances.
        /// </summary>
        public PlaywrightObject()
        {
            this.Instance = Playwright.CreateAsync().GetAwaiter().GetResult();
            this.BrowserManager = new BrowserManager(this);
            this.NextBrowserID = 0;
        }

        /// <summary>
        /// Retrieves the next unique browser ID for browser instances.
        /// </summary>
        /// <returns>The next unique Browser ID</returns>
        public int GetNextBrowserID()
        {
            return Interlocked.Increment(ref NextBrowserID);
        }

        /// <summary>
        /// Processes a given request asynchronously using the BrowserManager.
        /// </summary>
        /// <param name="request">The processed request after completion (failure or success)</param>
        /// <returns></returns>
        public async Task<Request> ProcessRequestAsync(Request request)
        {
            return await BrowserManager!.ProcessRequestAsync(request);
        }
    }
}
