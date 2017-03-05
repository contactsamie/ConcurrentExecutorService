using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using ConcurrentExecutorService.ActorSystemFactory;

namespace ConcurrentExecutorServiceLib
{
    public class ConcurrentExecutorService
    {
        private ActorSystemCreator ActorSystemCreator { set; get; }

        public ConcurrentExecutorService(string serverActorSystemName = null, ActorSystem actorSystem = null, string actorSystemConfig = null)
        {
            ActorSystemCreator=new ActorSystemCreator();
            ActorSystemCreator.CreateOrSetUpActorSystem( serverActorSystemName ,  actorSystem ,  actorSystemConfig );
          //  ActorSystemCreator.ServiceActorSystem
        }

        public async Task<TResult> GoAsync<TCommand, TResult, TIdentity>(TCommand command, Func<TCommand, TResult> operation, TIdentity id)
        {
            return await Task.FromResult(operation(command));
        }
        public async Task<TResult> GoAsync<TCommand, TResult,TIdentity>(TCommand command,Func<TCommand,Task<TResult>> operation, TIdentity id)
        {
            return await operation(command);
        }
    }
}
