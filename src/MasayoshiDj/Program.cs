using FastEndpoints;
using MasayoshiDj.Features.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationAuthentication();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddFastEndpoints();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseFastEndpoints();

app.UseDefaultFiles();
app.UseStaticFiles();

await app.InitializeAuthenticationAsync();

app.Run();
