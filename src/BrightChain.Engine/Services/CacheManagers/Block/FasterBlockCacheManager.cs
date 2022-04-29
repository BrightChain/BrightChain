﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Faster.Enumerations;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Services.CacheManagers.Block;
using FASTER.core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BrightChain.Engine.Faster.CacheManager;

/// <summary>
///     Disk/Memory hybrid Block Cache Manager based on Microsoft FASTER KV.
/// </summary>
/// <remarks>
///     The plan is to keep a couple separate caches of data in sync using the FasterKV checkpointing.
///     Hopefully errors where we need to put back or take out blocks that have already been altered on disk are rare.
///     The primary, Write Once*, Read Many cache:
///     - The Data cache contains the actual raw block data only. These blocks are not to be altered unless deleted through a revocation
///     certificate or normal expiration.
///     The secondary, Multiple Write Multiple Read cache is a combined cache that serves as a shared index table
///     - The BlockMetadata cache contains block metadata that may be updated.
///     - BlockExpirationIndexValues contain a list of Block ID's expiring in any given second.
///     - CBLDataHashIndexValues contain the latest CBL source hash associated with a given correlation ID.
///     - BrightHandleIndexValues contain the CBL handles for a given source hash ID.
///     - CBLTagIndexValue contain the correlation IDs for a given tag.
///     Sessions are what you issue a sequence of operations against. \
///     You checkpoint the database periodically, which ensures some prefix of operations \
///     on the session are persisted, i.e., can survive process failure. If you Upsert and try \
///     to read back the data in the same session, it should be there.
/// </remarks>
public partial class FasterBlockCacheManager : BrightenedBlockCacheManagerBase, IDisposable
{
    /// <summary>
    ///     hash table size (number of 64-byte buckets).
    /// </summary>
    private const long HashTableBuckets = 1L << 20;

    /// <summary>
    ///     Directory where the block tree root will be placed.
    /// </summary>
    private readonly DirectoryInfo baseDirectory;

    private readonly Dictionary<CacheDeviceType, IDevice> fasterDevices;
    private readonly FasterBase fasterStore;

    /// <summary>
    ///     Whether we enable a read cache.
    ///     Updated from config.
    /// </summary>
    private readonly bool useReadCache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FasterBlockCacheManager" /> class.
    /// </summary>
    /// <param name="logger">Instance of the logging provider.</param>
    /// <param name="configuration">Instance of the configuration provider.</param>
    /// <param name="databaseName">Database/directory name for the store.</param>
    /// <param name="rootBlock">Block containing key information for store.</param>
    /// <param name="testingSelfDestruct">Whether to delete device files on shutdown.</param>
    public FasterBlockCacheManager(ILogger logger, IConfiguration configuration, RootBlock rootBlock, bool testingSelfDestruct = false)
        : base(logger: logger,
            configuration: configuration,
            rootBlock: rootBlock,
            testingSelfDestruct: testingSelfDestruct)
    {
        var nodeOptions = configuration.GetSection(key: "NodeOptions");
        if (nodeOptions is null)
        {
            throw new BrightChainException(message: "'NodeOptions' config section must be defined, but is not");
        }

        var configOption = nodeOptions.GetSection(key: "BasePath");
        var dir = configOption is not null && configOption.Value.Any()
            ? configOption.Value
            : Path.Join(path1: Path.GetTempPath(),
                path2: "brightchain");

        this.baseDirectory = this.EnsuredDirectory(dir: dir);

        var configuredDbName
            = nodeOptions.GetSection(key: "DatabaseName");

        if (configuredDbName is null || !configuredDbName.Value.Any())
        {
            //ConfigurationHelper.AddOrUpdateAppSetting("NodeOptions:DatabaseName", this.databaseName);
        }
        else
        {
            var expectedGuid = Guid.Parse(input: configuredDbName.Value);
            if (expectedGuid != this.RootBlock.Guid)
            {
                throw new BrightChainException(message: "Provided root block does not match configured root block guid");
            }
        }

        var readCache = nodeOptions.GetSection(key: "EnableReadCache");
        this.useReadCache = readCache is null || readCache.Value is null ? false : Convert.ToBoolean(value: readCache.Value);

        (this.fasterDevices, this.fasterStore) = this.InitFaster();
        this.lastHead = this.HeadAddresses();
        this.lastCommit = this.lastHead;
        this.lastCheckpoint = this.TakeFullCheckpoint();
    }

    /// <summary>
    ///     Gets the full path to the configuration file.
    /// </summary>
    public string ConfigurationFilePath
        => this.ConfigFile;

    public void Dispose()
    {
        foreach (var entry in this.fasterDevices)
        {
            var faster = this.fasterStore;
            (faster as IDisposable).Dispose();
            entry.Value.Dispose();
        }
    }
}