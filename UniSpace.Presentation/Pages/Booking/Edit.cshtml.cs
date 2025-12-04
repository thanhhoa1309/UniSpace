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
    public class EditModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly IRoomService _roomService;
        private readonly ICampusService _campusService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            IBookingService bookingService,
            IRoomService roomService,
            ICampusService campusService,
            ILogger<EditModel> logger)
        {
            _bookingService = bookingService;
            _roomService = roomService;
            _campusService = campusService;
            _logger = logger;
        }

        [BindProperty]
        public UpdateBookingDto UpdateBooking { get; set; } = new();

        public List<RoomDto> Rooms { get; set; } = new();
        public List<CampusDto> Campuses { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);

                UpdateBooking = new UpdateBookingDto
                {
                    Id = booking.Id,
                    RoomId = booking.RoomId,
                    StartTime = booking.StartTime.ToLocalTime(),
                    EndTime = booking.EndTime.ToLocalTime(),
                    Purpose = booking.Purpose
                };

                await LoadDataAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking for edit");
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("Index");
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
                await _bookingService.UpdateBookingAsync(UpdateBooking);
                TempData["SuccessMessage"] = "Booking updated successfully!";
                return RedirectToPage("Details", new { id = UpdateBooking.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking");
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
