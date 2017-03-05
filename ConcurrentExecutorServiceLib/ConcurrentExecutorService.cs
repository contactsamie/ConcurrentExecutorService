using System;
using System.Threading.Tasks;
using Akka.Actor;
using ConcurrentExecutorService.ActorSystemFactory;
using ConcurrentExecutorService.Common;
using ConcurrentExecutorService.Messages;
using ConcurrentExecutorService.Reception;

namespace ConcurrentExecutorServiceLib
{
    public class ConcurrentExecutorService
    {
        public ConcurrentExecutorService(TimeSpan maxExecutionTimePerAskCall, string serverActorSystemName = null,
            ActorSystem actorSystem = null, string actorSystemConfig = null)
        {
            ActorSystemCreator = new ActorSystemCreator();

            if (string.IsNullOrEmpty(serverActorSystemName) && actorSystem == null)
            {
                serverActorSystemName = Guid.NewGuid().ToString();
            }

            ActorSystemCreator.CreateOrSetUpActorSystem(serverActorSystemName, actorSystem, actorSystemConfig);
            ReceptionActorRef = ActorSystemCreator.ServiceActorSystem.ActorOf(Props.Create(() => new ReceptionActor()));
            MaxExecutionTimePerAskCall = maxExecutionTimePerAskCall;
        }

        private ActorSystemCreator ActorSystemCreator { get; }
        private TimeSpan MaxExecutionTimePerAskCall { get; }

        private IActorRef ReceptionActorRef { get; }


        public async Task<TResult> GoAsync<TResult>(Func<Task<object>> operation, string id) where TResult : class
        {
            var result =
                await
                    ReceptionActorRef.Ask<IConcurrentExecutorResponseMessage>(
                        new SetWorkMessage(id, new WorkFactory(operation)), MaxExecutionTimePerAskCall)
                        .ConfigureAwait(false);

            return (result as SetWorkCompletedMessage)?.Result as TResult;
        }
    }
}