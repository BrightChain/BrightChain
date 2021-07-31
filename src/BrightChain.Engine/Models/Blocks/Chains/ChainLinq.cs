﻿namespace BrightChain.Engine.Models.Blocks.Chains
{
using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using global::BrightChain.Engine.Models.Blocks.DataObjects;
    using global::BrightChain.Engine.Services;

    public class ChainLinq<T>
        where T : ISerializable
    {
        public ChainLinq(IEnumerable<ChainLinqObjectBlock<T>> blocks)
        {
            this.Blocks = (ChainLinqObjectBlock<T>[])blocks;
            for (int i = this.Blocks.Length - 1; i >= 1; i--)
            {
                var block = this.Blocks[i];
                var previousBlock = this.Blocks[i - 1];
                previousBlock.Next = block.Id;
            }
        }

        public ChainLinqObjectBlock<T>[] Blocks { get; }

        public long Count() => this.Blocks.LongLength;

        public ChainLinqObjectBlock<T> First() =>
            this.Blocks[0];

        public ChainLinqObjectBlock<T> Last() =>
            this.Blocks[this.Blocks.Length - 1];

        public IEnumerable<T> All() =>
            this.Blocks.Select(b => b.BlockObject);

        public BrightChain BrightenAll(BrightBlockService brightBlockService) =>
            brightBlockService.BrightenBlocks(sourceBlocks: this.Blocks);

        public static BrightChain BrightenAll(BrightBlockService brightBlockService, IEnumerable<SourceBlock> sourceBlocks)
         => brightBlockService.BrightenBlocks(sourceBlocks: sourceBlocks);
    }
}
