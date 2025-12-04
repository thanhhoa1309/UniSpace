using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.BookingDTOs;
using UniSpace.BusinessObject.DTOs.CampusDTOs;
using UniSpace.BusinessObject.DTOs.RoomDTOs;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Booking
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly IRoomService _roomService;
        private readonly ICampusService _campusService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            IBookingService bookingService,
            IRoomService roomService,
            ICampusService campusService,
            ILogger<CreateModel> logger)
        {
            _bookingService = bookingService;
            _roomService = roomService;
            _campusService = campusService;
            _logger = logger;
        }

        [BindProperty]
        public CreateBookingDto CreateBooking { get; set; } = new();

        public List<RoomDto> Rooms { get; set; } = new();
        public List<CampusDto> Campuses { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadDataAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking creation page");
                ErrorMessage = "Error loading page. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            try
            {
                var booking = await _bookingService.CreateBookingAsync(CreateBooking);
                TempData["SuccessMessage"] = "Booking created successfully! Waiting for admin approval.";
                return RedirectToPage("Details", new { id = booking!.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                ErrorMessage = ex.Message;
                await LoadDataAsync();
                return Page();
            }
        }

        private async Task LoadDataAsync()
        {
            var roomsPagination = await _roomService.GetRoomsAsync(pageSize: 1000);
            Rooms = roomsPagination.ToList();

            var campusesPagination = await _campusService.GetCampusesAsync(pageSize: 100);
            Campuses = campusesPagination.ToList();
        }
    }
}
