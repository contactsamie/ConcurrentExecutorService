using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private ActorSystemCreator ActorSystemCreator { set; get; }
        private TimeSpan MaxExecutionTimePerAskCall { set; get; }

        public ConcurrentExecutorService(TimeSpan maxExecutionTimePerAskCall  ,string serverActorSystemName = null, ActorSystem actorSystem = null, string actorSystemConfig = null)
        {
            ActorSystemCreator=new ActorSystemCreator();

            if (string.IsNullOrEmpty(serverActorSystemName) && actorSystem == null)
            {
                serverActorSystemName = Guid.NewGuid().ToString();
            }

            ActorSystemCreator.CreateOrSetUpActorSystem( serverActorSystemName ,  actorSystem ,  actorSystemConfig );
            ReceptionActorRef= ActorSystemCreator.ServiceActorSystem.ActorOf(Props.Create(() => new ReceptionActor()));
            MaxExecutionTimePerAskCall = maxExecutionTimePerAskCall;
        }

        private IActorRef ReceptionActorRef { get; set; }

        public async Task<TResult> GoAsync< TResult>(Func< TResult> operation, string id) where TResult : class
        {
          var result=await  ReceptionActorRef.Ask<IConcurrentExecutorResponseMessage>(new SetWorkMessage(id, new WorkFactory(operation)), MaxExecutionTimePerAskCall);

             return  (result as SetWorkCompletedMessage)?.Result as TResult;
        }
        public async Task<TResult> GoAsync< TResult>(Func<Task<TResult>> operation, string id) where TResult : class
        {
            var result = await ReceptionActorRef.Ask<IConcurrentExecutorResponseMessage>(new SetWorkMessage(id, new WorkFactory(operation)), MaxExecutionTimePerAskCall);

            return  (result as SetWorkCompletedMessage)?.Result as TResult;
        }
    }
}
