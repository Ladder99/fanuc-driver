namespace fanuc.veneers
{
    public class Connect : Veneer
    {
        public Connect(string name = "") : base(name)
        {

        }

        protected override dynamic First(dynamic input)
        {
            var current_value = new {input.success};
            
            this.onDataArrived(input, current_value);
            this.onDataChanged(input, current_value);

            return new { veneer = this };
        }

        protected override dynamic Any(dynamic input)
        {
            var current_value = new {input.success };
            
            this.onDataArrived(input, current_value);
            
            if (!current_value.Equals(_lastChangedValue))
            {
                this.onDataChanged(input, current_value);
            }
            
            return new { veneer = this };
        }
    }
}