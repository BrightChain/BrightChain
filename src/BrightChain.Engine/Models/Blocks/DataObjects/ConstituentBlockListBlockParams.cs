﻿using System;
using System.Collections.Generic;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Models.Hashes;
using NeuralFabric.Models.Hashes;

namespace BrightChain.Engine.Models.Blocks.DataObjects;

public class ConstituentBlockListBlockParams : BlockParams
{
    public readonly IEnumerable<BlockHash> ConstituentBlockHashes;

    public readonly Guid CorrelationId;

    public readonly BlockHash Next;

    public readonly BlockHash Previous;

    public readonly DataHash PreviousVersionHash;
    public readonly DataHash SourceId;

    public readonly long TotalLength;

    /// <summary>
    ///     Hash of the sum bytes of the segment of the file contained in this CBL when assembled in order.
    /// </summary>
    public SegmentHash SegmentId;

    public ConstituentBlockListBlockParams(BlockParams blockParams, DataHash sourceId, SegmentHash segmentId, long totalLength,
        IEnumerable<BlockHash> constituentBlockHashes, BlockHash previous = null, BlockHash next = null, Guid? correlationId = null,
        DataHash previousVersionHash = null)
        : base(
            blockSize: blockParams.BlockSize,
            requestTime: blockParams.RequestTime,
            keepUntilAtLeast: blockParams.KeepUntilAtLeast,
            redundancy: blockParams.Redundancy,
            privateEncrypted: blockParams.PrivateEncrypted,
            originalType: blockParams.OriginalType)
    {
        this.SourceId = sourceId;
        this.TotalLength = totalLength;
        this.ConstituentBlockHashes = constituentBlockHashes;
        this.SegmentId = segmentId;
        this.Previous = previous;
        this.Next = next;
        this.CorrelationId = correlationId.HasValue ? correlationId.Value : Guid.NewGuid();
        this.PreviousVersionHash = previousVersionHash;
    }

    public ConstituentBlockListBlockParams Merge(ConstituentBlockListBlockParams otherBlockParams)
    {
        if (otherBlockParams.BlockSize != this.BlockSize)
        {
            throw new BrightChainException(message: "BlockSize mismatch");
        }

        var newConstituentBlocks = new List<BlockHash>(collection: this.ConstituentBlockHashes);
        newConstituentBlocks.AddRange(collection: otherBlockParams.ConstituentBlockHashes);

        return new ConstituentBlockListBlockParams(
            blockParams: this.Merge(otherBlockParams: otherBlockParams),
            sourceId: this.SourceId,
            segmentId: this.SegmentId,
            totalLength: this.TotalLength > otherBlockParams.TotalLength ? this.TotalLength : otherBlockParams.TotalLength,
            constituentBlockHashes: newConstituentBlocks,
            previous: this.Previous,
            next: this.Next,
            correlationId: this.CorrelationId,
            previousVersionHash: this.PreviousVersionHash);
    }
}
