using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Util.Internal;
using Xunit;

namespace ConcurrentExecutorService.Tests
{
    public class TestHelper
    {
        public static long TestOperationExecution(int numberOfBaskets, int numberOfPurchaseFromOneBasketCount)
        {
            //Arrange
            var baskets = new ConcurrentDictionary<string, bool>();
            var orders = new ConcurrentDictionary<string, string>();

            var buyOperation = GetAsyncPurchaseService(baskets, orders);

            //Act - obtain expected result
            var basketIds = CreateBaskets(numberOfBaskets, numberOfPurchaseFromOneBasketCount, baskets);
            PurchaseSequencially(basketIds,  buyOperation);
            var expected = GetOperationsResults(baskets);
            Assert.All(baskets, b => Assert.Equal(1, orders.Count(o => o.Value == b.Key)));

            //undo
            baskets.ForEach(b => { baskets[b.Key] = true; });
            orders = new ConcurrentDictionary<string, string>();
            buyOperation = GetAsyncPurchaseService(baskets, orders);

            var service = new ConcurrentExecutorServiceLib.ConcurrentExecutorService();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            Parallel.ForEach(basketIds, (basket) =>
            {
                var result = service.GoAsync(basket, buyOperation, basket).Result;
            });
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            var actual = GetOperationsResults(baskets);
            Assert.All(baskets, b => Assert.Equal(1, orders.Count(o => o.Value == b.Key)));

            //Assert
            Assert.Equal(expected, actual);

            return elapsedMs;
        }

        private static List<string> GetOperationsResults(ConcurrentDictionary<string, bool> baskets)
        {
            var expected = new List<string>();
            baskets.ForEach(b => { expected.Add(b.Key); });
            return expected;
        }

        private static void PurchaseSequencially(List<string> basketIds,
            Func<string, Task<bool>> buyOperation)
        {
            basketIds.ForEach(basket =>
            {
                var result = buyOperation(basket).Result;
            });
        }

        private static List<string> ObtainBasketIds(ConcurrentDictionary<string, bool> baskets, int numberOfPurchaseFromOneBasketCount)
        {
            var basketIds = new List<string>();
            foreach (var keyValuePair in baskets)
                for (var i = 0; i < numberOfPurchaseFromOneBasketCount; i++)
                    basketIds.Add(keyValuePair.Key);
            return basketIds;
        }

        private static Func<string, Task<bool>> GetAsyncPurchaseService(ConcurrentDictionary<string, bool> baskets, ConcurrentDictionary<string, string> orders)
        {
            Func<string, Task<bool>> buyOperation = async (o) =>
            {
                var canBuy = baskets[o];
                if (canBuy)
                {
                    //server overloaded
                    await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(1, 2)));
                    baskets[o] = false;
                    orders[Guid.NewGuid().ToString()] = o;
                }

                return true;
            };
            return buyOperation;
        }

        private static List<string> CreateBaskets(int numberOfBaskets, int numberOfPurchaseFromOneBasketCount, ConcurrentDictionary<string, bool> baskets)
        {
            for (var i = 0; i < numberOfBaskets; i++)
                baskets[Guid.NewGuid().ToString()] = true;

            return ObtainBasketIds(baskets, numberOfPurchaseFromOneBasketCount);
        }
    }
}