using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly ICampusService _campusService;
        private readonly IScheduleService _scheduleService;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(
            ICampusService campusService,
            IScheduleService scheduleService,
            ILogger<DashboardModel> logger)
        {
            _campusService = campusService;
            _scheduleService = scheduleService;
            _logger = logger;
        }

        public int TotalCampuses { get; set; }
        public int TotalRooms { get; set; }
        public int TotalSchedules { get; set; }
        public int TotalBookings { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Get campuses data
                var campuses = await _campusService.GetCampusesAsync();
                TotalCampuses = campuses.Count;
                TotalRooms = campuses.Sum(c => c.TotalRooms);

                // Get schedules data
                var schedules = await _scheduleService.GetAllSchedulesAsync();
                TotalSchedules = schedules.Count;

                // TODO: Get from other services when implemented
                TotalBookings = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                TotalCampuses = 0;
                TotalRooms = 0;
                TotalSchedules = 0;
                TotalBookings = 0;
            }
        }
    }
}
