using System.ComponentModel.DataAnnotations;

namespace MasayoshiDj.Authentication;

public class AuthenticationOptions : IValidatableObject, IKeyedOptions
{
    public static string SectionKey => "Authentication";

    /// <summary>
    /// How long should cookies last after their last use
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public required TimeSpan CookieExpirationTime { get; init; }

    // TODO(jupjohn): existing connection string validator for sqlite?
    [Required(AllowEmptyStrings = false)]
    public required string TokenDatabaseConnection { get; init; }

    private static readonly TimeSpan MaximumCookieAge = TimeSpan.FromDays(7);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CookieExpirationTime <= TimeSpan.Zero || CookieExpirationTime >= MaximumCookieAge)
        {
            yield return ValidationResult.ForField(
                $"Expiry time must be a positive value, less than {(int)MaximumCookieAge.TotalDays} day(s).",
                nameof(MaximumCookieAge)
            );
        }
    }
}
