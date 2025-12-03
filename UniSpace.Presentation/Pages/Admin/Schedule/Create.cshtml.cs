using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSpace.BusinessObject.DTOs.ScheduleDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Schedule
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IScheduleService _scheduleService;
        private readonly IRoomService _roomService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            IScheduleService scheduleService,
            IRoomService roomService,
            ILogger<CreateModel> logger)
        {
            _scheduleService = scheduleService;
            _roomService = roomService;
            _logger = logger;
        }

        [BindProperty]
        public CreateScheduleDto Input { get; set; } = new CreateScheduleDto
        {
            BreakTimeMinutes = 15,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddMonths(4)
        };

        public List<SelectListItem> RoomOptions { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                await LoadRoomOptions();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create schedule page");
                ErrorMessage = "Error loading page. Please try again.";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadRoomOptions();
                return Page();
            }

            try
            {
                var schedule = await _scheduleService.CreateScheduleAsync(Input);

                if (schedule != null)
                {
                    TempData["SuccessMessage"] = $"Schedule '{schedule.Title}' created successfully!";
                    return RedirectToPage("Details", new { id = schedule.Id });
                }
                else
                {
                    ErrorMessage = "Failed to create schedule. Please try again.";
                    await LoadRoomOptions();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating schedule");
                ErrorMessage = ex.Message;
                await LoadRoomOptions();
                return Page();
            }
        }

        private async Task LoadRoomOptions()
        {
            var rooms = await _roomService.GetRoomsAsync(pageNumber: 1, pageSize: 1000);
            RoomOptions = rooms
                .Where(r => r.CurrentStatus == BookingStatus.Approved)
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = $"{r.Name} - {r.CampusName} ({r.TypeDisplay}, {r.Capacity} seats)"
                })
                .ToList();

            RoomOptions.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "-- Select Room --"
            });
        }
    }
}
