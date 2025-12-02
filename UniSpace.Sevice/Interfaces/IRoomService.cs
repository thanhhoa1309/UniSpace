using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniSpace.BusinessObject.DTOs.RoomDTOs;
using UniSpace.BusinessObject.Enums;

namespace UniSpace.Service.Interfaces
{
    public interface IRoomService
    {
        // Create
        Task<RoomDto?> CreateRoomAsync(CreateRoomDto createDto);

        // Read
        Task<List<RoomDto>> GetAllRoomsAsync();
        Task<RoomDto?> GetRoomByIdAsync(Guid id);
        Task<List<RoomDto>> GetRoomsByCampusAsync(Guid campusId);
        Task<List<RoomDto>> GetRoomsByTypeAsync(RoomType type);
        Task<List<RoomDto>> GetRoomsByStatusAsync(BookingStatus status);
        Task<List<RoomDto>> SearchRoomsAsync(string searchTerm);
        Task<List<RoomDto>> GetAvailableRoomsAsync(DateTime startTime, DateTime endTime);

        // Update
        Task<RoomDto?> UpdateRoomAsync(UpdateRoomDto updateDto);
        Task<bool> UpdateRoomStatusAsync(Guid roomId, BookingStatus status);

        // Delete
        Task<bool> SoftDeleteRoomAsync(Guid id);
        Task<bool> DeleteRoomAsync(Guid id);

        // Helper Methods
        Task<bool> RoomExistsAsync(Guid id);
        Task<bool> RoomNameExistsInCampusAsync(Guid campusId, string name, Guid? excludeId = null);
        Task<int> GetRoomCapacityAsync(Guid roomId);
        Task<bool> IsRoomAvailableAsync(Guid roomId, DateTime startTime, DateTime endTime);
    }
}
