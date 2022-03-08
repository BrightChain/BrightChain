﻿using System;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models.Blocks.Chains;
using BrightChain.Engine.Models.Blocks.DataObjects;
using BrightChain.Engine.Models.Hashes;
using NeuralFabric.Models.Hashes;

namespace BrightChain.Engine.Services.CacheManagers.Block;

/// <summary>
///     Block Cache Manager.
/// </summary>
public abstract partial class BrightenedBlockCacheManagerBase : IBrightenedBlockCacheManager
{
    public abstract BrightHandle GetCbl(DataHash sourceHash);

    public abstract void SetCbl(BlockHash cblHash, DataHash dataHash, BrightHandle brightHandle);

    public virtual void UpdateCblVersion(ConstituentBlockListBlock newCbl, ConstituentBlockListBlock oldCbl = null)
    {
        if (oldCbl is not null && oldCbl.CorrelationId != newCbl.CorrelationId)
        {
            throw new BrightChainException(message: nameof(newCbl.CorrelationId));
        }

        if (oldCbl is not null && newCbl.StorageContract.RequestTime.CompareTo(value: oldCbl.StorageContract.RequestTime) < 0)
        {
            throw new BrightChainException(message: "New CBL must be newer than old CBL");
        }
    }

    public abstract BrightHandle GetCbl(Guid correlationID);
}
