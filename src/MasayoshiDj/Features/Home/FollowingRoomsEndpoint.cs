using FastEndpoints;
using MasayoshiDj.Features.Authentication.Twitch;

namespace MasayoshiDj.Features.Home;

public class FollowingRoomsEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/v1/rooms/following");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken cancellation)
    {
        var user = HttpContext.User.Twitch;
        if (user is null)
        {
            await Send.HtmlAsync(
                """
                <span>
                Log in to see :)
                </span>
                """,
                cancellation
            );
            return;
        }

        await Task.Delay(100, cancellation);
        await Send.HtmlAsync(
            """
            <span>
            not implemented
            </span>
            """,
            cancellation
        );
    }
}
