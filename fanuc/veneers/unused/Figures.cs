using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers
{
    public class Figures : Veneer
    {
        public Figures(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            LastChangedValue = new
            {
               
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
        {
            if (input.success)
            {
                var currentValue = new
                {
                    input.response.cnc_getfigure
                };
                
                await OnDataArrivedAsync(input, currentValue);
                
                //Console.WriteLine(current_value.GetHashCode() + "  ==  " + LastChangedValue.GetHashCode());
                
                //if (!current_value.Equals(this.LastChangedValue))
                //if(!JObject.FromObject(current_value).ToString().Equals(JObject.FromObject(LastChangedValue).ToString()))
                if(currentValue.IsDifferentString((object)LastChangedValue))
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