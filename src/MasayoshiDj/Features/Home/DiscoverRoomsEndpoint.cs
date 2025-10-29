namespace MasayoshiDj.Features.Home;

public class DiscoverRoomsEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/v1/rooms/discover");
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken cancellation)
    {
        return Send.HtmlAsync(
            """
            <a href="/~/masayoshi"><li>Masayoshi</li></a>
            """,
            cancellation
        );
    }
}
