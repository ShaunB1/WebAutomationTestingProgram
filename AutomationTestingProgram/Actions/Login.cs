using AutomationTestingProgram.Actions;
using AutomationTestingProgram.Core;
using AutomationTestingProgram.Modules.TestRunnerModule;
using AutomationTestingProgram.Modules.TestRunnerModule.Services.Playwright.Objects;
using Microsoft.Graph.Reports.GetPrinterArchivedPrintJobsWithPrinterIdWithStartDateTimeWithEndDateTime;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class Login : WebAction
{
    private readonly PasswordResetService _passwordResetService;
    private readonly AzureKeyVaultService _azureKeyVaultService;
    private readonly CSVEnvironmentGetter _csvEnvironmentGetter;

    public Login(PasswordResetService passwordResetService, AzureKeyVaultService azureKeyVaultService, CSVEnvironmentGetter cSVEnvironmentGetter)
    {
        _azureKeyVaultService = azureKeyVaultService;
        _passwordResetService = passwordResetService;
        _csvEnvironmentGetter = cSVEnvironmentGetter;
    }
    
    public override async Task ExecuteAsync(Page pageObject,
        string groupID,
        TestStep step,
        Dictionary<string, string> envVars,
        Dictionary<string, string> saveParams)
    {

        Func<LogLevel, string, Task> Log = pageObject.Log;


        string username = step.Object;
        string password = step.Value;

        // If password is blank or commented, attempt to get from Azure Key Vault
        bool isHardcodedPassword = password != string.Empty && !password.StartsWith("##");
        if (!isHardcodedPassword)
        {
            password = await _azureKeyVaultService.GetKvSecret(Log, username);
        }

        try
        {
            string environment;
            if (envVars.ContainsKey("environment"))
            {
                environment = envVars["environment"];
                await pageObject.LogInfo($"Testing in environment: {environment}");
            }
            else
            {
                throw new Exception($"Login failed - Could not find environment key in environment variable dictionary");
            }

            string url;
            IPage page;

            if (step.Object.Contains("ontario.ca"))
            {
                await pageObject.LogInfo("Login via AAD.");
                url = _csvEnvironmentGetter.GetAdURL(environment);
                try
                {
                    await pageObject.RefreshAsync(url);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to navigate to url {url}: {ex.Message}");
                }

                page = pageObject.Instance!;

                ILocator usernameInput = page.Locator("//*[@id=\"i0116\"]");
                ILocator passwordInput = page.Locator("//*[@id=\"i0118\"]");
                ILocator nextButton = page.Locator("//*[@id=\"idSIButton9\"]");
                ILocator signInButton = page.Locator("//*[@id=\"idSIButton9\"]");

                await usernameInput.FillAsync(username);
                await passwordInput.FillAsync(password);
                await nextButton.ClickAsync();
                await signInButton.ClickAsync();
            }
            else if (step.Object.Contains("ontarioemail.ca"))
            {
                await pageObject.LogInfo("Login via OPS BPS.");
                url = _csvEnvironmentGetter.GetOpsBpsURL(environment);
                try
                {
                    await pageObject.RefreshAsync(url);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to navigate to url {url}: {ex.Message}");
                }

                page = pageObject.Instance!;

                ILocator usernameInput = page.Locator("//*[@id=\"username\"]");
                ILocator passwordInput = page.Locator("//*[@id=\"password\"]");
                ILocator signInButton = page.Locator("//*[@title=\"Select to Sign In to your account\"]");

                await usernameInput.FillAsync(username);
                await passwordInput.FillAsync(password);
                await signInButton.ClickAsync();

                ILocator invalidPassword = page.Locator("//*[contains(text(), 'incorrect email address or password')]");
                bool isInvalid = await invalidPassword.IsVisibleAsync();
                ILocator expiredPassword = page.Locator("//*[contains(text(), 'Your password has expired')]");
                bool isExpired = await expiredPassword.IsVisibleAsync();
                ILocator lockedAccount = page.Locator("//*[contains(text(), 'your account is locked')]");
                bool isLocked = await expiredPassword.IsVisibleAsync();

                if (isInvalid || isExpired || isLocked)
                {
                    // If password triggered any of the above, then called PasswordResetService
                    
                    try
                    {
                        await _passwordResetService.ResetPassword(Log, username);
                    }
                    catch (PasswordAlreadyResetException)
                    {
                        await pageObject.LogInfo("Password already reset today. Refetching..");
                    }

                    // The new password is updated in Azure Key Vault, fetch it
                    password = await _azureKeyVaultService.GetKvSecret(Log, username);

                    try
                    {
                        await pageObject.RefreshAsync(url);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to navigate to url {url}: {ex.Message}");
                    }

                    await usernameInput.FillAsync(username);
                    await passwordInput.FillAsync(password);
                    await signInButton.ClickAsync();
                }
            }
            else
            {
                throw new Exception($"Invalid email: {step.Object}");
            }

            // Hard wait for now, but we need to implement function that detects loading spinner completion
            await pageObject.LogInfo("HARD WAIT FOR LOADING SPINNER - 30 Seconds");
            Task.Delay(30000).Wait();
        }
        catch (Exception)
        {
            throw;
        }
    }
}