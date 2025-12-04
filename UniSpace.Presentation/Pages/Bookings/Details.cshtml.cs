using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.BookingDTOs;
using UniSpace.Domain.Interfaces;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Bookings
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly IClaimsService _claimsService;

        public DetailsModel(IBookingService bookingService, IClaimsService claimsService)
        {
            _bookingService = bookingService;
            _claimsService = claimsService;
        }

        public BookingDto Booking { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                Booking = await _bookingService.GetBookingByIdAsync(id);

                // Check if user owns this booking (unless they're admin)
                var currentUserId = _claimsService.GetCurrentUserId;
                var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (Booking.UserId != currentUserId && currentUserRole != "Admin")
                {
                    TempData["Error"] = "You don't have permission to view this booking.";
                    return RedirectToPage("./MyBookings");
                }

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading booking: {ex.Message}";
                return RedirectToPage("./MyBookings");
            }
        }

        public async Task<IActionResult> OnPostCancelAsync(Guid id)
        {
            try
            {
                var result = await _bookingService.CancelBookingAsync(id);
                if (result)
                {
                    TempData["Success"] = "Booking cancelled successfully.";
                    return RedirectToPage("./MyBookings");
                }
                else
                {
                    TempData["Error"] = "Failed to cancel booking.";
                    return RedirectToPage(new { id });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error cancelling booking: {ex.Message}";
                return RedirectToPage(new { id });
            }
        }
    }
}
