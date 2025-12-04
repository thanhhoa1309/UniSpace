using UniSpace.BusinessObject.DTOs.BookingDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Services.Utils;

namespace UniSpace.Service.Interfaces
{
    public interface IBookingService
    {
        // Create
        Task<BookingDto?> CreateBookingAsync(CreateBookingDto createDto);

        // Read
        Task<Pagination<BookingDto>> GetBookingsAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? searchTerm = null,
            Guid? roomId = null,
            Guid? userId = null,
            BookingStatus? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        Task<BookingDto?> GetBookingByIdAsync(Guid id);
        Task<List<BookingDto>> GetMyBookingsAsync();
        Task<List<BookingDto>> GetUpcomingBookingsAsync(int take = 10);

        // Update
        Task<BookingDto?> UpdateBookingAsync(UpdateBookingDto updateDto);
        Task<bool> CancelBookingAsync(Guid id);

        // Delete
        Task<bool> SoftDeleteBookingAsync(Guid id);

        // Helper Methods
        Task<bool> BookingExistsAsync(Guid id);
        Task<bool> HasBookingConflictAsync(Guid roomId, DateTime startTime, DateTime endTime, Guid? excludeBookingId = null);
        Task<List<BookingDto>> GetRoomBookingsAsync(Guid roomId, DateTime date);
        Task<bool> CanUserModifyBookingAsync(Guid bookingId, Guid userId);
    }
}
