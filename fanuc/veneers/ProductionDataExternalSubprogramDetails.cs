using System.Dynamic;
using System.IO;
using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers;

public class ProductionDataExternalSubprogramDetails : Veneer
{
    public ProductionDataExternalSubprogramDetails(Veneers veneers, string name = "", bool isCompound = false,
        bool isInternal = false) : base(veneers, name, isCompound, isInternal)
    {
    }

    protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        if (nativeInputs.Slice(0, 5).All(o => o.success == true))
        {
            var extractionParameters = additionalInputs[0];
            var currentCurrentProgramNumber = nativeInputs[0].response.cnc_rdprgnum.prgnum.data;
            var previousCurrentProgramNumber = nativeInputs[6] == null
                ? -1
                : nativeInputs[6].response.cnc_rdprgnum.prgnum.data;

            var blockCount = 0;
            List<string> blocks = new();
            var extractions = new ExpandoObject() as IDictionary<string, object>;

            if (currentCurrentProgramNumber != previousCurrentProgramNumber)
            {
                Logger.Debug(
                    $"[{Veneers.Machine.Id}] Current program has changed {previousCurrentProgramNumber} => {currentCurrentProgramNumber}");

                if (extractionParameters["files"].ContainsKey(currentCurrentProgramNumber.ToString()))
                    try
                    {
                        string fn = extractionParameters["files"][currentCurrentProgramNumber.ToString()];
                        blockCount = extractionParameters["lines"]; // Desired number of blocks to read.
                        blocks = File.ReadLines(fn).Take(blockCount).ToList();
                        blockCount = blocks.Count; // Actual number of blocks read.

                        /*
                        if (!string.IsNullOrEmpty(additionalInputs[2]))
                        {
                            var regex = new Regex(additionalInputs[2]);
                            foreach (var block in blocks)
                            {
                                var match = regex.Match(block);
                                if (match.Success)
                                    comments.Add(new KeyValuePair<string, string>(
                                        match.Groups["key"].Value,
                                        match.Groups["value"].Value
                                    ));
                            }
                        }
                        */
                        
                        foreach (var block in blocks)
                        {
                            foreach (var extractionKey in extractionParameters["properties"].Keys)
                            {
                                var regex = new Regex(extractionParameters["properties"][extractionKey]);
                                var match = regex.Match(block);
                                if (match.Success)
                                {
                                    extractions.Add(extractionKey, new { unavailable = false, value = match.Groups["value"].Value });
                                    break;
                                }
                            }
                        }
                        
                        foreach (var extractionKey in extractionParameters["properties"].Keys)
                        {
                            if (!extractions.ContainsKey(extractionKey))
                            {   
                                extractions.Add(extractionKey, new { unavailable = true, value = (object)null });
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Warn(
                            $"[{Veneers.Machine.Id}] Failed to process file '{additionalInputs[1][currentCurrentProgramNumber.ToString()]}'.");
                    }
            }

            var currentValue = new
            {
                program = new
                {
                    current = new
                    {
                        name = $"O{nativeInputs[0].response.cnc_rdprgnum.prgnum.data}",
                        number = nativeInputs[0].response.cnc_rdprgnum.prgnum.data,
                        block_count = blockCount,
                        blocks,
                        extractions
                    },
                    selected = new
                    {
                        name = $"O{nativeInputs[0].response.cnc_rdprgnum.prgnum.mdata}",
                        number = nativeInputs[0].response.cnc_rdprgnum.prgnum.mdata
                    }
                },
                pieces = new
                {
                    produced = nativeInputs[1]!.response.cnc_rdparam.param.data.ldata,
                    produced_life = nativeInputs[2]!.response.cnc_rdparam.param.data.ldata,
                    remaining = nativeInputs[3]!.response.cnc_rdparam.param.data.ldata
                },
                timers = new
                {
                    cycle_time_ms = nativeInputs[4]!.response.cnc_rdparam.param.data.ldata * 60000 +
                                    nativeInputs[5]!.response.cnc_rdparam.param.data.ldata
                }
            };

            await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);

            if (currentValue.IsDifferentString((object) LastChangedValue))
                await OnDataChangedAsync(nativeInputs, additionalInputs, currentValue);
        }
        else
        {
            await OnHandleErrorAsync(nativeInputs, additionalInputs);
        }

        return new {veneer = this};
    }
}