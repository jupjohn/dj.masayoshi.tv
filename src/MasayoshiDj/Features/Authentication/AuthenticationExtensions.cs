using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MasayoshiDj.Features.Authentication;

public static class AuthenticationExtensions
{
    public static WebApplicationBuilder AddApplicationAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddBoundOptions<AuthenticationOptions>();
        builder.Services.AddTwitchAuth(builder.Configuration);

        builder.Services.AddDbContext<AuthenticationDbContext>((services, options) =>
        {
            // TODO(jupjohn): expiry of DB records?
            var authOptions = services.GetRequiredService<IOptions<AuthenticationOptions>>();
            options.UseSqlite(authOptions.Value.TokenDatabaseConnection);
            options.UseOpenIddict();
        });

        builder.Services.AddOpenIddict()
            .AddClient(options =>
            {
                options.AllowAuthorizationCodeFlow();
                options.AllowRefreshTokenFlow();
                options.UseSystemNetHttp();

                if (builder.Environment.IsDevelopment())
                {
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                }
                else
                {
                    // TODO(jupjohn): implement me!
                    throw new NotImplementedException("TODO(jupjohn): implement me!");
                }

                options.UseAspNetCore()
                    .EnableRedirectionEndpointPassthrough();
            })
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<AuthenticationDbContext>();
            });

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = TwitchAuthConstants.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                // Validated elsewhere, assume it's valid
                var authOptions = builder.Configuration
                    .GetRequiredSection(AuthenticationOptions.SectionKey)
                    .Get<AuthenticationOptions>();
                if (authOptions is null)
                {
                    // TODO(jupjohn): implement me!
                    throw new NotImplementedException("TODO(jupjohn): implement me!");
                }

                // TODO(jupjohn): more to fuck around with in here, tinker with it
                options.ExpireTimeSpan = authOptions.CookieExpirationTime;
                options.SlidingExpiration = true;
            });

        builder.Services.AddAuthorization();

        return builder;
    }

    public static async Task InitializeAuthenticationAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
        await context.Database.MigrateAsync();
    }
}
