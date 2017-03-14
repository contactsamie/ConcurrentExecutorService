﻿using System.Collections.Generic;

namespace ConcurrentExecutorService.Common
{
    public class ExecutionResult<T> where T : class
    {
        public ExecutionResult()
        {
            Errors = new List<string>();
        }

        public T Result { set; get; }
        public List<string> Errors { set; get; }
        public bool Succeeded { set; get; }
    }
}