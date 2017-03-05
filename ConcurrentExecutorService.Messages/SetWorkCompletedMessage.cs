namespace ConcurrentExecutorService.Messages
{
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
}