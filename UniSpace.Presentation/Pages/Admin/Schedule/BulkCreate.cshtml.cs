using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.ScheduleDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Schedule
{
    [Authorize(Roles = "Admin")]
    public class BulkCreateModel : PageModel
    {
        private readonly IScheduleService _scheduleService;
        private readonly IRoomService _roomService;
        private readonly ILogger<BulkCreateModel> _logger;

        public BulkCreateModel(
            IScheduleService scheduleService,
            IRoomService roomService,
            ILogger<BulkCreateModel> logger)
        {
            _scheduleService = scheduleService;
            _roomService = roomService;
            _logger = logger;
        }

        [BindProperty]
        public BulkCreateScheduleDto Input { get; set; } = new BulkCreateScheduleDto
        {
            BreakTimeMinutes = 15,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddMonths(4),
            SkipConflicts = true
        };

        public List<RoomOptionDto> AvailableRooms { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public BulkCreateScheduleResultDto? Result { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                await LoadRooms();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading bulk create schedule page");
                ErrorMessage = "Error loading page. Please try again.";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadRooms();
                return Page();
            }

            try
            {
                Result = await _scheduleService.BulkCreateSchedulesAsync(Input);

                if (Result.SuccessfulSchedules > 0)
                {
                    TempData["SuccessMessage"] = $"Successfully created {Result.SuccessfulSchedules} schedule(s)!";
                    
                    if (Result.FailedSchedules > 0)
                    {
                        TempData["WarningMessage"] = $"{Result.FailedSchedules} schedule(s) failed due to conflicts.";
                    }
                }
                else
                {
                    ErrorMessage = "No schedules were created. Please check for conflicts.";
                    await LoadRooms();
                    return Page();
                }

                // Show result page
                await LoadRooms();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk schedules");
                ErrorMessage = ex.Message;
                await LoadRooms();
                return Page();
            }
        }

        private async Task LoadRooms()
        {
            var rooms = await _roomService.GetRoomsAsync(pageNumber: 1, pageSize: 1000);
            AvailableRooms = rooms
                .Where(r => r.CurrentStatus == BookingStatus.Approved)
                .Select(r => new RoomOptionDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    CampusName = r.CampusName,
                    TypeDisplay = r.TypeDisplay,
                    Capacity = r.Capacity
                })
                .ToList();
        }

        public class RoomOptionDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string CampusName { get; set; } = string.Empty;
            public string TypeDisplay { get; set; } = string.Empty;
            public int Capacity { get; set; }
        }
    }
}
