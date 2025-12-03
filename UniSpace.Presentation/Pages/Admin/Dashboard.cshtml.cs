using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly ICampusService _campusService;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(ICampusService campusService, ILogger<DashboardModel> logger)
        {
            _campusService = campusService;
            _logger = logger;
        }

        public int TotalCampuses { get; set; }
        public int TotalRooms { get; set; }
        public int TotalBookings { get; set; }
        public int PendingReports { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var campuses = await _campusService.GetCampusesAsync();
                TotalCampuses = campuses.Count;
                TotalRooms = campuses.Sum(c => c.TotalRooms);

                // TODO: Get from other services when implemented
                TotalBookings = 0;
                PendingReports = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                TotalCampuses = 0;
                TotalRooms = 0;
                TotalBookings = 0;
                PendingReports = 0;
            }
        }
    }
}
