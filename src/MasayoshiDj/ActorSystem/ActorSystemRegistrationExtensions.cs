using MasayoshiDj.ActorSystem.Generated;
using MasayoshiDj.Features.Room;
using Proto;
using Proto.Cluster;
using Proto.Cluster.Partition;
using Proto.Cluster.Testing;
using Proto.DependencyInjection;
using Proto.OpenTelemetry;
using Proto.Remote;
using Proto.Remote.GrpcNet;

namespace MasayoshiDj.ActorSystem;

public static class ActorSystemRegistrationExtensions
{
    public const string ClusterName = "MasayoshiDJ";

    public static void AddActorSystem(this WebApplicationBuilder builder)
    {
        builder.Services.AddHostedService<ActorSystemManager>();

        builder.Services.AddSingleton(provider =>
        {
            // NOTE(jupjohn): this was taken from protoactor's example, so defaults need to be sanity checked
            var actorSystemConfig = ActorSystemConfig
                .Setup()
                .WithConfigureRootContext(context => context.WithTracing());

            // remote configuration
            var remoteConfig = RemoteConfig
                .BindToLocalhost()
                .WithProtoMessages(MessagesReflection.Descriptor);

            // cluster configuration
            var clusterConfig = ClusterConfig
                .Setup(
                    clusterName: ClusterName,
                    // TODO(jupjohn): move to different provider when deployed
                    clusterProvider: new TestProvider(new TestProviderOptions(), new InMemAgent()),
                    identityLookup: new PartitionIdentityLookup()
                )
                .WithClusterKind(
                    kind: RoomGrainActor.Kind,
                    // NOTE(jupjohn): I still don't like this, but I don't remember how I did it dynamically last time
                    prop: Props.FromProducer(() =>
                        new RoomGrainActor((context, clusterIdentity) =>
                            ActivatorUtilities.CreateInstance<RoomGrain>(provider, context, clusterIdentity)
                        )
                    ).WithTracing()
                );

            // create the actor system
            return new Proto.ActorSystem(actorSystemConfig)
                .WithServiceProvider(provider)
                .WithRemote(remoteConfig)
                .WithCluster(clusterConfig)
                .WithServiceProvider(provider);
        });

        builder.Services.AddTransient(provider => provider
            .GetRequiredService<Proto.ActorSystem>()
            .Cluster()
        );
    }

    public static void UseActorSystem(this WebApplication app)
    {
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        Log.SetLoggerFactory(loggerFactory);
    }
}
