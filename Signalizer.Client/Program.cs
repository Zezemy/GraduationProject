using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Signalizer.Client.Components;
using Signalizer.Client.State_Management;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddMudServices();
// register the cookie handler
builder.Services.AddTransient<CookieHandler>();

// set up authorization
builder.Services.AddAuthorizationCore();

// register the custom state provider
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();

// register the account management interface
builder.Services.AddScoped(
    sp => (IAccountManagement)sp.GetRequiredService<AuthenticationStateProvider>());


// set base address for default host
builder.Services.AddSingleton(sp =>
    new HttpClient { BaseAddress = new Uri(builder.Configuration["FrontendUrl"] ?? "https://localhost:5002") });



// configure client for auth interactions
builder.Services.AddHttpClient(
    "Auth",
    opt => opt.BaseAddress = new Uri(builder.Configuration["BackendUrl"] ?? "https://localhost:5001"))
    .AddHttpMessageHandler<CookieHandler>();

builder.Services.AddHttpClient(
    "BackendService",
    opt => opt.BaseAddress = new Uri(builder.Configuration["BackendUrl"] ?? "https://localhost:5001"))
    .AddHttpMessageHandler<CookieHandler>();

builder.Services.AddSingleton<StrategyManagementState>();

await builder.Build().RunAsync();
