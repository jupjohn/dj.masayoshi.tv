using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace MasayoshiDj;

public static class OptionsExtensions
{
    extension(ValidationResult)
    {
        public static ValidationResult ForField(string errorMessage, string field) => new(errorMessage, [field]);
    }

    extension(IServiceCollection services)
    {
        public OptionsBuilder<T> AddBoundOptions<T>() where T : class, IKeyedOptions =>
            services.AddOptions<T>()
                .BindConfiguration(T.SectionKey)
                .ValidateDataAnnotations()
                .ValidateOnStart();
    }
}

public interface IKeyedOptions
{
    /// <summary>
    /// The configuration key for this section
    /// </summary>
    public static abstract string SectionKey { get; }
}
