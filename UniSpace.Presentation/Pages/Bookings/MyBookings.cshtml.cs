using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.BookingDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Domain.Interfaces;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Bookings
{
    [Authorize]
    public class MyBookingsModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly IClaimsService _claimsService;

        public MyBookingsModel(IBookingService bookingService, IClaimsService claimsService)
        {
            _bookingService = bookingService;
            _claimsService = claimsService;
        }

        public List<BookingDto> Bookings { get; set; } = new List<BookingDto>();

        [BindProperty(SupportsGet = true)]
        public BookingStatus? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var userId = _claimsService.GetCurrentUserId;

                var result = await _bookingService.GetBookingsAsync(
                    pageNumber: PageNumber,
                    pageSize: 10,
                    userId: userId,
                    status: StatusFilter);

                Bookings = result.ToList();
                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading bookings: {ex.Message}";
                return Page();
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
                }
                else
                {
                    TempData["Error"] = "Failed to cancel booking.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error cancelling booking: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
