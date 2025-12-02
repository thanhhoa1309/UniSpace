using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.Services.Interfaces;

namespace UniSpace.Presentation.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(IAuthService authService, ILogger<LogoutModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            return await LogoutUser();
        }

        public async Task<IActionResult> OnPost()
        {
            return await LogoutUser();
        }

        private async Task<IActionResult> LogoutUser()
        {
            try
            {
                // Get user ID from claims if available
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    await _authService.LogoutAsync(userId);
                }

                // Clear session
                HttpContext.Session.Clear();

                _logger.LogInformation("User logged out successfully");

                return RedirectToPage("/Auth/Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return RedirectToPage("/Auth/Login");
            }
        }
    }
}
