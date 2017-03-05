using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
          var service = new ConcurrentExecutorServiceLib.ConcurrentExecutorService(TimeSpan.FromSeconds(3));
          var result=service.ExecuteAsync<object>( () =>
            {
                Console.WriteLine("Done");
                return  Task.FromResult(new object());
            },Guid.NewGuid().ToString()).Result;
            Console.ReadLine();
        }
    }
}
