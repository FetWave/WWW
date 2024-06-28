using FetWaveWWW.Data.DTOs.Events;
using FetWaveWWW.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace FetWaveWWW.Pages.Events
{
    [Authorize]
    public partial class EventList : ComponentBase
    {
#nullable disable
        [Inject]
        public EventsService Events { get; set; }
        [Inject]
        public IConfiguration Configuration { get; set; }
        [Inject]
        private NavigationManager navigation { get; set; }
#nullable enable

        protected override async Task OnInitializedAsync()
        {
            Regions = await Events.GetRegions();
            var statesList = new List<string>()
            {
                "Online"
            };
            statesList.AddRange(
                Regions
                ?.Where(r => !r.StateCode?.Equals("Online", StringComparison.OrdinalIgnoreCase) ?? false)
                .Select(r => r.StateCode)
                .Distinct()
                .Order()
                .Where(c => !string.IsNullOrEmpty(c))
                .Select(c => c!)
                ?? []);

            States = statesList;
        }

        protected override bool ShouldRender()
        {
            return shouldRender ?? true;
        }
        private bool? shouldRender { get; set; }


        private string? StateCode { get; set; }
        private int? RegionId { get; set; }

        private IEnumerable<Region>? regionsForState { get; set; }

        void StateOnChange(ChangeEventArgs e)
        {
            StateCode = e.Value!.ToString();
            GetRegionsForState();
        }
        async void RegionOnChange(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value!.ToString(), out var regionId))
            {
                RegionId = regionId;
            }
            else
            {
                RegionId = null;
            }
            await GetEventsForRegion();
        }

        private IEnumerable<Region>? GetRegionsForState()
            => regionsForState = StateCode == null
                ? Regions ?? []
                : Regions?.Where(r => r.StateCode?.Equals(StateCode, StringComparison.OrdinalIgnoreCase) ?? false) ?? [];

        private async Task<IEnumerable<CalendarEvent>?> GetEventsForRegion()
            => CalendarEvents = (RegionId ?? 0) > 0
            ? await Events.GetEventsForRegion(CalendarStartDate, CalendarEndDate, RegionId!.Value)
            : [];

        private IEnumerable<Region>? Regions { get; set; }
        private IEnumerable<string>? States { get; set; }
        private string? UserId { get; set; }



        private DateTime CalendarStartDate { get; set; } = DateTime.Now.AddDays(-2).Date;
        private DateTime CalendarEndDate { get; set; } = DateTime.Now.AddMonths(1).Date;

        private IEnumerable<CalendarEvent>? CalendarEvents { get; set; }
    }
}
