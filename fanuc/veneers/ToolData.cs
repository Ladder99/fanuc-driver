#pragma warning disable CS8602

using l99.driver.@base;

// ReSharper disable once CheckNamespace
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
                var toolNum = additionalInputs[0];
                
                var tool = toolNum.success 
                    ? toolNum.response.cnc_toolnum.toolnum.data :
                        input.success ? input.response.cnc_modal.modal.aux.aux_data : -1;
                
                var group = toolNum.success 
                    ? toolNum.response.cnc_toolnum.toolnum.datano : -1;
                
                var currentValue = new
                {
                    tool,
                    group
                };
                
                await OnDataArrivedAsync(input, currentValue);
                
                if (!currentValue.Equals(this.lastChangedValue))
                {
                    await OnDataChangedAsync(input, currentValue);
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
#pragma warning restore CS8602