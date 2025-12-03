using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.ScheduleDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Schedule
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly IScheduleService _scheduleService;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(
            IScheduleService scheduleService,
            ILogger<DashboardModel> logger)
        {
            _scheduleService = scheduleService;
            _logger = logger;
        }

        // Statistics
        public int TotalSchedules { get; set; }
        public int AcademicCourses { get; set; }
        public int MaintenanceSchedules { get; set; }
        public int ActiveSchedules { get; set; }

        // Charts Data
        public Dictionary<string, int> SchedulesByDay { get; set; } = new();
        public Dictionary<string, int> SchedulesByType { get; set; } = new();
        public Dictionary<string, int> SchedulesByRoom { get; set; } = new();

        // Lists
        public List<ScheduleDto> RecentSchedules { get; set; } = new();
        public List<ScheduleDto> UpcomingSchedules { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                // Get all schedules
                var allSchedules = await _scheduleService.GetAllSchedulesAsync();

                // Calculate statistics
                TotalSchedules = allSchedules.Count;
                AcademicCourses = allSchedules.Count(s => s.ScheduleType == ScheduleType.Academic_Course);
                MaintenanceSchedules = allSchedules.Count(s => s.ScheduleType == ScheduleType.Recurring_Maintenance);

                var now = DateTime.UtcNow;
                var currentDayOfWeek = (int)now.DayOfWeek;
                var currentTime = now.TimeOfDay;

                ActiveSchedules = allSchedules.Count(s =>
                    s.DayOfWeek == currentDayOfWeek &&
                    s.StartTime <= currentTime &&
                    s.EndTime >= currentTime &&
                    s.StartDate <= now &&
                    s.EndDate >= now);

                // Schedules by Day of Week
                SchedulesByDay = new Dictionary<string, int>
                {
                    { "Monday", allSchedules.Count(s => s.DayOfWeek == 1) },
                    { "Tuesday", allSchedules.Count(s => s.DayOfWeek == 2) },
                    { "Wednesday", allSchedules.Count(s => s.DayOfWeek == 3) },
                    { "Thursday", allSchedules.Count(s => s.DayOfWeek == 4) },
                    { "Friday", allSchedules.Count(s => s.DayOfWeek == 5) },
                    { "Saturday", allSchedules.Count(s => s.DayOfWeek == 6) },
                    { "Sunday", allSchedules.Count(s => s.DayOfWeek == 0) }
                };

                // Schedules by Type
                SchedulesByType = new Dictionary<string, int>
                {
                    { "Academic Course", AcademicCourses },
                    { "Maintenance", MaintenanceSchedules }
                };

                // Top Rooms by Schedule Count (Top 10)
                SchedulesByRoom = allSchedules
                    .GroupBy(s => s.RoomName)
                    .Select(g => new { RoomName = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToDictionary(x => x.RoomName, x => x.Count);

                // Recent Schedules (Last 5 created)
                RecentSchedules = allSchedules
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(5)
                    .ToList();

                // Upcoming Schedules (Next 5 by date)
                UpcomingSchedules = allSchedules
                    .Where(s => s.StartDate >= now.Date)
                    .OrderBy(s => s.StartDate)
                    .ThenBy(s => s.StartTime)
                    .Take(5)
                    .ToList();

                _logger.LogInformation("Dashboard loaded successfully with {Count} schedules", TotalSchedules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading schedule dashboard");
                TotalSchedules = 0;
                AcademicCourses = 0;
                MaintenanceSchedules = 0;
                ActiveSchedules = 0;
            }
        }
    }
}
