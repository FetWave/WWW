using FetWaveWWW.Data;
using FetWaveWWW.Services;
using Ixnas.AltchaNet;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Radzen;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("FetWaveWWWContextConnection") ?? throw new InvalidOperationException("Connection string 'FetWaveWWWContextConnection' not found.");

builder.Services.AddDbContext<FetWaveWWWContext>(
    options => options.UseSqlServer(connectionString),
    contextLifetime: ServiceLifetime.Transient,
    optionsLifetime: ServiceLifetime.Transient);

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<FetWaveWWWContext>();


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMemoryCache();

builder.Services.AddRadzenComponents();

builder.Services.AddSingleton<AltchaPageService>();
builder.Services.AddSingleton<IAltchaChallengeStore, AltchaCache>();

builder.Services.AddSingleton<GoogleService>();
builder.Services.AddSingleton<IEmailSender, GoogleService>();

builder.Services.AddScoped<SeedDataService>();

builder.Services.AddTransient<EventsService>();
builder.Services.AddTransient<MessagesService>();
builder.Services.AddTransient<ProfilesService>();
builder.Services.AddTransient<AuthHelperService>();

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

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

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
await seedService?.SeedProfileInfra();

app.Run();
