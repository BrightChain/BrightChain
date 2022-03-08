﻿using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Hashes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrightChain.Engine.Tests;

/// <summary>
///     Test transactable blocks using the BPlusTreeCacheManagerTest
/// </summary>
[TestClass]
public abstract class TransactableBlockCacheManagerTest<TcacheManager> : CacheManagerTest<TcacheManager, BlockHash, BrightenedBlock>
    where TcacheManager : ICacheManager<BlockHash, BrightenedBlock>
{
}
