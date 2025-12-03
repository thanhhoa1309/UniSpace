using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSpace.BusinessObject.DTOs.ScheduleDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Service.Interfaces;
using UniSpace.Services.Utils;

namespace UniSpace.Presentation.Pages.Admin.Schedule
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IScheduleService _scheduleService;
        private readonly IRoomService _roomService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            IScheduleService scheduleService,
            IRoomService roomService,
            ILogger<IndexModel> logger)
        {
            _scheduleService = scheduleService;
            _roomService = roomService;
            _logger = logger;
        }

        public Pagination<ScheduleDto> Schedules { get; set; } = new Pagination<ScheduleDto>(new List<ScheduleDto>(), 0, 1, 20);
        public string? SearchTerm { get; set; }
        public Guid? FilterRoomId { get; set; }
        public ScheduleType? FilterType { get; set; }
        public int? FilterDay { get; set; }
        public List<SelectListItem> RoomOptions { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? search = null,
            Guid? roomId = null,
            ScheduleType? type = null,
            int? day = null)
        {
            try
            {
                SearchTerm = search;
                FilterRoomId = roomId;
                FilterType = type;
                FilterDay = day;

                // Get schedules with filters
                Schedules = await _scheduleService.GetSchedulesAsync(
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    searchTerm: search,
                    roomId: roomId,
                    scheduleType: type,
                    dayOfWeek: day);

                // Load room options for filter
                var rooms = await _roomService.GetRoomsAsync(pageNumber: 1, pageSize: 1000);
                RoomOptions = rooms.Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = $"{r.Name} - {r.CampusName}",
                    Selected = r.Id == FilterRoomId
                }).ToList();

                RoomOptions.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "All Rooms",
                    Selected = !FilterRoomId.HasValue
                });

                // Success/Error messages from TempData
                if (TempData["SuccessMessage"] != null)
                {
                    SuccessMessage = TempData["SuccessMessage"]?.ToString();
                }

                if (TempData["ErrorMessage"] != null)
                {
                    ErrorMessage = TempData["ErrorMessage"]?.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading schedules");
                ErrorMessage = "Error loading schedules. Please try again.";
                Schedules = new Pagination<ScheduleDto>(new List<ScheduleDto>(), 0, 1, 20);
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var success = await _scheduleService.SoftDeleteScheduleAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Schedule deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete schedule.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting schedule: {id}");
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        public string GetDayName(int day)
        {
            return day switch
            {
                1 => "Sunday",
                2 => "Monday",
                3 => "Tuesday",
                4 => "Wednesday",
                5 => "Thursday",
                6 => "Friday",
                7 => "Saturday",
                _ => "Unknown"
            };
        }
    }
}
