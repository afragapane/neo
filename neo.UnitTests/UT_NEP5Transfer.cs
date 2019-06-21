using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.IO.Json;
using Neo.SmartContract;
using Neo.Network.P2P.Payloads;
using Neo.VM;
using System.IO;
using System;
using Neo.Ledger;
using Neo;
using Neo.Cryptography.ECC;
using System.Collections.Generic;

namespace Neo.UnitTests
{
    [TestClass]
    public class UT_NEP5Transfer
    {

        public static readonly byte[] script = File.ReadAllText("/Users/alexfrag/Documents/Development/NEO/nixtest/neo/neo.UnitTests/hexScript.txt").HexToBytes();

        private Persistence.Snapshot RunScript(byte[] script, string gasConsumed, Persistence.Snapshot mockSnapshot, InvocationTransaction transaction = null) {            
            ApplicationEngine engine = ApplicationEngine.Run(script, mockSnapshot, container: transaction, skipWitnessVerify: true);
            JObject json = new JObject();
            
            json["script"] = script.ToHexString();
            json["state"] = engine.State;
            json["gas_consumed"] = engine.GasConsumed.ToString();

            // json["stack_top"] = engine.ResultStack.Peek().GetByteArray().ToHexString();
            // json["stack_top"] = engine.ResultStack.Peek().GetBigInteger().ToString();
            // json["stack_top"] = engine.ResultStack.Peek().GetString();
            // Console.WriteLine(json["stack_top"].AsString());

            json["state"].AsString().Should().Be("HALT");
            json["gas_consumed"].AsString().Should().Be(gasConsumed);

            return engine.snapshot;
        }

        private void AppendMethodName(string name) {
            using (StreamWriter sw = File.AppendText("/Users/alexfrag/Documents/Development/NEO/nixtest/neo/neo.UnitTests/callDump.txt")) 
                {
                    sw.WriteLine(name);
                }	
        }

        [TestMethod]
        public void ExecuteTest() {
            var nOSTokenScript = File.ReadAllText("/Users/alexfrag/Documents/Development/NEO/nixtest/neo/neo.UnitTests/nOSTokenScript.txt").HexToBytes();
            var nOSContract = new ContractState{
                Script = nOSTokenScript,
                ParameterList = new ContractParameterType[] {ContractParameterType.String, ContractParameterType.Array},
                ReturnType = ContractParameterType.ByteArray,
                ContractProperties = ContractPropertyState.HasStorage | ContractPropertyState.HasDynamicInvoke | ContractPropertyState.Payable,
                Name = "nOS Token",
                CodeVersion = "1.0",
                Author = "nOS Limited",
                Email = "contact@nos.io",
                Description = "nOS Token NEP-5 Smart Contract - https://nos.io"
            };
            var nOSDeployScript = "11496e6974536d617274436f6e747261637451c10561646d696e678d085e441a6e2e751e60146b9da2662b5afcc0c9".HexToBytes();
            var nOSWhiteListTransferFromAdd = "14d4c357a466cf12e8167b00a440f782705dcf2ba31857686974656c6973745472616e7366657246726f6d41646452c10561646d696e678d085e441a6e2e751e60146b9da2662b5afcc0c9".HexToBytes();
            var nOSTransfer = "0688c132e98e00142fbaa22d64a8c8f5a9940f359cb0cb1dfe49eb2c14a34ef9ba9549f2a5ffae1966ea8fbdde47839f2053c1087472616e73666572678d085e441a6e2e751e60146b9da2662b5afcc0c9".HexToBytes();
            
            var switcheoV3ContractScript = File.ReadAllText("/Users/alexfrag/Documents/Development/NEO/nixtest/neo/neo.UnitTests/switcheoV3ContractScript.txt").HexToBytes();
            var switcheoV3Contract = new ContractState{
                Script = switcheoV3ContractScript,
                ParameterList = new ContractParameterType[] {ContractParameterType.String, ContractParameterType.Array},
                ReturnType = ContractParameterType.ByteArray,
                ContractProperties = ContractPropertyState.HasStorage | ContractPropertyState.HasDynamicInvoke | ContractPropertyState.Payable,
                Name = "Switcheo Exchange",
                CodeVersion = "3.1",
                Author = "Switcheo",
                Email = "engineering@switcheo.network",
                Description = "Switcheo Exchange V3.1"
            };
            var switcheoInitialize = "14b7634295e58c6d7513c22d5881ba116db154e8de147335f929546270b8f811a0f9427b5712457107e714c202200f681f5d3b933c956cfedec18ee635bf5c53c10a696e697469616c697a6567d4c357a466cf12e8167b00a440f782705dcf2ba3".HexToBytes();
            var switcheoAddToWhitelist = "148d085e441a6e2e751e60146b9da2662b5afcc0c951c10e616464546f57686974656c69737467d4c357a466cf12e8167b00a440f782705dcf2ba3".HexToBytes();
            var switcheoDeposit = "0688c132e98e00148d085e441a6e2e751e60146b9da2662b5afcc0c9142fbaa22d64a8c8f5a9940f359cb0cb1dfe49eb2c53c1076465706f73697467d4c357a466cf12e8167b00a440f782705dcf2ba3".HexToBytes();
            var switcheoWithdraw = "00c108776974686472617769d4c357a466cf12e8167b00a440f782705dcf2ba3".HexToBytes();
            TransactionAttribute[] attributes = {
                new TransactionAttribute{
                    Usage = TransactionAttributeUsage.Hash1,
                    Data = "5100000000000000000000000000000000000000000000000000000000000000".HexToBytes()
                },
                new TransactionAttribute{
                    Usage = TransactionAttributeUsage.Hash2,
                    Data = "8d085e441a6e2e751e60146b9da2662b5afcc0c9000000000000000000000000".HexToBytes()
                },
                new TransactionAttribute{
                    Usage = TransactionAttributeUsage.Hash4,
                    Data = "2fbaa22d64a8c8f5a9940f359cb0cb1dfe49eb2c000000000000000000000000".HexToBytes()
                },
                new TransactionAttribute{
                    Usage = TransactionAttributeUsage.Hash5,
                    Data = "8023bce88e000000000000000000000000000000000000000000000000000000".HexToBytes()
                }
            };
            var transaction = new InvocationTransaction
                {
                    Version = 0x01,
                    Script = switcheoWithdraw,
                    Attributes = attributes,
                    Gas = new Fixed8(0),
                    Inputs = new CoinReference[0],
                    Outputs = new TransactionOutput[0],
                };

            var ContractStates = new Dictionary<UInt160, ContractState>();
            ContractStates.Add(nOSContract.ScriptHash, nOSContract);
            ContractStates.Add(switcheoV3Contract.ScriptHash, switcheoV3Contract);

            var BlockState = new BlockState{SystemFeeAmount = 0, TrimmedBlock = new Block{Timestamp = 1641700000}.Header.Trim()};

            var mockSnapshot = TestBlockchain.InitializeMockStorageSnapshot(
                BlockState: BlockState,
                ContractStates: ContractStates
            );

            File.Delete("/Users/alexfrag/Documents/Development/NEO/nixtest/neo/neo.UnitTests/callDump.txt");
            AppendMethodName("nOSDeploy");
            Console.WriteLine("nOSDeploy");
            RunScript(nOSDeployScript, "6.793", mockSnapshot.Object);

            AppendMethodName("nOSWhiteListTransferFromAdd");
            Console.WriteLine("nOSWhiteListTransferFromAdd");
            RunScript(nOSWhiteListTransferFromAdd, "2.373", mockSnapshot.Object);

            AppendMethodName("nOSTransfer");
            Console.WriteLine("nOSTransfer");
            RunScript(nOSTransfer, "6.04", mockSnapshot.Object);

            AppendMethodName("switcheoInit");
            Console.WriteLine("switcheoInit");
            RunScript(switcheoInitialize, "5.712", mockSnapshot.Object);

            AppendMethodName("switcheoAddToWhitelist");
            Console.WriteLine("switcheoAddToWhitelist");
            RunScript(switcheoAddToWhitelist, "1.807", mockSnapshot.Object);

            AppendMethodName("switcheoDeposit");
            Console.WriteLine("switcheoDeposit");
            RunScript(switcheoDeposit, "8.237", mockSnapshot.Object);

            AppendMethodName("switcheoWithdraw");
            Console.WriteLine("switcheoWithdraw");
            RunScript(switcheoWithdraw, "9.768", mockSnapshot.Object, transaction);

        }

