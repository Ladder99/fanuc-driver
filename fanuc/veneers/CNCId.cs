using System.Linq;

namespace fanuc.veneers
{
    public class CNCId : Veneer
    {
        public CNCId(string name = "") : base(name)
        {
            _lastChangedValue = new
            {
                cncid = string.Empty
            };
        }
        
        protected override dynamic Any(dynamic input)
        {
            if (input.success)
            {;
                var current_value = new
                {
                    cncid = string.Join("-", ((uint[])input.response.cnc_rdcncid.cncid).Select(x => x.ToString("X")).ToArray())
                };
                
                this.onDataArrived(input, current_value);
                
                if (!current_value.Equals(this._lastChangedValue))
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