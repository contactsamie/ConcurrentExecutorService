using Akka.Util.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ConcurrentExecutorService.Tests
{
    public class TestHelper
    {
        public static long TestOperationExecution(int numberOfBaskets, int numberOfPurchaseFromOneBasketCount, TimeSpan maxExecutionTimePerAskCall)
        {
            //Arrange
            var baskets = new ConcurrentDictionary<string, bool>();
            var orders = new ConcurrentDictionary<string, string>();

            //Act - obtain expected result
            var basketIds = CreateBaskets(numberOfBaskets, numberOfPurchaseFromOneBasketCount, baskets);
            var purchaseService = new DelayedPurchaseServiceFactory(baskets, orders);
            basketIds.ForEach(basket =>
            {
                var result = purchaseService.RunPurchaseService(basket).Result;
            });

            var expected = GetOperationsResults(baskets);
            Assert.All(baskets, b => Assert.Equal(1, orders.Count(o => o.Value == b.Key)));

            //undo
            baskets.ForEach(b => { baskets[b.Key] = true; });
            orders = new ConcurrentDictionary<string, string>();

            var service = new ConcurrentExecutorServiceLib.ConcurrentExecutorService(maxExecutionTimePerAskCall);

            var watch = System.Diagnostics.Stopwatch.StartNew();
             purchaseService = new DelayedPurchaseServiceFactory(baskets, orders);
            Parallel.ForEach(basketIds, (basketId) =>
            {
                var result = service.GoAsync(async () =>
                {
                    await purchaseService.RunPurchaseService(basketId);
                }, basketId).Result;
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

        private static List<string> ObtainBasketIds(ConcurrentDictionary<string, bool> baskets, int numberOfPurchaseFromOneBasketCount)
        {
            var basketIds = new List<string>();
            foreach (var keyValuePair in baskets)
                for (var i = 0; i < numberOfPurchaseFromOneBasketCount; i++)
                    basketIds.Add(keyValuePair.Key);
            return basketIds;
        }

        private static List<string> CreateBaskets(int numberOfBaskets, int numberOfPurchaseFromOneBasketCount, ConcurrentDictionary<string, bool> baskets)
        {
            for (var i = 0; i < numberOfBaskets; i++)
                baskets[Guid.NewGuid().ToString()] = true;

            return ObtainBasketIds(baskets, numberOfPurchaseFromOneBasketCount);
        }
    }

    public interface IPurchaseServiceFactory
    {
        Task<object> RunPurchaseService(string o);
    }

    public class DelayedPurchaseServiceFactory : IPurchaseServiceFactory
    {
        private ConcurrentDictionary<string, bool> Baskets { set; get; }
        private ConcurrentDictionary<string, string> Orders { set; get; }

        public DelayedPurchaseServiceFactory(ConcurrentDictionary<string, bool> baskets, ConcurrentDictionary<string, string> orders)
        {
            Orders = orders;
            Baskets = baskets;
        }

        public async Task<object> RunPurchaseService(string o)
        {
            var canBuy = Baskets[o];
            if (canBuy)
            {
                //server overloaded
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(1, 2)));
                Baskets[o] = false;
                Orders[Guid.NewGuid().ToString()] = o;
            }

            return new object();
        }
    }

    public class NoDelayPurchaseServiceFactory : IPurchaseServiceFactory
    {
        private ConcurrentDictionary<string, bool> Baskets { set; get; }
        private ConcurrentDictionary<string, string> Orders { set; get; }

        public NoDelayPurchaseServiceFactory(ConcurrentDictionary<string, bool> baskets, ConcurrentDictionary<string, string> orders)
        {
            Orders = orders;
            Baskets = baskets;
        }

        public async Task<object> RunPurchaseService(string o)
        {
            var canBuy = Baskets[o];
            if (canBuy)
            {
                Baskets[o] = false;
                Orders[Guid.NewGuid().ToString()] = o;
            }

            return new object();
        }
    }

    public class NullPurchaseServiceFactory : IPurchaseServiceFactory
    {
        public Task<object> RunPurchaseService(string o)
        {
            return Task.FromResult(new object());
        }
    }
}