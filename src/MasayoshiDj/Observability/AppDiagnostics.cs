namespace MasayoshiDj.Observability;

public static class AppDiagnostics
{
    public const string SourceName = "masayoshi-dj";
}

public static class AppDiagnosticSource
{
    public static readonly ActivitySource AppSource = new(AppDiagnostics.SourceName);
}
