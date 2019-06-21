using Moq;
using Neo.Cryptography.ECC;
using Neo.IO.Wrappers;
using Neo.Ledger;
using Neo.Persistence;
using System;
using System.Collections.Generic;

namespace Neo.UnitTests
{
    public static class TestBlockchain
    {
        private static NeoSystem TheNeoSystem;

        public static Mock<Snapshot> InitializeMockSnapshot(BlockState BlockState = null, TransactionState TransactionState = null, AccountState AccountState = null, 
            UnspentCoinState UnspentCoinState = null, SpentCoinState SpentCoinState = null, ValidatorState ValidatorState = null, AssetState AssetState = null, 
            ContractState ContractState = null, StorageItem StorageItem = null, HeaderHashList HeaderHashList = null
        ) 
        {
            var mockSnapshot = new Mock<Snapshot>();
            mockSnapshot.SetupGet(p => p.Blocks).Returns(new TestDataCache<UInt256, BlockState>(BlockState));
            mockSnapshot.SetupGet(p => p.Transactions).Returns(new TestDataCache<UInt256, TransactionState>(TransactionState));
            mockSnapshot.SetupGet(p => p.Accounts).Returns(new TestDataCache<UInt160, AccountState>(AccountState));
            mockSnapshot.SetupGet(p => p.UnspentCoins).Returns(new TestDataCache<UInt256, UnspentCoinState>(UnspentCoinState));
            mockSnapshot.SetupGet(p => p.SpentCoins).Returns(new TestDataCache<UInt256, SpentCoinState>(SpentCoinState));
            mockSnapshot.SetupGet(p => p.Validators).Returns(new TestDataCache<ECPoint, ValidatorState>(ValidatorState));
            mockSnapshot.SetupGet(p => p.Assets).Returns(new TestDataCache<UInt256, AssetState>(AssetState));
            mockSnapshot.SetupGet(p => p.Contracts).Returns(new TestDataCache<UInt160, ContractState>(ContractState));
            mockSnapshot.SetupGet(p => p.Storages).Returns(new TestDataCache<StorageKey, StorageItem>(StorageItem));
            mockSnapshot.SetupGet(p => p.HeaderHashList)
                .Returns(new TestDataCache<UInt32Wrapper, HeaderHashList>(HeaderHashList));
            mockSnapshot.SetupGet(p => p.ValidatorsCount).Returns(new TestMetaDataCache<ValidatorsCountState>());
            mockSnapshot.SetupGet(p => p.BlockHashIndex).Returns(new TestMetaDataCache<HashIndexState>());
            mockSnapshot.SetupGet(p => p.HeaderHashIndex).Returns(new TestMetaDataCache<HashIndexState>());

            return mockSnapshot;
        }

        public static Mock<Snapshot> InitializeMockStorageSnapshot(BlockState BlockState = null, Dictionary<UInt160, ContractState> ContractStates = null) 
        {
            var mockSnapshot = new Mock<Snapshot>();
            mockSnapshot.SetupGet(p => p.Blocks).Returns(new TestDataCache<UInt256, BlockState>(BlockState));
            mockSnapshot.SetupGet(p => p.Transactions).Returns(new TestStorageCache<UInt256, TransactionState>());
            mockSnapshot.SetupGet(p => p.Accounts).Returns(new TestStorageCache<UInt160, AccountState>());
            mockSnapshot.SetupGet(p => p.UnspentCoins).Returns(new TestStorageCache<UInt256, UnspentCoinState>());
            mockSnapshot.SetupGet(p => p.SpentCoins).Returns(new TestStorageCache<UInt256, SpentCoinState>());
            mockSnapshot.SetupGet(p => p.Validators).Returns(new TestStorageCache<ECPoint, ValidatorState>());
            mockSnapshot.SetupGet(p => p.Assets).Returns(new TestStorageCache<UInt256, AssetState>());
            mockSnapshot.SetupGet(p => p.Contracts).Returns(new TestStorageCache<UInt160, ContractState>(ContractStates));
            mockSnapshot.SetupGet(p => p.Storages).Returns(new TestStorageCache<StorageKey, StorageItem>());
            mockSnapshot.SetupGet(p => p.HeaderHashList)
                .Returns(new TestStorageCache<UInt32Wrapper, HeaderHashList>());
            mockSnapshot.SetupGet(p => p.ValidatorsCount).Returns(new TestMetaDataCache<ValidatorsCountState>());
            mockSnapshot.SetupGet(p => p.BlockHashIndex).Returns(new TestMetaDataCache<HashIndexState>());
            mockSnapshot.SetupGet(p => p.HeaderHashIndex).Returns(new TestMetaDataCache<HashIndexState>());

            return mockSnapshot;
        }

        public static Mock<Store> InitializeMockStore() 
        {
                var mockSnapshot = InitializeMockSnapshot();

                var mockStore = new Mock<Store>();

                var defaultTx = TestUtils.CreateRandomHashInvocationMockTransaction().Object;
                mockStore.Setup(p => p.GetBlocks()).Returns(new TestDataCache<UInt256, BlockState>());
                mockStore.Setup(p => p.GetTransactions()).Returns(new TestDataCache<UInt256, TransactionState>(
                    new TransactionState
                    {
                        BlockIndex = 1,
                        Transaction = defaultTx
                    }));

                mockStore.Setup(p => p.GetAccounts()).Returns(new TestDataCache<UInt160, AccountState>());
                mockStore.Setup(p => p.GetUnspentCoins()).Returns(new TestDataCache<UInt256, UnspentCoinState>());
                mockStore.Setup(p => p.GetSpentCoins()).Returns(new TestDataCache<UInt256, SpentCoinState>());
                mockStore.Setup(p => p.GetValidators()).Returns(new TestDataCache<ECPoint, ValidatorState>());
                mockStore.Setup(p => p.GetAssets()).Returns(new TestDataCache<UInt256, AssetState>());
                mockStore.Setup(p => p.GetContracts()).Returns(new TestDataCache<UInt160, ContractState>());
                mockStore.Setup(p => p.GetStorages()).Returns(new TestDataCache<StorageKey, StorageItem>());
                mockStore.Setup(p => p.GetHeaderHashList()).Returns(new TestDataCache<UInt32Wrapper, HeaderHashList>());
                mockStore.Setup(p => p.GetValidatorsCount()).Returns(new TestMetaDataCache<ValidatorsCountState>());
                mockStore.Setup(p => p.GetBlockHashIndex()).Returns(new TestMetaDataCache<HashIndexState>());
                mockStore.Setup(p => p.GetHeaderHashIndex()).Returns(new TestMetaDataCache<HashIndexState>());
                mockStore.Setup(p => p.GetSnapshot()).Returns(mockSnapshot.Object);

                return mockStore;
        }

        public static NeoSystem InitializeMockNeoSystem()
        {
            if (TheNeoSystem == null)
            {
                var mockStore = InitializeMockStore();

                Console.WriteLine("initialize NeoSystem");
                TheNeoSystem = new NeoSystem(mockStore.Object); // new Mock<NeoSystem>(mockStore.Object);
            }

            return TheNeoSystem;
        }
    }
}