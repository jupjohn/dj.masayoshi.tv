using Proto.Cluster;

namespace MasayoshiDj.ActorSystem;

public class ActorSystemManager(Proto.ActorSystem actorSystem) : IHostedService
{
    public Task StartAsync(CancellationToken cancellation) =>
        actorSystem.Cluster().StartMemberAsync();

    public Task StopAsync(CancellationToken cancellationToken) =>
        actorSystem.Cluster().ShutdownAsync();
}
