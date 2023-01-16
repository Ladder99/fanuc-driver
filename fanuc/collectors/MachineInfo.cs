using l99.driver.fanuc.strategies;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors
{
    // ReSharper disable once UnusedType.Global
    public class MachineInfo : FanucMultiStrategyCollector
    {
        public MachineInfo(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }

        public override async Task InitPathsAsync()
        {
            await Strategy.Apply(typeof(veneers.SysInfo), "machine");
        }
        
        public override async Task CollectForEachPathAsync(short currentPath, string[] axis, string[] spindle, dynamic pathMarker)
        {
            // TODO: shdr transport, no-filter, requires continuous
            //  evaluation, otherwise item never gets set to UNAVAILABLE
            //if (strategy.HasKeyed($"{this.GetType().Name}"))
            //    return;

            await Strategy.SetKeyed($"{this.GetType().Name}", true);
            
            await Strategy.SetNativeKeyed($"machine",
                await Strategy.Platform.SysInfoAsync());
            
            var obsMachine = await Strategy.Peel($"machine", 
                Strategy.GetKeyed($"machine"));

            Strategy.SetKeyed($"obs+focas_support", 
                obsMachine.veneer.LastArrivedValue.focas_support);
        }
    }
}