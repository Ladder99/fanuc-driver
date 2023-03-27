using System.Dynamic;
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
        /*
            nativeInputs
                0: current messages
                1: previous messages
            
            additionalInputs
                0: currentPath
        */
        if (nativeInputs[0].success == true)
        {
            var path = additionalInputs[0];
            var currentInput = nativeInputs[0];
            //var previousInput = nativeInputs[1];

            // check success to use
            //List<dynamic> previousMessageList = GetMessageListFromMessages(previousInput, path);
            List<dynamic> currentMessageList = GetMessageListFromMessages(currentInput, path);

            dynamic currentValue = new ExpandoObject();
            currentValue.messages = currentMessageList;
            
            /*
            var currentValue = new
            {
                messages = currentMessageList
            };
            */

            await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);

            if (((object)currentValue).IsDifferentString((object) LastChangedValue))
                await OnDataChangedAsync(nativeInputs, additionalInputs, currentValue);
        }
        else
        {
            await OnHandleErrorAsync(nativeInputs, additionalInputs);
        }

        return new {veneer = this};
    }
    
    protected List<dynamic> GetMessageListFromMessages(dynamic nativeInput, short path)
    {
        var list = new List<dynamic>();

        if (nativeInput == null) return list;
        
        var fields = nativeInput.response.cnc_rdopmsg.opmsg.GetType().GetFields();
        for (var x = 0; x <= fields.Length - 1; x++)
        {
            var msg = fields[x].GetValue(nativeInput.response.cnc_rdopmsg.opmsg);
            if(msg.datano > -1 && msg.char_num > 0)
                list.Add(new
                {
                    path,
                    msg.type,
                    number = msg.datano,
                    message = ((string) msg.data).AsAscii()
                });
        }

        return list;
    }
}