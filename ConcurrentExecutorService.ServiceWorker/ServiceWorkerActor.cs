using Akka.Actor;
using ConcurrentExecutorService.Messages;
using System;

namespace ConcurrentExecutorService.ServiceWorker
{
    public class ServiceWorkerActor : ReceiveActor
    {
        public ServiceWorkerActor()
        {
            ReceiveAsync<SetWorkMessage>(async message =>
            {
                IConcurrentExecutorResponseMessage resultMessage;
                try
                {
                    var workFactory = message.WorkFactory;
                    object result;
                    if (workFactory.RunAsyncMethod)
                    {
                        result = await workFactory.ExecuteAsync();
                    }
                    else
                    {
                        result = workFactory.Execute();
                    }
                    resultMessage = new SetWorkCompletedMessage(result, message.Id);
                    Sender.Tell(resultMessage);
                }
                catch (Exception e)
                {
                    resultMessage = new SetWorkErrorMessage(e.Message + " " + e.InnerException?.Message, message.Id);
                    Sender.Tell(resultMessage);
                    Context.Parent.Tell(resultMessage);
                }
            });
        }
    }
}