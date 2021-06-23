using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;
using l99.driver.fanuc.gcode;

namespace l99.driver.fanuc.veneers
{
    public class GCodeBlocks : Veneer
    {
        private Blocks _blocks;
        
        public GCodeBlocks(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            _blocks = new Blocks();
            
            lastChangedValue = new
            {
                blocks = new List<gcode.Block>()
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success && additionalInputs[0].success && additionalInputs[1].success)
            {
                _blocks.Add(input.response.cnc_rdblkcount.prog_bc, 
                    additionalInputs[0].response.cnc_rdactpt.blk_no, 
                    additionalInputs[1].response.cnc_rdexecprog.data);
                
                var current_value = new
                {
                    blocks = _blocks.ExecutedBlocks
                };
                
                await onDataArrivedAsync(input, current_value);
                
                var last_keys = ((List<gcode.Block>)lastChangedValue.blocks).Select(x => x.BlockNumber);
                var current_keys = ((List<gcode.Block>)current_value.blocks).Select(x => x.BlockNumber);

                if (last_keys.Except(current_keys).Count() + current_keys.Except(last_keys).Count() > 0)
                {
                    await onDataChangedAsync(input, current_value);
                }
            }
            else
            {
                await onErrorAsync(input);
            }

            return new { veneer = this };
        }
    }
}