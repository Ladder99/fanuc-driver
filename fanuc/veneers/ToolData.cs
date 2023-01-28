using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers;

public class ToolData : Veneer
{
    public ToolData(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false) : base(veneers,
        name, isCompound, isInternal)
    {
    }

    protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        if (nativeInputs.Any(o => o.success == true))
        {
            var tool = nativeInputs[1].success
                ? nativeInputs[1].response.cnc_toolnum.toolnum.data
                : nativeInputs[0].success
                    ? nativeInputs[0].response.cnc_modal.modal.aux.aux_data
                    : -1;

            var group = nativeInputs[1].success
                ? nativeInputs[1].response.cnc_toolnum.toolnum.datano
                : -1;

            var currentValue = new
            {
                tool,
                group
            };

            await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);

            if (currentValue.IsDifferentString((object) LastChangedValue))
                await OnDataChangedAsync(nativeInputs, additionalInputs, currentValue);
        }
        else
        {
            await OnHandleErrorAsync(nativeInputs, additionalInputs);
        }

        return new {veneer = this};
    }
}