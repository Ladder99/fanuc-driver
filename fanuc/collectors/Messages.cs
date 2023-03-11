using l99.driver.fanuc.strategies;
using l99.driver.fanuc.veneers;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors;

// ReSharper disable once UnusedType.Global
public class Messages : FanucMultiStrategyCollector
{
    public Messages(FanucMultiStrategy strategy, object configuration) : base(strategy, configuration)
    {
        if (!Configuration.ContainsKey("stateful")) Configuration.Add("stateful", false);
        
        if (!Configuration.ContainsKey("warned")) Configuration.Add("warned", false);
    }

    public override async Task InitPathsAsync()
    {
        await Strategy.Apply(Configuration["stateful"] ? typeof(OpMsgsStateful) : typeof(OpMsgs), "messages");
    }

    public override async Task CollectForEachPathAsync(short currentPath, string[] axis, string[] spindle,
        dynamic pathMarker)
    {
        var obsFocasSupport = Strategy.GetKeyed("obs+focas_support");

        if (obsFocasSupport == null)
        {
            if (!Configuration["warned"])
            {
                Logger.Warn(
                    $"[{Strategy.Machine.Id}] Machine info observation is required to correctly evaluate operator messages.");
                Configuration["warned"] = true;
            }
        }
        else
        {
            //TODO: read all messages based on NC model
            
            short msgType = 0;
            short msgLength = 6 + 256;

            if (Regex.IsMatch(string.Join("", obsFocasSupport), "15"))
            {
                msgType = -1;
                msgLength = 578;
            }

            await Strategy.SetNativeKeyed("messages",
                await Strategy.Platform.RdOpMsgAsync(msgType, msgLength));

            var obsMessages = await Strategy.Peel("messages",
                new dynamic[]
                {
                    Strategy.GetKeyed("messages")!,
                    Strategy.GetKeyed("messages+last")!
                },
                new dynamic[]
                {
                    currentPath
                });
            
            // save native message data structure for comparison on next iteration
            Strategy.SetKeyed("messages+last",
                Strategy.GetKeyed("messages"));
            
            // track the resulting data structure
            Strategy.SetKeyed("obs+messages",
                obsMessages!.veneer.LastChangedValue?.messages);
        }
    }
}