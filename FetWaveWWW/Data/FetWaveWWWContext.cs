using FetWaveWWW.Data.DTOs.Events;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace FetWaveWWW.Data;

public class FetWaveWWWContext : IdentityDbContext<IdentityUser>
{
    public FetWaveWWWContext(DbContextOptions<FetWaveWWWContext> options)
        : base(options) {}

    public DbSet<CalendarEvent> Events { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<DressCode> DressCodes { get; set; }
    public DbSet<Region> Regions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
