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
        public void it_should_run_one_operation2()
        {
            var nameOperation = new NameOperationIdObject("test", 28);
            Func<NameOperationIdObject, uint> operation = (o) => o.Data * 3;
            var expected = operation(nameOperation);
            var service = new ConcurrentExecutorServiceLib.ConcurrentExecutorService();
            var result = service.GoAsync<NameOperationIdObject, uint, string>(nameOperation, operation, nameOperation.Name).Result;
            Assert.Equal(expected, result);
        }

        [Fact]
        public void it_should_run_multiple_operations()
        {
            var baskets = new ConcurrentDictionary<string, bool>();
            var orders = new ConcurrentDictionary<string, string>();

            const int numberOfBaskets = 10;

            CreateBaskets(numberOfBaskets, baskets);

            var buyOperation = GetPurchaseService(baskets, orders);

            const int multipleBuyFromOneBasketCount = 10;

            var basketIds = PurchaseSequencially(baskets, multipleBuyFromOneBasketCount, buyOperation);

            var expected = CheckAndGetResults(orders, baskets);

            ResetStorages(baskets, orders);

            var service = new ConcurrentExecutorServiceLib.ConcurrentExecutorService();
            Parallel.ForEach(basketIds, (basket) =>
            {
                var result = service.GoAsync(basket, buyOperation, basket).Result;
            });

            var actual = CheckAndGetResults(orders, baskets);
            Assert.Equal(expected, actual);
        }

        private static List<string> CheckAndGetResults(ConcurrentDictionary<string, string> orders, ConcurrentDictionary<string, bool> baskets)
        {
            List<string> expected = new List<string>();
            orders.ForEach(o => { expected.Add(o.Value); });

            Assert.All(baskets, b => { Assert.Equal(1, orders.Count(o => o.Value == b.Key)); });
            return expected;
        }

        private static List<string> PurchaseSequencially(ConcurrentDictionary<string, bool> baskets, int multipleBuyFromOneBasketCount,
            Func<string, Task<bool>> buyOperation)
        {
            var basketIds = new List<string>();
            foreach (var keyValuePair in baskets)
                for (var i = 0; i < multipleBuyFromOneBasketCount; i++)
                    basketIds.Add(keyValuePair.Key);

            basketIds.ForEach(basket =>
            {
                var result = buyOperation(basket).Result;
            });
            return basketIds;
        }

        private static Func<string, Task<bool>> GetPurchaseService(ConcurrentDictionary<string, bool> baskets, ConcurrentDictionary<string, string> orders)
        {
            Func<string, Task<bool>> buyOperation = async (o) =>
            {
                //server overloaded
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(1, 200)));
                var canBuy = baskets[o];
                //business processing
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(1, 200)));
                if (!canBuy) return true;
                baskets[o] = false;
                orders[Guid.NewGuid().ToString()] = o;
                return true;
            };
            return buyOperation;
        }

        private static void CreateBaskets(int numberOfBaskets, ConcurrentDictionary<string, bool> baskets)
        {
            for (var i = 0; i < numberOfBaskets; i++)
                baskets[Guid.NewGuid().ToString()] = true;
        }

        private static void ResetStorages(ConcurrentDictionary<string, bool> Baskets, ConcurrentDictionary<string, string> Orders)
        {
            //RESET THINGS
            Baskets.ForEach(b => { Baskets[b.Key] = true; });
            Orders = new ConcurrentDictionary<string, string>();
            Assert.All(Baskets, b => { Assert.True(b.Value); });
        }

        //[Property(Arbitrary = new[] { typeof(ArbitraryNameOperationIdObject) }, MaxTest = 1000)]
        //public void it_should_run_operation(NameOperationIdObject nameOperation)
        //{
        //    Func<NameOperationIdObject, uint> operation = (o) => o.Data * 3; ;
        //    var expected = operation(nameOperation);
        //    var service = new ConcurrentExecutorServiceLib.ConcurrentExecutorService();
        //    var result = service.GoAsync<NameOperationIdObject, uint, string>(nameOperation, operation, nameOperation.Name).Result;
        //    Assert.Equal(expected, result);
        //}
        //[Property]
        //public void it_should_run_operation()
        //{
        //    var nameOperation = new NameOperationIdObject("test", 28);
        //    Func<NameOperationIdObject, uint> operation = (o) => o.Data * 3; ;
        //    var expected = operation(nameOperation);
        //    var service = new ConcurrentExecutorServiceLib.ConcurrentExecutorService();
        //    var result = service.GoAsync<NameOperationIdObject, uint, string>(nameOperation, operation, nameOperation.Name).Result;
        //    Assert.Equal(expected, result);
        //}
    }
}