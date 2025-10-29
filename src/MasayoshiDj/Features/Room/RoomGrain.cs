using MasayoshiDj.ActorSystem.Generated;
using Proto;
using Proto.Cluster;

namespace MasayoshiDj.Features.Room;

public class RoomGrain(IContext context, ClusterIdentity identity) : RoomGrainBase(context)
{
    public override Task<BartResponse> SayTheLineBart()
    {
        return Task.FromResult(
            new BartResponse
            {
                TheLine = "yo im live"
            }
        );
    }
}
