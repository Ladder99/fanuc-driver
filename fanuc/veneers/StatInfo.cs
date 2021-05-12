namespace fanuc.veneers
{
    public class StatInfo : Veneer
    {
        public StatInfo(string name = "", bool isInternal = false) : base(name, isInternal)
        {
            _lastChangedValue = new
            {
                aut = -1,
                run = -1,
                edit = -1,
                motion = -1,
                mstb = -1,
                emergency = -1,
                alarm = -1
            };
        }
        
        protected override dynamic Any(dynamic input, dynamic? input2)
        {
            if (input.success)
            {
                var current_value = new
                {
                    input.response.cnc_statinfo.statinfo.aut,
                    input.response.cnc_statinfo.statinfo.run,
                    input.response.cnc_statinfo.statinfo.edit,
                    input.response.cnc_statinfo.statinfo.motion,
                    input.response.cnc_statinfo.statinfo.mstb,
                    input.response.cnc_statinfo.statinfo.emergency,
                    input.response.cnc_statinfo.statinfo.alarm
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