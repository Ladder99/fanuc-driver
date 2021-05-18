using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class Connect : Veneer
    {
        public Connect(string name = "", bool isInternal = false) : base(name, isInternal)
        {

        }

        protected override dynamic First(dynamic input, dynamic? input2)
        {
            var current_value = new {input.success};
            
            this.onDataArrived(input, current_value);
            this.onDataChanged(input, current_value);

            return new { veneer = this };
        }

        protected override dynamic Any(dynamic input, dynamic? input2)
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