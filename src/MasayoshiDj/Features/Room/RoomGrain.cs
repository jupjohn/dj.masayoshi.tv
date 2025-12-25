using System.Diagnostics.CodeAnalysis;
using Google.Protobuf.WellKnownTypes;
using MasayoshiDj.ActorSystem.Generated;
using MasayoshiDj.Observability;
using Microsoft.AspNetCore.WebUtilities;
using Proto;
using Proto.Cluster;

namespace MasayoshiDj.Features.Room;

public class RoomGrain(IContext context, ClusterIdentity identity) : RoomGrainBase(context)
{
    private readonly Queue<QueuedMedia> _tempQueue = new();

    public override async Task<QueueResponse> ListQueue()
    {
        await Task.Yield();
        // NOTE(jupjohn): temp, will be fed up from child queue actor
        return new QueueResponse
        {
            Media = { _tempQueue.Select(item => new QueueItem { Title = item.Title, Url = item.Url.ToString() }) }
        };
    }

    public override async Task<MediaEnqueueResponse> QueueMedia(EnqueueMediaRequest request)
    {
        // ReSharper disable once ExplicitCallerInfoArgument - TODO(jupjohn): move to .editorconfig
        var activity = AppSource.StartActivity(ActivityNames.RoomMediaEnqueue);
        activity?.EnrichWithRoomEnqueueMedia(request);

        await Task.Yield();

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var mediaUri))
        {
            activity?.AddEvent(RoomGrainEvents.MediaNotFound);
            return new MediaEnqueueResponse { Failure = "Invalid URL" };
        }

        // NOTE(jupjohn): temp, media fetching will be pushed out
        var onlySegment = mediaUri.Segments.SingleOrDefault();
        var ytVideoId = mediaUri.Host switch
        {
            "youtu.be" when onlySegment is not null => onlySegment,
            "youtube.com" when onlySegment == "watch" && TryGetQueryParameterValue(mediaUri, "v", out var videoId) => videoId,
            _ => null
        };

        if (ytVideoId is null)
        {
            activity?.AddEvent(RoomGrainEvents.MediaNotFound);
            return new MediaEnqueueResponse { UnsupportedMediaUrl = new Empty() };
        }

        activity?.EnrichWithRoomEnqueueMediaYouTubeVideoId(ytVideoId);

        _tempQueue.Enqueue(new QueuedMedia($"ytid:{ytVideoId}", new Uri($"https://youtu.be/{ytVideoId}")));
        activity?.AddEvent(RoomGrainEvents.MediaEnqueued);

        return new MediaEnqueueResponse { Success = new Empty() };
    }

    private static bool TryGetQueryParameterValue(Uri uri, string name, [NotNullWhen(true)] out string? value)
    {
        var enumerable = new QueryStringEnumerable(uri.Query);
        foreach (var pair in enumerable)
        {
            if (pair.DecodeName().Span.SequenceEqual(name.AsSpan()))
            {
                value = new string(pair.DecodeValue().Span);
                return true;
            }
        }

        value = null;
        return false;
    }

    private readonly record struct QueuedMedia(
        string Title,
        Uri Url
    );
}
