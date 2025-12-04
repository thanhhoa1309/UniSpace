using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.BookingDTOs;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Booking
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IBookingService bookingService, ILogger<DetailsModel> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        public BookingDto Booking { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                Booking = await _bookingService.GetBookingByIdAsync(id);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking details");
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("Index");
            }
        }

        public async Task<IActionResult> OnPostCancelAsync(Guid id)
        {
            try
            {
                await _bookingService.CancelBookingAsync(id);
                TempData["SuccessMessage"] = "Booking cancelled successfully.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking");
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("Details", new { id });
            }
        }
    }
}
