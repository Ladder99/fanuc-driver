using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers
{
    public class Figures : Veneer
    {
        public Figures(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
               
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success)
            {
                var currentValue = new
                {
                    input.response.cnc_getfigure
                };
                
                await OnDataArrivedAsync(input, currentValue);
                
                //Console.WriteLine(current_value.GetHashCode() + "  ==  " + lastChangedValue.GetHashCode());
                
                //if (!current_value.Equals(this.lastChangedValue))
                //if(!JObject.FromObject(current_value).ToString().Equals(JObject.FromObject(lastChangedValue).ToString()))
                if(currentValue.IsDifferentString((object)lastChangedValue))
                {
                    await OnDataChangedAsync(input, currentValue);
                }
            }
            else
            {
                await OnHandleErrorAsync(input);
            }
            
            return new { veneer = this };
        }
    }
}