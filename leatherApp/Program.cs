using Microsoft.EntityFrameworkCore;
using leatherApp.Components;
using leatherApp.Data;
using MudBlazor.Services;
using leatherApp.Models;
using Auth0.AspNetCore.Authentication;
using Resend;

var builder = WebApplication.CreateBuilder(args);

// Add database context
builder.Services.AddDbContextFactory<LeatherShopContext>(options =>
    options.UseNpgsql("Host=localhost;Database=leathershop_db;Username=postgres;Password=postgres"));

builder.Services.AddScoped<LeatherShopContext>(p =>
    p.GetRequiredService<IDbContextFactory<LeatherShopContext>>().CreateDbContext());

builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<BreadcrumbService>();

// Add Auth0 authentication
builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"]!;
    options.ClientId = builder.Configuration["Auth0:ClientId"]!;
    options.ClientSecret = builder.Configuration["Auth0:ClientSecret"]!;
    options.Scope = "openid profile email";
    options.OpenIdConnectEvents = new Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents
    {
        OnRemoteFailure = context =>
        {
            // User declined consent or login failed — send them home instead of showing an error
            context.Response.Redirect("/");
            context.HandleResponse();
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            
            var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
            var roleClaims = context.Principal?
                .FindAll("https://leathershop.example.com/roles")
                .ToList();
            if (identity != null && roleClaims != null)
            {
                foreach (var role in roleClaims)
                {
                    identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role.Value));
                }
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddRazorPages();
builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken = builder.Configuration["Resend:ApiToken"]!;
});
builder.Services.AddTransient<IResend, ResendClient>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomCenter;
});
builder.Services.AddLocalization();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Seed the database with sample data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LeatherShopContext>();
    DbSeeder.Seed(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

var supportedCultures = new[] { "sv-SE", "en-US" };
var localizationOptions = new Microsoft.AspNetCore.Builder.RequestLocalizationOptions()
    .SetDefaultCulture("sv-SE")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapRazorPages();

app.Run();