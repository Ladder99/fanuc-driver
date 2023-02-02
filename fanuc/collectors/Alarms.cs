using l99.driver.fanuc.strategies;
using l99.driver.fanuc.veneers;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors;

// ReSharper disable once UnusedType.Global
public class Alarms : FanucMultiStrategyCollector
{
    public Alarms(FanucMultiStrategy strategy, object configuration) : base(strategy, configuration)
    {
        if (!Configuration.ContainsKey("warned")) Configuration.Add("warned", false);
    }

    public override async Task InitPathsAsync()
    {
        await Strategy.Apply(typeof(AlarmsSeries), "alarms");
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
                    $"[{Strategy.Machine.Id}] Machine info observation is required to correctly evaluate alarms.");
                Configuration["warned"] = true;
            }

            await Strategy.SetNativeKeyed("alarms",
                await Strategy.Platform.RdAlmMsg2Async(-1, 10));
        }
        else
        {
            if (Regex.IsMatch(string.Join("", obsFocasSupport),
                    "30i[A-Z]?|31i[A-Z]?|32i[A-Z]?|0i[D|F]|PMi[A]?"))
                await Strategy.SetNativeKeyed("alarms",
                    await Strategy.Platform.RdAlmMsg2Async(-1, 10));
            else
                await Strategy.SetNativeKeyed("alarms",
                    await Strategy.Platform.RdAlmMsgAsync(-1, 10));
        }

        var obsAlarms = await Strategy.Peel("alarms",
            new dynamic[]
            {
                Strategy.GetKeyed("alarms")!,
                Strategy.GetKeyed("alarms+last")!
            },
            new dynamic[]
            {
                currentPath,
                axis,
                obsFocasSupport!
            });

        // save native alarm data structure for comparison on next iteration
        Strategy.SetKeyed("alarms+last",
            Strategy.GetKeyed("alarms"));

        // track the resulting data structure
        Strategy.SetKeyed("obs+alarms",
            obsAlarms!.veneer.LastChangedValue.alarms);
    }
}