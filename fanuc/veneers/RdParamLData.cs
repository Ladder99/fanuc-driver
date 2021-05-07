namespace fanuc.veneers
{
    public class RdParamLData : Veneer
    {
        public RdParamLData(string name = "") : base(name)
        {
            _lastValue = -1;
        }
        
        protected override dynamic Any(dynamic input)
        {
            if (input.success)
            {
                var current_value = new {ldata = input.response.cnc_rdparam.param.ldata};
                
                if (!current_value.Equals(_lastValue))
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