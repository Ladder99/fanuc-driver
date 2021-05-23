using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        
        public override async Task<dynamic?> InitializeAsync()
        {
            try
            {
                while (!_machine.VeneersApplied)
                {
                    dynamic connect = await _machine["platform"].ConnectAsync();
                    
                    if (connect.success)
                    {
                        _machine.ApplyVeneer(typeof(fanuc.veneers.Connect), "connect");
                        _machine.ApplyVeneer(typeof(fanuc.veneers.SysInfo), "sys_info");
                        
                        dynamic disconnect = await _machine["platform"].DisconnectAsync();
                        _machine.VeneersApplied = true;

                        Console.WriteLine("fanuc - created veneers");
                    }
                    else
                    {
                        await Task.Delay(_sweepMs);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[{_machine.Id}] Collector initialization failed.");
            }

            return null;
        }

        public override async Task<dynamic?> CollectAsync()
        {
            try
            {
                dynamic connect = await _machine["platform"].ConnectAsync();
                //await _machine.PeelVeneerAsync("connect", connect);

                if (connect.success)
                {
                    //dynamic info = _machine["platform"].SysInfo();
                    //await _machine.PeelVeneerAsync("sys_info", info);
        
                    dynamic blkcount = await _machine["platform"].RdBlkCountAsync();
                    //Console.WriteLine($"RdBlkCount({blkcount.rc})::prog_bc = {blkcount.response.cnc_rdblkcount.prog_bc}");
                        
                    dynamic actpt = await _machine["platform"].RdActPtAsync();
                    //Console.WriteLine($"RdActPt({actpt.rc})::prog_no = {actpt.response.cnc_rdactpt.prog_no}, blk_no = {actpt.response.cnc_rdactpt.blk_no}");
        
                    dynamic execprog = await _machine["platform"].RdExecProgAsync(_readAheadBytes);
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
                    dynamic prgnum = _machine["platform"].RdPrgNum();
                    Console.WriteLine($"RdPrgNum({prgnum.rc})::data = {prgnum.response.cnc_rdprgnum.prgnum.data}, mdata = {prgnum.response.cnc_rdprgnum.prgnum.mdata}");
                    */
                    
                    /*
                    dynamic prgname = _machine["platform"].ExePrgName();
                    Console.WriteLine($"ExePrgName({prgname.rc})::o_num = {prgname.response.cnc_exeprgname.exeprg.o_num}, name = {string.Join("",prgname.response.cnc_exeprgname.exeprg.name).Trim('\0')}");
                    */
                    
                    /*
                    dynamic prgname2 = _machine["platform"].ExePrgName2();
                    Console.WriteLine($"ExePrgName2({prgname2.rc})::path_name = {string.Join("",prgname2.response.cnc_exeprgname2.path_name).Trim('\0')}");
                    */
                    
                    /*
                    dynamic seqnum = _machine["platform"].RdSeqNum();
                    Console.WriteLine($"RdSeqNum({seqnum.rc})::seqnum = {seqnum.response.cnc_rdseqnum.seqnum.data}");
                    */
                    
                    /*
                    dynamic execpt = _machine["platform"].RdExecPt();
                    Console.WriteLine($"RdExecPt({execpt.rc})::pact.prog_no = {execpt.response.cnc_rdexecpt.pact.prog_no}, pact.blk_no = {execpt.response.cnc_rdexecpt.pact.blk_no}");
                    Console.WriteLine($"RdExecPt({execpt.rc})::pnext.prog_no = {execpt.response.cnc_rdexecpt.pnext.prog_no}, pnext.blk_no = {execpt.response.cnc_rdexecpt.pnext.blk_no}");
                    */
                    
                    /*
                    dynamic progline = _machine["platform"].RdProgLine(-1, 0, 128);
                    var lines = string.Join("", progline.response.cnc_rdprogline.prog_data).Trim().Split('\n');
                    Console.WriteLine($"RdProgLine({progline.rc})::line_len = {progline.response.cnc_rdprogline.line_len}, data_len = {progline.response.cnc_rdprogline.data_len}");
                    Console.WriteLine($"RdProgLine({progline.rc})::prog_data = ");
                    foreach (var line in lines)
                    {
                        Console.WriteLine($"\t{line}");
                    }
                    */
                    
                    dynamic disconnect = await _machine["platform"].DisconnectAsync();

                    LastSuccess = connect.success;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[{_machine.Id}] Collector sweep failed.");
            }

            return null;
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