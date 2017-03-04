using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace ConcurrentExecutorService.ActorSystemFactory
{
    public class ActorSystemCreator
    {
        
        public void CreateOrSetUpActorSystem(string serverActorSystemName = null, ActorSystem actorSystem = null, string actorSystemConfig = null)
        {
          
                ServiceActorSystem = string.IsNullOrEmpty(serverActorSystemName)
                  ? actorSystem
                  : (string.IsNullOrEmpty(actorSystemConfig)
                      ? Akka.Actor.ActorSystem.Create(serverActorSystemName)
                      : Akka.Actor.ActorSystem.Create(serverActorSystemName, actorSystemConfig));

            if (ServiceActorSystem != null) return;
            const string message = "Invalid ActorSystemName.Please set up 'ServerActorSystemName' in the config file";
          
            throw new Exception(message);
        }

        public ActorSystem ServiceActorSystem { get; set; }

        public void TerminateActorSystem()
        {
            ServiceActorSystem?.Terminate().Wait();
            ServiceActorSystem?.Dispose();
        }
    }

}
