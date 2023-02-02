using System.Dynamic;
using System.IO;
using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers;

public class ProductionDataExternalSubprogramDetails : Veneer
{
    private int _lineCount;
    private List<string> _lines = new();
    private IDictionary<string, object> _properties = new ExpandoObject() as IDictionary<string, object>;

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

            if (currentCurrentProgramNumber != previousCurrentProgramNumber)
            {
                Logger.Debug(
                    $"[{Veneers.Machine.Id}] Current program has changed {previousCurrentProgramNumber} => {currentCurrentProgramNumber}");

                // Reset cached variables since a new program is being executed by the control.
                _lineCount = 0;
                _lines = new List<string>();
                _properties = new ExpandoObject() as IDictionary<string, object>;

                if (extractionParameters["files"].ContainsKey(currentCurrentProgramNumber.ToString()))
                    try
                    {
                        string fn = extractionParameters["files"][
                            currentCurrentProgramNumber.ToString()]; // Get file path of the program number.
                        _lineCount = extractionParameters["lines"]["count"]; // Desired number of blocks to read.
                        _lines = File.ReadLines(fn).Take(_lineCount).ToList(); // Read lines from file.
                        _lineCount = _lines.Count; // Actual number of blocks read.

                        foreach (var line in _lines) // Check if we can extract data from each line.
                        foreach (var extractionKey in extractionParameters["properties"]["map"].Keys)
                        {
                            var regex = new Regex(extractionParameters["properties"]["map"][extractionKey]);
                            var match = regex.Match(line);
                            if (match.Success)
                            {
                                _properties.Add(extractionKey,
                                    new {unavailable = false, value = match.Groups["value"].Value});
                                break;
                            }
                        }

                        // Add properties not found in program lines as unavailable.
                        foreach (var extractionKey in Enumerable.Except(extractionParameters["properties"]["map"].Keys,
                                     _properties.Keys))
                            _properties.Add(extractionKey, new {unavailable = true, value = (object) null});
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
                        block_count = _lineCount,
                        blocks = extractionParameters["lines"]["show"] ? _lines : new List<string>(),
                        extractions = _properties
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

            if (!extractionParameters["lines"]["keep"]) // Reset cached lines variables.
            {
                _lines = new List<string>();
                _lineCount = 0;
            }

            if (!extractionParameters["properties"]["keep"]) // Reset cached properties variables.
                _properties = new ExpandoObject() as IDictionary<string, object>;
        }
        else
        {
            await OnHandleErrorAsync(nativeInputs, additionalInputs);
        }

        return new {veneer = this};
    }
}