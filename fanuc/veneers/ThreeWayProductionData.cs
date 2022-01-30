using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;

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
                    program = new {
                        name = new string(input.response.cnc_exeprgname.exeprg.name).AsAscii(),
                        number = input.response.cnc_exeprgname.exeprg.o_num,
                        size_b = additionalInputs[0].response.cnc_rdprogdir3.buf.dir1.length,
                        comment = additionalInputs[0].response.cnc_rdprogdir3.buf.dir1.comment,
                        modified = new DateTimeOffset(new DateTime(additionalInputs[0].response.cnc_rdprogdir3.buf.dir1.mdate.year,
                            additionalInputs[0].response.cnc_rdprogdir3.buf.dir1.mdate.month,
                            additionalInputs[0].response.cnc_rdprogdir3.buf.dir1.mdate.day,
                            additionalInputs[0].response.cnc_rdprogdir3.buf.dir1.mdate.hour,
                            additionalInputs[0].response.cnc_rdprogdir3.buf.dir1.mdate.minute, 0)).ToUnixTimeMilliseconds()
                    },
                    pieces = new {
                        produced = additionalInputs[1].response.cnc_rdparam.param.data.ldata,
                        produced_life = additionalInputs[2].response.cnc_rdparam.param.data.ldata,
                        remaining = additionalInputs[3].response.cnc_rdparam.param.data.ldata
                    },
                    timers = new {
                        cycle_time_ms = (additionalInputs[4].response.cnc_rdparam.param.data.ldata * 60000) +
                                        additionalInputs[5].response.cnc_rdparam.param.data.ldata
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