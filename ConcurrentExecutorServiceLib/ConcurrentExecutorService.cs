using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentExecutorServiceLib
{
    public class ConcurrentExecutorService
    {
        public async Task<TResult> GoAsync<TCommand, TResult, TIdentity>(TCommand command, Func<TCommand, TResult> operation, TIdentity id)
        {
            return await Task.FromResult(operation(command));
        }
        public async Task<TResult> GoAsync<TCommand, TResult,TIdentity>(TCommand command,Func<TCommand,Task<TResult>> operation, TIdentity id)
        {
            return await operation(command);
        }
    }
}
