using Newtonsoft.Json.Linq;

namespace fanuc.veneers
{
    public class RdDynamic2 : Veneer
    {
        public RdDynamic2(string name = "", bool isInternal = false) : base(name, isInternal)
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