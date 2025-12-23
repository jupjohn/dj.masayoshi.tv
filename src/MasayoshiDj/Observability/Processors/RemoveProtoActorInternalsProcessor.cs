using System.Buffers;
using OpenTelemetry;
using Proto.Cluster;

namespace MasayoshiDj.Observability.Processors;

// TODO(jupjohn): this should be done in the collector, but this is enough for now to reduce throughput
/// <summary>
/// Prevents protoactor-only spans from being recorded, as they're noisy with gossip updates.
/// </summary>
public class RemoveProtoActorInternalsProcessor(Cluster cluster) : BaseProcessor<Activity>
{
    private readonly SearchValues<string> _knownClusterKinds = SearchValues.Create(
        cluster.Config.ClusterKinds.Select(kind => kind.Name).ToArray(),
        StringComparison.OrdinalIgnoreCase
    );

    public override void OnEnd(Activity activity)
    {
        var isError =
            activity.Status == ActivityStatusCode.Error ||
            activity.Events.Any(e => e.Name == "exception");

        if (isError)
        {
            return;
        }

        var sourceName = activity.Source.Name;
        var isRecordable = sourceName switch
        {
            "System.Net.Http" when HttpActivityIsProtoActor() => false,
            "OpenTelemetry.Instrumentation.GrpcNetClient"
                when GrpcActivityIsProtoActor() && !AnySpansIncludeClusterKinds() => false,
            _ when AllSpansAreProtoActor() && !AnySpansIncludeClusterKinds() => false,
            _ => true
        };

        if (!isRecordable)
        {
            activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
        }

        return;

        bool AllSpansAreProtoActor() =>
            EnumerateSpanHierarchy(activity)
                .Select(span => span.Source.Name)
                .All(name => name.StartsWith("proto.actor", StringComparison.InvariantCultureIgnoreCase));

        bool AnySpansIncludeClusterKinds() =>
            EnumerateSpanHierarchy(activity)
                .SelectMany(span => (IEnumerable<string?>)[
                    span.GetTagItem("proto.actortype") as string,
                    span.GetTagItem("proto.messagetype") as string,
                    span.OperationName
                ])
                .Where(val => val is not null)
                .Cast<string>()
                .Any(val => val.ContainsAny(_knownClusterKinds));

        bool HttpActivityIsProtoActor() =>
            activity.GetTagItem("url.full") is string url &&
            url.EndsWith("/remote.Remoting/Receive");

        bool GrpcActivityIsProtoActor() =>
            activity.GetTagItem("grpc.method") is string url &&
            url.StartsWith("/remote.Remoting/");
    }

    private static IEnumerable<Activity> EnumerateSpanHierarchy(Activity sourceSpan)
    {
        var current = sourceSpan;
        do
        {
            yield return current;
            current = current.Parent;
        } while (current is not null);
    }
}
