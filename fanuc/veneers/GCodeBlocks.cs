using System.Dynamic;
using l99.driver.@base;
using l99.driver.fanuc.utils.gcode;

//TODO: review relationship between pointer and block counter

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers;

public class GCodeBlocks : Veneer
{
    private readonly Blocks _blocks;

    public GCodeBlocks(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false) : base(
        veneers, name, isCompound, isInternal)
    {
        _blocks = new Blocks();
    }

    protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        if (nativeInputs.All(o => o.success == true))
        {
            if (!nativeInputs[0].@null && nativeInputs[0].success)
                _blocks.Add(nativeInputs[0].response.cnc_rdblkcount.prog_bc,
                    nativeInputs[1].response.cnc_rdactpt.blk_no,
                    nativeInputs[2].response.cnc_rdexecprog.data);
            else
                _blocks.Add(nativeInputs[1].response.cnc_rdactpt.blk_no,
                    nativeInputs[2].response.cnc_rdexecprog.data);

            dynamic currentValue = new ExpandoObject();
            currentValue.blocks = _blocks.ExecutedBlocks;
            
            /*
            var currentValue = new
            {
                blocks = _blocks.ExecutedBlocks
            };
            */

            //Console.WriteLine(_blocks.ToString(showMissedBlocks: true));
            /*
            if (currentValue.blocks.Count() > 0)
            {
                Console.WriteLine("--- executed ---");
                foreach (var block in currentValue.blocks)
                {
                    Console.WriteLine(block.ToString());
                }

                Console.WriteLine("");
            }
            */

            await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);

            //var lastKeys = ((List<gcode.Block>)LastChangedValue.blocks).Select(x => x.BlockNumber);
            //var currentKeys = ((List<gcode.Block>)currentValue.blocks).Select(x => x.BlockNumber);

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