namespace AutomationTestingProgram.Models.Backend
{
    public class FileBreakpoint
    {   
        /// <summary>
        /// The row at which the breakpoint occurs.
        /// Occurs by default at the end of every test case.
        /// If a test case is too long (over 50 steps), recursvely add breakpoints in the middle.
        /// </summary>
        public int RowNum { get; set; }

        /// <summary>
        /// The reason for the breakpoint
        /// </summary>
        public BreakpointReason Reason { get; set; }

    }

    public enum BreakpointReason
    {
        /// <summary>
        /// Defines that a new test case starts (DEFAULT)
        /// </summary>
        NewTestCase,

        /// <summary>
        /// Breakpoint within a singular test case. Only occurs if a test case is too long (50+ steps)
        /// </summary>
        MoreSteps,

        /// <summary>
        /// Defines that a cycle starts at this point
        /// </summary>
        CycleStart,

        /// <summary>
        /// Defines that a cycle ends at this point
        /// </summary>
        CycleEnd,
    }
}
