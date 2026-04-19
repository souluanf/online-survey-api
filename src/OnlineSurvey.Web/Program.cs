using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using OnlineSurvey.Web;
using OnlineSurvey.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Use relative URL when served by nginx (Docker), or configured URL for local dev
var apiBaseAddress = builder.Configuration["ApiBaseAddress"];
if (string.IsNullOrEmpty(apiBaseAddress))
{
    // When no config, use the current origin (works with nginx proxy)
    apiBaseAddress = builder.HostEnvironment.BaseAddress;
}

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseAddress) });
builder.Services.AddScoped<ISurveyApiService, SurveyApiService>();
builder.Services.AddScoped<FirebaseAuthService>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();