using Microsoft.EntityFrameworkCore;
using FetWaveWWW.Services;
using Ixnas.AltchaNet;
using Microsoft.AspNetCore.Identity.UI.Services;
using FetWaveWWW.Data;
using Microsoft.AspNetCore.Identity;
using System.Runtime.InteropServices;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("FetWaveWWWContextConnection") ?? throw new InvalidOperationException("Connection string 'FetWaveWWWContextConnection' not found.");

builder.Services.AddDbContext<FetWaveWWWContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<FetWaveWWWContext>();


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMemoryCache();


builder.Services.AddSingleton<AltchaPageService>();
builder.Services.AddSingleton<IAltchaChallengeStore, AltchaCache>();

builder.Services.AddSingleton<GoogleServices>();
builder.Services.AddSingleton<IEmailSender, GoogleServices>();

builder.Services.AddScoped<SeedDataService>();

builder.Services.AddScoped<EventsService>();

builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    })
    .AddDiscord(discordOptions =>
    {
        discordOptions.ClientId = builder.Configuration["Authentication:Discord:ClientId"]!;
        discordOptions.ClientSecret = builder.Configuration["Authentication:Discord:ClientSecret"]!;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

//Data Seeding Scope
using var scope = app.Services.CreateScope();
var seedService = scope.ServiceProvider.GetService<SeedDataService>();
await seedService?.SeedEventInfra();

app.Run();
