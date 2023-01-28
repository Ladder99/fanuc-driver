using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers;

public class OpMsgs : Veneer
{
    public OpMsgs(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false) : base(veneers,
        name, isCompound, isInternal)
    {
    }

    protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        if (nativeInputs.All(o => o.success == true))
        {
            var tempValue = new List<dynamic>();

            var fields = nativeInputs[0].response.cnc_rdopmsg.opmsg.GetType().GetFields();
            for (var x = 0; x <= fields.Length - 1; x++)
            {
                var msg = fields[x].GetValue(nativeInputs[0].response.cnc_rdopmsg.opmsg);
                if (msg.char_num > 0)
                    tempValue.Add(new
                    {
                        position = msg.type,
                        number = msg.datano,
                        message = msg.data
                    });
            }

            var currentValue = new
            {
                messages = tempValue
            };

            //var currentHc = currentValue.messages.Select(x => x.GetHashCode());
            //var lastHc = ((List<dynamic>)LastChangedValue.messages).Select(x => x.GetHashCode());

            await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);

            //if(currentHc.Except(lastHc).Count() + lastHc.Except(currentHc).Count() > 0)
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