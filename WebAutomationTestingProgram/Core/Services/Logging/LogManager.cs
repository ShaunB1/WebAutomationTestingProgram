using System.Collections.Concurrent;
using System.Runtime.Versioning;
using System.Text;

namespace WebAutomationTestingProgram.Core.Services.Logging;

/// <summary>
/// LogManager manages all logs RUN WIDE.
/// </summary>
public static class LogManager
{
    /* INFO:
     * 
     * 1. All logs are kept in C Drive
     * 2. Inside each folder, there is a log.txt file. All logs are written to these files.
     * 3. Logs are written to their own log.txt file depending on level.
     * 4. Two main folders: Requests and Browser. Links are used to connect between them if needed.
     * 5. Requests are always guaranteed to have a unique ID
     * 6. Browser, Context and Page all have incremental unique ids based on their parent.
     *    However, it does not guarantee unique ids between different runs.
     *    
     * 
     */

    // Major folders
    private static readonly string BasePath = @"C:\AutomationTestingProgramLogs\_runs"; // Base Path (from C Drive)
    private static readonly string RequestPath = "_requests"; // Request folder. Each request gets its own folder.
    private static readonly string BrowserPath = "_browsers"; // Browser folder. Each browser gets its own folder.
    private static readonly string ContextPath = "_contexts"; // Context folder. Each context gets its own folder within a browser folder.
    private static readonly string PagePath = "_pages"; // Page folder. Each page gets its own folder within a context folder.

    // Subfolders within Page Folder
    public static readonly string DownloadPath = "_downloads"; // The path for downloaded files during playwright automation.
    public static readonly string ResultsPath = "_results"; // The path for any result files produced throughout a run
    public static readonly string ScreenShotPath = "_screenshots"; // The path for screenshots during a run
    public static readonly string TempFilePath = "_tempFiles"; // The path for named downloaded files during playwright automation

    private static string RunFolderPath = ""; // Base Path (from C Drive) of the run folder-> Each run has unique id/name

    // First semaphore used to update stringbuilder. Second to write to file.
    private static readonly ConcurrentDictionary<string, (SemaphoreSlim bufferSemaphore, SemaphoreSlim fileSemaphore, StringBuilder logMessage)> LogBuffer = new ConcurrentDictionary<string, (SemaphoreSlim, SemaphoreSlim, StringBuilder)>();
    private static readonly int MaxCharSize = 1000; // Used to limit # of I/O operations. Flush is used to ensure its written when something ends/closes.

    static LogManager()
    {
        Initialize();
    }

