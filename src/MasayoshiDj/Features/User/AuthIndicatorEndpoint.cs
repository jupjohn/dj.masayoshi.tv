namespace MasayoshiDj.Features.User;

// TODO(jupjohn): give this a better name
public class AuthIndicatorEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        // TODO(jupjohn): move api/v1 prefix out to group
        Get("/api/v1/user/auth-indicator");
        AllowAnonymous(Http.GET);
    }

    public override Task HandleAsync(CancellationToken cancellation)
    {
        var user = HttpContext.User.Twitch;
        if (user is null)
        {
            // not authed
            return Send.HtmlAsync("<a href=\"/login\">Login with Twitch</a>", cancellation);
        }

        var (id, _, displayName) = user.Value;
        return Send.HtmlAsync(
            $"""
             <span>Logged in as {displayName} ({id})</span>
             <br>
             <a href="/logout">Logout</a>
             """, cancellation);
    }
}
