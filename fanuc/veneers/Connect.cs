using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers
{
    public class Connect : Veneer
    {
        public Connect(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {

        }

        protected override async Task<dynamic> FirstAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            var currentValue = new {input.success};
            
            await OnDataArrivedAsync(input, currentValue);
            await OnDataChangedAsync(input, currentValue);

            return new { veneer = this };
        }

        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            var currentValue = new {input.success };
            
            await OnDataArrivedAsync(input, currentValue);
            
            if (!currentValue.Equals(lastChangedValue))
            {
                await OnDataChangedAsync(input, currentValue);
            }
            
            return new { veneer = this };
        }
    }
}