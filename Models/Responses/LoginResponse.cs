namespace RbacApi.Models.Responses;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public int ExpiresIn { get; set; }

    public List<string> Permissions { get; set; } = [];
}
