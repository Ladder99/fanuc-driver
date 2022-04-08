using l99.driver.fanuc.strategies;

namespace l99.driver.fanuc.collectors
{
    public class FanucMultiStrategyCollector
    {
        protected ILogger logger;
        protected FanucMultiStrategy strategy;
        
        public FanucMultiStrategyCollector(FanucMultiStrategy strategy)
        {
            logger = LogManager.GetLogger(this.GetType().FullName);
            this.strategy = strategy;
        }

        public virtual async Task InitRootAsync()
        {
            
        }

        public virtual async Task InitPathsAsync()
        {
            
        }

        public virtual async Task InitAxisAsync()
        {
            
        }
        
        public virtual async Task InitSpindleAsync()
        {
            
        }

        public virtual async Task PostInitAsync(Dictionary<string, List<string>> structure)
        {
            
        }
        
        public virtual async Task CollectRootAsync()
        {
            
        }

        public virtual async Task CollectForEachPathAsync(short current_path, string[] axis, string[] spindle, dynamic path_marker)
        {
            
        }

        public virtual async Task CollectForEachAxisAsync(short current_path, short current_axis, string axis_name, dynamic axis_split, dynamic axis_marker)
        {
            
        }

        public virtual async Task CollectForEachSpindleAsync(short current_path, short current_spindle, string spindle_name, dynamic spindle_split, dynamic spindle_marker)
        {
            
        }
    }
}