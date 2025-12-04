using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.BookingDTOs;
using UniSpace.BusinessObject.DTOs.RoomDTOs;
using UniSpace.Domain.Interfaces;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Bookings
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly IRoomService _roomService;
        private readonly IClaimsService _claimsService;

        public EditModel(IBookingService bookingService, IRoomService roomService, IClaimsService claimsService)
        {
            _bookingService = bookingService;
            _roomService = roomService;
            _claimsService = claimsService;
        }

        [BindProperty]
        public UpdateBookingDto Booking { get; set; } = new UpdateBookingDto();

        public RoomDto? Room { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                
                // Check if user owns this booking
                var currentUserId = _claimsService.GetCurrentUserId;
                if (booking.UserId != currentUserId)
                {
                    TempData["Error"] = "You don't have permission to edit this booking.";
                    return RedirectToPage("./MyBookings");
                }

                // Only pending bookings can be edited
                if (booking.Status != BusinessObject.Enums.BookingStatus.Pending)
                {
                    TempData["Error"] = "Only pending bookings can be edited.";
                    return RedirectToPage("./Details", new { id });
                }

                Room = await _roomService.GetRoomByIdAsync(booking.RoomId);

                Booking = new UpdateBookingDto
                {
                    Id = booking.Id,
                    StartTime = booking.StartTime.ToLocalTime(),
                    EndTime = booking.EndTime.ToLocalTime(),
                    Purpose = booking.Purpose
                };

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading booking: {ex.Message}";
                return RedirectToPage("./MyBookings");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var booking = await _bookingService.GetBookingByIdAsync(Booking.Id);
                    Room = await _roomService.GetRoomByIdAsync(booking.RoomId);
                    return Page();
                }

                // Additional validation
                if (Booking.StartTime >= Booking.EndTime)
                {
                    ModelState.AddModelError("Booking.EndTime", "End time must be after start time.");
                    var booking = await _bookingService.GetBookingByIdAsync(Booking.Id);
                    Room = await _roomService.GetRoomByIdAsync(booking.RoomId);
                    return Page();
                }

                if (Booking.StartTime < DateTime.Now)
                {
                    ModelState.AddModelError("Booking.StartTime", "Cannot book rooms in the past.");
                    var booking = await _bookingService.GetBookingByIdAsync(Booking.Id);
                    Room = await _roomService.GetRoomByIdAsync(booking.RoomId);
                    return Page();
                }

                var result = await _bookingService.UpdateBookingAsync(Booking);

                if (result != null)
                {
                    TempData["Success"] = "Booking updated successfully!";
                    return RedirectToPage("./Details", new { id = result.Id });
                }
                else
                {
                    TempData["Error"] = "Failed to update booking.";
                    var booking = await _bookingService.GetBookingByIdAsync(Booking.Id);
                    Room = await _roomService.GetRoomByIdAsync(booking.RoomId);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var booking = await _bookingService.GetBookingByIdAsync(Booking.Id);
                Room = await _roomService.GetRoomByIdAsync(booking.RoomId);
                return Page();
            }
        }

        public async Task<IActionResult> OnGetCheckAvailabilityAsync(Guid roomId, DateTime startTime, DateTime endTime, Guid bookingId)
        {
            try
            {
                var isAvailable = await _bookingService.IsRoomAvailableForBookingAsync(roomId, startTime, endTime, bookingId);
                return new JsonResult(new { available = isAvailable });
            }
            catch
            {
                return new JsonResult(new { available = false });
            }
        }
    }
}
