using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.VM;
using Neo.VM.Types;
using System;
using System.IO;

namespace Neo.SmartContract
{
    public class ApplicationEngine : ExecutionEngine
    {
        private const long ratio = 100000;
        private const long gas_free = 10 * 100000000;
        private readonly long gas_amount;
        private long gas_consumed = 0;
        private readonly bool testMode;
        public readonly Snapshot snapshot;

        public readonly bool skipWitnessVerify;

        public Fixed8 GasConsumed => new Fixed8(gas_consumed);
        public new NeoService Service => (NeoService)base.Service;

        public ApplicationEngine(TriggerType trigger, IScriptContainer container, Snapshot snapshot, Fixed8 gas, bool testMode = false, bool skipWitnessVerify = false)
            : base(container, Cryptography.Crypto.Default, snapshot, new NeoService(trigger, snapshot))
        {
            this.gas_amount = gas_free + gas.GetData();
            this.testMode = testMode;
            this.snapshot = snapshot;
            this.skipWitnessVerify = skipWitnessVerify;
        }

        private bool CheckDynamicInvoke()
        {
            Instruction instruction = CurrentContext.CurrentInstruction;
            switch (instruction.OpCode)
            {
                case OpCode.APPCALL:
                case OpCode.TAILCALL:
                    if (instruction.Operand.NotZero()) return true;
                    // if we get this far it is a dynamic call
                    // now look at the current executing script
                    // to determine if it can do dynamic calls
                    return snapshot.Contracts[new UInt160(CurrentContext.ScriptHash)].HasDynamicInvoke;
                case OpCode.CALL_ED:
                case OpCode.CALL_EDT:
                    return snapshot.Contracts[new UInt160(CurrentContext.ScriptHash)].HasDynamicInvoke;
                default:
                    return true;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            Service.Dispose();
        }

        protected virtual long GetPrice()
        {
            Instruction instruction = CurrentContext.CurrentInstruction;
            if (instruction.OpCode <= OpCode.NOP) return 0;
            switch (instruction.OpCode)
            {
                case OpCode.APPCALL:
                case OpCode.TAILCALL:
                    return 10;
                case OpCode.SYSCALL:
                    return GetPriceForSysCall();
                case OpCode.SHA1:
                case OpCode.SHA256:
                    return 10;
                case OpCode.HASH160:
                case OpCode.HASH256:
                    return 20;
                case OpCode.CHECKSIG:
                case OpCode.VERIFY:
                    return 100;
                case OpCode.CHECKMULTISIG:
                    {
                        if (CurrentContext.EvaluationStack.Count == 0) return 1;

                        var item = CurrentContext.EvaluationStack.Peek();

                        int n;
                        if (item is Neo.VM.Types.Array array) n = array.Count;
                        else n = (int)item.GetBigInteger();

                        if (n < 1) return 1;
                        return 100 * n;
                    }
                default: return 1;
            }
        }

        protected virtual long GetPriceForSysCall()
        {
            Instruction instruction = CurrentContext.CurrentInstruction;
            uint api_hash = instruction.Operand.Length == 4
                ? instruction.TokenU32
                : instruction.TokenString.ToInteropMethodHash();
            // Console.WriteLine(instruction.TokenString);
            long price = Service.GetPrice(api_hash);
            if (price > 0) return price;
            if (api_hash == "Neo.Asset.Create".ToInteropMethodHash() ||
               api_hash == "AntShares.Asset.Create".ToInteropMethodHash())
                return 5000L * 100000000L / ratio;
            if (api_hash == "Neo.Asset.Renew".ToInteropMethodHash() ||
                api_hash == "AntShares.Asset.Renew".ToInteropMethodHash())
                return (byte)CurrentContext.EvaluationStack.Peek(1).GetBigInteger() * 5000L * 100000000L / ratio;
            if (api_hash == "Neo.Contract.Create".ToInteropMethodHash() ||
                api_hash == "Neo.Contract.Migrate".ToInteropMethodHash() ||
                api_hash == "AntShares.Contract.Create".ToInteropMethodHash() ||
                api_hash == "AntShares.Contract.Migrate".ToInteropMethodHash())
            {
                long fee = 100L;

                ContractPropertyState contract_properties = (ContractPropertyState)(byte)CurrentContext.EvaluationStack.Peek(3).GetBigInteger();

                if (contract_properties.HasFlag(ContractPropertyState.HasStorage))
                {
                    fee += 400L;
                }
                if (contract_properties.HasFlag(ContractPropertyState.HasDynamicInvoke))
                {
                    fee += 500L;
                }
                return fee * 100000000L / ratio;
            }
            if (api_hash == "System.Storage.Put".ToInteropMethodHash() ||
                api_hash == "System.Storage.PutEx".ToInteropMethodHash() ||
                api_hash == "Neo.Storage.Put".ToInteropMethodHash() ||
                api_hash == "AntShares.Storage.Put".ToInteropMethodHash())
                return ((CurrentContext.EvaluationStack.Peek(1).GetByteArray().Length + CurrentContext.EvaluationStack.Peek(2).GetByteArray().Length - 1) / 1024 + 1) * 1000;
            return 1;
        }

        protected override bool PreExecuteInstruction()
        {
            var instruction = CurrentContext.CurrentInstruction;
            if (CurrentContext.InstructionPointer >= CurrentContext.Script.Length)
                return true;

            if (instruction.OpCode == OpCode.SYSCALL) {
                using (StreamWriter sw = File.AppendText("/Users/alexfrag/Documents/Development/NEO/nixtest/neo/neo.UnitTests/callDump.txt")) 
                {
                    // sw.WriteLine(CurrentContext.CurrentInstruction.TokenString + " : " + gas_consumed.ToString());
                    sw.WriteLine(CurrentContext.CurrentInstruction.TokenString);
                }	
                // Console.WriteLine(gas_consumed.ToString());
            }
            // if (CurrentContext.CurrentInstruction.OpCode == OpCode.NUMEQUAL) {
            //     var val1 = CurrentContext.EvaluationStack.Peek();
            //     var val2 = CurrentContext.EvaluationStack.Peek(1);
            //     var valsEq = val1.GetBigInteger() == val2.GetBigInteger();
            //     Console.WriteLine("NUMEQUAL " + val1.GetByteArray().ToHexString() + " : " + val2.GetByteArray().ToHexString() + " : " + valsEq);
            //     // Console.WriteLine(val1.GetBigInteger());
            //     // Console.WriteLine(val2.GetBigInteger());
            // }

            // if (CurrentContext.CurrentInstruction.OpCode == OpCode.JMPIF) {
            //     var val = CurrentContext.EvaluationStack.Peek();
            //     Console.WriteLine("JMPIF " + val.GetBoolean());
            // }

            if (instruction.OpCode.ToString() == "CALL_ED") {
                Console.WriteLine(instruction.OpCode.ToString());
                var rvCount = instruction.Operand[0];
                var pCount = instruction.Operand[1];
                var scriptHash = CurrentContext.EvaluationStack.Peek();
                Console.WriteLine("rvCount: " + rvCount);
                Console.WriteLine("pCount: " + pCount);
                Console.WriteLine("scriptHash: " + scriptHash.GetByteArray().ToHexString());
            }

            // using (StreamWriter sw = File.AppendText("/Users/alexfrag/Documents/Development/NEO/nixtest/neo/neo.UnitTests/callDump.txt")) 
            //     {
            //         var op = CurrentContext.CurrentInstruction.OpCode;
            //         var opString = op.ToString();
            //         if ((int)op > 1 && (int)op < 75) {
            //             opString = "PUSHBYTES" + op.ToString();
            //         }
            //         if (opString == "PUSHT") {
            //             opString = "PUSH1";
            //         }
            //         sw.WriteLine(opString);
            //     }	            
            gas_consumed = checked(gas_consumed + GetPrice() * ratio);
            if (!testMode && gas_consumed > gas_amount) return false;
            if (!CheckDynamicInvoke()) return false;
            return true;
        }

        public static ApplicationEngine Run(byte[] script, Snapshot snapshot,
            IScriptContainer container = null, Block persistingBlock = null, bool testMode = false, Fixed8 extraGAS = default(Fixed8), bool skipWitnessVerify = false)
        {
            snapshot.PersistingBlock = persistingBlock ?? snapshot.PersistingBlock ?? new Block
            {
                Version = 0,
                PrevHash = snapshot.CurrentBlockHash,
                MerkleRoot = new UInt256(),
                Timestamp = snapshot.Blocks[snapshot.CurrentBlockHash].TrimmedBlock.Timestamp + Blockchain.SecondsPerBlock,
                Index = snapshot.Height + 1,
                ConsensusData = 0,
                NextConsensus = snapshot.Blocks[snapshot.CurrentBlockHash].TrimmedBlock.NextConsensus,
                Witness = new Witness
                {
                    InvocationScript = new byte[0],
                    VerificationScript = new byte[0]
                },
                Transactions = new Transaction[0]
            };
            ApplicationEngine engine = new ApplicationEngine(TriggerType.Application, container, snapshot, extraGAS, testMode, skipWitnessVerify);
            Console.WriteLine("Created Application Engine.");
            engine.LoadScript(script);
            engine.Execute();
            return engine;
        }

        public static ApplicationEngine Run(byte[] script, IScriptContainer container = null, Block persistingBlock = null, bool testMode = false, Fixed8 extraGAS = default(Fixed8))
        {
            using (Snapshot snapshot = Blockchain.Singleton.GetSnapshot())
            {
                return Run(script, snapshot, container, persistingBlock, testMode, extraGAS);
            }
        }
    }
}
