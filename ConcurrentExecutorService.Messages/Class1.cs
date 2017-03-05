using System;
using System.Threading.Tasks;

namespace ConcurrentExecutorService.Messages
{
    public interface IWorkFactory
    {
        bool RunAsyncMethod { set; get; }
        object Execute();
        Task<object> ExecuteAsync();
    }

    public class SetWorkMessage : IConcurrentExecutorRequestMessage
    {
        public SetWorkMessage(string id, IWorkFactory workFactory)
        {
            Id = id;
            WorkFactory = workFactory;
        }

        public string Id { get; private set; }
        public IWorkFactory WorkFactory { private set; get; }
    }

    public class WorkerStatus
    {
        public DateTime CreatedDateTime { get; set; }
        public DateTime CompletedDateTime { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class SetWorkCompletedMessage : IConcurrentExecutorResponseMessage
    {
        public SetWorkCompletedMessage(object result, string id)
        {
            Result = result;
            Id = id;
        }

        public object Result { get; private set; }
        public string Id { get; private set; }
    }

    public class SetWorkErrorMessage : IConcurrentExecutorResponseMessage
    {
        public SetWorkErrorMessage(string error, string workerId)
        {
            Error = error;
            WorkerId = workerId;
        }

        public string Error { get; private set; }
        public string WorkerId { get; private set; }
    }

    public interface IConcurrentExecutorRequestMessage
    {
    }

    public interface IConcurrentExecutorResponseMessage
    {
    }
}