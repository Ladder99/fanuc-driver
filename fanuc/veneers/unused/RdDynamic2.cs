using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.veneers
{
    public class RdDynamic2 : Veneer
    {
        public RdDynamic2(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            LastChangedValue = new
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
        
        protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
        {
            if (input.success)
            {
                dynamic d = input.response.cnc_rddynamic2.rddynamic;
                
                var current_value = new
                {
                    d.actf,
                    d.acts,
                    d.alarm,
                    d.prgmnum,
                    d.prgnum,
                    d.seqnum,
                    d.pos
                };
                
                await OnDataArrivedAsync(input, current_value);
                
                // TODO: equality or hash code do not match on this object (x86)
                //if (!current_value.Equals(_lastValue))
                // TODO: can't do this because pos does not expand
                //if(!current_value.ToString().Equals(LastChangedValue.ToString())) 
                //if(!JObject.FromObject(current_value).ToString().Equals(JObject.FromObject(LastChangedValue).ToString()))
                if(current_value.IsDifferentString((object)LastChangedValue))
                {
                    await OnDataChangedAsync(input, current_value);
                }
            }
            else
            {
                await OnHandleErrorAsync(input);
            }

            return new { veneer = this };
        }
    }
}