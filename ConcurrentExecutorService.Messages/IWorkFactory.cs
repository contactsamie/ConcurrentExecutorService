﻿using System.Threading.Tasks;

namespace ConcurrentExecutorService.Messages
{
    public interface IWorkFactory
    {
        bool RunAsyncMethod { set; get; }

        object Execute();

        Task<object> ExecuteAsync(object command);

        bool IsAFailedResult(object result);
    }
}