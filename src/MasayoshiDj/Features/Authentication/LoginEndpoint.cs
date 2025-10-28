using FastEndpoints;
using MasayoshiDj.Features.Authentication.Twitch;
using Microsoft.AspNetCore.Authentication;

namespace MasayoshiDj.Features.Authentication;

public class LoginEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/login");
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken cancellation)
    {
        var challenge = TypedResults.Challenge(new AuthenticationProperties
        {
            RedirectUri = Query<string>("returnUrl", isRequired: false) ?? "/"
        }, [TwitchAuthConstants.AuthenticationScheme]);
        return Send.ResultAsync(challenge);
    }
}
