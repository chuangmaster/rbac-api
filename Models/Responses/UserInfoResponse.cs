using System.Text.Json.Serialization;

namespace RbacApi.Models.Responses;

public class UserInfoResponse
{
    [JsonPropertyName("sub")]
    public string Sub { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("permissions")]
    public List<string> Permissions { get; set; } = [];
}
