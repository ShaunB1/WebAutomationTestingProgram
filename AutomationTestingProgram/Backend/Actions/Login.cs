using Microsoft.Playwright;

namespace AutomationTestingProgram.Backend.Actions;

public class Login : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStep step, int iteration)
    {
        var username = step.Object;
        var password = step.Value;

        try
        {
            ILocator usernameInput;
            ILocator passwordInput;
            ILocator nextButton;
            ILocator signInButton;

            if (step.Object.Contains("ontario.ca"))
            {
                Console.WriteLine("Login via AAD.");

                usernameInput = page.Locator("//*[@id=\"i0116\"]");
                passwordInput = page.Locator("//*[@id=\"i0118\"]");
                nextButton = page.Locator("//*[@id=\"idSIButton9\"]");
                signInButton = page.Locator("//*[@id=\"idSIButton9\"]");

                await usernameInput.FillAsync(username);
                await passwordInput.FillAsync(password);
                await nextButton.ClickAsync();
                await signInButton.ClickAsync();
            }

            else if (step.Object.Contains("ontarioemail.ca"))
            {
                Console.WriteLine("Login via OPS BPS.");

                usernameInput = page.Locator("//*[@id=\"username\"]");
                passwordInput = page.Locator("//*[@id=\"password\"]");
                signInButton = page.Locator("//*[@title=\"Select to Sign In to your account\"]");

                await usernameInput.FillAsync(username);
                await passwordInput.FillAsync(password);
                await signInButton.ClickAsync();
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