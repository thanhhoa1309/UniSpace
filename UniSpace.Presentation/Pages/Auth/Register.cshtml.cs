using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniSpace.BusinessObject.DTOs.AuthDTOs;
using UniSpace.BusinessObject.Enums;
using UniSpace.Services.Interfaces;

namespace UniSpace.Presentation.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(IAuthService authService, ILogger<RegisterModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        public UserRegistrationDto RegistrationRequest { get; set; } = null!;

        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        [BindProperty]
        public string SelectedRole { get; set; } = "Student"; // Default to Student

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            // Initialize with default role
            SelectedRole = "Student";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Log the selected role for debugging
            _logger.LogInformation($"Registration attempt with role: {SelectedRole}");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Validate password confirmation
            if (RegistrationRequest.Password != ConfirmPassword)
            {
                ErrorMessage = "Password and confirmation password do not match.";
                return Page();
            }

            try
            {
                // Parse the selected role
                RoleType roleType;
                if (!Enum.TryParse<RoleType>(SelectedRole, true, out roleType))
                {
                    ErrorMessage = "Invalid role selected.";
                    _logger.LogWarning($"Invalid role selected: {SelectedRole}");
                    return Page();
                }

                // Validate role selection (only Student and Lecturer allowed for registration)
                if (roleType != RoleType.Student && roleType != RoleType.Lecturer)
                {
                    ErrorMessage = "You can only register as Student or Lecturer.";
                    _logger.LogWarning($"Unauthorized role registration attempt: {roleType}");
                    return Page();
                }

                var result = await _authService.RegisterUserAsync(RegistrationRequest, roleType);

                if (result == null)
                {
                    ErrorMessage = "Registration failed. Email may already be in use.";
                    _logger.LogWarning($"Registration failed for {RegistrationRequest.Email}");
                    return Page();
                }

                _logger.LogInformation($"User {RegistrationRequest.Email} registered successfully as {roleType}");
                SuccessMessage = $"Registration successful as {roleType}! Please login.";
                
                // Redirect to login page after successful registration
                return RedirectToPage("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during registration for {RegistrationRequest.Email}");
                
                // Check if it's a custom error with status code
                if (ex.Data.Contains("StatusCode"))
                {
                    var statusCode = (int)ex.Data["StatusCode"]!;
                    ErrorMessage = statusCode switch
                    {
                        400 => "Invalid registration data. Please check your input.",
                        409 => "Email already exists. Please use a different email.",
                        _ => "An error occurred during registration. Please try again."
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
