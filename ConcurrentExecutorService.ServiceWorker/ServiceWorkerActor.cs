using System;
using System.Threading.Tasks;
using Akka.Actor;
using ConcurrentExecutorService.Messages;

namespace ConcurrentExecutorService.ServiceWorker
{
    public class ServiceWorkerActor : ReceiveActor
    {
        public ServiceWorkerActor()
        {
            Receive<SetWorkMessage>(message =>
            {
                var senderClosure = Sender;
                IConcurrentExecutorResponseMessage resultMessage;
                try
                {
                    var workFactory = message.WorkFactory;
                    if (workFactory.RunAsyncMethod)
                    {
                        workFactory.ExecuteAsync()
                            .ContinueWith(r =>
                            {
                                if (r.IsFaulted)
                                {
                                    resultMessage = new SetWorkErrorMessage("Unable to complete operation", message.Id);
                                    Context.Parent.Tell(resultMessage);//todo investigate potential double tell. I dont know if its faulted ends up throwing which in catch will send to parent again
                                }
                                else
                                {
                                    resultMessage = new SetWorkCompletedMessage(r.Result, message.Id);
                                }
                                return resultMessage;
                            },
                                TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                            .PipeTo(senderClosure);
                    }
                    else
                    {
                        workFactory.Execute();
                    }
                }
                catch (Exception e)
                {
                    resultMessage = new SetWorkErrorMessage(e.Message + " " + e.InnerException?.Message, message.Id);
                    senderClosure.Tell(resultMessage);
                    Context.Parent.Tell(resultMessage);
                }
            });
        }
    }
}