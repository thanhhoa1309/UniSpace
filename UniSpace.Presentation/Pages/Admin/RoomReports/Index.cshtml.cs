using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.RoomReportDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.RoomReports
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IRoomReportService _roomReportService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            IRoomReportService roomReportService,
            ILogger<IndexModel> logger)
        {
            _roomReportService = roomReportService;
            _logger = logger;
        }

        public List<RoomReportDto> Reports { get; set; } = new();
        public int TotalReports { get; set; }
        public int OpenReports { get; set; }
        public int ResolvedReports { get; set; }

        [BindProperty(SupportsGet = true)]
        public ReportStatus? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var result = await _roomReportService.GetRoomReportsAsync(
                    pageNumber: PageNumber,
                    pageSize: PageSize,
                    searchTerm: SearchTerm,
                    status: StatusFilter
                );

                Reports = result.ToList();
                TotalReports = result.TotalCount;
                TotalPages = result.TotalPages;
                
                // Get statistics
                OpenReports = await _roomReportService.GetPendingReportsCountAsync();
                ResolvedReports = TotalReports - OpenReports;

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading room reports");
                TempData["ErrorMessage"] = "An error occurred while loading reports.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostResolveAsync(Guid id)
        {
            try
            {
                await _roomReportService.UpdateReportStatusAsync(id, ReportStatus.Resolved);
                TempData["SuccessMessage"] = "Report marked as resolved successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resolving report: {id}");
                TempData["ErrorMessage"] = "Failed to resolve report.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReopenAsync(Guid id)
        {
            try
            {
                await _roomReportService.UpdateReportStatusAsync(id, ReportStatus.Open);
                TempData["SuccessMessage"] = "Report reopened successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reopening report: {id}");
                TempData["ErrorMessage"] = "Failed to reopen report.";
            }

            return RedirectToPage();
        }
    }
}
