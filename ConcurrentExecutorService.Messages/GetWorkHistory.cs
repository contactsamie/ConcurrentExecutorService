namespace ConcurrentExecutorService.Messages
{
    public class GetWorkHistoryMessage : IConcurrentExecutorRequestMessage
    {
        public string WorkId { get; private set; }

        public GetWorkHistoryMessage(string workId)
        {
            WorkId = workId;
        }
    }
}