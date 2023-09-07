using l99.driver.fanuc.strategies;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors;

// ReSharper disable once UnusedType.Global
public class Macro : FanucMultiStrategyCollector
{
    public Macro(FanucMultiStrategy strategy, object configuration) : base(strategy, configuration)
    {
        if (!Configuration.ContainsKey("map")) 
            Configuration.Add("map", new Dictionary<object, object>());
    }

    public override async Task InitPathsAsync()
    {
        await Strategy.Apply(typeof(veneers.Macro), "macro");
    }

    public override async Task CollectForEachPathAsync(short currentPath, string[] axis, string[] spindle, dynamic pathMarker)
    {
        var cacheDict = new Dictionary<string, dynamic>();
        
        foreach (var macroEntry in Configuration["map"])
        {
            if (!new[] { "path", "id", "address" }.All(key => macroEntry.ContainsKey(key))) continue;
            
            //checks if the value is a string or a list and stores in pathlist
            var pathList = macroEntry["path"] switch
            {
                List<string> list => list,
                List<object> objectList => objectList.Select(obj => obj.ToString()).ToList(),
                string singleValue => new List<string> { singleValue },
                _ => new List<string>()
            };

            if (!pathList.Intersect(new[] { currentPath.ToString(), "%" }).Any()) continue;
            
            var nr = await Strategy.Platform.RdMacroAsync((short)macroEntry["address"], 10);
            nr.bag["id"] = macroEntry["id"];
            
            await Strategy.SetNativeKeyed($"macro_{macroEntry["id"]}", nr);
        }
        
        await Strategy.Peel("macro",
            Strategy.GetKeyedStartsWith("macro_").ToArray(),
            new dynamic[]
            {
                
            });
    }
}