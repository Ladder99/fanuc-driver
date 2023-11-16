using System.Dynamic;
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
            Func<dynamic, ExpandoObject> toExpando = (item) =>
            {
                dynamic eo = new ExpandoObject();
                eo.address = item.bag["address"];
                eo.type = item.bag["type"];
                eo.value = item.bag["value"];
                return eo;
            };

            var currentValue = nativeInputs.ToDictionary(
                item => item.bag["id"],
                item => toExpando(item));
            
                /*item => new dynamic()
                {
                    address = item.bag["address"],
                    type = item.bag["type"],
                    value = item.bag["value"]
                });*/
            
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