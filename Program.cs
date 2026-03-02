using LC360.Components;
using BlazorStrap;
using Supabase;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// ── Razor + Blazor + Controllers ─────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddBlazorStrap();
builder.Services.AddControllers();

// ── Supabase Client ───────────────────────────────────────────────────────
var supabaseUrl = builder.Configuration["Supabase:Url"]!;
var supabaseKey = builder.Configuration["Supabase:SecretKey"]!;

builder.Services.AddScoped<Supabase.Client>(_ =>
    new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
    {
        AutoRefreshToken = true,
        AutoConnectRealtime = true
    })
);

// ── JWT Authentication ────────────────────────────────────────────────────
var jwtSecret = builder.Configuration["Jwt:Secret"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// ── HTTP Client (for alert notifications) ────────────────────────────────
builder.Services.AddHttpClient();

// ── App Services ──────────────────────────────────────────────────────────
builder.Services.AddSingleton<LC360.Services.IAuthService, LC360.Services.AuthService>();

// ── Memory Cache (for TRL caching) ───────────────────────────────────────
builder.Services.AddMemoryCache();

// ── Supabase Service ──────────────────────────────────────────────────────
builder.Services.AddScoped<LC360.Services.SupabaseService>();

// ── Build ─────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseMiddleware<LC360.Components.RateLimitingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// ── Prometheus Metrics Endpoint ───────────────────────────────────────────
app.UseMetricServer();
app.UseHttpMetrics();

// ── Static + Razor + API Controllers ────────────────────────────────────
app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();