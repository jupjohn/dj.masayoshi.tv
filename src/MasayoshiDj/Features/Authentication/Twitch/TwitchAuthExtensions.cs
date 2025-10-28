using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace MasayoshiDj.Features.Authentication.Twitch;

public static class TwitchAuthExtensions
{
    public static IServiceCollection AddTwitchAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBoundOptions<TwitchAuthOptions>();

        services.AddOpenIddict()
            .AddClient(options => options.UseWebProviders()
                .AddTwitch(twitchOptions =>
                {
                    var authOptions = configuration
                        .GetRequiredSection(TwitchAuthOptions.SectionKey)
                        .Get<TwitchAuthOptions>();
                    if (authOptions is null)
                    {
                        // TODO(jupjohn): implement me!
                        throw new NotImplementedException("TODO(jupjohn): implement me!");
                    }

                    twitchOptions.SetProviderName(TwitchAuthConstants.AuthenticationScheme);

                    twitchOptions.SetRedirectUri(authOptions.CallbackPath);
                    twitchOptions.SetClientId(authOptions.ClientId);
                    twitchOptions.SetClientSecret(authOptions.ClientSecret);
                    twitchOptions.AddScopes(authOptions.Scopes);
                }));

        services.AddHttpClient(
                TwitchAuthConstants.BackChannelHttpClientKey,
                (serviceProvider, client) =>
                {
                    client.BaseAddress = new Uri("https://api.twitch.tv/helix/users");
                    var authOptions = serviceProvider.GetRequiredService<IOptions<TwitchAuthOptions>>();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("Client-Id", authOptions.Value.ClientId);
                })
            // TODO(jupjohn): polly
            .AddAsKeyed();

        return services;
    }
}

public static class TwitchClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal principal)
    {
        public TwitchUser? Twitch => TwitchUser.FromPrincipal(principal);
    }
}

public readonly record struct TwitchUser(string Id, string Login, string DisplayName)
{
    internal static TwitchUser? FromPrincipal(ClaimsPrincipal principal)
    {
        var id = principal.FindFirst(TwitchAuthConstants.Claims.Id)?.Value;
        var login = principal.FindFirst(TwitchAuthConstants.Claims.Login)?.Value;
        var displayName = principal.FindFirst(TwitchAuthConstants.Claims.DisplayName)?.Value;

        if (id is null || login is null || displayName is null)
        {
            return null;
        }

        return new TwitchUser(id, login, displayName);
    }
}
