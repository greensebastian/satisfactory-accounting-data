using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SatisfactoryAccountingData.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient<SatisfactoryApiClient>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<SatisfactoryApiClient>();

await builder.Build().RunAsync();
