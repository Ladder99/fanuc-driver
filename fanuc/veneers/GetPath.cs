namespace fanuc.veneers
{
    public class GetPath : Veneer
    {
        public GetPath(string name = "") : base(name)
        {
            _lastChangedValue = new
            {
                path_no = -1,
                maxpath_no = -1
            };
        }
        
        protected override dynamic Any(dynamic input)
        {
            if (input.success)
            {
                var current_value = new
                {
                    input.response.cnc_getpath.path_no,
                    input.response.cnc_getpath.maxpath_no
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