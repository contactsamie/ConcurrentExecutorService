using System;

namespace ConcurrentExecutorService.Messages
{
    public class WorkerStatus
    {
        public DateTime CreatedDateTime { get; set; }
        public DateTime CompletedDateTime { get; set; }
        public bool IsCompleted { get; set; }
    }
}