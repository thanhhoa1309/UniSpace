using Microsoft.Extensions.Configuration;
using UniSpace.BusinessObject.DTOs.AuthDTOs;

namespace UniSpace.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto?> RegisterUserAsync(UserRegistrationDto registrationDto);

        Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginDto, IConfiguration configuration);

        Task<bool> LogoutAsync(Guid userId);
    }
}
