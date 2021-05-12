using System.Collections.Generic;
using System.Linq;

namespace fanuc.veneers
{
    public class RdActs2: Veneer
    {
        public RdActs2(string name = "", bool isInternal = false) : base(name, isInternal)
        {
            _lastChangedValue = new 
            {
                data = -1
            };
        }
        
        protected override dynamic Any(dynamic input, dynamic? input2)
        {
            if (input.success)
            {
                var current_value = new { data = input.response.cnc_acts2.actualspindle.data[0] };
                
                var fields = input.response.cnc_acts2.actualspindle.GetType().GetFields();
                
                this.onDataArrived(input, current_value);
                
                if(!current_value.Equals(_lastChangedValue))
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