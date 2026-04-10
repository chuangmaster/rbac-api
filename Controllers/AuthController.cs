using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RbacApi.Models.Requests;
using RbacApi.Models.Responses;
using RbacApi.Services.Interfaces;

namespace RbacApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;

    public AuthController(IAuthService authService, IJwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
    }

    /// <summary>
    /// 使用員工編號與密碼登入，取得 RS256 簽章的 JWT
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.EmployeeId, request.Password);
        if (result is null)
            return Unauthorized(new { message = "員工編號或密碼錯誤" });

        return Ok(result);
    }

    /// <summary>
    /// 使用 refresh token 換發新的 access token 與 refresh token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshAsync(request.RefreshToken);
        if (result is null)
            return Unauthorized(new { message = "Refresh token 無效或已過期" });

        return Ok(result);
    }

    /// <summary>
    /// 登出：撤銷 refresh token
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        return NoContent();
    }

    /// <summary>
    /// 回傳 RSA 公鑰 (JWK 格式)，外部服務可用此公鑰離線驗證 JWT 簽章
    /// </summary>
    [HttpGet("public-key")]
    public IActionResult PublicKey()
    {
        var jwk = _jwtService.GetPublicKeyJwkJson();
        return Content(jwk, "application/json");
    }

    /// <summary>
    /// 取得目前登入使用者資訊 (從 JWT 解析)
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub")
                  ?? string.Empty;
        var name = User.FindFirstValue("name") ?? string.Empty;
        var permissions = User.FindAll("permissions").Select(c => c.Value).ToList();

        return Ok(new UserInfoResponse
        {
            Sub = sub,
            Name = name,
            Permissions = permissions
        });
    }
}
