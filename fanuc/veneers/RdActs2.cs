using System.Collections.Generic;
using System.Linq;

namespace fanuc.veneers
{
    public class RdActs2: Veneer
    {
        public RdActs2(string name = ""): base(name)
        {
            _lastValue = new 
            {
                data = -1
            };
        }
        
        protected override dynamic Any(dynamic input)
        {
            if (input.success)
            {
                var current_value = new { data = input.response.cnc_acts2.actualspindle.data[0] };
                
                var fields = input.response.cnc_acts2.actualspindle.GetType().GetFields();
                
                if(!current_value.Equals(_lastValue))
                {
                    this.dataChanged(input, current_value);
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