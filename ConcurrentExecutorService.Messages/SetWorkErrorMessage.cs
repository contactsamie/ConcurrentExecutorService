namespace ConcurrentExecutorService.Messages
{
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
}