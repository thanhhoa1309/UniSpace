using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.RoomDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Room
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly ICampusService _campusService;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(
            IRoomService roomService,
            ICampusService campusService,
            ILogger<DashboardModel> logger)
        {
            _roomService = roomService;
            _campusService = campusService;
            _logger = logger;
        }

        // Statistics
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int UnavailableRooms { get; set; }
        public int TotalCapacity { get; set; }
        public double AverageCapacity { get; set; }
        public double UtilizationRate { get; set; }

        // Room Type Distribution
        public int TotalClassrooms { get; set; }
        public int TotalLabs { get; set; }
        public int TotalStadiums { get; set; }

        // Lists
        public List<RoomDto> RecentRooms { get; set; } = new();
        public List<RoomDto> LargestRooms { get; set; } = new();
        public List<RoomDto> MostBookedRooms { get; set; } = new();

        // Campus Statistics
        public Dictionary<string, int> RoomsByCampus { get; set; } = new();
        public Dictionary<string, int> CapacityByCampus { get; set; } = new();

        // Chart Data
        public Dictionary<string, int> RoomTypeDistribution { get; set; } = new();
        public Dictionary<string, int> RoomStatusDistribution { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                // Get all rooms using the unified method with large page size for statistics
                var roomsPagination = await _roomService.GetRoomsAsync(pageNumber: 1, pageSize: 10000);
                var allRooms = roomsPagination.ToList();

                // Basic statistics
                TotalRooms = roomsPagination.TotalCount;
                AvailableRooms = allRooms.Count(r => r.CurrentStatus == BookingStatus.Approved);
                UnavailableRooms = allRooms.Count(r => r.CurrentStatus != BookingStatus.Approved);
                TotalCapacity = allRooms.Sum(r => r.Capacity);
                AverageCapacity = TotalRooms > 0 ? Math.Round((double)TotalCapacity / TotalRooms, 1) : 0;

                // Utilization rate (rooms with bookings vs total rooms)
                var roomsWithBookings = allRooms.Count(r => r.TotalBookings > 0);
                UtilizationRate = TotalRooms > 0 
                    ? Math.Round((double)roomsWithBookings / TotalRooms * 100, 1) 
                    : 0;

                // Room type distribution
                TotalClassrooms = allRooms.Count(r => r.Type == RoomType.Classroom);
                TotalLabs = allRooms.Count(r => r.Type == RoomType.Lab);
                TotalStadiums = allRooms.Count(r => r.Type == RoomType.Stadium);

                // Recent rooms (top 5 by creation date)
                RecentRooms = allRooms
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .ToList();

                // Largest rooms by capacity
                LargestRooms = allRooms
                    .OrderByDescending(r => r.Capacity)
                    .Take(5)
                    .ToList();

                // Most booked rooms
                MostBookedRooms = allRooms
                    .Where(r => r.TotalBookings > 0)
                    .OrderByDescending(r => r.TotalBookings)
                    .Take(5)
                    .ToList();

                // Campus statistics
                RoomsByCampus = allRooms
                    .GroupBy(r => r.CampusName)
                    .ToDictionary(g => g.Key, g => g.Count());

                CapacityByCampus = allRooms
                    .GroupBy(r => r.CampusName)
                    .ToDictionary(g => g.Key, g => g.Sum(r => r.Capacity));

                // Chart data - Room Type Distribution
                RoomTypeDistribution = new Dictionary<string, int>
                {
                    { "Classroom", TotalClassrooms },
                    { "Laboratory", TotalLabs },
                    { "Stadium", TotalStadiums }
                };

                // Chart data - Room Status Distribution
                RoomStatusDistribution = new Dictionary<string, int>
                {
                    { "Available", AvailableRooms },
                    { "Unavailable", UnavailableRooms }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading room dashboard data");

                // Initialize with default values
                TotalRooms = 0;
                AvailableRooms = 0;
                UnavailableRooms = 0;
                TotalCapacity = 0;
                AverageCapacity = 0;
                UtilizationRate = 0;
                TotalClassrooms = 0;
                TotalLabs = 0;
                TotalStadiums = 0;
            }
        }
    }
}
