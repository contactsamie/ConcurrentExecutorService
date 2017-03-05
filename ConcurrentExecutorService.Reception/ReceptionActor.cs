using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Routing;
using ConcurrentExecutorService.Messages;
using ConcurrentExecutorService.ServiceWorker;

namespace ConcurrentExecutorService.Reception
{
    public class ReceptionActor:ReceiveActor
    {
        private Dictionary<string, WorkerStatus> ServiceWorkerStore { set; get; }

        public ReceptionActor()
        {
            ServiceWorkerStore=new Dictionary<string, WorkerStatus>();
            var serviceWorkerActorRef = Context.ActorOf(Props.Create(() =>new ServiceWorkerActor()).WithRouter(new RoundRobinPool(100)));
            Receive<SetWorkMessage>(message =>
            {
                if (ServiceWorkerStore.ContainsKey(message.Id))
                {
                    Sender.Tell(new SetWorkErrorMessage($"Duplicate work ID: {message.Id}",message.Id));
                }
                else
                {
                ServiceWorkerStore.Add(message.Id, new WorkerStatus()
                {
                    CreatedDateTime = DateTime.UtcNow
                });
                    serviceWorkerActorRef.Forward(message);
                }
            });
            Receive<SetWorkErrorMessage>(message =>
            {
                RemoveWorkerFromDictionary(message.WorkerId);
            });
            Receive<SetWorkCompletedMessage>(message =>
            {
                RemoveWorkerFromDictionary(message.Id);
            });
        }

        private void RemoveWorkerFromDictionary(string workerId)
        {
            if (ServiceWorkerStore.ContainsKey(workerId))
                ServiceWorkerStore.Remove(workerId);
        }
    }

    
}
