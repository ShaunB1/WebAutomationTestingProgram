using Microsoft.Playwright;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationTestingProgram.Actions
{
    public class SaveFile : IWebAction
    {
        public string Name { get; set; } = "Save File";

        /// <inheritdoc/>
        public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
        {
            string filePath = step.Value;

            // Get the name of the file ignoring the path
            string fileName = Path.GetFileName(filePath);

            // Append it to our temp folder
            /*filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ConfigurationManager.AppSettings["TEMPORARY_FILES_FOLDER"], fileName);*/
            filePath = Path.Combine(Directory.GetCurrentDirectory(), "tempFiles", fileName);


            // Condition: Temp folder is empty except for Archive Folder
            // Search for any new files
            /*string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "downloaded_files"); // WON'T WORK*/
            string path = Path.Combine(Directory.GetCurrentDirectory(), "tempFiles", "downloaded_files"); // DOWNLOADED_FILES -> Page.FolderPath
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] filesFound = di.GetFiles();

            // If there are zero files downloaded, then fail
            if (filesFound.Length == 0)
            {
                return false;
            }

            // There was only one downloaded file.
            // We rename this file to be the name provided.
            FileInfo latestDownloadedFile = filesFound[0];
            string origName = Path.Combine(path, latestDownloadedFile.Name);

            if (!origName.Contains("crdownload"))
            {
                // Wait for the download to complete (no .crdownload extension)
                while (latestDownloadedFile.Name.EndsWith(".crdownload"))
                {
                    await Task.Delay(500); // Sleep for half a second
                    latestDownloadedFile.Refresh(); // Refresh to get the latest file state
                }

                latestDownloadedFile.MoveTo(filePath);
                /*step.Status = true;
                step.Actual = $"Renamed file {origName} to {filePath}.";*/
                return true;
            }
            else
            {
                // Sleep for half a second
                await Task.Delay(500);
                return false;
            }
        }
    }
}
