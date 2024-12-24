// <copyright file="AutoUpdaterWrapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using AutomationTestingProgram;
using System.IO;
using System.Net.Http;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using System;

namespace AutoUpdater
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutomationTestingProgram;
    using System.Linq;
    using System.Text.Json;
    using Microsoft.Win32;
    using System.Runtime.Versioning;
    using System.Reflection;
    using System.Configuration;
    using System.Net;

    public class AutoUpdater
    {   
        /*
         * string Type, string Version
         * Ex: Chrome 123
         * Ex: Chrome 123.45
         */

        /// <summary>
        /// Runs then re runs PrefXML.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            AutoUpdateBrowsers(args);
        }

        /// <summary>
        /// Automatically updates the drivers according to the binaries.
        /// How it works: Takes the user's --browser and --version args. --browser specifies which Browser the user wants to use and
        /// --version specifies which browser version the user wants to use. --version default takes the default browser's version, 
        /// --version latest takes the latest binary of a browser located in C:\Automation\Browsers, and --version 114 (for example) takes
        /// the first binary with a version starting with 114. If --browser is empty, defaults to Chrome, if --version is empty, defaults
        /// to --version default.
        /// </summary>
        /// <param name="args"></param>
        public static void AutoUpdateBrowsers(string[] args)
        {
            string exeLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string temp_files = exeLocation + @"\temp_files";
            if (Directory.Exists(temp_files))
            {
                Directory.Delete(temp_files, true);
            }
            Directory.CreateDirectory(temp_files);
            Dictionary<string, string> cmdArgs = ParseArguments(args);

            if (cmdArgs.Count == 0)
            {
                cmdArgs["browser"] = "Chrome";
                cmdArgs["version"] = "default";
            }

            if (cmdArgs["browser"].ToLower() == "remotechrome" || cmdArgs["browser"].ToLower() == "remoteedge" || cmdArgs["browser"].ToLower() == "remotefirefox") { return; }
            string browser = cmdArgs["browser"].ToLower();
            string version = cmdArgs["version"].ToLower();
            string agentHome = "";

            if (!cmdArgs.ContainsKey("agent")) {
                agentHome = "";
            } else {
                agentHome = cmdArgs["agent"];
            }
            string browsersPath = $@"C:\Automation\Browsers";
            if (!Directory.Exists(browsersPath))
            {
                AULogger.Info($"Creating {browsersPath}");
                Directory.CreateDirectory(browsersPath);
            }

            Dictionary<string, string> binaryDict = new Dictionary<string, string> {
                { "chrome", "chrome.exe" },
                { "edge","msedge.exe" },
                { "firefox", "firefox.exe" }
            };

            Dictionary<string, string> driverDict = new Dictionary<string, string> {
                { "chrome", "chromedriver.exe" },
                { "edge","msedgedriver.exe" },
                { "firefox", "geckodriver.exe" }
            };

            // Check if the specified browser is in the dictionary
            if (!binaryDict.ContainsKey(browser))
            {
                // Log the error message or throw an exception
                AULogger.Error($"Error: The browser '{browser}' is not supported.");
                throw new ArgumentException($"The browser '{browser}' is not supported.");
            }

            string binaryVersion = "";
            string latestPath = FindLatestLocalBinary(browsersPath, browser);

            Directory.CreateDirectory(browsersPath);

          /*  string chromeBinaries = ConfigurationManager.AppSettings["CHROME_BINARIES"];
            string chromeBinaries2 = ConfigurationManager.AppSettings["CHROME_BINARIES_2"];*/
            string chromeBinaries = "https://googlechromelabs.github.io/chrome-for-testing";
            string chromeBinaries2 = "https://chromedriver.storage.googleapis.com";

            string firefoxBinaries2 = "https://github.com/mozilla/geckodriver/releases/download/v0.35.0/geckodriver-v0.35.0-win64.zip";
            string firefoxBinaries3 = "https://ftp.mozilla.org/pub/firefox/releases";


            // We do not need this if block as default option is not a valid option for browser version at this time. 
            // We do need it for browsers besides chrome as they only for on default.
            if (version == "default") {
                string systemBrowserPath = GetAppRegistryPath(binaryDict[browser]);
                string defaultVersion = GetFileVersion(systemBrowserPath);
                string defaultBinary = $@"{browsersPath}\{browser}-default";
                Version defVer = new Version(defaultVersion);
                Version curDefVer = null;

                if (Directory.Exists($@"{defaultBinary}")) {
                    string binaryPath = FindBinaryFilePath(defaultBinary, $"{binaryDict[browser]}");
                    curDefVer = new(GetFileVersion(binaryPath));
                    AULogger.Info($"Default {browser.ToUpper()} v{curDefVer}");
                } else {
                    Directory.CreateDirectory($@"{browsersPath}\{browser}-default");
                }

                if (defVer == curDefVer) {
                    AULogger.Info("Default Version Unchanged");
                } else {
                    AULogger.Info("Default Version Outdated");
                    if (browser == "chrome") {
                        CopyDirContents(Path.GetDirectoryName(systemBrowserPath), $@"{browsersPath}\{browser}-{version}\chrome-win64");
                    } else {
                        CopyDirContents(Path.GetDirectoryName(systemBrowserPath), $@"{browsersPath}\{browser}-{version}");
                    }
                }

                AULogger.Info("System Browser Path: " + systemBrowserPath);
                binaryVersion = GetFileVersion(systemBrowserPath);
                Directory.CreateDirectory($@"{browsersPath}\{browser}-default");
                if (browser == "firefox")
                {
                    //binaryVersion = GetFileVersion(@"C:\Program Files\Mozilla Firefox\firefox.exe");
                    binaryVersion = "0.34.0"; // https://firefox-source-docs.mozilla.org/testing/geckodriver/Support.html
                }
            } else if (version == "latest") {
                string latestStable = string.Empty;
                string driverPlatform = "win64";
                string latestBinary = $@"{browsersPath}\{browser}-latest";
                string binaryPath = "";
                Version stableLatest = null;
                Version curLatest = null;

                if (Directory.Exists($@"{latestBinary}")) {
                    binaryPath = FindBinaryFilePath(latestBinary, $"{binaryDict[browser]}");
                    curLatest = new(GetFileVersion(binaryPath));
                    AULogger.Info($"Default {browser.ToUpper()} v{curLatest}");
                } else {
                    Directory.CreateDirectory($@"{browsersPath}\{browser}-latest");
                }

                if (browser == "chrome") {
                    AULogger.Info("Downloading JSON endpoint.");
                    latestStable = $@"{chromeBinaries}/LATEST_RELEASE_STABLE";
                    DownloadAsync(latestStable, "chromeBinaryVer.json").GetAwaiter().GetResult();
                    stableLatest = new(ReadJson($@"{temp_files}\chromeBinaryVer.json").Result);

                    if (stableLatest == curLatest) {
                        AULogger.Info("Latest Stable Version Unchanged");
                    } else {
                        AULogger.Info("Current Stable Version Outdated!");
                        AULogger.Info("Latest Stable Version: " + stableLatest.ToString());
                        string chromeBinaryVerJson = ChromeDriverURLDownload(stableLatest.ToString(), browser).Result;
                        string binaryUrl = BinaryDownloadJson(chromeBinaryVerJson, driverPlatform);
                        AULogger.Info($"Binary To Be Downloaded: {browser.ToUpper()} {stableLatest.ToString()}");
                        AULogger.Info($@"Binary URL: " + binaryUrl);
                        DownloadAsync(binaryUrl, "binary.zip").GetAwaiter().GetResult(); // downloads the .zip file under temp_files
                        string zipPath = exeLocation + @"\temp_files\binary.zip";
                        string extractPath = $@"{browsersPath}\{browser}-latest";
                        ExtractZipFile(zipPath, extractPath);
                    }
                }
                binaryVersion = browser == "firefox" ? "0.34.0" : stableLatest.ToString();
            } else {
                string[] tempversionParts = version.Split('.');
                int tempmajor = 0, tempminor = 0, tempbuild = 0, temprevision = 0;

                if (tempversionParts.Length >= 1)
                {
                    tempmajor = int.Parse(tempversionParts[0]);
                }
                if (tempversionParts.Length >= 2)
                {
                    tempminor = int.Parse(tempversionParts[1]);
                }
                if (tempversionParts.Length >= 3)
                {
                    tempbuild = int.Parse(tempversionParts[2]);
                }
                if (tempversionParts.Length == 4)
                {
                    temprevision = int.Parse(tempversionParts[3]);
                }

                Version tempVer = new Version(tempmajor, tempminor, tempbuild, temprevision);

                string tempVersion = version == "latest" ? "0" : version;
                string binaryPath = "";

                // Different browsers have different ways of storing the exe files.
                // Chrome would store the full build inside the major build folder for e.g 114.0.5735.133 would automatically go into chrome-114.
                if (browser == "chrome")
                {
                    binaryPath = GetExeFromVersion(tempversionParts[0], $"{browser}.exe", browsersPath);
                }
                // Firefox would require the user to manually download the specific version and copy the program files into a new folder.
                // for e.g version 99.0.1 would be a different folder and 99.0 a different one.
                else if (browser == "firefox")
                {
                    binaryPath = GetExeFromVersion(tempVersion, $"{browser}.exe", browsersPath);
                }

                string driverPlatform = "win64";
                Version ancientChromeVer = new Version("113.0");
                /*Version chromeVer = new Version(int.Parse(version), 0);*/
                string[] versionParts = version.Split('.');
                int major = 0, minor = 0, build = 0, revision = 0;

                if (versionParts.Length >= 1)
                {
                    major = int.Parse(versionParts[0]);
                }
                if (versionParts.Length >= 2)
                {
                    minor = int.Parse(versionParts[1]);
                }
                if (versionParts.Length >= 3)
                {
                    build = int.Parse(versionParts[2]);
                }
                if (versionParts.Length == 4)
                {
                    revision = int.Parse(versionParts[3]);
                }

                Version chromeVer = new Version(major, minor, build, revision);

                // if the binary doesn't exist, download it asynchronously
                if (string.IsNullOrEmpty(binaryPath) || binaryPath.Contains($"{browser}-default") || binaryPath.Contains($"{browser}-latest")) {
                    if (browser == "chrome" && chromeVer >= ancientChromeVer || version == "0") {
                        tempVersion = version == "latest" ? "STABLE" : tempVersion;
                        string getLatestMinorVer = $@"{chromeBinaries}/LATEST_RELEASE_{tempmajor}";
                        bool successful = CheckUrlSucces(getLatestMinorVer).Result;

                        if (!successful) {
                            getLatestMinorVer = $@"{chromeBinaries2}/LATEST_RELEASE_{tempmajor}";
                            successful = CheckUrlSucces(getLatestMinorVer).Result;
                        }

                        AULogger.Info("Downloading JSON endpoint.");
                        DownloadAsync(getLatestMinorVer, "chromeBinaryVer.json").GetAwaiter().GetResult();

                        string jsonString = ReadJson($@"{temp_files}\chromeBinaryVer.json").Result;
                        Version temp = new Version(jsonString);
                        tempVersion = temp.Major.ToString();
                        AULogger.Info("Latest Minor Binary Version: " + jsonString);

                        string chromeBinaryVerJson = ChromeDriverURLDownload(jsonString, browser).Result;
                        string binaryUrl = BinaryDownloadJson(chromeBinaryVerJson, driverPlatform);
                        AULogger.Info($"Binary To Be Downloaded: {browser.ToUpper()} {jsonString}");
                        AULogger.Info($@"Binary URL: " + binaryUrl);

                        DownloadAsync(binaryUrl, "binary.zip").GetAwaiter().GetResult(); // downloads the .zip file under temp_files
                    }
                    // Working on adding async downloading functionality of firefox 
                    else if (browser == "firefox")
                    {
                        string getVersion = $@"{firefoxBinaries3}/{version}/win64/en-CA/Firefox%20Setup%20{version}.exe"; // exe file
                        string getVersion1 = $@"{firefoxBinaries3}/{version}/win64/en-CA/Firefox%20Setup%20{version}.msi"; // msi file

                        AULogger.Info("Checking Download URL");
                        bool successful = CheckUrlSucces(getVersion).Result;

                        if (successful)
                        {
                            AULogger.Info("Downloading .exe File");
                            bool downloadSuccess = DownloadAsync(getVersion, "firefox.exe").GetAwaiter().GetResult();
                            if (downloadSuccess)
                            {
                                string installerPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "temp_files", "firefox.exe");
                                /*InstallFirefox(installerPath);*/
                                InstallFirefox(installerPath, browsersPath, version);
                                File.Delete(installerPath);
                            }
                        }
                        else
                        {
                            AULogger.Error("URL is not accessible");
                        }
                    }
                   
                    else
                    {
                        string targetVersion = string.Join(".", versionParts);
                        string binaryUrl = "";
                        if (browser == "edge") {
                            //binaryUrl = $"https://msedgedriver.azureedge.net/{binaryVersion}/edgedriver_win64.zip";
                            AULogger.Error("Please set --version to default for Edge");
                            throw new Exception("Please set --version to default for Edge");

                        // Finds the browser version saved by the user manually in automation/browsers
                        } else if (browser == "firefox") {
                            string versionDirectory = $@"C:\Automation\browsers\firefox-{targetVersion}";

                            if (Directory.Exists(versionDirectory))
                            {
                                AULogger.Info($"Firefox version found in: {versionDirectory}");
                                binaryUrl = $@"{versionDirectory}\firefox.exe"; 
                            }
                            else
                            {
                                AULogger.Error("No local Firefox version found. Please set --version to default for Firefox.");
                                throw new Exception("No local Firefox version found. Please set --version to default for Firefox.");
                            }

                            // binaryUrl = $"https://github.com/mozilla/geckodriver/releases/download/v{binaryVersion}/geckodriver-v{binaryVersion}-win64.zip"; manual way of setting driver for firefox
                        } else if (browser == "chrome") {
                            AULogger.Error("CHROME VER TOO OLD!");
                            throw new Exception("CHROME VER TOO OLD!"); // no binaries exist prior to v113.0.5672.0
                        }

                        AULogger.Info("Binary URL: " + binaryUrl);
                        DownloadAsync(binaryUrl, "binary.zip").GetAwaiter().GetResult();
                    }

                    if (browser == "chrome")
                    {
                        string zipPath = exeLocation + @"\temp_files\binary.zip";
                        string extractPath = $@"{browsersPath}\{browser}-{tempVersion}";
                        ExtractZipFile(zipPath, extractPath);
                        binaryPath = GetExeFromVersion(tempversionParts[0], $"{browser}.exe", browsersPath); // after downloading the binary, get the path to it
                    }
                    else if (browser == "firefox")
                    {
                        // Define the source and destination paths
                        string sourcePath = @"C:\Program Files\Mozilla Firefox"; 
                        string destinationPath = Path.Combine(browsersPath, $"{browser}-{version}");
                        /*string destinationPath = Path.Combine(browsersPath, $"{browser}-{versionParts[0]}");*/

                        // Copying is now done in InstallFirefox method
                        // Call your existing CopyDirContents function 
                        /*CopyDirContents(sourcePath, destinationPath);*/
                        binaryPath = GetExeFromVersion(version, $"{browser}.exe", browsersPath);
                    }
                } else {
                    AULogger.Info("Binary Exists: " + binaryPath);
                }

                binaryVersion = GetFileVersion(binaryPath);
            }

            /*string edgeDrivers = ConfigurationManager.AppSettings["EDGE_DRIVERS"];
            string firefoxDrivers = ConfigurationManager.AppSettings["FIREFOX_DRIVERS"];
            string chromeDrivers2 = ConfigurationManager.AppSettings["CHROME_DRIVERS_2"];*/
            string edgeDrivers = "https://msedgedriver.azureedge.net";
            string firefoxDrivers = "https://github.com/mozilla/geckodriver/releases/download";
            string firefoxDrivers2 = "https://github.com/mozilla/geckodriver/releases/download";
            string chromeDrivers2 = "https://chromedriver.storage.googleapis.com/chrome-for-testing";

            try {
                // Split the version string by '.'
                var versionParts = version.Split('.');
                AULogger.Info($"Binary To Be Downloaded: {browser.ToUpper()} {binaryVersion}");
                string driverPlatform = "win64";
                Version curDriverVer = new Version(binaryVersion);

                if (browser != "firefox")
                {
                    AULogger.Info($"Driver To Be Downloaded: {browser.ToUpper()} {curDriverVer.Major}");
                }
                Version nonChromeDriverVer = new Version("115.0.5762.4");

                string binaryPath = $@"{browsersPath}\{browser}-{version}\{driverDict[browser]}";
                bool driverExists = DriverExistsInRoot(binaryPath);
                if (!driverExists)
                {
                    AULogger.Info("Driver not installed.");
                    if (browser == "chrome" && curDriverVer > nonChromeDriverVer)
                    {
                        string chromeDriverJson = ChromeDriverURLDownload(binaryVersion, browser).Result;
                        string driverUrl = ChromeDriverDownload(chromeDriverJson, driverPlatform);
                        AULogger.Info($"Driver URL: {driverUrl}");
                        DownloadAsync(driverUrl, "driver.zip").GetAwaiter().GetResult(); // downloads the .zip file under temp_files
                } else {
                    string driverUrl = "";

                    if (browser == "edge") {
                        driverUrl = $"{edgeDrivers}/{binaryVersion}/edgedriver_win64.zip";

                    } else if (browser == "firefox") {
                        // For Firefox the driver version and binary version numbers are completely different so have to check compatible driver
                        // taken from https://firefox-source-docs.mozilla.org/testing/geckodriver/Support.html
                        // Parse the major version
                        int major = int.Parse(versionParts[0]);

                        // Check the major version to choose the compatible driver version
                        if (major >= 116) {
                            driverUrl = $"{firefoxDrivers2}/v0.35.0/geckodriver-v0.35.0-win64.zip";
                            AULogger.Info($"Driver To Be Downloaded: {browser.ToUpper()} v0.35.0");
                        }
                        else if (major >= 103 && major < 116)
                        {
                            driverUrl = $"{firefoxDrivers2}/v0.33.0/geckodriver-v0.33.0-win64.zip";
                            AULogger.Info($"Driver To Be Downloaded: {browser.ToUpper()} v0.33.0");
                        }
                        else if (major >= 92 && major < 103)
                        {
                            driverUrl = $"{firefoxDrivers2}/v0.31.0/geckodriver-v0.31.0-win64.zip";
                            AULogger.Info($"Driver To Be Downloaded: {browser.ToUpper()} v0.31.0");
                        }

                    } else if (browser == "chrome") {
                        string latestMinorVer = $@"{chromeDrivers2}/LATEST_RELEASE_{version}";
                        bool successful = CheckUrlSucces(latestMinorVer).Result;
                        DownloadAsync(latestMinorVer, "oldChromeVer.json").GetAwaiter().GetResult();
                        string driverVersion = ReadJson($@"{temp_files}\oldChromeVer.json").Result;
                        AULogger.Info("Latest Minor Driver Version: " + driverVersion);
                        driverUrl = $"{chromeDrivers2}/{driverVersion}/chromedriver_win32.zip";
                    }

                        AULogger.Info("Driver URL: " + driverUrl);
                        DownloadAsync(driverUrl, "driver.zip").GetAwaiter().GetResult();
                    }

                    string zipPath = exeLocation + @"\temp_files\driver.zip";
                    string extractPath = exeLocation + @"\temp_files";

                    AULogger.Info("Extracting driver .zip files.");

                    // extract files
                    ExtractZipFile(zipPath, extractPath);

                    string driverStart = "";

                    if (browser == "chrome" && curDriverVer > nonChromeDriverVer)
                    {
                        driverStart = exeLocation + $@"\temp_files\chromedriver-win64\{driverDict[browser]}";
                    }
                    else if (browser == "firefox")
                    {
                        /*driverStart = $@"{browsersPath}\{browser}-{versionParts[0]}\{driverDict[browser]}"; if doing the firefox-99 method instead of firefox-99.0 */ 
                        driverStart = exeLocation + $@"\temp_files\{driverDict[browser]}";
                    }
                    else
                    {
                        driverStart = exeLocation + $@"\temp_files\{driverDict[browser]}";
                    }

                    Console.WriteLine("Copying from" + driverStart);

                    string driverDest = exeLocation + $@"\win-x64\{driverDict[browser]}";
                    string driverDest2 = exeLocation + $@"\win-x64\";
                    string backupPath = exeLocation + @"\temp_files\backup.exe";

                    AULogger.Info("Copying the driver to its respective binary.");

                    if (browser == "firefox") // The firefox version is saved in a folder in automation/browsers with the full version e.g firefox-99.0.1
                    {
                        /*File.Copy($@"{browsersPath}\{browser}-{versionParts[0]}\{driverDict[browser]}", driverDest2, true);*/
                        /*File.Copy(driverStart, $@"{browsersPath}\{browser}-{versionParts[0]}\{driverDict[browser]}", true);*/
                        File.Copy(driverStart, $@"{browsersPath}\{browser}-{version}\{driverDict[browser]}", true);
                    }
                    else
                    {
                        File.Copy(driverStart, $@"{browsersPath}\{browser}-{version.Split('.')[0]}\{driverDict[browser]}", true);
                    }

                    if (agentHome != "") {
                        AULogger.Info("Copying the driver to the TAP folder.");
                        File.Copy(driverStart, $@"{agentHome}\TAP\{driverDict[browser]}", true);
                    } else {
                        AULogger.Info("Local machine detected.");
                    }

                } else {
                    AULogger.Info("Driver currently installed at " + binaryPath);
                }

                Directory.Delete(temp_files, true);
            }
            catch (Exception e)
            {
                Directory.Delete(temp_files, true);
                AULogger.Error(e.Message);
                throw;
            }
        }

        private static bool DriverExistsInRoot(string driverPath)
        {
            if (File.Exists(driverPath))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the path to the system's chrome.exe
        /// </summary>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        private static string GetAppRegistryPath(string exeFile)
        {
            string chromPath = string.Empty;

            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\{exeFile}"))
                {
                    if (key != null)
                    {
                        chromPath = key.GetValue("") as string;
                    }
                }
            }
            catch (Exception e)
            {
                AULogger.Error(e.Message);
            }

            return chromPath;
        }

        /// <summary>
        /// Reads the json and returns as a string.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private async static Task<string> ReadJson(string location)
        {
            return await File.ReadAllTextAsync(location);
        }

        /// <summary>
        /// Copies the contents from source dir over to dest dir
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        private static void CopyDirContents(string source, string dest)
        {
            DirectoryInfo dir = new DirectoryInfo(source);

            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }

            foreach (string filePath in Directory.GetFiles(source))
            {
                string destPath = Path.Combine(dest, Path.GetFileName(filePath));
                File.Copy(filePath, destPath, true);
            }

            foreach (string dirPath in Directory.GetDirectories(source))
            {
                string destDirPath = Path.Combine(dest, Path.GetFileName(dirPath));
                CopyDirContents(dirPath, destDirPath);
            }

        }

        /// <summary>
        /// Searches for the binary of that version.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="browser"></param>
        /// <param name="browsersPath"></param>
        /// <returns>The path to the binary with the same major version.</returns>
        private static string GetExeFromVersion(string version, string exeFile, string browsersPath)
        {
            Version userVer;

            // Split the version string by '.' and parse the components
            var versionParts = version.Split('.');

            // Parse the major version (first part)
            int major = int.Parse(versionParts[0]);

            // Parse the minor, build, and revision numbers if they exist
            int minor = versionParts.Length > 1 ? int.Parse(versionParts[1]) : 0;
            int build = versionParts.Length > 2 ? int.Parse(versionParts[2]) : 0;
            int revision = versionParts.Length > 3 ? int.Parse(versionParts[3]) : 0;

            // Create the Version object
            userVer = new Version(major, minor, build, revision);

            try
            {
                foreach (string file in Directory.GetFiles(browsersPath, exeFile, SearchOption.AllDirectories))
                {
                    string fileVersion = GetFileVersion(file);
                    if (Version.TryParse(fileVersion, out Version currentVersion))
                    {
                        // Check for Firefox or Chrome in exeFile
                        if (exeFile.Contains("firefox", StringComparison.OrdinalIgnoreCase))
                        {
                            // Compare full versions for Firefox because it requires full builds to be in different folders and currently formats supported are 99.0 and 99.0.1
                            // for e.g 99.0 and 99.0.1 would be different folders 
                            // Compare major and minor versions if version is of the format 99.0
                            if (versionParts.Length == 2 && currentVersion.Major == userVer.Major && currentVersion.Minor == userVer.Minor)
                            {
                                return file;
                            }
                            // Compare major, minor, and build if the version is of the format 99.0.1
                            if (versionParts.Length == 3 && currentVersion.Major == userVer.Major && currentVersion.Minor == userVer.Minor && currentVersion.Build == userVer.Build)
                            {
                                return file;
                            }
                        }
                        else if (exeFile.Contains("chrome", StringComparison.OrdinalIgnoreCase))
                        {
                            // Compare major versions for Chrome because it has download async functionality so full builds are placed in major build folders
                            // for e.g 114.0.5735.133 would be placed in chrome-114
                            if (currentVersion.Major == userVer.Major)
                            {
                                return file;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AULogger.Error(ex.Message);
            }

            return null;
        }


        /// <summary>
        /// Parses the cmd line args
        /// </summary>
        /// <param name="args"></param>
        /// <returns>A dict where the key is --{key} and the value is after the key</returns>
        private static Dictionary<string, string> ParseArguments(string[] args)
        {
            var arguments = new Dictionary<string, string>();

            if (args.Length == 0)
            {
                return arguments;
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--"))
                {
                    var key = args[i].Substring(2);
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                    {
                        var value = args[i + 1];
                        arguments[key] = value;
                    }
                }
            }

            return arguments;
        }

        /// <summary>
        /// From the starting path, find the binary with the latest version.
        /// </summary>
        /// <param name="browsersPath"></param>
        /// <param name="browser"></param>
        /// <returns>The binary file path.</returns>
        private static string FindLatestLocalBinary(string browsersPath, string browser)
        {
            string latestFilePath = null;
            Version latestVersion = new Version(0, 0, 0, 0);
            string browserExe = "";
            browser = browser.ToLower();

            if (browser == "chrome")
            {
                browserExe = "chrome.exe";
            }
            else if (browser == "edge")
            {
                browserExe = "msedge.exe";
            }
            else if (browser == "firefox")
            {
                browserExe = "firefox.exe";
            }

            try
            {
                foreach (string file in Directory.GetFiles(browsersPath, browserExe, SearchOption.AllDirectories))
                {
                    string fileVersion = GetFileVersion(file);
                    if (Version.TryParse(fileVersion, out Version currentVersion))
                    {
                        if (currentVersion > latestVersion)
                        {
                            latestVersion = currentVersion;
                            latestFilePath = file;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AULogger.Error(ex.Message);
            }

            return latestFilePath;
        }

        /// <summary>
        /// Goes through the JSON of a specified driver version and searches for the appropriate platform.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="platform"></param>
        /// <returns>The .zip url.</returns>
        private static string ChromeDriverDownload(string json, string platform)
        {
            string result = string.Empty;

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("downloads", out JsonElement downloads) &&
                        downloads.TryGetProperty("chromedriver", out JsonElement chromeDrivers))
                    {
                        foreach (JsonElement chromeDriver in chromeDrivers.EnumerateArray())
                        {
                            if (chromeDriver.GetProperty("platform").GetString() == platform)
                            {
                                return chromeDriver.GetProperty("url").GetString();
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                AULogger.Error(e.Message);
                throw;
            }

            return null;
        }

        /// <summary>
        /// Takes a json and searches for the binary url.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="platform"></param>
        /// <returns>The url to the binary version</returns>
        private static string BinaryDownloadJson(string json, string platform)
        {
            string result = string.Empty;

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("downloads", out JsonElement downloads) &&
                        downloads.TryGetProperty("chrome", out JsonElement chromeDrivers))
                    {
                        foreach (JsonElement chromeDriver in chromeDrivers.EnumerateArray())
                        {
                            if (chromeDriver.GetProperty("platform").GetString() == platform)
                            {
                                return chromeDriver.GetProperty("url").GetString();
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                AULogger.Error(e.Message);
                throw;
            }

            return null;
        }

        /// <summary>
        /// Navigates to the URL and downloads it under temp_files/fileName.
        /// </summary>
        /// <param name="url"></param>
        private static async Task<bool> DownloadAsync(string url, string fileName)
        {
            string exeLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string zipPath = Path.Combine(exeLocation, "temp_files", fileName);
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(10);
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();
                    using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }
                    AULogger.Info("Download completed.");
                    return true;
                }
                catch (TaskCanceledException)
                {
                    AULogger.Error($"Download timed out.");
                    return false;
                }
                catch (Exception e)
                {
                    AULogger.Error($"{e.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Extracts the .zip file into an extract path
        /// </summary>
        /// <param name="zipPath"></param>
        /// <param name="extractPath"></param>
        public static void ExtractZipFile(string zipPath, string extractPath)
        {
            try
            {
                ZipFile.ExtractToDirectory(zipPath, extractPath);
            }
            catch (Exception e)
            {
                AULogger.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Downloads the json file as driverUrl.json and returns the json text.
        /// </summary>
        /// <param name="ver"></param>
        /// <returns></returns>
        private static async Task<string> ChromeDriverURLDownload(string ver, string browser) {
            /*string chromeDrivers = ConfigurationManager.AppSettings["CHROME_DRIVERS"];*/
            string chromeDrivers = "https://googlechromelabs.github.io/chrome-for-testing";
            string jsonUrl= @$"{chromeDrivers}/{ver}.json";
            string exeLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (!CheckUrlSucces(jsonUrl).Result)
            {
                AULogger.Warn($"Driver v{ver} does not exist.");

                string tempVer = string.Join(".", ver.Split('.').Take(3));
                ver = DecrementLastDigit(tempVer) + ".0";
                AULogger.Info($"Installed Driver Version: {ver}.");
                jsonUrl = $"{chromeDrivers}/{ver}.json";
            }
            string path = exeLocation + @"\temp_files\driverUrl.json";
            string jsonFilePath = await GetChromeDriverVersion(jsonUrl, path);
            string jsonContents = await File.ReadAllTextAsync(jsonFilePath);

            return jsonContents;
        }

        /// <summary>
        /// Used to update the version in case the JSON url doesn't exist.
        /// </summary>
        /// <param name="ver"></param>
        /// <returns>Updated version.</returns>
        private static string DecrementLastDigit(string ver)
        {
            string[] parts = ver.Split('.');
            int lastPart = int.Parse(parts[parts.Length - 1]);
            lastPart--;
            parts[parts.Length - 1] = lastPart.ToString();
            return string.Join('.', parts);
        }

        /// <summary>
        /// Checks if a URL is succeeds.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>True or False.</returns>
        private static async Task<bool> CheckUrlSucces(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    return response.IsSuccessStatusCode;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Goes to the URL, converts the json as a string and stores in in the download path.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="downloadPath"></param>
        /// <returns>The download path.</returns>
        private static async Task<string> GetChromeDriverVersion(string url, string downloadPath)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    string jsonString = await response.Content.ReadAsStringAsync();
                    await File.WriteAllTextAsync(downloadPath, jsonString);

                    return downloadPath;
                }
            }
            catch (Exception e)
            {
                AULogger.Error(e.Message);
                AULogger.Info("URL: " + url);
                AULogger.Info("Download Path: " + downloadPath);
                throw;
            }
        }

        /// <summary>
        /// Gets the product version of the file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>The file's product version.</returns>
        private static string GetFileVersion(string filePath)
        {
            try
            {
                FileVersionInfo fileInfo = FileVersionInfo.GetVersionInfo(filePath);
                return fileInfo.ProductVersion;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Starts in the given directory and searches for the given file name.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        /// <returns>The path to the file.</returns>
        public static string FindBinaryFilePath(string directory, string fileName)
        {
            try
            {
                foreach (string file in Directory.GetFiles(directory, fileName))
                {
                    return file;
                }

                foreach (string dir in Directory.GetDirectories(directory))
                {
                    string foundPath = FindBinaryFilePath(dir, fileName);

                    if (foundPath != null)
                    {
                        return foundPath;
                    }
                }
            }
            catch (Exception ex)
            {
                AULogger.Error($"{ex.Message}");
            }

            return null;
        }

        /*private static void InstallFirefox(string installerPath)
        {
            try
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = installerPath,
                        Arguments = "/silent -P \"newProfileName\" -no-remote",
                        UseShellExecute = true, // Use this to run the installer with elevated privileges
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                AULogger.Info($"Running installer: {installerPath}");
                process.WaitForExit(); // Wait for the installation to complete
            }
            catch (Exception ex)
            {
                AULogger.Error($"Error running installer: {ex}");
            }
        }*/

        /// <summary>
        /// Uses the exe/msi file for the firefox installer downloaded in temp_files to install the binary of the specified version.
        /// Automatically puts the binary into a created folder in Automation/Browsers
        /// </summary>
        /// <param name = "exePath" ></ param >
        /// <param name="targetDirectory"></param>
        /// <param name="browserVer"></param>
        /// <returns>If succussfully installed</returns>
        private static void InstallFirefox(string exePath, string targetDirectory, string browserVer)
        {
            try
            {
                /*string installerPath = Path.Combine(exePath, $"FirefoxSetup_{browserVer}.exe");*/

                // Where the downloaded exe file is.
                string installerPath = Path.Combine(exePath);

                // Where you want the installed binary to be placed.
                string installPath = Path.Combine(targetDirectory, $"firefox-{browserVer}");
                if (!Directory.Exists(installPath))
                {
                    Directory.CreateDirectory(installPath);
                }

                // Step 1: Install Firefox silently to the custom folder
                AULogger.Info($"Installing Firefox {browserVer}");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = installerPath,
                        Arguments = $"/S /D={installPath}",
                        UseShellExecute = false, // run with elevated priveleges 
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    }
                };

                process.OutputDataReceived += (sender, args) => AULogger.Info(args.Data);
                process.ErrorDataReceived += (sender, args) => AULogger.Error(args.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    AULogger.Info($"Firefox {browserVer} installed to: {installPath}");

                    // Step 2: Create a custom profile for this version
                    string profileName = $"Firefox_{browserVer}_profile";
                    var profileProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = Path.Combine(installPath, "firefox.exe"),
                            Arguments = $"-CreateProfile {profileName}",
                            UseShellExecute = true,
                            CreateNoWindow = true,
                        }
                    };
                    profileProcess.Start();
                    profileProcess.WaitForExit();

                    // Step 3: Disable auto-updates via policies.json
                    string distributionFolder = Path.Combine(installPath, "distribution");
                    Directory.CreateDirectory(distributionFolder);

                    var policiesJson = new
                    {
                        policies = new
                        {
                            DisableAppUpdate = true
                        }
                    };

                    string jsonContent = JsonSerializer.Serialize(policiesJson, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(Path.Combine(distributionFolder, "policies.json"), jsonContent);

                    AULogger.Info($"Firefox {browserVer} installed and configured.");
                }
                else
                {
                    AULogger.Error($"Error installing Firefox. Exit code: {process.ExitCode}");
                }
            }
            catch (Exception ex)
            {
                AULogger.Error($"Error during installation: {ex.Message}");
            }
        }
    }
}