using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;
using l99.driver.fanuc.gcode;

namespace l99.driver.fanuc.veneers
{
    public class ThreeWayProductionData : Veneer
    {
        public ThreeWayProductionData(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success && additionalInputs.All(o => o.success == true))
            {
                var current_value = new
                {
                    feed_override = 255-input.response.pmc_rdpmcrng.buf.cdata[0],
                    rapid_override = additionalInputs[0].response.pmc_rdpmcrng.buf.cdata[0],
                    spindle_override = additionalInputs[1].response.pmc_rdpmcrng.buf.cdata[0],
                    program_name = new string(additionalInputs[2].response.cnc_exeprgname.exeprg.name).AsAscii(),
                    pieces_produced = additionalInputs[3].response.cnc_rdparam.param.data.ldata,
                    pieces_produced_life = additionalInputs[4].response.cnc_rdparam.param.data.ldata,
                    pieces_remaining = additionalInputs[5].response.cnc_rdparam.param.data.ldata,
                    cycle_time_min = additionalInputs[6].response.cnc_rdparam.param.data.ldata,
                    cycle_time_sec = additionalInputs[7].response.cnc_rdparam.param.data.ldata / 1000
                };
                
                await onDataArrivedAsync(input, current_value);
                
                if (current_value.IsDifferentString((object)lastChangedValue))
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