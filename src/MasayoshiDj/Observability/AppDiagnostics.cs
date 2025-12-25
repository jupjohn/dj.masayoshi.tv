using MasayoshiDj.ActorSystem.Generated;

namespace MasayoshiDj.Observability;

using Prefix = ActivityPrefixes;

public static class AppDiagnostics
{
    public const string SourceName = "masayoshi-dj";
}

public static class AppDiagnosticSource
{
    public static readonly ActivitySource AppSource = new(AppDiagnostics.SourceName);
}

public static class ActivityNames
{
    public const string RoomMediaEnqueue = "Enqueue media with room";
}

public static class ActivityPrefixes
{
    private const string RoomGrain = "room";
    public const string RoomGrainEnqueueMedia = $"{RoomGrain}.enqueue";
}

public static class RoomGrainEvents
{
    public static ActivityEvent MediaEnqueued => new($"{Prefix.RoomGrainEnqueueMedia}.enqueued");
    public static ActivityEvent MediaNotFound => new($"{Prefix.RoomGrainEnqueueMedia}.media_not_found");
}

public static class ActivityEnrichment
{
    extension(Activity source)
    {
        public Activity EnrichWithRoomEnqueueMedia(EnqueueMediaRequest request) => source
            .SetTag($"{Prefix.RoomGrainEnqueueMedia}.media_url", request.Url);
        public Activity EnrichWithRoomEnqueueMediaYouTubeVideoId(string videoId) => source
            .SetTag($"{Prefix.RoomGrainEnqueueMedia}.youtube.video_id", videoId);
    }
}
