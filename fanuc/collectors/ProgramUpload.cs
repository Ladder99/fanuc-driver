using l99.driver.@base;
using l99.driver.fanuc.strategies;
using l99.driver.fanuc.veneers;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors;

// ReSharper disable once UnusedType.Global
public class ProgramUpload : FanucMultiStrategyCollector
{
    public ProgramUpload(FanucMultiStrategy strategy, object configuration) : base(strategy, configuration)
    {
    }

    public override async Task InitPathsAsync()
    {
        await Strategy.Apply(typeof(veneers.ProgramUpload), "program_upload");
    }

    public override async Task CollectForEachPathAsync(short currentPath, string[] axis, string[] spindle, dynamic pathMarker)
    {
        var rdprgnum = await Strategy.Platform.RdPrgNumAsync();
        var currentProgramNumber = rdprgnum.response.cnc_rdprgnum.prgnum.data;
        await Strategy.SetKeyed("current_program_number", currentProgramNumber);

        var lastKnownProgramNumber = Strategy.GetKeyed("current_program_number+last");

        if (lastKnownProgramNumber != currentProgramNumber)
        {
            Console.WriteLine($"program number changed, getting program {currentProgramNumber} from NC");
            
            string program_contents = "";

            var start = await Strategy.Platform.UpStart3Async(0, currentProgramNumber, currentProgramNumber);

            if (Focas.focas_ret.EW_OK == start.rc)
            {
                var upload = await Strategy.Platform.Upload3Async(256);

                while (Focas.focas_ret.EW_OK == upload.rc || Focas.focas_ret.EW_BUFFER == upload.rc)
                {
                    if (Focas.focas_ret.EW_OK == upload.rc)
                    {
                        program_contents += new string(upload.response.cnc_upload3.data.data);
                        program_contents = program_contents.Replace("\0", string.Empty);
                    }

                    if (program_contents.Length > 0 &&
                        program_contents.Substring(program_contents.Length - 1, 1) == "%")
                    {
                        var end = await Strategy.Platform.UpEnd3Async();
                        break;
                    }

                    upload = await Strategy.Platform.Upload3Async(256);
                }
            }
            
            Console.WriteLine($"program {currentProgramNumber} read from NC");
            //Console.WriteLine(program_contents);
            
            await Strategy.SetKeyed("current_program_code", program_contents);
            
            await Strategy.Peel("program_upload",
                new dynamic[]
                {
                    true // function call Peel requires at least one native input
                },
                new dynamic[]
                {
                    Strategy.GetKeyed("current_program_number")!,
                    Strategy.GetKeyed("current_program_code")!
                });
        }

        await Strategy.SetKeyed("current_program_number+last", Strategy.GetKeyed("current_program_number"));
    }
}