    /// <summary>
    /// Creates all the main directories for the Run:
    /// - Run Directory
    /// - Requests Directory
    /// - Browsers Directory
    /// </summary>
    private static void Initialize()
    {
        if (!Directory.Exists(BasePath))
        {
            Directory.CreateDirectory(BasePath);
        }

        // Create a log folder for the current project run

        string runFolderName = $"Run_{(DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff"))}"; // Chronological sorting by date
        RunFolderPath = Path.Combine(BasePath, runFolderName);
        if (!Directory.Exists(RunFolderPath))
        {
            Directory.CreateDirectory(RunFolderPath);
        }

        // Create browser log folder under run folder
        string browserFolder = Path.Combine(RunFolderPath, BrowserPath);
        if (!Directory.Exists(browserFolder))
        {
            Directory.CreateDirectory(browserFolder);
        }

        // Create request log folder under run folder
        string requestFolder = Path.Combine(RunFolderPath, RequestPath);
        if (!Directory.Exists(requestFolder))
        {
            Directory.CreateDirectory(requestFolder);
        }

    }

    /// <summary>
    /// Retrieves the current Run Directory Path.
    /// </summary>
    /// <returns>A string path</returns>
    public static string GetRunFolderPath()
    {
        return RunFolderPath;
    }

    /// <summary>
    /// Creates a new Request Folder based on given Request ID
    /// </summary>
    /// <param name="requestID">The ID of the request</param>
    /// <returns>The folderpath of the newly created folder</returns>
    public static string CreateRequestFolder(string requestID)
    {
        string requestFolderName = $"Request_{requestID}_{(DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff"))}";
        string requestFolderPath = Path.Combine(Path.Combine(RunFolderPath, RequestPath), requestFolderName);

        if (!Directory.Exists(requestFolderPath))
        {
            Directory.CreateDirectory(requestFolderPath);
        }

        return requestFolderPath;
    }    

    /// <summary>
    /// Create a shortcut at the request folder path that points to the context folder path, and vice versa.
    /// Note: Junctions, symbolic links, require admin permissions. Using shortcuts instead
    /// </summary>
    /// <param name="requestFolderPath">The path of the request folder</param>
    /// <param name="contextFolderPath">The path of the context folder</param>
    [SupportedOSPlatform("windows")]
    public static void MapRequestToContextFolders(string requestFolderPath, string contextFolderPath)
    {
        if (!Directory.Exists(requestFolderPath))
        {
            throw new Exception($"Request Folder Path: {requestFolderPath} does not exist!");
        }

        if (!Directory.Exists(contextFolderPath))
        {
            throw new Exception($"Context Folder Path: {contextFolderPath} does not exist!");
        }

        string ContextShortcutPath = Path.Combine(contextFolderPath, "Request.url");
        string RequestShortcutPath = Path.Combine(requestFolderPath, "Context.url");

        CreateUrlShortcut(ContextShortcutPath, requestFolderPath);
        CreateUrlShortcut(RequestShortcutPath, contextFolderPath);
    }

    /// <summary>
    /// Creates a .url shortcut file
    /// </summary>
    /// <param name="shortcutPath">The path where the shortcut will be created</param>
    /// <param name="targetPath">The target path the shortcut points to</param>
    private static void CreateUrlShortcut(string shortcutPath, string targetPath)
    {
        using (StreamWriter writer = new StreamWriter(shortcutPath))
        {
            writer.WriteLine("[InternetShortcut]");
            writer.WriteLine($"URL=file:///{targetPath.Replace("\\", "/")}");
            writer.WriteLine("IconIndex=0");
            string icon = targetPath.Replace("\\", "/");
            writer.WriteLine($"IconFile={icon}");
        }
    }

    /// <summary>
    /// Creates a new Browser Folder based on given Browser Data
    /// </summary>
    /// <param name="browserID">The ID of the Browser</param>
    /// <param name="browserType">The Type of Browser</param>
    /// <param name="browserVersion">The Version of Browser</param>
    /// <returns>The folderpath of the newly created folder</returns>
    public static string CreateBrowserFolder(int browserID, string browserType, string browserVersion)
    {
        string browserFolderName = $"Browser_{browserID}_{browserType}_{browserVersion}_{(DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff"))}";
        string browserFolderPath = Path.Combine(Path.Combine(RunFolderPath, BrowserPath), browserFolderName);

        if (!Directory.Exists(browserFolderPath))
        {
            Directory.CreateDirectory(browserFolderPath);
        }

        string contextsFolder = Path.Combine(browserFolderPath, ContextPath);

        if (!Directory.Exists(contextsFolder))
        {
            Directory.CreateDirectory(contextsFolder);
        }

        return browserFolderPath;
    }

    /// <summary>
    /// Creates a new Context Folder based on given Context Data and the parent folder path.
    /// </summary>
    /// <param name="browserFolderPath">The folderpath of the browser</param>
    /// <param name="contextID">The ID of the context</param>
    /// <returns>The folderpath of the newly created folder</returns>
    public static string CreateContextFolder(string browserFolderPath, int contextID) {
        string contextFolderName = $"Context_{contextID}_{(DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff"))}";
        string contextFolderPath = Path.Combine(Path.Combine(browserFolderPath, ContextPath), contextFolderName);

        if (!Directory.Exists(contextFolderPath))
        {
            Directory.CreateDirectory(contextFolderPath);
        }

        string pagesFolder = Path.Combine(contextFolderPath, PagePath);

        if (!Directory.Exists(pagesFolder))
        {
            Directory.CreateDirectory(pagesFolder);
        }

        return contextFolderPath;
    }

    /// <summary>
    /// Creates a new Page Folder based on the given Page Data and the parent folder path.
    /// </summary>
    /// <param name="contextFolderPath">The Folderpath of the context</param>
    /// <param name="pageID">The ID of the page</param>
    /// <returns>The folderpath of the newly created folder</returns>
    public static string CreatePageFolder(string contextFolderPath, int pageID)
    {
        string pageFolderName = $"Page_{pageID}_{(DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff"))}";
        string pageFolderPath = Path.Combine(Path.Combine(contextFolderPath, PagePath), pageFolderName);

        if (!Directory.Exists(pageFolderPath))
        {
            Directory.CreateDirectory(pageFolderPath);
        }

        string downloads = Path.Combine(pageFolderPath, DownloadPath);
        if (!Directory.Exists(downloads))
        {
            Directory.CreateDirectory(downloads);
        }

        string results = Path.Combine(pageFolderPath, ResultsPath);
        if (!Directory.Exists(results))
        {
            Directory.CreateDirectory(results);
        }

        string screenshots = Path.Combine(pageFolderPath, ScreenShotPath);
        if (!Directory.Exists(screenshots))
        {
            Directory.CreateDirectory(screenshots);
        }

        string tempFiles = Path.Combine(pageFolderPath, TempFilePath);
        if (!Directory.Exists(tempFiles))
        {
            Directory.CreateDirectory(tempFiles);
        }

        return pageFolderPath;
    }

    /// <summary>
    /// Flushes all logs for a given path.
    /// Removes the entry from dictionary.
    /// ONLY CALL AT THE END (no more logs). Else, IO issues may arise.
    /// </summary>
    /// <param name="path">The path to flush</param>
    public static void Flush(string path, bool removeEntry = true, string message = "")
    {
        (SemaphoreSlim bufferSemaphore, SemaphoreSlim fileSemaphore, StringBuilder logMessage) logObject;

        // We dont remove entry for shutdown -> only case when flushing too early
        if (removeEntry)
        {
            if (!LogBuffer.TryRemove(path, out logObject))
            {
                return; // Nothing to flush
            }
        }
        else
        {
            if (!LogBuffer.TryGetValue(path, out logObject))
            {
                return;
            }
        }

        SemaphoreSlim stringSemaphore = logObject.bufferSemaphore;
        stringSemaphore.Wait();

        StringBuilder logMessage = logObject.logMessage;

        if (!string.IsNullOrEmpty(message))
        {
            logMessage.AppendLine(message);
        }

        string fileMessage = logMessage.ToString();

        if (!string.IsNullOrEmpty(fileMessage))
        {
            logMessage.Clear();
            stringSemaphore.Release();

            SemaphoreSlim fileSemaphore = logObject.fileSemaphore;
            fileSemaphore.Wait();
            WriteLog(path, fileMessage, fileSemaphore);
        }
        else
        {
            stringSemaphore.Release();
        }
    }

    /// <summary>
    /// Flushes all logs.
    /// </summary>
    public static void FlushAll(string message)
    {   
        foreach (var entry in LogBuffer)
        {
            string path = entry.Key;
            Flush(path, false, message);
        }
    }


    /// <summary>
    /// Will perform the following operations:
    /// - Check if Context Link exists. If so, go to page log.
    /// - Lock log file
    /// - Flush all logs to file
    /// - Creates a copy of the log file using IO (with unique name)
    /// - Unlocks log file
    /// - Returns copy.
    /// 
    /// - NOTE: Must delete copy using OnCompleted Event
    /// </summary>
    /// <param name="path"></param>
    public static (string, bool) RetrieveLog(string requestID)
    {
        // First get the request Folder
        string path = RetrieveRequestFolder(requestID);
        
        // First, check if a context.url exists
        string ContextShortCut = Path.Combine(path, "Context.url");

        if (File.Exists(ContextShortCut))
        { // If exists, go to page folder

            string ContextPath = string.Empty;
            
            try
            {
                var lines = File.ReadAllLines(ContextShortCut);
                foreach (var line in lines)
                {
                    if (line.StartsWith("URL="))
                    {
                        ContextPath = line.Substring(4);
                        if (ContextPath.StartsWith("file:///"))
                        {
                            Uri uri = new Uri(ContextPath);
                            ContextPath = uri.LocalPath;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            if (string.IsNullOrEmpty(ContextPath))
            {
                throw new Exception("Context.url found, but url could not be extracted");
            }

            string PagesFolder = Path.Combine(ContextPath, PagePath);

            string[] directories = Directory.GetDirectories(PagesFolder);
            path = directories[0];
        }

        // If in dictionary, then still active log
        if (LogBuffer.TryGetValue(path, out (SemaphoreSlim bufferSemaphore, SemaphoreSlim fileSemaphore, StringBuilder logMessage)  logObject))
        {
            SemaphoreSlim stringSemaphore = logObject.bufferSemaphore;
            stringSemaphore.Wait();

            StringBuilder logMessage = logObject.logMessage;

            string fileMessage = logMessage.ToString();

            logMessage.Clear();
            stringSemaphore.Release();

            SemaphoreSlim fileSemaphore = logObject.fileSemaphore;
            fileSemaphore.Wait();
            if (!string.IsNullOrEmpty(fileMessage))
            {
                WriteLog(path, fileMessage, null);
            }

            string source = Path.Combine(path, "log.txt");
            string destination = Path.Combine(path, $"{Guid.NewGuid()}.txt");

            File.Copy(source, destination);
            fileSemaphore.Release();

            return (destination, true);

        }
        else // Not active, just return log
        {
            return (Path.Combine(path, "log.txt"), false);
        }



    }

    /// <summary>
    /// Attempts to retrieve the request folder of a request with the given ID.
    /// If not found, throws error.
    /// </summary>
    /// <param name="requestID"></param>
    /// <returns></returns>
    private static string RetrieveRequestFolder(string requestID)
    {
        string requestFolderName = $"Request_{requestID}_";
        string requestFolderPath = Path.Combine(RunFolderPath, RequestPath);

        string[] directories = Directory.GetDirectories(requestFolderPath);

        string? match = directories.FirstOrDefault(directory =>
                                Path.GetFileName(directory).StartsWith(requestFolderName));

        if (match == null)
        {
            throw new Exception("Request directory not found!");
        }

        return match;
    }

    /// <summary>
    /// Logs to a log.txt file within a given folderpath.
    /// </summary>
    /// <param name="path">The folderpath where the log.txt is located</param>
    /// <param name="message">The message to log</param>
    public static void Log(string path, string message)
    {
        /* INFO
         * - Synchronous to force the calling thread to finish logging without using any other threads.
         * - Limits IO operations by holding up to a certain amount of characters in memory before writing to file
         * - Uses two semaphores -> One to update message in memory, one to write to file.
         * - While this method will result in less IO operations, MUST Flush once something closes/ends to ensure logs are outputted.
         * 
         */

        var logObject = LogBuffer.GetOrAdd(path, _ => (
            new SemaphoreSlim(1),
            new SemaphoreSlim(1),
            new StringBuilder()
        ));

        SemaphoreSlim stringSemaphore = logObject.bufferSemaphore;
        stringSemaphore.Wait();

        StringBuilder logMessage = logObject.logMessage;
        logMessage.AppendLine(message);

        if (logMessage.Length >= MaxCharSize)
        {
            string fileMessage = logMessage.ToString();
            logMessage.Clear();
            stringSemaphore.Release();

            SemaphoreSlim fileSemaphore = logObject.fileSemaphore;
            fileSemaphore.Wait();
            WriteLog(path, fileMessage, fileSemaphore);
        }
        else
        {
            stringSemaphore.Release();
        }
    }

    
    /// <summary>
    /// Writes to the log.txt file within a given folder path
    /// </summary>
    /// <param name="path">The path to the folder</param>
    /// <param name="message">The message to write</param>
    /// <param name="semaphore">Semaphore to release once writing is complete</param>
    /// <returns></returns>
    private static void WriteLog(string path, string message, SemaphoreSlim? semaphore)
    { 
        try
        {
            string filePath = Path.Combine(path, "log.txt"); // All folders have a log.txt created and appended to from here
            File.AppendAllText(filePath, message);
        }
        finally
        {
            if (semaphore != null)
            {
                semaphore.Release();
            }
        }            
    }
}
