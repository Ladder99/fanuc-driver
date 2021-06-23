using System.Threading.Tasks;
using l99.driver.@base;

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
                var current_value = new
                {
                    input.response.cnc_getfigure
                };
                
                await onDataArrivedAsync(input, current_value);
                
                //Console.WriteLine(current_value.GetHashCode() + "  ==  " + lastChangedValue.GetHashCode());
                
                //if (!current_value.Equals(this.lastChangedValue))
                //if(!JObject.FromObject(current_value).ToString().Equals(JObject.FromObject(lastChangedValue).ToString()))
                if(current_value.IsDifferentString((object)lastChangedValue))
                {
                    await onDataChangedAsync(input, current_value);
                }
            }
            else
            {
                await onErrorAsync(input);
            }
            
            return new { veneer = this };
        }
    }
}