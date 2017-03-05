using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Akka.Util.Internal;
using ConcurrentExecutorService.Messages;
using Xunit;

namespace ConcurrentExecutorService.Tests
{
    public class TestHelper
    {
        public static long TestOperationExecution(int numberOfBaskets, int numberOfPurchaseFromOneBasketCount,
            TimeSpan maxExecutionTimePerAskCall)
        {
            //Arrange
            var baskets = new ConcurrentDictionary<string, bool>();
            var orders = new ConcurrentDictionary<string, string>();

            //Act - obtain expected result
            for (var i = 0; i < numberOfBaskets; i++)
                baskets[Guid.NewGuid().ToString()] = true;

            var basketIds = ObtainBasketIds(baskets, numberOfPurchaseFromOneBasketCount);

            var purchaseService = new DelayedPurchaseServiceFactory(baskets, orders);
            basketIds.ForEach(basket => { var result = purchaseService.RunPurchaseService(basket).Result; });

            //var expected = baskets.Select(b => b.Key);
            Assert.All(baskets, b => Assert.Equal(1, orders.Count(o => o.Value == b.Key)));

            //undo
            baskets.ForEach(b => { baskets[b.Key] = true; });
            orders = new ConcurrentDictionary<string, string>();

            var service = new ConcurrentExecutorServiceLib.ConcurrentExecutorService(maxExecutionTimePerAskCall);

            purchaseService = new DelayedPurchaseServiceFactory(baskets, orders);
            var watch = Stopwatch.StartNew();

            var results = new List<object>();
            Parallel.ForEach(basketIds, basketId =>
            {
                var result =
                    service.GoAsync<IConcurrentExecutorResponseMessage>(
                        () => { return purchaseService.RunPurchaseService(basketId); }, basketId).Result;
                results.Add(result);
            });


            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;


            Assert.All(baskets, b => Assert.Equal(1, orders.Count(o => o.Value == b.Key)));


            return elapsedMs;
        }


        private static List<string> ObtainBasketIds(ConcurrentDictionary<string, bool> baskets,
            int numberOfPurchaseFromOneBasketCount)
        {
            var basketIds = new List<string>();
            foreach (var keyValuePair in baskets)
                for (var i = 0; i < numberOfPurchaseFromOneBasketCount; i++)
                    basketIds.Add(keyValuePair.Key);
            return basketIds;
        }
    }

    public interface IPurchaseServiceFactory
    {
        Task<object> RunPurchaseService(string o);
    }

    public class DelayedPurchaseServiceFactory : IPurchaseServiceFactory
    {
        public DelayedPurchaseServiceFactory(ConcurrentDictionary<string, bool> baskets,
            ConcurrentDictionary<string, string> orders)
        {
            Orders = orders;
            Baskets = baskets;
        }

        private ConcurrentDictionary<string, bool> Baskets { get; }
        private ConcurrentDictionary<string, string> Orders { get; }

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
        public NoDelayPurchaseServiceFactory(ConcurrentDictionary<string, bool> baskets,
            ConcurrentDictionary<string, string> orders)
        {
            Orders = orders;
            Baskets = baskets;
        }

        private ConcurrentDictionary<string, bool> Baskets { get; }
        private ConcurrentDictionary<string, string> Orders { get; }

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