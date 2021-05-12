using System;
using Newtonsoft.Json.Linq;

namespace fanuc.veneers
{
    public class RdDynamic2_1 : Veneer
    {
        public RdDynamic2_1(string name = "", bool isInternal = false) : base(name, isInternal)
        {
            _lastChangedValue = new
            {
                actual_feedrate = -1,
                actual_spindle_speed = -1,
                alarm = -1,
                main_program = -1, 
                current_program = -1, 
                sequence_number = -1
            };
        }
        
        protected override dynamic Any(dynamic input)
        {
            if (input.axis_data.success && input.figures.success)
            {
                dynamic ad = input.axis_data.response.cnc_rddynamic2.rddynamic;
                dynamic fig_in = input.figures.response.cnc_getfigure.dec_fig_in;
                
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
                        absolute = ad.pos.absolute / Math.Pow(10.0, fig_in[input.axis_index]),
                        machine = ad.pos.machine / Math.Pow(10.0, fig_in[input.axis_index]),
                        relative = ad.pos.relative / Math.Pow(10.0, fig_in[input.axis_index]),
                        distance = ad.pos.distance / Math.Pow(10.0, fig_in[input.axis_index])
                    }
                };

                input = input.axis_data;
                
                this.onDataArrived(input, current_value);
                
                // TODO: equality or hash code do not match on this object (x86)
                //if (!current_value.Equals(_lastValue))
                // TODO: can't do this because pos does not expand
                //if(!current_value.ToString().Equals(_lastChangedValue.ToString())) 
                if(!JObject.FromObject(current_value).ToString().Equals(JObject.FromObject(_lastChangedValue).ToString()))
                {
                    this.onDataChanged(input, current_value);
                }
            }
            else
            {
                onError(input);
            }

            return new { veneer = this };
        }
    }
}