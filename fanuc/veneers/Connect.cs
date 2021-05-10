namespace fanuc.veneers
{
    public class Connect : Veneer
    {
        public Connect(string name = "") : base(name)
        {

        }

        protected override dynamic First(dynamic input)
        {
            this.dataChanged(input, new {input.success, input.invocationMs});

            return new { veneer = this };
        }

        protected override dynamic Any(dynamic input)
        {
            var current_value = new {input.success, input.invocationMs};
            
            if (!current_value.Equals(_lastValue))
            {
                this.dataChanged(input, current_value);
            }
            
            return new { veneer = this };
        }
    }
}