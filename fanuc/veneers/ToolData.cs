using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class ToolData : Veneer
    {
        public ToolData(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                tool = -1,
                group = -1
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success || additionalInputs[0].success)
            {
                var toolnum = additionalInputs[0];
                
                var tool = toolnum.success 
                    ? toolnum.response.cnc_toolnum.toolnum.data :
                        input.success ? input.response.cnc_modal.modal.aux.aux_data : -1;
                
                var group = toolnum.success 
                    ? toolnum.response.cnc_toolnum.toolnum.datano : -1;
                
                var current_value = new
                {
                    tool,
                    group
                };
                
                await onDataArrivedAsync(input, current_value);
                
                if (!current_value.Equals(this.lastChangedValue))
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