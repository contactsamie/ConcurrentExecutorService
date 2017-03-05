namespace ConcurrentExecutorService.Messages
{
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
}