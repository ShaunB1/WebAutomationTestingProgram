using AutomationTestingProgram.Services;
using Microsoft.Graph.Reports.GetPrinterArchivedPrintJobsWithPrinterIdWithStartDateTimeWithEndDateTime;
using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class Login : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration, Dictionary<string, string> envVars, Dictionary<string, string> saveParams)
    {
        string username = step.Object;
        string password = step.Value;

        // If password is blank, attempt to get from Azure Key Vault
        if (password == string.Empty)
        {
            var result = await AzureKeyVaultService.GetKvSecret(username);
            if (result.success)
            {
                Console.WriteLine($"Password for {username} successfully fetched from Azure Key Vault, proceeding with Login");
                password = result.message;
            }
            else
            {
                Console.WriteLine("Login step failed - Password could not be fetched from Azure Key Vault");
                return false;
            }
        }

        try
        {
            string environment;
            if (envVars.ContainsKey("environment"))
            {
                environment = envVars["environment"];
                Console.WriteLine($"Testing in environment: {environment}");
            }
            else
            {
                Console.WriteLine($"Login failed - Could not find environment key in environment variable dictionary");
                return false;
            }

            string url;

            if (step.Object.Contains("ontario.ca"))
            {
                Console.WriteLine("Login via AAD.");
                url = CSVEnvironmentGetter.GetAdURL(environment);
                try
                {
                    await page.GotoAsync(url);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to navigate to url {url}: {ex.Message}");
                }

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
                Console.WriteLine("Login via OPS BPS.");
                url = CSVEnvironmentGetter.GetOpsBpsURL(environment);
                try
                {
                    await page.GotoAsync(url);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to navigate to url {url}: {ex.Message}");
                }

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
                    var result = await PasswordResetService.ResetPassword(username);
                    if (!result.success)
                    {
                        Console.WriteLine("Login failed - Attempted to reset password but failed");
                        return false;
                    }

                    // The new password is updated in Azure Key Vault, fetch it
                    Console.WriteLine($"Getting Azure Key Vault secret key for {secretName}");
                    result = await AzureKeyVaultService.GetKvSecret(username);
                    if (result.success)
                    {
                        Console.WriteLine($"Password for {username} successfully fetched from Azure Key Vault, proceeding with Login");
                        password = result.message;
                    }
                    else
                    {
                        Console.WriteLine("Login step failed - Password could not be fetched from Azure Key Vault");
                        return false;
                    }

                    try
                    {
                        await page.GotoAsync(url);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to navigate to url {url}: {ex.Message}");
                    }

                    await usernameInput.FillAsync(username);
                    await passwordInput.FillAsync(password);
                    await signInButton.ClickAsync();
                }
            }
            else
            {
                Console.WriteLine($"Invalid email: {step.Object}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Something went wrong while logging in: {ex.Message}");
            return false;
        }
    }
}