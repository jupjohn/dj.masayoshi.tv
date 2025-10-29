using Dunet;
using MasayoshiDj.ActorSystem.Generated;
using Proto.Cluster;

namespace MasayoshiDj.Features.Room;

public class RoomEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/~/{RoomName}");
        AllowAnonymous();
    }

    // TODO(jupjohn): we're hx-boosting this page, so wrap in HTML shell if HX-Request header is missing
    public override async Task HandleAsync(CancellationToken cancellation)
    {
        // TODO(jupjohn): have parent endpoint/redir
        var roomName = Route<string>("RoomName", isRequired: false);
        if (roomName is null)
        {
            // TODO(jupjohn): show room missing page
            await SendRoomNotFoundAsync(cancellation);
            return;
        }

        var userInfo = "viewing as anonymous";

        var user = HttpContext.User.Twitch;
        if (user is not null)
        {
            userInfo = $"viewing as {user.Value.DisplayName} ({user.Value.Id})";
        }

        var roomLookup = await new FindRoom(roomName)
            .ExecuteAsync(cancellation);
        if (roomLookup is RoomResult.NotFound)
        {
            await SendRoomNotFoundAsync(cancellation);
            return;
        }

        // TODO(jupjohn): switch on type
        var foundRoom = roomLookup.UnwrapFound();
        var queueElements = foundRoom.QueuedItems.Select(item => $"<li>{item.Title}</li>").ToArray();
        var concatQueueElements = queueElements.Length > 0
            ? string.Join(string.Empty, queueElements)
            : "<span>nothing queued</span>";

        await Send.HtmlAsync(
            $"""
             <h1>Room: {foundRoom.Id}</h1>
             <span>{userInfo}</span>
             <br/>
             <br/>
             <span>&gt; media player would be here</span>
             <br/>
             <br/>
             <ol>
             {concatQueueElements}
             </ol>
             """,
            cancellation
        );
    }

    private Task<Void> SendRoomNotFoundAsync(CancellationToken cancellation)
    {
        return Send.HtmlAsync(
            """
            <span>
                room not found!
            </span>
            """,
            cancellation
        );
    }
}

// TODO(jupjohn): extract out
public class RoomExistsQuery(Cluster actorCluster) : ICommandHandler<FindRoom, RoomResult>
{
    private static readonly string[] TemporaryWhitelistedRooms = ["masayoshi"];

    public async Task<RoomResult> ExecuteAsync(FindRoom command, CancellationToken cancellation)
    {
        // TODO(jupjohn): move out to validator
        var login = command.Login.ToLowerInvariant();
        if (string.IsNullOrEmpty(login))
        {
            return new RoomResult.NotFound();
        }

        // NOTE(jupjohn): temp
        if (!TemporaryWhitelistedRooms.Contains(login))
        {
            return new RoomResult.NotFound();
        }

        var queue = await actorCluster.GetRoomGrain(login).ListQueue(cancellation);
        if (queue is null)
        {
            // TODO(jupjohn): handle better
            return new RoomResult.NotFound();
        }

        return new RoomResult.Found("idk_why_we_need_the_id", queue.Media.ToArray());
    }
}

public record FindRoom(string Login) : ICommand<RoomResult>;

[Union]
public partial record RoomResult
{
    public partial record Found(string Id, QueueItem[] QueuedItems);
    public partial record NotFound;
}
