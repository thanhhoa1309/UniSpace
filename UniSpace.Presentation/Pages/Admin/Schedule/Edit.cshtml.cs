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
    public class EditModel : PageModel
    {
        private readonly IScheduleService _scheduleService;
        private readonly IRoomService _roomService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            IScheduleService scheduleService,
            IRoomService roomService,
            ILogger<EditModel> logger)
        {
            _scheduleService = scheduleService;
            _roomService = roomService;
            _logger = logger;
        }

        [BindProperty]
        public UpdateScheduleDto Input { get; set; } = new UpdateScheduleDto();

        public ScheduleDto? CurrentSchedule { get; set; }
        public List<SelectListItem> RoomOptions { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                CurrentSchedule = await _scheduleService.GetScheduleByIdAsync(id);

                if (CurrentSchedule == null)
                {
                    TempData["ErrorMessage"] = "Schedule not found.";
                    return RedirectToPage("Index");
                }

                // Map to input
                Input = new UpdateScheduleDto
                {
                    Id = CurrentSchedule.Id,
                    RoomId = CurrentSchedule.RoomId,
                    ScheduleType = CurrentSchedule.ScheduleType,
                    Title = CurrentSchedule.Title,
                    StartTime = CurrentSchedule.StartTime,
                    EndTime = CurrentSchedule.EndTime,
                    DayOfWeek = CurrentSchedule.DayOfWeek,
                    StartDate = CurrentSchedule.StartDate,
                    EndDate = CurrentSchedule.EndDate
                };

                await LoadRoomOptions();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading edit page for schedule: {id}");
                TempData["ErrorMessage"] = "Error loading schedule. Please try again.";
                return RedirectToPage("Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                CurrentSchedule = await _scheduleService.GetScheduleByIdAsync(Input.Id);
                await LoadRoomOptions();
                return Page();
            }

            try
            {
                var schedule = await _scheduleService.UpdateScheduleAsync(Input);

                if (schedule != null)
                {
                    TempData["SuccessMessage"] = $"Schedule '{schedule.Title}' updated successfully!";
                    return RedirectToPage("Details", new { id = schedule.Id });
                }
                else
                {
                    ErrorMessage = "Failed to update schedule. Please try again.";
                    CurrentSchedule = await _scheduleService.GetScheduleByIdAsync(Input.Id);
                    await LoadRoomOptions();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating schedule: {Input.Id}");
                ErrorMessage = ex.Message;
                CurrentSchedule = await _scheduleService.GetScheduleByIdAsync(Input.Id);
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
                    Text = $"{r.Name} - {r.CampusName} ({r.TypeDisplay}, {r.Capacity} seats)",
                    Selected = r.Id == Input.RoomId
                })
                .ToList();
        }
    }
}
