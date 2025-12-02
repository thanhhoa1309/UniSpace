using Microsoft.Extensions.Configuration;
using UniSpace.BusinessObject.DTOs.AuthDTOs;
using UniSpace.BusinessObject.Enums;

namespace UniSpace.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto?> RegisterUserAsync(UserRegistrationDto registrationDto, RoleType role = RoleType.Student);

        Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginDto, IConfiguration configuration);

        Task<bool> LogoutAsync(Guid userId);
    }
}
