﻿using ConcurrentExecutorService.Tests.TestSystem;
using FsCheck;
using System;

namespace ConcurrentExecutorService.Tests
{
    public static class ArbitraryNameOperationIdObject
    {
        public static Arbitrary<NameOperationIdObject> Inventories()
        {
            var genInventories = from name in Arb.Generate<Guid>()
                                 from operationId in Arb.Generate<uint>()
                                 select new NameOperationIdObject(name.ToString(), operationId);
            return genInventories.ToArbitrary();
        }
    }
}