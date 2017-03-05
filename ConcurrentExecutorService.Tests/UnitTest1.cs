using Akka.Util.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ConcurrentExecutorService.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void it_should_run_one_operations_sequentially()
        {
            const int numberOfBaskets = 10;
            const int numberOfPurchaseFromOneBasketCount = 1;
            TestHelper.TestOperationExecution(numberOfBaskets, numberOfPurchaseFromOneBasketCount);
        }

        [Fact]
        public void it_should_run_many_operations_sequentially()
        {
            const int numberOfBaskets = 10;
            const int numberOfPurchaseFromOneBasketCount = 10;
            TestHelper.TestOperationExecution(numberOfBaskets, numberOfPurchaseFromOneBasketCount);
        }

        //[Property(Arbitrary = new[] { typeof(ArbitraryNameOperationIdObject) }, MaxTest = 1000)]
        
    }
}