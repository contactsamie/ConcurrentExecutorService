namespace ConcurrentExecutorService.Messages
{
    public class SetWorkSucceededMessage : IConcurrentExecutorResponseMessage
    {
        public SetWorkSucceededMessage(object result, string workerId)
        {
            Result = result;
            WorkerId = workerId;
        }

        public object Result { get; private set; }
        public string WorkerId { get; private set; }
    }
}