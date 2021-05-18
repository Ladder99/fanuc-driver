using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.collectors
{
    public class BlockTracker : FanucCollector
    {
        private Blocks _blocks = new Blocks();
        private short _readAheadBytes = 128;
    
        public BlockTracker(Machine machine, int sweepMs = 1000) : base(machine, sweepMs)
        {
            
        }
        
        public override void Initialize()
        {
            while (!_machine.VeneersApplied)
            {
                Console.WriteLine("fanuc - creating veneers");

                dynamic connect = ((FanucMachine)_machine).Platform.Connect();
                Console.WriteLine(JObject.FromObject(connect).ToString());

                if (connect.success)
                {
                    _machine.ApplyVeneer(typeof(fanuc.veneers.Connect), "connect");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.SysInfo), "sys_info");
                    
                    dynamic disconnect = ((FanucMachine)_machine).Platform.Disconnect();
                    _machine.VeneersApplied = true;

                    Console.WriteLine("fanuc - created veneers");
                }
                else
                {
                    // not in here
                    System.Threading.Thread.Sleep(_sweepMs);
                }
            }
        }

        public override void Collect()
        {
            dynamic connect = ((FanucMachine)_machine).Platform.Connect();
            //_machine.PeelVeneer("connect", connect);

            if (connect.success)
            {
                //dynamic info = ((FanucMachine)_machine).Platform.SysInfo();
                //_machine.PeelVeneer("sys_info", info);
    
                dynamic blkcount = ((FanucMachine)_machine).Platform.RdBlkCount();
                //Console.WriteLine($"RdBlkCount({blkcount.rc})::prog_bc = {blkcount.response.cnc_rdblkcount.prog_bc}");
                    
                dynamic actpt = ((FanucMachine)_machine).Platform.RdActPt();
                //Console.WriteLine($"RdActPt({actpt.rc})::prog_no = {actpt.response.cnc_rdactpt.prog_no}, blk_no = {actpt.response.cnc_rdactpt.blk_no}");
    
                dynamic execprog = ((FanucMachine)_machine).Platform.RdExecProg(_readAheadBytes);
                /*
                var execlines = string.Join("", execprog.response.cnc_rdexecprog.data).Trim().Split('\n');
                Console.WriteLine($"RdExecProg({execprog.rc})::length = {execprog.response.cnc_rdexecprog.length}, blknum = {execprog.response.cnc_rdexecprog.blknum}");
                Console.WriteLine($"RdExecProg({execprog.rc})::data = ");
                foreach (var line in execlines)
                {
                    Console.WriteLine($"\t{line}");
                }
                */
                
                _blocks.Add(blkcount.response.cnc_rdblkcount.prog_bc, actpt.response.cnc_rdactpt.blk_no, execprog.response.cnc_rdexecprog.data);
                Console.WriteLine(_blocks.ToString(showMissedBlocks: true));
                
                Console.WriteLine();
                
                /*
                dynamic prgnum = ((FanucMachine)_machine).Platform.RdPrgNum();
                Console.WriteLine($"RdPrgNum({prgnum.rc})::data = {prgnum.response.cnc_rdprgnum.prgnum.data}, mdata = {prgnum.response.cnc_rdprgnum.prgnum.mdata}");
                */
                
                /*
                dynamic prgname = ((FanucMachine)_machine).Platform.ExePrgName();
                Console.WriteLine($"ExePrgName({prgname.rc})::o_num = {prgname.response.cnc_exeprgname.exeprg.o_num}, name = {string.Join("",prgname.response.cnc_exeprgname.exeprg.name).Trim('\0')}");
                */
                
                /*
                dynamic prgname2 = ((FanucMachine)_machine).Platform.ExePrgName2();
                Console.WriteLine($"ExePrgName2({prgname2.rc})::path_name = {string.Join("",prgname2.response.cnc_exeprgname2.path_name).Trim('\0')}");
                */
                
                /*
                dynamic seqnum = ((FanucMachine)_machine).Platform.RdSeqNum();
                Console.WriteLine($"RdSeqNum({seqnum.rc})::seqnum = {seqnum.response.cnc_rdseqnum.seqnum.data}");
                */
                
                /*
                dynamic execpt = ((FanucMachine)_machine).Platform.RdExecPt();
                Console.WriteLine($"RdExecPt({execpt.rc})::pact.prog_no = {execpt.response.cnc_rdexecpt.pact.prog_no}, pact.blk_no = {execpt.response.cnc_rdexecpt.pact.blk_no}");
                Console.WriteLine($"RdExecPt({execpt.rc})::pnext.prog_no = {execpt.response.cnc_rdexecpt.pnext.prog_no}, pnext.blk_no = {execpt.response.cnc_rdexecpt.pnext.blk_no}");
                */
                
                /*
                dynamic progline = ((FanucMachine)_machine).Platform.RdProgLine(-1, 0, 128);
                var lines = string.Join("", progline.response.cnc_rdprogline.prog_data).Trim().Split('\n');
                Console.WriteLine($"RdProgLine({progline.rc})::line_len = {progline.response.cnc_rdprogline.line_len}, data_len = {progline.response.cnc_rdprogline.data_len}");
                Console.WriteLine($"RdProgLine({progline.rc})::prog_data = ");
                foreach (var line in lines)
                {
                    Console.WriteLine($"\t{line}");
                }
                */
                
                dynamic disconnect = ((FanucMachine)_machine).Platform.Disconnect();

                LastSuccess = connect.success;
            }
        }

        private class Blocks
        {
            public string ToString(bool showCurrentBlock = true, bool showMissedBlocks = false, bool showAllProgBlocks = false)
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"time: {DateTime.UtcNow}");
                sb.AppendLine($"previous block pointer = {PreviousBlockPointer}");
                sb.AppendLine($"current block pointer = {CurrentBlockPointer}");
                if (IsPointerValid)
                {
                    sb.AppendLine($"missed block count = {MissedBlockCount}");
                    sb.AppendLine($"missed block pointers = {string.Join(",", MissedBlockNumbers)}");

                    if (showCurrentBlock)
                    {
                        sb.AppendLine($"current executing block:");
                        sb.AppendLine(_blocks[CurrentBlockPointer].ToString());
                    }

                    if (showMissedBlocks)
                    {
                        if (MissedBlockCount > 0)
                        {
                            sb.AppendLine($"missed blocks:");

                            foreach (var missedBlock in MissedBlockNumbers.Reverse())
                            {
                                if (_blocks.ContainsKey(missedBlock))
                                {
                                    sb.AppendLine(_blocks[missedBlock].ToString());
                                }
                                else
                                {
                                    sb.AppendLine($"[{missedBlock}]\tMISSING!!!");
                                }
                            }
                        }
                    }

                    if (showAllProgBlocks)
                    {
                        sb.AppendLine("all program blocks:");
                        foreach (var kv in _blocks)
                        {
                            sb.AppendLine(_blocks[kv.Key].ToString());
                        }
                    }
                }
                else
                {
                    sb.AppendLine("ERROR: Invalid block pointers!");
                }

                return sb.ToString();
            }

            private Dictionary<int,Block> _blocks = new Dictionary<int,Block>();

            public int InitializedBlockPointer = -1;
            public int PreviousBlockPointer=-1;
            public int CurrentBlockPointer=-1;
            public bool IsPointerValid = false;

            public bool Add(int blockCounter, int blockPointer, char[] nativeBlockText, bool dropLast = true)
            {
                if (blockCounter - blockPointer > 1)
                {
                    IsPointerValid = false;
                    return false;
                }

                IsPointerValid = true;
                
                int baseBlockPointer = blockCounter - 1;
                string[] blockTextArray = string.Join("", nativeBlockText).Trim().Split('\n');
                
                for (int blockPointerOffset = 0; blockPointerOffset < blockTextArray.Length - (dropLast?1:0); blockPointerOffset++)
                {
                    if (!_blocks.ContainsKey(baseBlockPointer + blockPointerOffset))
                    {
                        _blocks.Add(baseBlockPointer + blockPointerOffset, 
                            new Block()
                            {
                                BlockNumber = baseBlockPointer + blockPointerOffset,
                                BlockText =  blockTextArray[blockPointerOffset]
                            });
                    }
                }

                PreviousBlockPointer = CurrentBlockPointer;
                CurrentBlockPointer = baseBlockPointer;

                return true;
            }
            
            public int MissedBlockCount
            {
                get
                {
                    if (PreviousBlockPointer == -1)
                    {
                        return CurrentBlockPointer;
                    }
                    else
                    {
                        if (CurrentBlockPointer - PreviousBlockPointer > 1)
                        {
                            return CurrentBlockPointer - PreviousBlockPointer - 1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    
                }
            }

            public int[] MissedBlockNumbers
            {
                get
                {
                    if (MissedBlockCount > 0)
                    {
                        List<int> missedBlocks = new List<int>();

                        for (int x = PreviousBlockPointer + 1; x < CurrentBlockPointer; x++)
                        {
                            missedBlocks.Add(x);
                        }

                        return missedBlocks.ToArray();
                    }
                    else
                    {
                        return new int[0];
                    }
                }
            }
        }

        private class Block
        {
            public int BlockNumber;
            public string BlockText;

            public override string ToString()
            {
                return $"[{BlockNumber}]\t{BlockText}";
            }
        }
    }
}