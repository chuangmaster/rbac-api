namespace RbacApi.Services.Interfaces;

public interface IRefreshTokenService
{
    /// <summary>
    /// 產生 refresh token 並存入 Redis，關聯到指定的 employeeId
    /// </summary>
    Task<string> CreateAsync(string employeeId);

    /// <summary>
    /// 驗證 refresh token，回傳關聯的 employeeId；無效則回傳 null
    /// </summary>
    Task<string?> ValidateAsync(string refreshToken);

    /// <summary>
    /// 撤銷指定的 refresh token
    /// </summary>
    Task RevokeAsync(string refreshToken);
}
