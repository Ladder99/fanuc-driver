namespace fanuc.veneers
{
    public class Connect : Veneer
    {
        public Connect(string name = "") : base(name)
        {

        }

        protected override dynamic First(dynamic input)
        {
            this.dataChanged(input, input.success);
            
            return new { veneer = this };
        }

        protected override dynamic Any(dynamic input)
        {
            var current_value = new {input.success};
            
            if (!current_value.Equals(_lastValue))
            {
                this.dataChanged(input, current_value);
            }
            
            return new { veneer = this };
        }
    }
}