        // [TestMethod]
        // public void ExecuteScript() {
        //     var BlockState = new BlockState{SystemFeeAmount = 0, TrimmedBlock = new Block().Header.Trim()};
        //     var AssetState = new AssetState{
        //         AssetId = new UInt256(), 
        //         AssetType = AssetType.Token, 
        //         Name = "asset", 
        //         Amount = new Fixed8(1000000000), 
        //         Available = new Fixed8(1000000000),
        //         Precision = 0x08,
        //         Fee = new Fixed8(0),
        //         FeeAddress = new UInt160(),
        //         Owner = new ECPoint(),
        //         Admin = new UInt160(),
        //         Issuer = new UInt160(),
        //         Expiration = 0,
        //         IsFrozen = false
        //         };
        //     var ContractState = new ContractState{
        //         Script = script,
        //         ParameterList = new ContractParameterType[] {ContractParameterType.String, ContractParameterType.Array},
        //         ReturnType = ContractParameterType.ByteArray,
        //         ContractProperties = ContractPropertyState.HasStorage,
        //         Name = "contract",
        //         CodeVersion = "1.0",
        //         Author = "author",
        //         Email = "author@email.com",
        //         Description = "test contract"
        //     };

        //     var mockSnapshot = TestBlockchain.InitializeMockSnapshot(
        //         BlockState: BlockState, 
        //         AssetState: AssetState,
        //         ContractState: ContractState
        //     );
        //     ApplicationEngine engine = ApplicationEngine.Run(script, mockSnapshot.Object);
        //     JObject json = new JObject();
            
        //     json["script"] = script.ToHexString();
        //     json["state"] = engine.State;
        //     json["gas_consumed"] = engine.GasConsumed.ToString();

        //     // json["stack_top"] = engine.ResultStack.Peek().GetByteArray().ToHexString();
        //     // json["stack_top"] = engine.ResultStack.Peek().GetBigInteger().ToString();
        //     // Console.WriteLine(json["stack_top"].AsString());

        //     json["state"].AsString().Should().Be("HALT");
        //     json["gas_consumed"].AsString().Should().Be("0.001");
        // }
    }
}
