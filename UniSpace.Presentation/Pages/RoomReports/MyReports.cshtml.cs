using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.RoomReportDTOs;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.RoomReports
{
    [Authorize]
    public class MyReportsModel : PageModel
    {
        private readonly IRoomReportService _roomReportService;
        private readonly ILogger<MyReportsModel> _logger;

        public MyReportsModel(IRoomReportService roomReportService, ILogger<MyReportsModel> logger)
        {
            _roomReportService = roomReportService;
            _logger = logger;
        }

        public List<RoomReportDto> Reports { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "");
                Reports = await _roomReportService.GetUserReportsAsync(currentUserId);

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
                _logger.LogError(ex, "Error loading user reports");
                ErrorMessage = "An error occurred while loading your reports";
                return Page();
            }
        }
    }
}
