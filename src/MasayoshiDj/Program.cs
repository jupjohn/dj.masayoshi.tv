using MasayoshiDj;
using MasayoshiDj.Features.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationAuthentication();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddFastEndpoints(options =>
{
    options.DisableAutoDiscovery = true;
    options.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All;
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.UseFastEndpoints(config =>
{
    config.Binding.ReflectionCache.AddFromMasayoshiDj();
});

app.UseDefaultFiles();
app.UseStaticFiles();

await app.InitializeAuthenticationAsync();

app.Run();
