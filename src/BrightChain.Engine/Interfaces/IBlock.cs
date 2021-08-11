using BrightChain.Engine.Enumerations;
using BrightChain.Engine.Models.Blocks;
using BrightChain.Engine.Models.Contracts;
using BrightChain.Engine.Models.Entities;
using BrightChain.Engine.Models.Hashes;
using BrightChain.Engine.Models.Nodes;

namespace BrightChain.Engine.Interfaces
{
    /// <summary>
    /// Basic description for a block
    /// </summary>
    public interface IBlock : IDisposable, IComparable<IBlock>, IValidatable
    {
        /// <summary>
        /// Gets the block's SHA-256 hash.
        /// </summary>
        BlockHash Id { get; }

        /// <summary>
        /// Gets a BlockSize enum associated with it's data length.
        /// </summary>
        BlockSize BlockSize { get; }

        /// <summary>
        /// Function to XOR this block's data with another.
        /// </summary>
        /// <param name="other">Block to XOR with.</param>
        /// <returns>Returns resultant block with its constituent blocks.</returns>
        Block XOR(IBlock other);

        /// <summary>
        /// Function to XOR this block's data with an array of others.
        /// </summary>
        /// <param name="others"></param>
        /// <returns></returns>
        Block XOR(IBlock[] others);

        BlockSignature Sign(Agent user, string password);

        /// <summary>
        /// Gets the parameters of the storage contract for this block.
        /// </summary>
        StorageContract StorageContract { get; set; }

        /// <summary>
        /// Gets the serialized MetaData pulled from attributes.
        /// </summary>
        ReadOnlyMemory<byte> Metadata { get; }

        /// <summary>
        /// Gets only the raw data for the block and none of the metadata. The hash is based only on this.
        /// </summary>
        ReadOnlyMemory<byte> Data { get; }

        /// <summary>
        /// Gets the node that originated the block.
        /// </summary>
        BrightChainNode SourceNode { get; }

        /// <summary>
        /// Gets the signature hash of the data by the committer.
        /// </summary>
        BlockSignature Signature { get; }

        /// <summary>
        /// Whether a signature hash is present
        /// </summary>
        bool Signed { get; }

        /// <summary>
        /// Whether the signature hash has been compared against the data
        /// </summary>
        bool SignatureVerified { get; }

        string OriginalType { get; }

        string AssemblyVersion { get; }
    }
}
