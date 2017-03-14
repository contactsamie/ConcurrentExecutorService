using System;
using System.Collections.Generic;

namespace ConcurrentExecutorService.Messages
{
    public class GetWorkHistoryCompletedMessage : IConcurrentExecutorResponseMessage
    {
        public GetWorkHistoryCompletedMessage(List<Worker> workHistory, DateTime lastSystemAccessedTime)
        {
            WorkHistory = workHistory;
            LastSystemAccessedTime = lastSystemAccessedTime;
        }

        public List<Worker> WorkHistory { get; private set; }
        public DateTime LastSystemAccessedTime { get; private set; }
    }
}