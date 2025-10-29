using System.Diagnostics.CodeAnalysis;

namespace MasayoshiDj.Features;

public static class EndpointExtensions
{
    extension<T1, T2>(ResponseSender<T1, T2> sender) where T1 : notnull
    {
        /// <summary>
        /// Send HTML UTF8-encoded content
        /// </summary>
        /// <param name="html">The HTML to return</param>
        /// <param name="cancellation">A cancellation token to stop sending</param>
        public Task<Void> HtmlAsync(
            [StringSyntax("html")] string html,
            CancellationToken cancellation = default
        ) => sender.StringAsync(html, StatusCodes.Status200OK, "text/html; charset=utf-8", cancellation);
    }
}
