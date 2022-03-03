using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace l99.driver.fanuc.gcode
{
    /* block tracking usage
     
    private Blocks _blocks = new Blocks();
    private short _readAheadBytes = 128;
    
    dynamic blkcount = await machine["platform"].RdBlkCountAsync();
    dynamic actpt = await machine["platform"].RdActPtAsync();
    dynamic execprog = await machine["platform"].RdExecProgAsync(_readAheadBytes);
    _blocks.Add(blkcount.response.cnc_rdblkcount.prog_bc, actpt.response.cnc_rdactpt.blk_no, execprog.response.cnc_rdexecprog.data);
    Console.WriteLine(_blocks.ToString(showMissedBlocks: true));
    
    */

    public class Blocks
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

        private bool DEBUG = false;
        
        private Dictionary<int,Block> _blocks = new Dictionary<int,Block>();

        public int InitializedBlockPointer = -1;
        public int PreviousBlockPointer=-1;
        public int CurrentBlockPointer=-1;
        public bool IsPointerValid = false;

        public bool Add1(int blockPointer, char[] nativeBlockText, bool dropLast = true)
        {
            IsPointerValid = true;

            int baseBlockPointer = blockPointer;

            return add(baseBlockPointer, nativeBlockText, dropLast);
        }
        
        public bool Add2(int blockCounter, int blockPointer, char[] nativeBlockText, bool dropLast = true)
        {
            if (blockCounter - blockPointer > 1)
            {
                IsPointerValid = false;
                return false;
            }

            IsPointerValid = true;
            
            int baseBlockPointer = blockCounter - 1;
            
            return add(baseBlockPointer, nativeBlockText, dropLast);
        }

        private bool add(int baseBlockPointer, char[] nativeBlockText, bool dropLast = true)
        {
            string[] blockTextArray = string.Join("", nativeBlockText).Trim().Split('\n');
            
            for (int blockPointerOffset = 0; 
                 blockPointerOffset < blockTextArray.Length - (dropLast?1:0); 
                 blockPointerOffset++)
            {
                int blockNumber = baseBlockPointer + blockPointerOffset;
                
                if (!_blocks.ContainsKey(blockNumber))
                {
                    string blockText = blockTextArray[blockPointerOffset];
                    
                    var block = new Block() 
                    { 
                        BlockNumber = blockNumber,
                        BlockText = blockText
                    };

                    if (DEBUG)
                    {
                        var cursor = blockNumber == baseBlockPointer ? "(.add) now" : "(.add) ahd";
                        Console.WriteLine($"{cursor} {block.ToString()}");
                    }

                    _blocks.Add(blockNumber, block);
                }
            }

            PreviousBlockPointer = CurrentBlockPointer;
            CurrentBlockPointer = baseBlockPointer;

            if (DEBUG)
            {
                if (PreviousBlockPointer != CurrentBlockPointer)
                {
                    Console.WriteLine($"(.add) mv  [{PreviousBlockPointer}] -> [{CurrentBlockPointer}]");
                    Console.WriteLine($"(.add) >>> {_blocks[CurrentBlockPointer].ToString()}");
                }
            }

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

        public IEnumerable<Block> ExecutedBlocks
        {
            get
            {
                List<Block> blocks = new List<Block>();
                
                if(_blocks.ContainsKey(CurrentBlockPointer))
                    blocks.Add(_blocks[CurrentBlockPointer]);
                
                if (MissedBlockCount > 0)
                {
                    foreach (var missedBlock in MissedBlockNumbers.Reverse())
                    {
                        if (_blocks.ContainsKey(missedBlock))
                        {
                            blocks.Add(_blocks[missedBlock]);
                        }
                        else
                        {
                            blocks.Add(new Block
                            {
                                BlockNumber = missedBlock, 
                                BlockText = "?"
                            });
                        }
                    }
                }

                return blocks;
            }
        }
    }

    public class Block
    {
        public int BlockNumber { get; set; }
        public string BlockText { get; set; }

        public override string ToString()
        {
            return $"[{BlockNumber}]\t{BlockText}";
        }
    }
}