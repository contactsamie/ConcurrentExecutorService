using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ConcurrentExecutorService.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void it_should_run_many_operations_fast()
        {
            const int numberOfRequests = 2000;
            var maxExecutionTimePerAskCall = TimeSpan.FromSeconds(3);
            const int maxTotalExecutionTimeMs = 3000;
            var service = new ConcurrentExecutorServiceLib.ConcurrentExecutorService(maxExecutionTimePerAskCall);
            var watch = Stopwatch.StartNew();
            Parallel.ForEach(Enumerable.Range(0, numberOfRequests),
                basket =>
                {
                    var result = service.GoAsync<object>(() => Task.FromResult(new object()), basket.ToString()).Result;
                });
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            Assert.True(elapsedMs < maxTotalExecutionTimeMs,
                $"Test took {elapsedMs} ms which is more than {maxTotalExecutionTimeMs}");
        }

        [Fact]
        public void it_should_run_one_operations_sequentially()
        {
            const int numberOfBaskets = 100;
            const int numberOfPurchaseFromOneBasketCount = 1;
            var maxExecutionTimePerAskCall = TimeSpan.FromSeconds(5);
            var durationMs = TestHelper.TestOperationExecution(numberOfBaskets, numberOfPurchaseFromOneBasketCount,
                maxExecutionTimePerAskCall);
            Console.WriteLine($"Test took {durationMs} ms");
        }

        [Fact]
        public void it_should_run_many_operations_sequentially()
        {
            const int numberOfBaskets = 100;
            const int numberOfPurchaseFromOneBasketCount = 10;
            var maxExecutionTimePerAskCall = TimeSpan.FromSeconds(5);
            var durationMs = TestHelper.TestOperationExecution(numberOfBaskets, numberOfPurchaseFromOneBasketCount,
                maxExecutionTimePerAskCall);
            Console.WriteLine($"Test took {durationMs} ms");
        }
    }
}