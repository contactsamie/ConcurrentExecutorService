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
        public ConcurrentExecutorService(TimeSpan? maxExecutionTimePerAskCall=null, string serverActorSystemName = null,ActorSystem actorSystem = null, string actorSystemConfig = null)
        {
            ActorSystemCreator = new ActorSystemCreator();

            if (string.IsNullOrEmpty(serverActorSystemName) && actorSystem == null)
            {
                serverActorSystemName = Guid.NewGuid().ToString();
            }

            ActorSystemCreator.CreateOrSetUpActorSystem(serverActorSystemName, actorSystem, actorSystemConfig);
            ReceptionActorRef = ActorSystemCreator.ServiceActorSystem.ActorOf(Props.Create(() => new ReceptionActor()));
            MaxExecutionTimePerAskCall = maxExecutionTimePerAskCall?? TimeSpan.FromSeconds(5);
        }

        private ActorSystemCreator ActorSystemCreator { get; }
        private TimeSpan MaxExecutionTimePerAskCall { get; }

        private IActorRef ReceptionActorRef { get; }

        public async Task ExecuteAsync(Action operation, string id, TimeSpan? maxExecutionTimePerAskCall = null) 
        {
             await ExecuteAsync<object>(async () => {
                operation();
                return await Task.FromResult(new object());
            }, id, maxExecutionTimePerAskCall);
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<TResult> operation, string id, TimeSpan? maxExecutionTimePerAskCall=null) where TResult : class
        {
           return await ExecuteAsync<TResult>(async() => await Task.FromResult(operation()), id, maxExecutionTimePerAskCall);
        }
        public async Task<TResult> ExecuteAsync<TResult>(Func<Task<object>> operation, string id,TimeSpan? maxExecutionTimePerAskCall=null) where TResult : class
        {
            var result = await ReceptionActorRef.Ask<IConcurrentExecutorResponseMessage>(new SetWorkMessage(id, new WorkFactory(operation)), maxExecutionTimePerAskCall?? MaxExecutionTimePerAskCall) .ConfigureAwait(false);

            return (result as SetWorkCompletedMessage)?.Result as TResult;
        }
    }
}