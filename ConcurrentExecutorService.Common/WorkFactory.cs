using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConcurrentExecutorService.Messages;

namespace ConcurrentExecutorService.Common
{
    public class WorkFactory : IWorkFactory
    {
        private Func<object> Operation { set; get; }
        private Func<Task<object>> OperationAsync { set; get; }
        public WorkFactory(Func<Task<object>> operation)
        {
            OperationAsync = operation;
            RunAsyncMethod = true;
        }
        public WorkFactory(Func<object> operation )
        {
            Operation = operation;
        }
        public object Execute()
        {
          return  Operation();
        }

        public Task<object> ExecuteAsync()
        {
            return OperationAsync();
        }

         public bool RunAsyncMethod { get; set; }
    }
}
