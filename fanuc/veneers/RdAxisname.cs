using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers;

public class RdAxisname : Veneer
{
    public RdAxisname(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false) : base(
        veneers, name, isCompound, isInternal)
    {
    }

    protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        if (nativeInputs.All(o => o.success == true))
        {
            var tempValue = new List<dynamic>();

            var fields = nativeInputs[0].response.cnc_rdaxisname.axisname.GetType().GetFields();
            for (var x = 0; x <= nativeInputs[0].response.cnc_rdaxisname.data_num - 1; x++)
            {
                var axis = fields[x].GetValue(nativeInputs[0].response.cnc_rdaxisname.axisname);
                tempValue.Add(new
                {
                    name = ((char) axis.name).AsAscii(),
                    suff = ((char) axis.suff).AsAscii()
                });
            }

            var currentValue = new
            {
                axes = tempValue
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