using System.Collections.Concurrent;
using System.Collections.Frozen;
using Dunet;

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
public class RoomExistsQuery : ICommandHandler<FindRoom, RoomResult>
{
    private static readonly FrozenDictionary<string, Room> InMemoryRooms = new Dictionary<string, Room>
    {
        { "masayoshi", new Room { RoomId = "46673989" } }
    }.ToFrozenDictionary();

    public async Task<RoomResult> ExecuteAsync(FindRoom command, CancellationToken cancellation)
    {
        // would be a DB/cache/actor lookup
        await Task.Yield();
        var login = command.Login;

        // TODO(jupjohn): move out to validator
        if (string.IsNullOrEmpty(login))
        {
            return new RoomResult.NotFound();
        }

        if (!InMemoryRooms.TryGetValue(login, out var foundRoom))
        {
            return new RoomResult.NotFound();
        }

        var queue = await foundRoom.ListQueuedMedia(cancellation);
        return new RoomResult.Found(foundRoom.RoomId, queue);
    }
}

public record FindRoom(string Login) : ICommand<RoomResult>;

[Union]
public partial record RoomResult
{
    public partial record Found(string Id, QueueItem[] QueuedItems);
    public partial record NotFound;
}

public class Room
{
    private readonly ConcurrentQueue<QueueItem> _queue = new();

    public required string RoomId { get; init; }

    public async Task Enqueue(Uri media, CancellationToken _)
    {
        // NOTE(jupjohn): this media metadata fetch will happen inside an actor with the name of the media (i.e. "media-youtube-OU_M2nNErMA")
        //      which indirectly will be persisted to DB. Treating actors as a cache layer makes life so much easier
        await Task.Yield();
        _queue.Enqueue(new QueueItem(media.ToString(), media));
    }

    public async Task<QueueItem[]> ListQueuedMedia(CancellationToken _)
    {
        await Task.Yield();
        return _queue.ToArray();
    }
}

public readonly record struct QueueItem(string Title, Uri? Url = null);
