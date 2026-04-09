namespace RbacApi.Models.Responses;

public class UserInfoResponse
{
    public string Sub { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public List<string> Permissions { get; set; } = [];
}
