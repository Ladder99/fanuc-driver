using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class StatInfo : Veneer
    {
        public StatInfo(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                mode = new
                {
                    automatic = -1,
                    manual = -1
                },
                status = new
                {
                    run = -1,
                    edit = -1,
                    motion = -1,
                    mstb = -1,
                    emergency = -1,
                    write = -1,
                    label_skip = -1,
                    alarm = -1,
                    warning = -1,
                    battery = -1
                }
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success)
            {
                var current_value = new
                {
                    mode = new
                    {
                        automatic = input.response.cnc_statinfo.statinfo.aut
                        // manual ?
                    },
                    status = new
                    {
                        input.response.cnc_statinfo.statinfo.run,
                        input.response.cnc_statinfo.statinfo.edit,
                        input.response.cnc_statinfo.statinfo.motion,
                        input.response.cnc_statinfo.statinfo.mstb,
                        input.response.cnc_statinfo.statinfo.emergency,
                        input.response.cnc_statinfo.statinfo.alarm
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