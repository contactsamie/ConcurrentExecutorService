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
            //Arrange
            var baskets = new ConcurrentDictionary<string, bool>();
            var orders = new ConcurrentDictionary<string, string>();

            const int numberOfBaskets = 10;
            const int numberOfPurchaseFromOneBasketCount = 1;
            var buyOperation = GetPurchaseService(baskets, orders);

            //Act - obtain expected result
            var basketIds =   CreateBaskets(numberOfBaskets, numberOfPurchaseFromOneBasketCount, baskets);
            PurchaseSequencially(basketIds, numberOfPurchaseFromOneBasketCount, buyOperation);
            var expected = CheckAndGetResults(orders, baskets);

            //undo
            ResetStorages(baskets, orders);

            //Act - tryout parallel purchase
            basketIds = CreateBaskets(numberOfBaskets, numberOfPurchaseFromOneBasketCount, baskets);
            PurchaseInParallel(basketIds, buyOperation);
            var actual = CheckAndGetResults(orders, baskets);

            //Assert
            Assert.Equal(expected, actual);
        }

        private static void PurchaseInParallel(List<string> basketIds, Func<string, Task<bool>> buyOperation)
        {
            var service = new ConcurrentExecutorServiceLib.ConcurrentExecutorService();
            Parallel.ForEach(basketIds, (basket) =>
            {
                var result = service.GoAsync(basket, buyOperation, basket).Result;
            });
        }

        private static List<string> CheckAndGetResults(ConcurrentDictionary<string, string> orders, ConcurrentDictionary<string, bool> baskets)
        {
            var expected = new List<string>();
            orders.ForEach(o => { expected.Add(o.Value); });

            Assert.All(baskets, b => { Assert.Equal(1, orders.Count(o => o.Value == b.Key)); });
            return expected;
        }

        private static void PurchaseSequencially(List<string> basketIds, int multipleBuyFromOneBasketCount,
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

        private static Func<string, Task<bool>> GetPurchaseService(ConcurrentDictionary<string, bool> baskets, ConcurrentDictionary<string, string> orders)
        {
            Func<string, Task<bool>> buyOperation = async (o) =>
            {
                //server overloaded
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(1, 20)));
                var canBuy = baskets[o];
                //business processing
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(1, 20)));
                if (!canBuy) return true;
                baskets[o] = false;
                orders[Guid.NewGuid().ToString()] = o;
                return true;
            };
            return buyOperation;
        }

        private static List<string> CreateBaskets(int numberOfBaskets,int numberOfPurchaseFromOneBasketCount, ConcurrentDictionary<string, bool> baskets)
        {
            for (var i = 0; i < numberOfBaskets; i++)
                baskets[Guid.NewGuid().ToString()] = true;

          return  ObtainBasketIds(baskets, numberOfPurchaseFromOneBasketCount);

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