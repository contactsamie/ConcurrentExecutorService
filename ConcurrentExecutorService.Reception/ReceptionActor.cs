using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Routing;
using ConcurrentExecutorService.Messages;
using ConcurrentExecutorService.ServiceWorker;

namespace ConcurrentExecutorService.Reception
{
    public class ReceptionActor : ReceiveActor
    {
        public ReceptionActor()
        {
            ServiceWorkerStore = new Dictionary<string, WorkerStatus>();
            var serviceWorkerActorRef =
                Context.ActorOf(Props.Create(() => new ServiceWorkerActor()).WithRouter(new RoundRobinPool(10)));

            Receive<SetWorkMessage>(message =>
            {
                if (ServiceWorkerStore.ContainsKey(message.Id))
                {
                    Sender.Tell(new SetWorkErrorMessage($"Duplicate work ID: {message.Id}", message.Id));
                }
                else
                {
                    ServiceWorkerStore.Add(message.Id, new WorkerStatus
                    {
                        CreatedDateTime = DateTime.UtcNow
                    });
                    serviceWorkerActorRef.Forward(message);
                }
            });
            Receive<SetWorkErrorMessage>(message => { RemoveWorkerFromDictionary(message.WorkerId); });
            Receive<SetWorkCompletedMessage>(message => { RemoveWorkerFromDictionary(message.Id); });
        }

        private Dictionary<string, WorkerStatus> ServiceWorkerStore { get; }

        private void RemoveWorkerFromDictionary(string workerId)
        {
            if (ServiceWorkerStore.ContainsKey(workerId))
                ServiceWorkerStore.Remove(workerId);
        }
    }
}