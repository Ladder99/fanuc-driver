using System.IO;
using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers
{
    public class ProductionDataExternalSubprogramDetails : Veneer
    {
        public ProductionDataExternalSubprogramDetails(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false) : base(veneers, name, isCompound, isInternal)
        {
            
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
        {
            if (nativeInputs.Slice(0,5).All(o => o.success == true))
            {
                var currentCurrentProgramNumber = nativeInputs[0].response.cnc_rdprgnum.prgnum.data;
                var previousCurrentProgramNumber = nativeInputs[6] == null
                    ? -1
                    : nativeInputs[6].response.cnc_rdprgnum.prgnum.data;

                int blockCount = 0;
                List<string> blocks = new();
                List<KeyValuePair<string, string>> comments = new();

                if (currentCurrentProgramNumber != previousCurrentProgramNumber)
                {
                    Logger.Debug($"[{Veneers.Machine.Id}] Current program has changed {previousCurrentProgramNumber} => {currentCurrentProgramNumber}");

                    if (additionalInputs[1].ContainsKey(currentCurrentProgramNumber.ToString()))
                    {
                        try
                        {
                            string fn = additionalInputs[1][currentCurrentProgramNumber.ToString()];
                            blockCount = additionalInputs[0];   // Desired number of blocks to read.
                            blocks = File.ReadLines(fn).Take(blockCount).ToList();
                            blockCount = blocks.Count;  // Actual number of blocks read.

                            if (!string.IsNullOrEmpty(additionalInputs[2]))
                            {
                                var regex = new Regex(additionalInputs[2]);
                                foreach (var block in blocks)
                                {
                                    var match = regex.Match(block);
                                    if (match.Success)
                                    {
                                        comments.Add(new KeyValuePair<string, string>(
                                            match.Groups["key"].Value,
                                            match.Groups["value"].Value
                                        ));
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Warn($"[{Veneers.Machine.Id}] Failed to process file '{additionalInputs[1][currentCurrentProgramNumber.ToString()]}'.");
                        }
                    }
                }
                
                var currentValue = new
                {
                    program = new {
                        current = new {
                            name = $"O{nativeInputs[0].response.cnc_rdprgnum.prgnum.data}",
                            number = nativeInputs[0].response.cnc_rdprgnum.prgnum.data,
                            block_count = blockCount,
                            blocks,
                            comments
                        },
                        selected = new {
                            name = $"O{nativeInputs[0].response.cnc_rdprgnum.prgnum.mdata}",
                            number = nativeInputs[0].response.cnc_rdprgnum.prgnum.mdata
                        }
                    },
                    pieces = new {
                        produced = nativeInputs[1]!.response.cnc_rdparam.param.data.ldata,
                        produced_life = nativeInputs[2]!.response.cnc_rdparam.param.data.ldata,
                        remaining = nativeInputs[3]!.response.cnc_rdparam.param.data.ldata
                    },
                    timers = new {
                        cycle_time_ms = (nativeInputs[4]!.response.cnc_rdparam.param.data.ldata * 60000) +
                                        nativeInputs[5]!.response.cnc_rdparam.param.data.ldata
                    }
                };

                await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);

                if (currentValue.IsDifferentString((object)LastChangedValue))
                {
                    await OnDataChangedAsync(nativeInputs, additionalInputs, currentValue);
                }
            }
            else
            {
                await OnHandleErrorAsync(nativeInputs, additionalInputs);
            }

            return new { veneer = this };
        }
    }
}