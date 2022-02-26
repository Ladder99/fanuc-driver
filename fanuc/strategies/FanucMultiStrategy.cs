using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        private dynamic _config;
        private List<FanucMultiStrategyCollector> _collectors = new List<FanucMultiStrategyCollector>();
        
        public Platform Platform
        {
            get => platform;
        }
        
        public override async Task<dynamic?> CreateAsync()
        {
            foreach (var collector_type in _config.strategy)
            {
                logger.Info($"[{machine.Id}] Creating collector: {collector_type}");
                var type = Type.GetType(collector_type);
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

        public override async Task InitAxisAndSpindleAsync()
        {
            foreach (var collector in _collectors)
            {
                await collector.InitAxisAndSpindleAsync();
            }
        }

        public override async Task<dynamic?> CollectAsync()
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

        public override async Task CollectForEachPathAsync(short current_path, string[] axis, string[] spindle, dynamic path_marker)
        {
            foreach (var collector in _collectors)
            {
                await collector.CollectForEachPathAsync(current_path, axis, spindle, path_marker);
            }
        }

        public override async Task CollectForEachAxisAsync(short current_path, short current_axis, string axis_name, dynamic axis_split, dynamic axis_marker)
        {
            foreach (var collector in _collectors)
            {
                await collector.CollectForEachAxisAsync(current_path, current_axis, axis_name, axis_split, axis_marker);
            }
        }

        public override async Task CollectForEachSpindleAsync(short current_path, short current_spindle, string spindle_name, dynamic spindle_split, dynamic spindle_marker)
        {
            foreach (var collector in _collectors)
            {
                await collector.CollectForEachSpindleAsync(current_path, current_spindle, spindle_name, spindle_split, spindle_marker);
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