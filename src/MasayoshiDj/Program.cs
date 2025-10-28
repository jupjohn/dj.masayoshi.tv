using FastEndpoints;
using MasayoshiDj.Features.Authentication;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    // TODO(jupjohn): don't hardcode this port
    options.ListenAnyIP(7216, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
        listenOptions.UseHttps();
    });
});

builder.AddApplicationAuthentication();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddFastEndpoints();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseFastEndpoints();

await app.InitializeAuthenticationAsync();

app.Run();
