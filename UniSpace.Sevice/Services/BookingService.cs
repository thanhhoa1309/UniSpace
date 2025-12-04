using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniSpace.BusinessObject.DTOs.BookingDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Domain.Entities;
using UniSpace.Domain.Interfaces;
using UniSpace.Service.Interfaces;
using UniSpace.Services.Utils;

namespace UniSpace.Service.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly ICurrentTime _currentTime;
        private readonly IRoomService _roomService;
        private readonly IScheduleService _scheduleService;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IUnitOfWork unitOfWork,
            IClaimsService claimsService,
            ICurrentTime currentTime,
            IRoomService roomService,
            IScheduleService scheduleService,
            ILogger<BookingService> logger)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _currentTime = currentTime;
            _roomService = roomService;
            _scheduleService = scheduleService;
            _logger = logger;
        }

        #region Create

        public async Task<BookingDto?> CreateBookingAsync(CreateBookingDto createDto)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                _logger.LogInformation($"User {currentUserId} creating booking for room {createDto.RoomId}");

                // Validate input
                if (createDto.StartTime >= createDto.EndTime)
                {
                    throw ErrorHelper.BadRequest("Start time must be before end time");
                }

                var now = DateTime.UtcNow;
                if (createDto.StartTime < now)
                {
                    throw ErrorHelper.BadRequest("Cannot book in the past");
                }

                // Validate minimum advance booking time (at least 30 minutes from now)
                if (createDto.StartTime < now.AddMinutes(30))
                {
                    throw ErrorHelper.BadRequest("Booking must be made at least 30 minutes in advance");
                }

                // Validate maximum advance booking time (max 30 days in future)
                if (createDto.StartTime > now.AddDays(30))
                {
                    throw ErrorHelper.BadRequest("Cannot book more than 30 days in advance");
                }

                // Validate booking duration
                var duration = (createDto.EndTime - createDto.StartTime).TotalMinutes;
                if (duration < 30)
                {
                    throw ErrorHelper.BadRequest("Minimum booking duration is 30 minutes");
                }

                if (duration > 1440) // 24 hours
                {
                    throw ErrorHelper.BadRequest("Maximum booking duration is 24 hours");
                }

                // Validate Purpose length (backend check)
                if (string.IsNullOrWhiteSpace(createDto.Purpose) || createDto.Purpose.Trim().Length < 10)
                {
                    throw ErrorHelper.BadRequest("Purpose must be at least 10 characters");
                }

                // Check if room exists and is available
                var room = await _unitOfWork.Room.GetByIdAsync(createDto.RoomId, r => r.Campus);
                if (room == null || room.IsDeleted)
                {
                    throw ErrorHelper.NotFound($"Room with ID '{createDto.RoomId}' not found");
                }

                if (room.RoomStatus != RoomStatus.Active)
                {
                    throw ErrorHelper.BadRequest($"Room is currently {room.RoomStatus}. Cannot book at this time.");
                }

                // Check for booking conflicts (including 15-minute break time)
                if (await HasBookingConflictAsync(createDto.RoomId, createDto.StartTime, createDto.EndTime))
                {
                    throw ErrorHelper.Conflict("Room is already booked for this time slot or conflicts with break time between bookings");
                }

                // Check for schedule conflicts
                // Handle cross-day bookings by checking all affected days
                var affectedDays = new List<(DateTime date, int dayOfWeek, TimeSpan start, TimeSpan end)>();

                var currentDate = createDto.StartTime.Date;
                var endDate = createDto.EndTime.Date;

                while (currentDate <= endDate)
                {
                    var dayStart = currentDate == createDto.StartTime.Date
                        ? createDto.StartTime.TimeOfDay
                        : TimeSpan.Zero;

                    var dayEnd = currentDate == createDto.EndTime.Date
                        ? createDto.EndTime.TimeOfDay
                        : new TimeSpan(23, 59, 59);

                    affectedDays.Add((currentDate, (int)currentDate.DayOfWeek, dayStart, dayEnd));
                    currentDate = currentDate.AddDays(1);
                }

                foreach (var (date, dayOfWeek, dayStart, dayEnd) in affectedDays)
                {
                    if (await _scheduleService.HasScheduleConflictAsync(
                        createDto.RoomId,
                        dayOfWeek,
                        dayStart,
                        dayEnd,
                        date,
                        date))
                    {
                        var conflicts = await _scheduleService.GetConflictingSchedulesAsync(
                            createDto.RoomId,
                            dayOfWeek,
                            dayStart,
                            dayEnd,
                            date,
                            date);

                        var conflictDetails = string.Join(", ", conflicts.Select(s => s.Title));
                        throw ErrorHelper.Conflict($"Time slot conflicts with scheduled activities on {date:yyyy-MM-dd}: {conflictDetails}");
                    }
                }
                var currentTime = _currentTime.GetCurrentTime();

                var booking = new Booking
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUserId,
                    RoomId = createDto.RoomId,
                    StartTime = createDto.StartTime.ToUniversalTime(),
                    EndTime = createDto.EndTime.ToUniversalTime(),
                    Status = BookingStatus.Pending,
                    Purpose = createDto.Purpose.Trim(),
                    AdminNote = string.Empty,
                    IsDeleted = false,
                    CreatedAt = currentTime,
                    CreatedBy = currentUserId
                };

                await _unitOfWork.Booking.AddAsync(booking);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Booking created successfully: {booking.Id}");

                return await GetBookingByIdAsync(booking.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                throw;
            }
        }

        #endregion

        #region Read

        public async Task<Pagination<BookingDto>> GetBookingsAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? searchTerm = null,
            Guid? roomId = null,
            Guid? userId = null,
            BookingStatus? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation($"Retrieving bookings - Page {pageNumber}, Size {pageSize}");

                IQueryable<Booking> query = _unitOfWork.Booking
                    .GetQueryable()
                    .Where(b => !b.IsDeleted)
                    .Include(b => b.User)
                    .Include(b => b.Room)
                    .Include(b => b.Room.Campus);

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(b =>
                        b.User.FullName.Contains(searchTerm) ||
                        b.User.Email.Contains(searchTerm) ||
                        b.Room.Name.Contains(searchTerm) ||
                        b.Purpose.Contains(searchTerm));
                }

                // Apply filters
                if (roomId.HasValue && roomId != Guid.Empty)
                {
                    query = query.Where(b => b.RoomId == roomId.Value);
                }

                if (userId.HasValue && userId != Guid.Empty)
                {
                    query = query.Where(b => b.UserId == userId.Value);
                }

                if (status.HasValue)
                {
                    query = query.Where(b => b.Status == status.Value);
                }

                if (startDate.HasValue)
                {
                    var startUtc = startDate.Value.ToUniversalTime();
                    query = query.Where(b => b.StartTime >= startUtc);
                }

                if (endDate.HasValue)
                {
                    var endUtc = endDate.Value.ToUniversalTime();
                    query = query.Where(b => b.EndTime <= endUtc);
                }

                // Order by start time descending
                query = query.OrderByDescending(b => b.StartTime);

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination
                var bookings = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var bookingDtos = bookings.Select(MapToDto).ToList();

                _logger.LogInformation($"Retrieved {bookingDtos.Count} of {totalCount} bookings");
                return new Pagination<BookingDto>(bookingDtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings");
                throw;
            }
        }

        public async Task<BookingDto?> GetBookingByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation($"Retrieving booking with ID: {id}");

                var booking = await _unitOfWork.Booking
                    .GetByIdAsync(id, b => b.User, b => b.Room, b => b.Room.Campus);

                if (booking == null || booking.IsDeleted)
                {
                    _logger.LogWarning($"Booking not found: {id}");
                    throw ErrorHelper.NotFound($"Booking with ID '{id}' not found");
                }

                return MapToDto(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving booking: {id}");
                throw;
            }
        }

        public async Task<List<BookingDto>> GetMyBookingsAsync()
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                _logger.LogInformation($"Retrieving bookings for user {currentUserId}");

                var bookings = await _unitOfWork.Booking
                    .GetAllAsync(
                        predicate: b => !b.IsDeleted && b.UserId == currentUserId,
                        includes: new System.Linq.Expressions.Expression<Func<Booking, object>>[]
                        {
                            b => b.Room,
                            b => b.Room.Campus,
                            b => b.User
                        }
                    );

                return bookings
                    .OrderByDescending(b => b.StartTime)
                    .Select(MapToDto)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user bookings");
                throw;
            }
        }

        public async Task<List<BookingDto>> GetUpcomingBookingsAsync(int take = 10)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                var now = DateTime.UtcNow;

                _logger.LogInformation($"Retrieving upcoming bookings for user {currentUserId}");

                var bookings = await _unitOfWork.Booking
                    .GetAllAsync(
                        predicate: b => !b.IsDeleted &&
                                       b.UserId == currentUserId &&
                                       b.StartTime > now &&
                                       (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Approved),
                        includes: new System.Linq.Expressions.Expression<Func<Booking, object>>[]
                        {
                            b => b.Room,
                            b => b.Room.Campus,
                            b => b.User
                        }
                    );

                return bookings
                    .OrderBy(b => b.StartTime)
                    .Take(take)
                    .Select(MapToDto)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming bookings");
                throw;
            }
        }

        #endregion

        #region Update

        public async Task<BookingDto?> UpdateBookingAsync(UpdateBookingDto updateDto)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                _logger.LogInformation($"User {currentUserId} updating booking {updateDto.Id}");

                var booking = await _unitOfWork.Booking.GetByIdAsync(updateDto.Id);

                if (booking == null || booking.IsDeleted)
                {
                    throw ErrorHelper.NotFound($"Booking with ID '{updateDto.Id}' not found");
                }

                // Only owner can update their booking
                if (booking.UserId != currentUserId)
                {
                    throw ErrorHelper.Forbidden("You can only update your own bookings");
                }

                // Can only update pending bookings
                if (booking.Status != BookingStatus.Pending)
                {
                    throw ErrorHelper.BadRequest($"Cannot update booking with status: {booking.Status}");
                }

                // Cannot update past bookings
                if (booking.StartTime < DateTime.UtcNow)
                {
                    throw ErrorHelper.BadRequest("Cannot update past bookings");
                }

                // Validate new times
                if (updateDto.StartTime >= updateDto.EndTime)
                {
                    throw ErrorHelper.BadRequest("Start time must be before end time");
                }

                var now = DateTime.UtcNow;
                if (updateDto.StartTime < now)
                {
                    throw ErrorHelper.BadRequest("Cannot book in the past");
                }

                // Validate minimum advance booking time (at least 30 minutes from now)
                if (updateDto.StartTime < now.AddMinutes(30))
                {
                    throw ErrorHelper.BadRequest("Booking must be made at least 30 minutes in advance");
                }

                // Validate maximum advance booking time (max 30 days in future)
                if (updateDto.StartTime > now.AddDays(30))
                {
                    throw ErrorHelper.BadRequest("Cannot book more than 30 days in advance");
                }

                // Validate booking duration
                var duration = (updateDto.EndTime - updateDto.StartTime).TotalMinutes;
                if (duration < 30)
                {
                    throw ErrorHelper.BadRequest("Minimum booking duration is 30 minutes");
                }

                if (duration > 1440) // 24 hours
                {
                    throw ErrorHelper.BadRequest("Maximum booking duration is 24 hours");
                }

                // Validate Purpose length (backend check)
                if (string.IsNullOrWhiteSpace(updateDto.Purpose) || updateDto.Purpose.Trim().Length < 10)
                {
                    throw ErrorHelper.BadRequest("Purpose must be at least 10 characters");
                }

                // Check room exists
                var room = await _unitOfWork.Room.GetByIdAsync(updateDto.RoomId);
                if (room == null || room.IsDeleted)
                {
                    throw ErrorHelper.NotFound($"Room with ID '{updateDto.RoomId}' not found");
                }

                // Check for conflicts (excluding current booking)
                if (await HasBookingConflictAsync(updateDto.RoomId, updateDto.StartTime, updateDto.EndTime, updateDto.Id))
                {
                    throw ErrorHelper.Conflict("Room is already booked for this time slot or conflicts with break time between bookings");
                }

                // Check for schedule conflicts when updating
                var affectedDays = new List<(DateTime date, int dayOfWeek, TimeSpan start, TimeSpan end)>();
                var currentDate = updateDto.StartTime.Date;
                var endDate = updateDto.EndTime.Date;

                while (currentDate <= endDate)
                {
                    var dayStart = currentDate == updateDto.StartTime.Date
                        ? updateDto.StartTime.TimeOfDay
                        : TimeSpan.Zero;

                    var dayEnd = currentDate == updateDto.EndTime.Date
                        ? updateDto.EndTime.TimeOfDay
                        : new TimeSpan(23, 59, 59);

                    affectedDays.Add((currentDate, (int)currentDate.DayOfWeek, dayStart, dayEnd));
                    currentDate = currentDate.AddDays(1);
                }

                foreach (var (date, dayOfWeek, dayStart, dayEnd) in affectedDays)
                {
                    if (await _scheduleService.HasScheduleConflictAsync(
                        updateDto.RoomId,
                        dayOfWeek,
                        dayStart,
                        dayEnd,
                        date,
                        date))
                    {
                        var conflicts = await _scheduleService.GetConflictingSchedulesAsync(
                            updateDto.RoomId,
                            dayOfWeek,
                            dayStart,
                            dayEnd,
                            date,
                            date);

                        var conflictDetails = string.Join(", ", conflicts.Select(s => s.Title));
                        throw ErrorHelper.Conflict($"Time slot conflicts with scheduled activities on {date:yyyy-MM-dd}: {conflictDetails}");
                    }
                }

                var currentTime = _currentTime.GetCurrentTime();

                booking.RoomId = updateDto.RoomId;
                booking.StartTime = updateDto.StartTime.ToUniversalTime();
                booking.EndTime = updateDto.EndTime.ToUniversalTime();
                booking.Purpose = updateDto.Purpose.Trim();
                booking.UpdatedAt = currentTime;
                booking.UpdatedBy = currentUserId;

                await _unitOfWork.Booking.Update(booking);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Booking updated successfully: {booking.Id}");

                return await GetBookingByIdAsync(booking.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating booking: {updateDto.Id}");
                throw;
            }
        }

        public async Task<bool> CancelBookingAsync(Guid id)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                _logger.LogInformation($"User {currentUserId} cancelling booking {id}");

                var booking = await _unitOfWork.Booking.GetByIdAsync(id);

                if (booking == null || booking.IsDeleted)
                {
                    throw ErrorHelper.NotFound($"Booking with ID '{id}' not found");
                }

                // Only owner can cancel their booking
                if (booking.UserId != currentUserId)
                {
                    throw ErrorHelper.Forbidden("You can only cancel your own bookings");
                }

                // Can only cancel pending or approved bookings
                if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Approved)
                {
                    throw ErrorHelper.BadRequest($"Cannot cancel booking with status: {booking.Status}");
                }

                // Cannot cancel past bookings
                if (booking.StartTime < DateTime.UtcNow)
                {
                    throw ErrorHelper.BadRequest("Cannot cancel past bookings");
                }

                var currentTime = _currentTime.GetCurrentTime();

                booking.Status = BookingStatus.Cancelled;
                booking.UpdatedAt = currentTime;
                booking.UpdatedBy = currentUserId;

                await _unitOfWork.Booking.Update(booking);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Booking cancelled successfully: {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling booking: {id}");
                throw;
            }
        }

        #endregion

        #region Delete

        public async Task<bool> SoftDeleteBookingAsync(Guid id)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                _logger.LogInformation($"Soft deleting booking: {id}");

                var booking = await _unitOfWork.Booking.GetByIdAsync(id);

                if (booking == null)
                {
                    throw ErrorHelper.NotFound($"Booking with ID '{id}' not found");
                }

                if (booking.IsDeleted)
                {
                    _logger.LogWarning($"Booking already deleted: {id}");
                    return false;
                }

                // Only owner can delete their booking
                if (booking.UserId != currentUserId)
                {
                    throw ErrorHelper.Forbidden("You can only delete your own bookings");
                }

                var currentTime = _currentTime.GetCurrentTime();

                await _unitOfWork.Booking.SoftRemove(booking);
                booking.DeletedAt = currentTime;
                booking.DeletedBy = currentUserId;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Booking soft deleted successfully: {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error soft deleting booking: {id}");
                throw;
            }
        }

        #endregion

        #region Helper Methods

        public async Task<bool> BookingExistsAsync(Guid id)
        {
            try
            {
                var booking = await _unitOfWork.Booking.GetByIdAsync(id);
                return booking != null && !booking.IsDeleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking booking existence: {id}");
                return false;
            }
        }

        public async Task<bool> HasBookingConflictAsync(Guid roomId, DateTime startTime, DateTime endTime, Guid? excludeBookingId = null)
        {
            try
            {
                var startUtc = startTime.ToUniversalTime();
                var endUtc = endTime.ToUniversalTime();

                // Add 15-minute break time buffer before and after
                var breakTimeMinutes = 15;
                var startWithBuffer = startUtc.AddMinutes(-breakTimeMinutes);
                var endWithBuffer = endUtc.AddMinutes(breakTimeMinutes);

                var bookings = await _unitOfWork.Booking
                    .GetAllAsync(b => !b.IsDeleted &&
                                     b.RoomId == roomId &&
                                     (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Approved));

                if (excludeBookingId.HasValue)
                {
                    bookings = bookings.Where(b => b.Id != excludeBookingId.Value).ToList();
                }

                // Check for time overlap with break time buffer
                var hasConflict = bookings.Any(b =>
                    (startWithBuffer >= b.StartTime && startWithBuffer < b.EndTime) ||
                    (endWithBuffer > b.StartTime && endWithBuffer <= b.EndTime) ||
                    (startWithBuffer <= b.StartTime && endWithBuffer >= b.EndTime));

                return hasConflict;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking booking conflict");
                return true; // Return true to be safe
            }
        }

        public async Task<List<BookingDto>> GetRoomBookingsAsync(Guid roomId, DateTime date)
        {
            try
            {
                _logger.LogInformation($"Retrieving bookings for room {roomId} on {date:yyyy-MM-dd}");

                var dateUtc = date.ToUniversalTime().Date;
                var nextDay = dateUtc.AddDays(1);

                var bookings = await _unitOfWork.Booking
                    .GetAllAsync(
                        predicate: b => !b.IsDeleted &&
                                       b.RoomId == roomId &&
                                       b.StartTime >= dateUtc &&
                                       b.StartTime < nextDay &&
                                       (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Approved),
                        includes: new System.Linq.Expressions.Expression<Func<Booking, object>>[]
                        {
                            b => b.User,
                            b => b.Room,
                            b => b.Room.Campus
                        }
                    );

                return bookings
                    .OrderBy(b => b.StartTime)
                    .Select(MapToDto)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room bookings");
                return new List<BookingDto>();
            }
        }

        public async Task<bool> CanUserModifyBookingAsync(Guid bookingId, Guid userId)
        {
            try
            {
                var booking = await _unitOfWork.Booking.GetByIdAsync(bookingId);
                return booking != null && !booking.IsDeleted && booking.UserId == userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user modification permission");
                return false;
            }
        }

        private BookingDto MapToDto(Booking booking)
        {
            return new BookingDto
            {
                Id = booking.Id,
                UserId = booking.UserId,
                UserName = booking.User?.FullName ?? "Unknown",
                UserEmail = booking.User?.Email ?? "Unknown",
                RoomId = booking.RoomId,
                RoomName = booking.Room?.Name ?? "Unknown",
                CampusName = booking.Room?.Campus?.Name ?? "Unknown",
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                Status = booking.Status,
                StatusDisplay = GetStatusDisplay(booking.Status),
                Purpose = booking.Purpose,
                AdminNote = booking.AdminNote ?? string.Empty,
                CreatedAt = booking.CreatedAt,
                DurationMinutes = (int)(booking.EndTime - booking.StartTime).TotalMinutes
            };
        }

        private string GetStatusDisplay(BookingStatus status)
        {
            return status switch
            {
                BookingStatus.Pending => "Pending Approval",
                BookingStatus.Approved => "Approved",
                BookingStatus.Rejected => "Rejected",
                BookingStatus.Completed => "Completed",
                BookingStatus.Cancelled => "Cancelled",
                _ => status.ToString()
            };
        }

        #endregion
    }
}
