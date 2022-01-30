using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;

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
            if (input.success && additionalInputs.All(o => o.success == true))
            {
                var current_value = new
                {
                    input.response.cnc_statinfo.statinfo.aut,
                    input.response.cnc_statinfo.statinfo.run,
                    input.response.cnc_statinfo.statinfo.motion,
                    input.response.cnc_statinfo.statinfo.mstb,
                    input.response.cnc_statinfo.statinfo.emergency,
                    input.response.cnc_statinfo.statinfo.alarm,
                    timers = new
                    {
                        poweron_min = additionalInputs[0].response.cnc_rdparam.param.data.ldata,
                        operating_min = additionalInputs[1].response.cnc_rdparam.param.data.ldata,
                        cutting_min = additionalInputs[2].response.cnc_rdparam.param.data.ldata
                    },
                    @override = new {
                        feed = 255-additionalInputs[3].response.pmc_rdpmcrng.buf.cdata[0],
                        rapid = additionalInputs[4].response.pmc_rdpmcrng.buf.cdata[0],
                        spindle = additionalInputs[5].response.pmc_rdpmcrng.buf.cdata[0]
                    },
                    modal = new
                    {
                        m1 = additionalInputs[6].response.cnc_modal.modal.aux.aux_data,
                        m2 = additionalInputs[7].response.cnc_modal.modal.aux.aux_data,
                        m3 = additionalInputs[8].response.cnc_modal.modal.aux.aux_data,
                        t = additionalInputs[9].response.cnc_modal.modal.aux.aux_data
                    }
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