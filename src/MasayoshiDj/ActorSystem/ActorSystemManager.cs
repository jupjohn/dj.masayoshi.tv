using MasayoshiDj.ActorSystem.Generated;
using Proto.Cluster;

namespace MasayoshiDj.ActorSystem;

public class ActorSystemManager(ILogger<ActorSystemManager> logger, Proto.ActorSystem actorSystem) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellation)
    {
        await actorSystem.Cluster().StartMemberAsync();

        // NOTE(jupjohn): will be removed after rooms are hooked up
        var theLineResponse = await actorSystem.Cluster().GetRoomGrain("testroomxd").SayTheLineBart(cancellation);
        logger.LogInformation("Room response from bart: {TheLine}", theLineResponse?.TheLine ?? "null");
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        actorSystem.Cluster().ShutdownAsync();
}
