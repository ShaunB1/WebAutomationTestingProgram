﻿using Microsoft.Identity.Client;
using Microsoft.TeamFoundation.Build.WebApi;
using System.Collections.Concurrent;
using Microsoft.Playwright;
using System.Text;

namespace AutomationTestingProgram.Services.Logging
{
    public class LogManager
    {
        /* NOTE: For DEVELOPMENT purposes, all logs will be kept inside
         * the project file system.
         * This may be changed for PROD, with logs being saved in a specific
         * directory from the C folder.
         */
        private static readonly string BasePath = @"C:\TestingLogs\_logs";
        private static readonly string BrowserPath = "_browsers";
        private static readonly string ContextPath = "_contexts";
        private static readonly string PagePath = "_pages";
        private static string RunFolderPath = "";

        private static readonly ConcurrentDictionary<string, (SemaphoreSlim bufferSemaphore, SemaphoreSlim fileSemaphore, StringBuilder logMessage)> LogBuffer = new ConcurrentDictionary<string, (SemaphoreSlim, SemaphoreSlim, StringBuilder)>();
        private static readonly int MaxCharSize = 1000;

        static LogManager()
        {
            Initialize();
        }

        public static void Initialize()
        {
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }

            // Create a log folder for the current project run

            string runFolderName = $"Run_{(DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_fff"))}";
            RunFolderPath = Path.Combine(BasePath, runFolderName);

            if (!Directory.Exists(RunFolderPath))
            {
                Directory.CreateDirectory(RunFolderPath);
            }

            string browsersFolder = Path.Combine(RunFolderPath, BrowserPath);

            if (!Directory.Exists(browsersFolder))
            {
                Directory.CreateDirectory(browsersFolder);
            }

        }

        public static string GetRunFolderPath()
        {
            return RunFolderPath;
        }

        public static string CreateBrowserFolder(int browserID, string browserType, string browserVersion)
        {
            string browserFolderName = $"Browser_{browserID}_{browserType}_{browserVersion}_{DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_fff")}";
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

        public static string CreateContextFolder(string browserFolderPath, int contextID) {
            string contextFolderName = $"Context_{contextID}_{DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_fff")}";
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

        public static string GetContextFolder(string pageFolderPath)
        {
            return Path.GetFullPath(Path.Combine(pageFolderPath, @"..\..\"));
        }


        public static string CreatePageFolder(string contextFolderPath, int pageID)
        {
            string pageFolderName = $"Page_{pageID}_{DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_fff")}";
            string pageFolderPath = Path.Combine(Path.Combine(contextFolderPath, PagePath), pageFolderName);

            if (!Directory.Exists(pageFolderPath))
            {
                Directory.CreateDirectory(pageFolderPath);
            }

            return pageFolderPath;
        }

        public static void Flush(string path)
        {
            var logObject = LogBuffer.GetOrAdd(path, _ => (
                new SemaphoreSlim(1),
                new SemaphoreSlim(1),
                new StringBuilder()
            ));

            SemaphoreSlim stringSemaphore = logObject.bufferSemaphore;
            stringSemaphore.Wait();

            StringBuilder logMessage = logObject.logMessage;
            string fileMessage = logMessage.ToString();

            if (!string.IsNullOrEmpty(fileMessage))
            {
                logMessage.Clear();
                stringSemaphore.Release();

                SemaphoreSlim fileSemaphore = logObject.fileSemaphore;
                fileSemaphore.Wait();
                _ = WriteLog(path, fileMessage, fileSemaphore);
            }
            else
            {
                stringSemaphore.Release();
            }
        }

        // Synchronous to force the calling thread to finish logging without using any other threads.
        public static void Log(string path, string message)
        {
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
                _ = WriteLog(path, fileMessage, fileSemaphore);
            }
            else
            {
                stringSemaphore.Release();
            }
        }

        // Should this be async? Then calling thread will continue...
        private static async Task WriteLog(string path, string message, SemaphoreSlim semaphore)
        {
            try
            {
                string filePath = Path.Combine(path, "log.txt");
                await File.AppendAllTextAsync(filePath, message);
            }
            finally
            {
                semaphore.Release();
            }
            
        }
    }
}
