﻿#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Blocks.Chains;
using BrightChain.Engine.Models.Blocks.DataObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BrightChain.Engine.Services
{
    /// <summary>
    /// Core service for BrightChain used by the webservice to retrieve and store blocks.
    /// </summary>
    public class BrightBlockService
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;

        private readonly MemoryBlockCacheManager blockMemoryCache;
        private readonly MemoryBlockCacheManager randomizerBlockMemoryCache;
        private readonly DiskBlockCacheManager blockDiskCache;
        private readonly BlockWhitener blockWhitener;

        public BrightBlockService(ILoggerFactory logger)
        {
            this.logger = logger.CreateLogger(nameof(BrightBlockService));
            if (this.logger is null)
            {
                throw new BrightChainException("CreateLogger failed");
            }

            this.logger.LogInformation(string.Format("<{0}>: logging initialized", nameof(BrightBlockService)));
            configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("brightchainSettings.json").Build();

            var services = new ServiceCollection();

            blockMemoryCache = new MemoryBlockCacheManager(
                logger: this.logger,
                configuration: configuration);
            blockDiskCache = new DiskBlockCacheManager(
                logger: this.logger,
                configuration: configuration);
            randomizerBlockMemoryCache = new MemoryBlockCacheManager(
                logger: this.logger,
                configuration: configuration);
            blockWhitener = new BlockWhitener(
                pregeneratedRandomizerCache: randomizerBlockMemoryCache);

            this.logger.LogInformation(string.Format("<{0}>: caches initialized", nameof(BrightBlockService)));
        }

        /// <summary>
        /// Creates a descriptor block for a given input file, found on disk.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="keepUntilAtLeast"></param>
        /// <param name="redundancy"></param>
        /// <param name="allowCommit"></param>
        /// <param name="privateEncrypted"></param>
        /// <param name="blockSize"></param>
        /// <returns>Resultant CBL block</returns>
        public ConstituentBlockListBlock CreateCblFromFile(string fileName, DateTime keepUntilAtLeast, RedundancyContractType redundancy, bool allowCommit, bool privateEncrypted = false, BlockSize? blockSize = null)
        {
            FileStream inFile = File.OpenRead(fileName);

            if (!blockSize.HasValue)
            {
                // decide best block size if null
                throw new NotImplementedException();
            }

            if (privateEncrypted)
            {
                throw new NotImplementedException();
            }

            var iBlockSize = BlockSizeMap.BlockSize(blockSize.Value);
            int tuplesRequired = (int)Math.Ceiling((double)(inFile.Length / iBlockSize));

            SHA256 hasher = SHA256.Create();
            byte[]? finalHash = null;
            // TODO: figure out how to stream huge files with yield, etc
            // TODO: use block whitener
            TupleStripe[] tupleStripes = new TupleStripe[tuplesRequired];
            List<Block> consumedBlocks = new List<Block>();
            ulong offset = 0;
            for (int i = 0; i < tuplesRequired; i++)
            {
                var finalBlock = i == (tuplesRequired - 1);
                byte[] buffer = new byte[iBlockSize];
                var bytesRead = (ulong)inFile.Read(buffer, 0, iBlockSize);
                offset += bytesRead;

                if ((bytesRead < (ulong)iBlockSize) && !finalBlock)
                {
                    throw new BrightChainException("Unexpected EOF");
                }
                else if ((bytesRead < (ulong)iBlockSize) && finalBlock)
                {
                    buffer = Helpers.RandomDataHelper.DataFiller(new ReadOnlyMemory<byte>(buffer), blockSize.Value).ToArray();
                }

                if (finalBlock)
                {
                    finalHash = hasher.TransformFinalBlock(buffer, 0, iBlockSize);
                }
                else
                {
                    hasher.TransformBlock(buffer, 0, iBlockSize, null, 0);
                }

                var sourceBlock = new SourceBlock(
                    new TransactableBlockParams(
                            cacheManager: blockMemoryCache, // SourceBlock itself cannot be persisted to cache, but resultant blocks from NewBlock via XOR go here
                            blockParams: new BlockParams(
                                blockSize: blockSize.Value,
                                requestTime: DateTime.Now,
                                keepUntilAtLeast: DateTime.MaxValue,
                                redundancy: RedundancyContractType.HeapAuto,
                                allowCommit: true,
                                privateEncrypted: privateEncrypted)),
                            data: buffer);

                Block whitened = blockWhitener.Whiten(sourceBlock);
                Block[] randomizersUsed = (Block[])whitened.ConstituentBlocks;
                Block[] allBlocks = new Block[BlockWhitener.TupleCount];
                allBlocks[0] = whitened;
                Array.Copy(sourceArray: randomizersUsed, sourceIndex: 0, destinationArray: allBlocks, destinationIndex: 1, length: randomizersUsed.Length);

                tupleStripes[i] = new TupleStripe(
                    tupleCount: BlockWhitener.TupleCount,
                    blocks: allBlocks);

                consumedBlocks.AddRange(allBlocks);
            }

            if (finalHash == null)
            {
                throw new BrightChainException("impossible");
            }

            var cbl = new ConstituentBlockListBlock(
                blockParams: new ConstituentBlockListBlockParams(
                    blockParams: new TransactableBlockParams(
                        cacheManager: blockMemoryCache,
                        blockParams: new BlockParams(
                            blockSize: blockSize.Value,
                            requestTime: DateTime.Now,
                            keepUntilAtLeast: keepUntilAtLeast,
                            redundancy: redundancy,
                            allowCommit: allowCommit,
                            privateEncrypted: privateEncrypted)),
                finalDataHash: new BlockHash(
                    originalBlockSize: blockSize.Value,
                    providedHashBytes:
                    finalHash, true),
                totalLength: (ulong)inFile.Length,
                constituentBlocks: consumedBlocks.ToArray()));

            return cbl;
        }
    }
}