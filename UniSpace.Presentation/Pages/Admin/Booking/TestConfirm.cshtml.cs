using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Booking
{
    [Authorize(Roles = "Admin")]
    public class TestConfirmModel : PageModel
    {
        private readonly IBookingService _bookingService;

        public TestConfirmModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public string Status { get; set; } = "";
        public List<string> PendingBookings { get; set; } = new();

        public async Task OnGetAsync()
        {
            Status = "Page is accessible ?";
            
            try
            {
                var bookings = await _bookingService.GetBookingsAsync(
                    pageNumber: 1, 
                    pageSize: 100,
                    status: UniSpace.BusinessObject.Enums.BookingStatus.Pending
                );

                if (bookings != null && bookings.Any())
                {
                    foreach (var booking in bookings)
                    {
                        PendingBookings.Add($"{booking.Id} - {booking.UserName} - {booking.RoomName} - {booking.StartTime:yyyy-MM-dd HH:mm}");
                    }
                }
                else
                {
                    PendingBookings.Add("No pending bookings found");
                }
            }
            catch (Exception ex)
            {
                Status = $"Error: {ex.Message}";
            }
        }
    }
}
