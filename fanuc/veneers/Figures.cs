using System;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.veneers
{
    public class Figures : Veneer
    {
        public Figures(string name = "", bool isInternal = false) : base(name, isInternal)
        {
            _lastChangedValue = new
            {
               
            };
        }
        
        protected override dynamic Any(dynamic input, dynamic? input2)
        {
            if (input.success)
            {
                var current_value = new
                {
                    input.response.cnc_getfigure
                };
                
                this.onDataArrived(input, current_value);
                
                //Console.WriteLine(current_value.GetHashCode() + "  ==  " + _lastChangedValue.GetHashCode());
                
                //if (!current_value.Equals(this._lastChangedValue))
                if(!JObject.FromObject(current_value).ToString().Equals(JObject.FromObject(_lastChangedValue).ToString()))
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