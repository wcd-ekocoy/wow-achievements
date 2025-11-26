using Microsoft.AspNetCore.Authentication.Cookies;
using AspNet.Security.OAuth.BattleNet;
using WowAchievementsApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<BlizzardService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "BattleNet";
})
.AddCookie()
.AddBattleNet(options =>
{
    options.ClientId = builder.Configuration["Blizzard:ClientId"] ?? throw new InvalidOperationException("Blizzard:ClientId is not configured.");
    options.ClientSecret = builder.Configuration["Blizzard:ClientSecret"] ?? throw new InvalidOperationException("Blizzard:ClientSecret is not configured.");
    options.SaveTokens = true;
    options.Scope.Add("wow.profile");
    options.Events.OnRedirectToAuthorizationEndpoint = context =>
    {
        if (context.Properties.Items.TryGetValue("region", out var region) &&
            Enum.TryParse<BattleNetAuthenticationRegion>(region, true, out var battleNetRegion))
        {
            options.Region = battleNetRegion;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// HTTPS redirection not needed since we only listen on HTTPS
app.UseStaticFiles(); // Ensure static files are served
app.UseRouting();

app.UseAuthentication(); // Must be before UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
