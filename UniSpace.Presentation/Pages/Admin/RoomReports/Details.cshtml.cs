using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.RoomReportDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.RoomReports
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly IRoomReportService _roomReportService;
        private readonly IBookingService _bookingService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(
            IRoomReportService roomReportService,
            IBookingService bookingService,
            ILogger<DetailsModel> logger)
        {
            _roomReportService = roomReportService;
            _bookingService = bookingService;
            _logger = logger;
        }

        public RoomReportDto? Report { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? id = null)
        {
            try
            {
                if (!id.HasValue || id.Value == Guid.Empty)
                {
                    TempData["ErrorMessage"] = "Report ID is required.";
                    return RedirectToPage("Index");
                }

                Report = await _roomReportService.GetRoomReportByIdAsync(id.Value);

                if (Report == null)
                {
                    TempData["ErrorMessage"] = "Report not found.";
                    return RedirectToPage("Index");
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading report details: {id}");
                TempData["ErrorMessage"] = "An error occurred while loading the report.";
                return RedirectToPage("Index");
            }
        }

        public async Task<IActionResult> OnPostResolveAsync(Guid id)
        {
            try
            {
                await _roomReportService.UpdateReportStatusAsync(id, ReportStatus.Resolved);
                TempData["SuccessMessage"] = "Report marked as resolved successfully!";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resolving report: {id}");
                ErrorMessage = "Failed to resolve report.";
                Report = await _roomReportService.GetRoomReportByIdAsync(id);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostReopenAsync(Guid id)
        {
            try
            {
                await _roomReportService.UpdateReportStatusAsync(id, ReportStatus.Open);
                TempData["SuccessMessage"] = "Report reopened successfully!";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reopening report: {id}");
                ErrorMessage = "Failed to reopen report.";
                Report = await _roomReportService.GetRoomReportByIdAsync(id);
                return Page();
            }
        }
    }
}
