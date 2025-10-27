using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using MasayoshiDj.Authentication.Twitch;
using Void = FastEndpoints.Void;

namespace MasayoshiDj.Landing;

// TODO(jupjohn): this is temporary. It should be served from a static HTML file
public class LandingPage : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/");
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken cancellation)
    {
        var isAuthed = HttpContext.User.Identity?.IsAuthenticated ?? false;
        if (isAuthed)
        {
            var (id, _, displayName) = HttpContext.User.Twitch!.Value;
            return Send.HtmlAsync(
                $"""
                 <a href="/logout">Logout</a>
                 <br/>
                 <span>👋 Hi {displayName} ({id})</span>
                 """, cancellation);
        }

        return Send.HtmlAsync("""<a href="/login">Log in with Twitch</a>""", cancellation);
    }
}

public static class ResponseExtensions
{
    extension<T1, T2>(ResponseSender<T1, T2> sender) where T1 : notnull
    {
        public Task<Void> HtmlAsync(
            [StringSyntax("html")] string html,
            CancellationToken cancellation = default
        ) => sender.StringAsync(html, StatusCodes.Status200OK, "text/html; charset=utf-8", cancellation);
    }
}
