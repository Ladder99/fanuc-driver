namespace fanuc.veneers
{
    public class RdParamLData : Veneer
    {
        public RdParamLData(string name = "") : base(name)
        {
            _lastChangedValue = new
            {
                ldata = -1
            };
        }
        
        protected override dynamic Any(dynamic input)
        {
            if (input.success)
            {
                var current_value = new { ldata = input.response.cnc_rdparam.param.ldata };
                
                this.onDataArrived(input, current_value);
                
                if (!current_value.Equals(_lastChangedValue))
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