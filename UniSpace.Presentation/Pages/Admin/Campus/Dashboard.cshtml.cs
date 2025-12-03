using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.CampusDTOs;
using UniSpace.Service.Interfaces;

namespace UniSpace.Presentation.Pages.Admin.Campus
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly ICampusService _campusService;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(ICampusService campusService, ILogger<DashboardModel> logger)
        {
            _campusService = campusService;
            _logger = logger;
        }

        // Statistics
        public int TotalCampuses { get; set; }
        public int TotalRooms { get; set; }
        public int ActiveCampuses { get; set; }
        public int CampusesWithRooms { get; set; }

        // Campus List
        public List<CampusDto> RecentCampuses { get; set; } = new();
        public List<CampusDto> TopCampusesByRooms { get; set; } = new();

        // Charts Data
        public Dictionary<string, int> CampusRoomDistribution { get; set; } = new();

        // Percentages
        public double CampusUtilizationRate { get; set; }
        public double AverageRoomsPerCampus { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Get all campuses
                var allCampuses = await _campusService.GetCampusesAsync();

                // Basic statistics
                TotalCampuses = allCampuses.Count;
                TotalRooms = allCampuses.Sum(c => c.TotalRooms);
                ActiveCampuses = allCampuses.Count; // All non-deleted are active
                CampusesWithRooms = allCampuses.Count(c => c.TotalRooms > 0);

                // Recent campuses (top 5 by creation date)
                RecentCampuses = allCampuses
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .ToList();

                // Top campuses by number of rooms
                TopCampusesByRooms = allCampuses
                    .OrderByDescending(c => c.TotalRooms)
                    .Take(5)
                    .ToList();

                // Campus room distribution for chart
                CampusRoomDistribution = allCampuses
                    .ToDictionary(c => c.Name, c => c.TotalRooms);

                // Calculate rates
                CampusUtilizationRate = TotalCampuses > 0
                    ? Math.Round((double)CampusesWithRooms / TotalCampuses * 100, 2)
                    : 0;

                AverageRoomsPerCampus = TotalCampuses > 0
                    ? Math.Round((double)TotalRooms / TotalCampuses, 2)
                    : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading campus dashboard data");

                // Initialize with default values
                TotalCampuses = 0;
                TotalRooms = 0;
                ActiveCampuses = 0;
                CampusesWithRooms = 0;
                CampusUtilizationRate = 0;
                AverageRoomsPerCampus = 0;
                RecentCampuses = new List<CampusDto>();
                TopCampusesByRooms = new List<CampusDto>();
                CampusRoomDistribution = new Dictionary<string, int>();
            }
        }
    }
}
