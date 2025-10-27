namespace MasayoshiDj.Authentication.Twitch;

public static class TwitchAuthConstants
{
    public const string AuthenticationScheme = "Twitch";

    public const string BackChannelHttpClientKey = "twitch-oauth-backchannel";

    public static class Claims
    {
        public const string Id = "twitch:user_id";
        public const string Login = "twitch:login";
        public const string DisplayName = "twitch:display_name";
    }
}
