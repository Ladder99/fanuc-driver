using System;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.veneers
{
    public class RdDynamic2_1 : Veneer
    {
        public RdDynamic2_1(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                actual_feedrate = -1,
                actual_spindle_speed = -1,
                alarm = -1,
                main_program = -1, 
                current_program = -1, 
                sequence_number = -1,
                pos = new
                {
                    absolute = 0,
                    machine = 0,
                    relative = 0,
                    distance = 0
                }
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success && additionalInputs[0].success)
            {
                dynamic ad = input.response.cnc_rddynamic2.rddynamic;
                dynamic fig_in = additionalInputs[0].response.cnc_getfigure.dec_fig_in;
                
                var current_value = new
                {
                    ad.actf,
                    ad.acts,
                    ad.alarm,
                    ad.prgmnum,
                    ad.prgnum,
                    ad.seqnum,
                    pos = new
                    {
                        // TODO: should index be current axis number?
                        absolute = ad.pos.absolute / Math.Pow(10.0, fig_in[additionalInputs[1]]),
                        machine = ad.pos.machine / Math.Pow(10.0, fig_in[additionalInputs[1]]),
                        relative = ad.pos.relative / Math.Pow(10.0, fig_in[additionalInputs[1]]),
                        distance = ad.pos.distance / Math.Pow(10.0, fig_in[additionalInputs[1]])
                    }
                };

                await onDataArrivedAsync(input, current_value);
                
                // TODO: equality or hash code do not match on this object (x86)
                //if (!current_value.Equals(_lastValue))
                // TODO: can't do this because pos does not expand
                //if(!current_value.ToString().Equals(lastChangedValue.ToString())) 
                //if(!JObject.FromObject(current_value).ToString().Equals(JObject.FromObject(lastChangedValue).ToString()))
                if(current_value.IsDifferentString((object)lastChangedValue))
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