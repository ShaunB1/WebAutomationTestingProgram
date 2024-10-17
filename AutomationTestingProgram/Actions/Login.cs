using Microsoft.Playwright;

namespace AutomationTestingProgram.Actions;

public class Login : IWebAction
{
    public async Task<bool> ExecuteAsync(IPage page, TestStep step)
    {
        var username = step.Object;
        var password = step.Value;
        var usernameInput = page.Locator("//*[@id=\"i0116\"]");
        var passwordInput = page.Locator("//*[@id=\"i0118\"]");
        var nextButton = page.Locator("//*[@id=\"idSIButton9\"]");
        var signInButton = page.Locator("//*[@id=\"idSIButton9\"]");

        try
        {
            await usernameInput.FillAsync(username);
            await passwordInput.FillAsync(password);
            await nextButton.ClickAsync();
            await signInButton.ClickAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Something went wrong while logging in: {ex.Message}");
            return false;
        }
    }
}