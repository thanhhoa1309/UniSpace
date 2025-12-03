using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.RoomDTOs;
using UniSpace.BusinessObject.DTOs.ScheduleDTOs;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Room
{
    [Authorize(Roles = "Admin")]
    public class RoomSchedulesModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly IScheduleService _scheduleService;
        private readonly ILogger<RoomSchedulesModel> _logger;

        public RoomSchedulesModel(
            IRoomService roomService,
            IScheduleService scheduleService,
            ILogger<RoomSchedulesModel> logger)
        {
            _roomService = roomService;
            _scheduleService = scheduleService;
            _logger = logger;
        }

        public RoomDto? Room { get; set; }
        public List<ScheduleDto> Schedules { get; set; } = new();
        public Dictionary<int, List<ScheduleDto>> SchedulesByDay { get; set; } = new();
        public string? ErrorMessage { get; set; }

        // Statistics
        public int TotalSchedules { get; set; }
        public int AcademicSchedules { get; set; }
        public int MaintenanceSchedules { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                // Get room details
                Room = await _roomService.GetRoomByIdAsync(id);

                if (Room == null)
                {
                    TempData["ErrorMessage"] = "Room not found.";
                    return RedirectToPage("Index");
                }

                // Get all schedules for this room
                Schedules = await _scheduleService.GetSchedulesByRoomAsync(id);

                // Calculate statistics
                TotalSchedules = Schedules.Count;
                AcademicSchedules = Schedules.Count(s => s.ScheduleType == BusinessObject.Enums.ScheduleType.Academic_Course);
                MaintenanceSchedules = Schedules.Count(s => s.ScheduleType == BusinessObject.Enums.ScheduleType.Recurring_Maintenance);

                // Group schedules by day of week
                SchedulesByDay = Schedules
                    .GroupBy(s => s.DayOfWeek)
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.OrderBy(s => s.StartTime).ToList());

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading schedules for room: {id}");
                ErrorMessage = "Error loading room schedules. Please try again.";
                return Page();
            }
        }

        public string GetDayName(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                0 => "Sunday",
                1 => "Monday",
                2 => "Tuesday",
                3 => "Wednesday",
                4 => "Thursday",
                5 => "Friday",
                6 => "Saturday",
                _ => "Unknown"
            };
        }

        public string GetDayColor(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                0 => "danger",    // Sunday - Red
                1 => "primary",   // Monday - Blue
                2 => "success",   // Tuesday - Green
                3 => "warning",   // Wednesday - Yellow
                4 => "info",      // Thursday - Cyan
                5 => "secondary", // Friday - Gray
                6 => "dark",      // Saturday - Dark
                _ => "secondary"
            };
        }
    }
}
