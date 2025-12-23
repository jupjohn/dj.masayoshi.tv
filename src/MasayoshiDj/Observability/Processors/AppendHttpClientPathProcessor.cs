using OpenTelemetry;

namespace MasayoshiDj.Observability.Processors;

/// <summary>
/// Appends the path from HttpClient requests to the operation name.
/// Without this, names are just the HTTP method i.e. "GET".
/// </summary>
public class AppendHttpClientPathProcessor : BaseProcessor<Activity>
{
    public override void OnEnd(Activity activity)
    {
        var source = activity.Source.Name;
        if (source != "System.Net.Http")
        {
            return;
        }

        var fullUri = (activity.GetTagItem("url.full") as string)?.Split('?', 2).First();
        var suffix = fullUri ?? activity.GetTagItem("url.path") as string;
        if (suffix is null)
        {
            return;
        }

        activity.DisplayName = $"{activity.DisplayName} {suffix}";
    }
}
