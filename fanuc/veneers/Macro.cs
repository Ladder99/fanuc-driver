using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers;

public class Macro : Veneer
{
    public Macro(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false) : base(veneers, name, isCompound, isInternal)
    {

    }
    
    protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        if (nativeInputs.All(o => o.success == true))
        {
            var currentValue = nativeInputs.ToDictionary(
                item => item.bag["id"],
                item => new
                {
                    address = item.request.cnc_rdmacro.number,
                    value = item.response.cnc_rdmacro.macro.mcr_val /
                            (double)Math.Pow(10, item.response.cnc_rdmacro.macro.dec_val)
                });
            
            await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);

            if (((object)currentValue).IsDifferentString((object) LastChangedValue))
                await OnDataChangedAsync(nativeInputs, additionalInputs, currentValue);
        }
        else
        {
            await OnHandleErrorAsync(nativeInputs, additionalInputs);
        }
        
        return new { veneer = this };
    }
}