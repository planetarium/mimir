using System.Collections.Immutable;
using Libplanet.Action;
using Libplanet.Action.Loader;
using Libplanet.Action.State;
using Libplanet.Blockchain;
using Libplanet.Blockchain.Policies;
using Libplanet.Crypto;
using Libplanet.RocksDBStore;
using Libplanet.Store;
using Libplanet.Store.Trie;
using Libplanet.Types.Blocks;
using Serilog;

namespace Mimir.Initializer.Util;

// Copy From Census
public static class ChainUtil
{
    private static readonly ImmutableDictionary<int, byte> _reverseConversionTable = new Dictionary<
        int,
        byte
    >()
    {
        [48] = 0, // '0'
        [49] = 1, // '1'
        [50] = 2, // '2'
        [51] = 3, // '3'
        [52] = 4, // '4'
        [53] = 5, // '5'
        [54] = 6, // '6'
        [55] = 7, // '7'
        [56] = 8, // '8'
        [57] = 9, // '9'
        [97] = 10, // 'a'
        [98] = 11, // 'b'
        [99] = 12, // 'c'
        [100] = 13, // 'd'
        [101] = 14, // 'e'
        [102] = 15, // 'f'
    }.ToImmutableDictionary();

    public static (BlockChain, IStore, IStateStore) LoadBlockChain(string storePath)
    {
        Uri uri = new Uri(storePath);
        (IStore store, IStateStore stateStore) = LoadStores(uri);
        BlockChain blockChain = LoadBlockChain(store, stateStore);
        Log.Logger.Information("BlockChain loaded");
        Log.Logger.Information("Block index: {Index}", blockChain.Tip.Index);
        Log.Logger.Information("Block hash: {BlockHash}", blockChain.Tip.Hash);
        Log.Logger.Information(
            "Block state root hash: {StateRootHash}",
            blockChain.Tip.StateRootHash
        );
        return (blockChain, store, stateStore);
    }

    private static (IStore, IStateStore) LoadStores(Uri uri)
    {
#pragma warning disable CS0168 // The variable '_' is declared but never used
        // FIXME: This is used to forcefully load the RocksDBStore assembly
        // so that StoreLoaderAttribute can find the appropriate loader.
        RocksDBStore _;
#pragma warning restore CS0168

        return StoreLoaderAttribute.LoadStore(uri)
            ?? throw new NullReferenceException("Failed to load store");
    }

    private static BlockChain LoadBlockChain(IStore store, IStateStore stateStore)
    {
        Guid canon =
            store.GetCanonicalChainId()
            ?? throw new NullReferenceException(
                $"Failed to load canonical chain from {nameof(store)}"
            );
        BlockHash genesisHash =
            store.IndexBlockHash(canon, 0)
            ?? throw new NullReferenceException(
                $"Failed to load genesis block from {nameof(store)}"
            );
        Block genesis = store.GetBlock(genesisHash);
        IBlockChainStates blockChainStates = new BlockChainStates(store, stateStore);
        IBlockPolicy policy = new BlockPolicy();
        IActionLoader actionLoader = new SingleActionLoader(typeof(MockAction));
        IActionEvaluator actionEvaluator = new ActionEvaluator(
            policy.PolicyActionsRegistry,
            stateStore: stateStore,
            actionTypeLoader: actionLoader
        );

        return new BlockChain(
            policy: policy,
            stagePolicy: new VolatileStagePolicy(),
            store: store,
            stateStore: stateStore,
            genesisBlock: genesis,
            blockChainStates: blockChainStates,
            actionEvaluator: actionEvaluator
        );
    }

    public static ITrie GetWorldTrie(BlockChain blockChain)
    {
        // Confirm block protocol version.
        Block tip = blockChain.Tip;
        if (tip.ProtocolVersion >= 5)
        {
            Log.Logger.Information(
                "Block protocol version confirmed: {ProtocolVersion}",
                tip.ProtocolVersion
            );
        }
        else
        {
            throw new ArgumentException($"Invalid block protocol version: {tip.ProtocolVersion}");
        }

        // Confirm trie metadata.
        ITrie worldTrie = blockChain.GetWorldState(tip.Hash).Trie;
        TrieMetadata? trieMetadata = worldTrie.GetMetadata();
        if (trieMetadata is { } metadata && metadata.Version == tip.ProtocolVersion)
        {
            Log.Logger.Information("Trie metadata confirmed: {Metadata}", metadata.Version);
        }
        else
        {
            throw new ArgumentException($"Invalid trie metadata: {trieMetadata}");
        }

        return worldTrie;
    }

    public static double EstimateProgress(string hex, bool reverse = true)
    {
        int high = int.Parse("ffff", System.Globalization.NumberStyles.HexNumber);
        int pos = int.Parse(hex.Substring(0, 4), System.Globalization.NumberStyles.HexNumber);

        return reverse ? (double)(high - pos) / high : (double)pos / high;
    }

    public static Address ToAddress(KeyBytes keyBytes)
    {
        const int length = Address.Size * 2;
        if (keyBytes.Length != length)
        {
            throw new ArgumentException(
                $"Given {nameof(keyBytes)} must be of length {length}: {keyBytes.Length}"
            );
        }

        byte[] buffer = new byte[Address.Size];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = Pack(keyBytes.ByteArray[i * 2], keyBytes.ByteArray[i * 2 + 1]);
        }

        return new Address(buffer);
    }

    private static byte Pack(byte x, byte y) =>
        (byte)((_reverseConversionTable[x] << 4) + _reverseConversionTable[y]);
}
