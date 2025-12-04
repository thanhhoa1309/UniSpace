using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.RoomReportDTOs;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.RoomReports
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IRoomReportService _roomReportService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IRoomReportService roomReportService, ILogger<DetailsModel> logger)
        {
            _roomReportService = roomReportService;
            _logger = logger;
        }

        public RoomReportDto? Report { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Invalid report ID";
                return RedirectToPage("./MyReports");
            }

            try
            {
                Report = await _roomReportService.GetRoomReportByIdAsync(id);

                if (Report == null)
                {
                    TempData["ErrorMessage"] = "Report not found";
                    return RedirectToPage("./MyReports");
                }

                // Check if current user owns this report or is admin
                var currentUserId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "");
                var isAdmin = User.IsInRole("Admin");

                if (Report.UserId != currentUserId && !isAdmin)
                {
                    TempData["ErrorMessage"] = "You don't have permission to view this report";
                    return RedirectToPage("./MyReports");
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading report details: {id}");
                TempData["ErrorMessage"] = "An error occurred while loading report details";
                return RedirectToPage("./MyReports");
            }
        }
    }
}
