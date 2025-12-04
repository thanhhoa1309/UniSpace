using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.BookingDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Booking
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(
            IBookingService bookingService,
            ILogger<DetailsModel> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        public BookingDto Booking { get; set; } = null!;
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                Booking = await _bookingService.GetBookingByIdAsync(id);

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
                _logger.LogError(ex, $"Error loading booking: {id}");
                TempData["ErrorMessage"] = "Booking not found or has been deleted.";
                return RedirectToPage("Index");
            }
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid id)
        {
            try
            {
                var success = await _bookingService.ApproveBookingAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Booking approved successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to approve booking.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving booking: {id}");
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage("Details", new { id });
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var success = await _bookingService.SoftDeleteBookingAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Booking deleted successfully!";
                    return RedirectToPage("Index");
                }

                TempData["ErrorMessage"] = "Failed to delete booking.";
                return RedirectToPage("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting booking: {id}");
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("Details", new { id });
            }
        }

        public string GetStatusBadgeClass(BookingStatus status)
        {
            return status switch
            {
                BookingStatus.Pending => "bg-warning text-dark",
                BookingStatus.Approved => "bg-success",
                BookingStatus.Rejected => "bg-danger",
                BookingStatus.Completed => "bg-info",
                BookingStatus.Cancelled => "bg-secondary",
                _ => "bg-secondary"
            };
        }
    }
}
