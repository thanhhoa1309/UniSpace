using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.ScheduleDTOs;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Schedule
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly IScheduleService _scheduleService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(
            IScheduleService scheduleService,
            ILogger<DetailsModel> logger)
        {
            _scheduleService = scheduleService;
            _logger = logger;
        }

        public ScheduleDto? Schedule { get; set; }
        public List<ScheduleDto> ConflictingSchedules { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                Schedule = await _scheduleService.GetScheduleByIdAsync(id);

                if (Schedule == null)
                {
                    TempData["ErrorMessage"] = "Schedule not found.";
                    return RedirectToPage("Index");
                }

                // Check for potential conflicts
                ConflictingSchedules = await _scheduleService.GetConflictingSchedulesAsync(
                    Schedule.RoomId,
                    Schedule.DayOfWeek,
                    Schedule.StartTime,
                    Schedule.EndTime,
                    Schedule.StartDate,
                    Schedule.EndDate);

                // Remove self from conflicts
                ConflictingSchedules = ConflictingSchedules
                    .Where(s => s.Id != Schedule.Id)
                    .ToList();

                // Success/Error messages from TempData
                if (TempData["SuccessMessage"] != null)
                {
                    SuccessMessage = TempData["SuccessMessage"]?.ToString();
                }

                if (TempData["ErrorMessage"] != null)
                {
                    ErrorMessage = TempData["ErrorMessage"]?.ToString();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading schedule details: {id}");
                TempData["ErrorMessage"] = "Error loading schedule details. Please try again.";
                return RedirectToPage("Index");
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
                    return RedirectToPage("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete schedule.";
                    return RedirectToPage("Details", new { id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting schedule: {id}");
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("Details", new { id });
            }
        }
    }
}
