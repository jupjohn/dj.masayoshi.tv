using System.ComponentModel.DataAnnotations;

namespace MasayoshiDj.Authentication.Twitch;

public class TwitchAuthOptions : IValidatableObject, IKeyedOptions
{
    public static string SectionKey => "Authentication:Twitch";

    [Required(AllowEmptyStrings = false)]
    [RegularExpression("^[a-z0-9]+$")]
    public required string ClientId { get; init; }

    [Required(AllowEmptyStrings = false)]
    [RegularExpression("^[a-z0-9]+$")]
    public required string ClientSecret { get; init; }

    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@"^(/[^\s?/]+)+$")]
    public required string CallbackPath { get; init; }

    public required string[] Scopes { get; init; } = [];

    private static readonly string[] RequiredScopes = [
        // Required by Twitch
        "user:read:email",
        // Required for this app's auth flow
        "openid"
    ];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Scopes.Length == 0)
        {
            yield return ValidationResult.ForField("One or more scopes are required.", nameof(Scopes));
        }

        foreach (var requiredScope in RequiredScopes)
        {
            if (Scopes.Contains(requiredScope))
            {
                continue;
            }

            yield return ValidationResult.ForField($"\"{requiredScope}\" is a required scope.", nameof(Scopes));
        }
    }
}
