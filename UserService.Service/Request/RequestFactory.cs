using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using UserService.Model.DTO.Notify;

namespace UserService.Service.Request;

public class RequestFactory(IOptions<RequestDomains> options)
{
    private readonly RequestDomains _domains = options.Value;
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public HttpRequestMessage NotifySendFriendRequest(FriendRequestDTO body, CancellationToken ct)
    {
        var url = $"{_domains.AuthService}{RequestPath.InviteNotify}";
        var json = JsonSerializer.Serialize(body, JsonOptions);
        return new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }
}