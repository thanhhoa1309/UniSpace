using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.RoomReportDTOs;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.RoomReports
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IRoomReportService _roomReportService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IRoomReportService roomReportService, ILogger<CreateModel> logger)
        {
            _roomReportService = roomReportService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public Guid BookingId { get; set; }

        [BindProperty]
        public CreateRoomReportDto Input { get; set; } = new();

        public string? BookingInfo { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (BookingId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Booking ID is required";
                return RedirectToPage("/Dashboard");
            }

            try
            {
                // Check if user can report this booking
                var currentUserId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "");
                
                if (!await _roomReportService.CanUserReportBookingAsync(currentUserId, BookingId))
                {
                    TempData["ErrorMessage"] = "You cannot report this booking. It may not be yours, already reported, or not completed/approved yet.";
                    return RedirectToPage("/Dashboard");
                }

                Input.BookingId = BookingId;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking booking eligibility: {BookingId}");
                TempData["ErrorMessage"] = "An error occurred while checking booking eligibility";
                return RedirectToPage("/Dashboard");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var report = await _roomReportService.CreateRoomReportAsync(Input);

                if (report != null)
                {
                    TempData["SuccessMessage"] = "Room report submitted successfully. Our team will review it soon.";
                    return RedirectToPage("./MyReports");
                }

                ErrorMessage = "Failed to create room report. Please try again.";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room report");
                ErrorMessage = ex.Message;
                return Page();
            }
        }
    }
}
