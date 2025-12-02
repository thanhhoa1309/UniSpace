using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UniSpace.Presentation.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(ILogger<DashboardModel> logger)
        {
            _logger = logger;
        }

        public string? UserEmail { get; set; }
        public string? UserRole { get; set; }
        public string? UserId { get; set; }

        public void OnGet()
        {
            UserEmail = User.Identity?.Name;
            UserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
