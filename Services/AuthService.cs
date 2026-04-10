using RbacApi.Models.Responses;
using RbacApi.Repositories.Interfaces;
using RbacApi.Services.Interfaces;

namespace RbacApi.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthService(
        IUserRepository userRepository,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<LoginResponse?> LoginAsync(string employeeId, string password)
    {
        var user = await _userRepository.GetByEmployeeIdAsync(employeeId);
        if (user is null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        var permissions = await _userRepository.GetPermissionsByUserIdAsync(user.Id);

        var accessToken = _jwtService.GenerateToken(
            subject: user.EmployeeId,
            name: user.Name,
            permissions: permissions);

        var refreshToken = await _refreshTokenService.CreateAsync(user.EmployeeId);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 1800,
            Permissions = permissions,
        };
    }

    public async Task<LoginResponse?> RefreshAsync(string refreshToken)
    {
        var employeeId = await _refreshTokenService.ValidateAsync(refreshToken);
        if (employeeId is null)
            return null;

        // 撤銷舊 token（rotation）
        await _refreshTokenService.RevokeAsync(refreshToken);

        var user = await _userRepository.GetByEmployeeIdAsync(employeeId);
        if (user is null)
            return null;

        var permissions = await _userRepository.GetPermissionsByUserIdAsync(user.Id);

        var accessToken = _jwtService.GenerateToken(
            subject: user.EmployeeId,
            name: user.Name,
            permissions: permissions);

        var newRefreshToken = await _refreshTokenService.CreateAsync(user.EmployeeId);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 1800,
            Permissions = permissions,
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _refreshTokenService.RevokeAsync(refreshToken);
    }
}
