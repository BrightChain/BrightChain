using BrightChain.Enumerations;
using BrightChain.Interfaces;
using BrightChain.Services;
using System;

namespace BrightChain.Models.Blocks
{
    /// <summary>
    /// Block that can be contained in a MemoryBlockCacheManager / Btree
    /// </summary>
    public class MemoryBlock : TransactableBlock, IBlock
    {
        public MemoryBlock(MemoryBlockCacheManager cacheManager, DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data, bool allowCommit) :
            base(
                cacheManager: cacheManager,
                requestTime: requestTime,
                keepUntilAtLeast: keepUntilAtLeast,
                redundancy: redundancy,
                data: data,
                allowCommit: allowCommit) => this.CacheManager.Set(this.Id, this);

        public override Block NewBlock(DateTime requestTime, DateTime keepUntilAtLeast, RedundancyContractType redundancy, ReadOnlyMemory<byte> data, bool allowCommit) => new MemoryBlock(
cacheManager: (MemoryBlockCacheManager)this.CacheManager,
requestTime: requestTime,
keepUntilAtLeast: keepUntilAtLeast,
redundancy: redundancy,
data: data,
allowCommit: allowCommit);

        public override void Dispose()
        {
        }
    }
}