using AutomationTestingProgram.Models;

var builder = WebApplication.CreateBuilder(args); // builder used to configure services and middleware

builder.Services.AddControllersWithViews(); // enables use of MVC patterns
builder.Services.Configure<AzureDevOpsSettings>(builder.Configuration.GetSection("AzureDevops"));

var app = builder.Build(); // represents configured web app

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowFrontend",
//         policy => policy.WithOrigins("http://localhost:5173")
//             .AllowAnyMethod()
//             .AllowAnyHeader());
// });

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // (HTTP Strict Transport Security) browsers interact with server only over HTTPS
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection(); // client and server comms encrypted
app.UseStaticFiles(); // can request static assets for frontend
app.MapFallbackToFile("index.html"); // fallback to index.html for SPA routes
app.UseRouting(); // adds routing capabilities
app.UseAuthorization(); // only authorized users can access certain endpoints
// sets up route patterns for MVC controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();