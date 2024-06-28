using FetWaveWWW.Data.DTOs.Events;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FetWaveWWW.Data
{
    public class SeedDataService
    {
        private readonly FetWaveWWWContext _context;
        public SeedDataService(FetWaveWWWContext context)
        {
            _context = context;
        }

        public async Task SeedEventInfra()
        {
            await SeedCategories();
            await SeedDressCodes();
            await SeedRegions();
        }

        private async Task SeedCategories()
        {
            using var sr = new StreamReader("Data/SEED/Categories.json");
            var categories = JsonSerializer.Deserialize<IEnumerable<Category>>(await sr.ReadToEndAsync());

            var existingCats = _context.Categories.Select(c => c.Name).ToList();

            var newCats = categories?.Where(c => !existingCats.Any(e => e.Equals(c.Name, StringComparison.OrdinalIgnoreCase)));
            if (newCats?.Any() ?? false)
            {
                await _context.Categories.AddRangeAsync(newCats);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedDressCodes()
        {
            using var sr = new StreamReader("Data/SEED/DressCodes.json");
            var categories = JsonSerializer.Deserialize<IEnumerable<DressCode>>(await sr.ReadToEndAsync());

            var existingDCs = _context.DressCodes.Select(c => c.Name).ToList();

            var newDCs = categories?.Where(c => !existingDCs.Any(e => e.Equals(c.Name, StringComparison.OrdinalIgnoreCase)));
            if (newDCs?.Any() ?? false)
            {
                await _context.DressCodes.AddRangeAsync(newDCs);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedRegions()
        {
            using var sr = new StreamReader("Data/SEED/Regions.json");
            var categories = JsonSerializer.Deserialize<IEnumerable<Region>>(await sr.ReadToEndAsync());

            var existingRegions = _context.Regions.Select(c => new Tuple<string, string>(c.StateCode, c.Name)).ToList();

            var newRegions = categories?.Where(c => !existingRegions.Any(e =>
                e.Item1.Equals(c.StateCode, StringComparison.OrdinalIgnoreCase)
                && e.Item2.Equals(c.Name, StringComparison.OrdinalIgnoreCase)));

            if (newRegions?.Any() ?? false)
            {
                await _context.Regions.AddRangeAsync(newRegions);
                await _context.SaveChangesAsync();
            }
        }
    }
}
