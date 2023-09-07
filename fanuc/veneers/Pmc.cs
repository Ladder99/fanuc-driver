using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers;

public class Pmc : Veneer
{
    public Pmc(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false) : base(
        veneers, name, isCompound, isInternal)
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
                    address = item.bag["address"],
                    type = item.bag["type"],
                    value = item.bag["value"]
                });
            
            await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);

            if (((object)currentValue).IsDifferentString((object) LastChangedValue))
                await OnDataChangedAsync(nativeInputs, additionalInputs, currentValue);
        }
        else
        {
            await OnHandleErrorAsync(nativeInputs, additionalInputs);
        }
        
        bool success = true;
        
        foreach(var pmclist in nativeInputs[0])
        {
            if (!pmclist.Value.data.success)
            {
                success = false;
                break;
            }
        }
        
        if (success)
        {
            dynamic currentValue = nativeInputs[1];

            await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);

            if (((object)currentValue).IsDifferentString((object)LastChangedValue))
                await OnDataChangedAsync(nativeInputs, additionalInputs, currentValue);
        }
        else
        {
            await OnHandleErrorAsync(nativeInputs, additionalInputs);
        }
        
        return new { veneer = this };
    }
}