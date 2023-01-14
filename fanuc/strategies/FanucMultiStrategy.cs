using l99.driver.@base;
using l99.driver.fanuc.collectors;

namespace l99.driver.fanuc.strategies
{
    public class FanucMultiStrategy : FanucExtendedStrategy
    {
        public FanucMultiStrategy(Machine machine, object cfg) : base(machine, cfg)
        {
            _config = cfg;
        }

        private readonly dynamic _config;
        private readonly List<FanucMultiStrategyCollector> _collectors = new();
        
        public Platform Platform => platform;
        
        public override async Task<dynamic?> CreateAsync()
        {
            foreach (var collectorType in _config.strategy)
            {
                Logger.Info($"[{machine.Id}] Creating collector: {collectorType}");
                var type = Type.GetType(collectorType);
                var collector = (FanucMultiStrategyCollector) Activator.CreateInstance(type, new object[] { this });
                _collectors.Add(collector);
            }
            
            return null;
        }

        public override async Task InitRootAsync()
        {
            foreach (var collector in _collectors)
            {
                await collector.InitRootAsync();
            }
        }

        public override async Task InitPathsAsync()
        {
            foreach (var collector in _collectors)
            {
                await collector.InitPathsAsync();
            }
        }

        public override async Task InitAxisAsync()
        {
            foreach (var collector in _collectors)
            {
                await collector.InitAxisAsync();
            }
        }
        
        public override async Task InitSpindleAsync()
        {
            foreach (var collector in _collectors)
            {
                await collector.InitSpindleAsync();
            }
        }

        protected override async Task<dynamic?> CollectAsync()
        {
            // user code before starting sweep
            
            // must call base to continue sweep
            return await base.CollectAsync();
        }

        public override async Task<bool> CollectBeginAsync()
        {
            // user code before connect
            
            // must call base to connect to machine and return result
            return await base.CollectBeginAsync();
        }

        public override async Task CollectRootAsync()
        {
            foreach (var collector in _collectors)
            {
                await collector.CollectRootAsync();
            }
        }

        public override async Task CollectForEachPathAsync(short currentPath, string[] axis, string[] spindle, dynamic pathMarker)
        {
            foreach (var collector in _collectors)
            {
                await collector.CollectForEachPathAsync(currentPath, axis, spindle, pathMarker);
            }
        }

        public override async Task CollectForEachAxisAsync(short currentPath, short currentAxis, string axis_name, dynamic axisSplit, dynamic axis_marker)
        {
            foreach (var collector in _collectors)
            {
                await collector.CollectForEachAxisAsync(currentPath, currentAxis, axis_name, axisSplit, axis_marker);
            }
        }

        public override async Task CollectForEachSpindleAsync(short currentPath, short currentSpindle, string spindle_name, dynamic spindleSplit, dynamic spindle_marker)
        {
            foreach (var collector in _collectors)
            {
                await collector.CollectForEachSpindleAsync(currentPath, currentSpindle, spindle_name, spindleSplit, spindle_marker);
            }
        }

        public override async Task CollectEndAsync()
        {
            // user code before disconnect
            
            // must call base to disconnect machine
            await base.CollectEndAsync();
            
            // user code after disconnect
        }
    }
}