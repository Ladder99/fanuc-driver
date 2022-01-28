using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;
using l99.driver.fanuc.gcode;

namespace l99.driver.fanuc.veneers
{
    public class ThreeWayStateData : Veneer
    {
        public ThreeWayStateData(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            
            if (input.success && additionalInputs[0].success)
            {
                var fields = input.response.cnc_statinfo.statinfo.GetType().GetFields();
                
                var current_value = new
                {
                    input.response.cnc_statinfo.statinfo.aut,
                    input.response.cnc_statinfo.statinfo.run,
                    input.response.cnc_statinfo.statinfo.motion,
                    input.response.cnc_statinfo.statinfo.mstb,
                    input.response.cnc_statinfo.statinfo.emergency,
                    input.response.cnc_statinfo.statinfo.alarm,
                    poweron_min = additionalInputs[0].response.cnc_rdparam.param.data.ldata,
                    operating_min = additionalInputs[1].response.cnc_rdparam.param.data.ldata,
                    cutting_min = additionalInputs[2].response.cnc_rdparam.param.data.ldata
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