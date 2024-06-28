using FetWaveWWW.Data;
using FetWaveWWW.Data.DTOs.Events;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Caching.Memory;

namespace FetWaveWWW.Services
{
    public class EventsService
    {
        private readonly IMemoryCache _cache;
        private readonly FetWaveWWWContext _context;

        public EventsService(IMemoryCache cache, FetWaveWWWContext context)
        {
            _cache = cache;
            _context = context;
        }

        private async Task<IList<Region>?> GetCachedRegions()
            => await _cache.GetOrCreateAsync("EventRegions",async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(12);
                return await _context.Regions.ToListAsync();
            });

        private async Task<IList<DressCode>?> GetCachedDressCodes()
            => await _cache.GetOrCreateAsync("EventDressCodes", async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(12);
                return await _context.DressCodes.ToListAsync();
            });

        private async Task<IList<Category>?> GetCachedCategories()
            => await _cache.GetOrCreateAsync("EventCategories", async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(12);
                return await _context.Categories.ToListAsync();
            });

        private async Task<IList<CalendarEvent>?> GetCachedEventsForRegion(DateTime startTime, DateTime endTime, int regionId)
            => (await _cache.GetOrCreateAsync($"Events:{regionId}", async entry =>
            {
                entry.AbsoluteExpiration = DateTime.UtcNow.AddMinutes(5);
                //Cache events from one month in the past until one year in the future
                return await _context.Events.Where(e => e.EndDate > DateTime.UtcNow.AddMonths(-1) && e.StartDate < DateTime.UtcNow.AddYears(1) && e.RegionId == regionId).ToListAsync();
            }) ?? []).Where(e => e.StartDate >= startTime && e.StartDate <= endTime).ToList();

        public async Task<IEnumerable<Region>?> GetRegions()
            =>  await GetCachedRegions();

        public async Task<IEnumerable<Region>?> GetRegions(string? stateCode = null, string? name = null)
            => (await GetCachedRegions())?.Where(r => 
            (stateCode == null || (r.StateCode?.Equals(stateCode, StringComparison.OrdinalIgnoreCase) ?? false)) 
            && (name == null || (r.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) ?? false)));

        public async Task<IEnumerable<DressCode>?> GetDressCodes()
            => await GetCachedDressCodes();

        public async Task<IEnumerable<Category>?> GetCategories()
            => await GetCachedCategories();

        public async Task<CalendarEvent?> GetEventById(int? id = null, Guid? guid = null)
            => await _context.Events.FirstOrDefaultAsync(e => e.Id == id || e.Unique_Id == guid);

        public async Task<IEnumerable<CalendarEvent>?> GetEventsForRegion(DateTime startTime, DateTime endTime, int regionId)
            => await GetCachedEventsForRegion(startTime, endTime, regionId);

        public async Task<IEnumerable<CalendarEvent>?> GetEventsForState(DateTime startTime, DateTime endTime, string stateCode)
            => (await Task.WhenAll((await GetRegions(stateCode: stateCode))?.Select(async r => await GetEventsForRegion(startTime, endTime, r.Id)) ?? []))?.SelectMany(e => e);
    }
}
