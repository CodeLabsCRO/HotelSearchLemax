using HotelSearchLemax.Core.DTOs;

namespace HotelSearchLemax.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
}
