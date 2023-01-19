using l99.driver.fanuc.strategies;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors
{
    // ReSharper disable once UnusedType.Global
    public class ToolData : FanucMultiStrategyCollector
    {
        public ToolData(FanucMultiStrategy strategy, object configuration) : base(strategy, configuration)
        {
            
        }

        public override async Task InitPathsAsync()
        {
            await Strategy.Apply(typeof(veneers.ToolData), "tool");
        }
        
        public override async Task CollectForEachPathAsync(short currentPath, string[] axis, string[] spindle, dynamic pathMarker)
        {
            await Strategy.Peel("tool",
                new dynamic[]
                {
                    await Strategy.SetNativeKeyed($"modal_t", 
                        await Strategy.Platform.ModalAsync(108,0,3)),
                    await Strategy.SetNativeKeyed($"toolnum", 
                        await Strategy.Platform.ToolNumAsync())
                },
                new dynamic[]
                {
                    
                });
        }
    }
}