using l99.driver.fanuc.strategies;
using l99.driver.fanuc.veneers;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors;

// ReSharper disable once UnusedType.Global
public class Messages : FanucMultiStrategyCollector
{
    public Messages(FanucMultiStrategy strategy, object configuration) : base(strategy, configuration)
    {
        if (!Configuration.ContainsKey("warned")) Configuration.Add("warned", false);
    }

    public override async Task InitPathsAsync()
    {
        await Strategy.Apply(typeof(OpMsgs), "messages");
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
            short msgType = 0;
            short msgLength = 6 + 256;

            if (Regex.IsMatch(string.Join("", obsFocasSupport), "15"))
            {
                msgType = -1;
                msgLength = 578;
            }

            await Strategy.SetNativeKeyed("messages",
                await Strategy.Platform.RdOpMsgAsync(msgType, msgLength));

            await Strategy.Peel("messages",
                new dynamic[]
                {
                    Strategy.GetKeyed("messages")!
                },
                new dynamic[]
                {
                });
        }
    }
}