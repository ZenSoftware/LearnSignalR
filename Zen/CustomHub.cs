using Microsoft.AspNetCore.SignalR;

namespace Zen;

public record Data(int Id, string Message);

public interface IClientInterface
{
    Task ClientHook(Data data);
}

public class CustomHub : Hub<IClientInterface>
{
    private readonly ILogger<CustomHub> _logger;
    public CustomHub(ILogger<CustomHub> logger) => _logger = logger;
    public void ServerHook(Data data)
    {
        _logger.LogInformation("Receiving data: {0}, {1}", data, Context.ConnectionId);
    }

    public Task PingAll()
    {
        _logger.LogInformation("pinging everyone");
        return Clients.All.ClientHook(new(111, "ping all"));
    }

    public Task SelfPing()
    {
        _logger.LogInformation("self pinging");
        return Clients.Caller.ClientHook(new(222, "self ping"));
    }

    [HubMethodName("invocation_with_return")]
    public Data JustAFunction()
    {
        return new(1, "returned data from JustAFunction");
    }

    [Authorize(Roles = "Super")]
    public object TokenProtected()
    {
        return CompileResult();
    }

    private object CompileResult() =>
            new
            {
                UserId = Context.UserIdentifier,
                Claims = Context.User.Claims.Select(x => new { x.Type, x.Value })
            };
}