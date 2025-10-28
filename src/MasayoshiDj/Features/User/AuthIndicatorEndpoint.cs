using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using MasayoshiDj.Features.Authentication.Twitch;
using Void = FastEndpoints.Void;

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
        if (HttpContext.User.Identity?.IsAuthenticated is not true)
        {
            // not authed
            return Send.HtmlAsync("<a href=\"/login\">Login with Twitch</a>", cancellation);
        }

        var (id, _, displayName) = HttpContext.User.Twitch!.Value;
        return Send.HtmlAsync(
            $"""
             <span>Logged in as {displayName} ({id})</span>
             <br>
             <a href="/logout">Logout</a>
             """, cancellation);
    }
}

public static class ResponseExtensions
{
    extension<T1, T2>(ResponseSender<T1, T2> sender) where T1 : notnull
    {
        // TODO(jupjohn): move out to shared extension class
        public Task<Void> HtmlAsync(
            [StringSyntax("html")] string html,
            CancellationToken cancellation = default
        ) => sender.StringAsync(html, StatusCodes.Status200OK, "text/html; charset=utf-8", cancellation);
    }
}
