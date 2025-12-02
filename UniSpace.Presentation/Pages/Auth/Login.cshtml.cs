using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.AuthDTOs;
using UniSpace.Services.Interfaces;

namespace UniSpace.Presentation.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IAuthService authService, IConfiguration configuration, ILogger<LoginModel> logger)
        {
            _authService = authService;
            _configuration = configuration;
            _logger = logger;
        }

        [BindProperty]
        public LoginRequestDto LoginRequest { get; set; } = null!;

        public string? ErrorMessage { get; set; }
        public string? ReturnUrl { get; set; }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Dashboard");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var result = await _authService.LoginAsync(LoginRequest, _configuration);

                if (result == null)
                {
                    ErrorMessage = "Invalid email or password.";
                    _logger.LogWarning($"Login failed for {LoginRequest.Email}");
                    return Page();
                }

                // Store token in session
                HttpContext.Session.SetString("AuthToken", result.Token);

                _logger.LogInformation($"User {LoginRequest.Email} logged in successfully");

                return LocalRedirect(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during login for {LoginRequest.Email}");

                // Check if it's a custom error with status code
                if (ex.Data.Contains("StatusCode"))
                {
                    var statusCode = (int)ex.Data["StatusCode"]!;
                    ErrorMessage = statusCode switch
                    {
                        400 => "Invalid login request. Please check your input.",
                        401 => "Invalid email or password.",
                        404 => "User not found or inactive.",
                        _ => "An error occurred during login. Please try again."
                    };
                }
                else
                {
                    ErrorMessage = "An unexpected error occurred. Please try again.";
                }

                return Page();
            }
        }
    }
}
