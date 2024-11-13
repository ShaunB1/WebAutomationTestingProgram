namespace AutomationTestingProgram.Actions
{
    /// <summary>
    /// All accepted commands for test steps.
    /// COMMANDs apply to one test step only.
    /// </summary>
    public enum COMMAND
    {
        /// <summary>
        /// Command used to specify the iframe to switch to. 
        /// Accepts either an int index to select a specific frame
        /// or locators to find the frame dynamically (based on element it contains).
        /// </summary>
        IFRAME,

        /// <summary>
        /// Command used to switch to the root/top-level context.
        /// Accepts no arguments.
        /// </summary>
        ROOT,

        /// <summary>
        /// Command used to specify the parent iframe to switch to. 
        /// Accepts an int indicating the number of levels to move up in the hierarchy.
        /// </summary>
        PARENT,

        /// <summary>
        /// Command used to specify the child iframe to switch to. 
        /// Accepts either an int index to select a specific child frame
        /// or locators to find the frame dynamically (based on element it contains).
        /// </summary>
        CHILD,

        /// <summary>
        /// Command used to specify that a screenshot should be taken for a test step, regardless
        /// of result.
        /// Accepts no arguments.
        /// 3 screenshots are taken: 1 before, 1 during (highlighting with hover over element) and one
        /// after. NOTE: Screenshot unfocuses element, so after 'during' screenshot, will refocus.
        /// </summary>
        TAKESCREENSHOT,

        /// <summary>
        /// Command used to specify that no screenshot should be taken for a test step, regardless of
        /// result.
        /// Accepts no arguments.
        /// </summary>
        DISABLESCREENSHOT,

        /// <summary>
        /// Command used to specify that a step should be highlighted or not. Overrides 
        /// highlight environment variable.
        /// Accepts a string value that indicates a colour. If none provided, uses default.
        /// </summary>
        HIGHLIGHT,

        /// <summary>
        /// Command used to add an extra wait before a certain step.
        /// Accepts an int determining the time to wait. Default: 30s
        /// </summary>
        WAITBEFORE,

        /// <summary>
        /// Command used to add an extra wait after a certain step.
        /// Accepts an int determining the time to wait. Default: 30s
        /// </summary>
        WAITAFTER,

        /// <summary>
        /// Command used to specify the number of times to repeat a test step.
        /// Accepts an int determining the # of times. Default: 1
        /// NOTE: Will repeat results that pass.
        /// </summary>
        REPEAT,

        /// <summary>
        /// Command used to specify that a step should be ignored (same as # in control).
        /// Accepts no arguments.
        /// </summary>
        IGNORE,

        /// <summary>
        /// Command used to specify that we expect this step to pass.
        /// Accepts no arguments.
        /// This is always on by default.
        /// </summary>
        EXPECTPASS,

        /// <summary>
        /// Command used to specify that we expect this step to fail.
        /// Accepts no arguments.
        /// </summary>
        EXPECTFAIL,

        /// <summary>
        /// Command used to specify to run AODA specifically for that test step.
        /// Accepts no arguments.
        /// This will override any AODA environment variables.
        /// </summary>
        RUNAODA,

        /// <summary>
        /// Command used to specify to not run AODA specifically for that test step.
        /// Accepts no arguments.
        /// This will override any AODA environment variables.
        /// </summary>
        NOAODA,

        /// <summary>
        /// Command used to set the browser window size.
        /// Accepts an int determining the new size of the window. Default: Max
        /// NOTE: Will only change the size for the current browser context.
        /// </summary>
        SETWINDOWSIZE,

        /// <summary>
        /// Command used for debugging purposes. Will stop the action just before
        /// we execute it without throwing errors, so that the user can debug it.
        /// Accepts no arguments.
        /// Note: Can only be used for local execution only!! 
        /// </summary>
        DEBUG,

        /// <summary>
        /// COmmand used to cancel/stop a test at a specific point.
        /// Accepts no arguments.
        /// Test will stop after step that contains command is finished executing.
        /// Will treat it as reaching bottom of the test file.
        /// </summary>
        CANCELTEST,
    }

    public static class CommandMappings
    {
        private static readonly Dictionary<string, COMMAND> GetCommandType = new Dictionary<string, COMMAND>()
        {
            { "iframe", COMMAND.IFRAME },
            { "frame", COMMAND.IFRAME },
            { "root", COMMAND.ROOT },
            { "parent", COMMAND.PARENT },
            { "child", COMMAND.CHILD },
            { "takescreenshot", COMMAND.TAKESCREENSHOT },
            { "screenshot", COMMAND.TAKESCREENSHOT },
            { "noscreenshot", COMMAND.DISABLESCREENSHOT },
            { "disablescreenshot", COMMAND.DISABLESCREENSHOT },
            { "highlight", COMMAND.HIGHLIGHT },
            { "wait", COMMAND.WAITBEFORE },
            { "waitbefore", COMMAND.WAITBEFORE },
            { "waitafter", COMMAND.WAITAFTER },
            { "repeat", COMMAND.REPEAT },
            { "loop", COMMAND.REPEAT },
            { "ignore", COMMAND.IGNORE },
            { "skip", COMMAND.IGNORE },
            { "expectpass", COMMAND.EXPECTPASS },
            { "pass", COMMAND.EXPECTPASS },
            { "expectfail", COMMAND.EXPECTFAIL },
            { "fail", COMMAND.EXPECTFAIL },
            { "runaoda", COMMAND.RUNAODA },
            { "aoda", COMMAND.RUNAODA },
            { "noaoda", COMMAND.NOAODA },
            { "setwindowsize", COMMAND.SETWINDOWSIZE },
            { "windowsize", COMMAND.EXPECTFAIL },
            { "debug", COMMAND.DEBUG },
            { "stoptest", COMMAND.CANCELTEST },
            { "canceltest", COMMAND.CANCELTEST },
            { "stop", COMMAND.CANCELTEST },
            { "cancel", COMMAND.CANCELTEST }
        };

        public static COMMAND? GetCommand(string commandString)
        {
            if (string.IsNullOrEmpty(commandString))
                return null;
            
            if (GetCommandType.TryGetValue(commandString.ToLower(), out var command))
            {
                return command;
            }
            return null;
        }
    }
}
