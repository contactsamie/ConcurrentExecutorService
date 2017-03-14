namespace ConcurrentExecutorService.Messages
{
    public class SetCompleteWorkErrorMessage : IConcurrentExecutorResponseMessage
    {
        public SetCompleteWorkErrorMessage(string error, string workerId, object lastSuccessfullResult, bool hasExistingResult)
        {
            Error = error;
            WorkerId = workerId;
            LastSuccessfullResult = lastSuccessfullResult;
            HasExistingResult = hasExistingResult;
        }

        public string Error { get; private set; }
        public string WorkerId { get; private set; }
        public object LastSuccessfullResult { get; private set; }
        public bool HasExistingResult { get; private set; }
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
}