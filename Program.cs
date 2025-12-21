using LC360.Components;
using BlazorStrap; // Make sure this is present (or BlazorStrap.V5 if you installed that specific package)

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
// This registers the service AND creates the HttpClient for it automatically
builder.Services.AddHttpClient<LC360.Services.IAuthService, LC360.Services.AuthService>(client =>
{
    // OPTIONAL: Set the base URL for your API if you know it
     client.BaseAddress = new Uri("https://localhost:7000/"); 
});
// ADD THE BLRAZORSTRAP SERVICE HERE (This is the fix)
builder.Services.AddBlazorStrap();
// -------------------------------------------------------------------



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();