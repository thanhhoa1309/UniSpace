using UniSpace.BusinessObject.Enums;

namespace UniSpace.BusinessObject.DTOs.RoomDTOs
{
    public class RoomDto
    {
        public Guid Id { get; set; }
        public Guid CampusId { get; set; }
        public string CampusName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public RoomType Type { get; set; }
        public string TypeDisplay { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public BookingStatus CurrentStatus { get; set; }
        public string CurrentStatusDisplay { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public int PendingReports { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
