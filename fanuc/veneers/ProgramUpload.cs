using System.Dynamic;
using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers;

public class ProgramUpload : Veneer
{
    public ProgramUpload(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false) 
        : base(veneers, name, isCompound, isInternal)
    {
    }

    protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        dynamic currentValue = new ExpandoObject();
        currentValue.programNumber = additionalInputs[0];
        currentValue.programCode = additionalInputs[1];
            
        await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);

        if (Extensions.IsDifferentExpando(currentValue, LastChangedValue))
            await OnDataChangedAsync(nativeInputs, additionalInputs, currentValue);

        return new {veneer = this};
    }
}