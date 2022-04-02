using System;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.handlers
{
    public class FanucOne: Handler
    {
        private dynamic _cfg;
        
        public FanucOne(Machine machine, object cfg) : base(machine, cfg)
        {
            _cfg = cfg;
        }
        
        public override async Task<dynamic?> OnDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? beforeArrival)
        {
            if (veneer.GetType().Name == "FocasPerf")
            {
                // always allow perf
            }
            else if (_cfg.handler["change_only"] == true)
            {
                // change only
                return null;
            }
            else
            {
                if (_cfg.handler["skip_internal"] == true && veneer.IsInternal == true)
                {
                    // all data, but skip internals
                    return null;
                }
            }
            
            dynamic payload = new
            {
                observation = new
                {
                    time =  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                    machine = veneers.Machine.Id,
                    name = veneer.Name,
                    marker = veneer.Marker
                },
                state = new
                {
                    time = veneer.ArrivalDelta.TotalMilliseconds,
                    data = veneer.LastArrivedValue
                }
            };

            return payload;
        }

        protected override async Task afterDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? onArrival)
        {
            if (onArrival == null)
                return;
            
            await veneers.Machine.Transport.SendAsync("DATA_ARRIVE", veneer, onArrival);
        }

        public override async Task<dynamic?> OnDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? beforeChange)
        {
            if (_cfg.handler["change_only"] == false)
                return null;
            
            if (_cfg.handler["skip_internal"] == true && veneer.IsInternal == true)
                return null;
            
            dynamic payload = new
            {
                observation = new
                {
                    time =  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                    machine = veneers.Machine.Id,
                    name = veneer.Name,
                    marker = veneer.Marker
                },
                state = new
                {
                    time = veneer.ChangeDelta.TotalMilliseconds,
                    data = veneer.LastChangedValue
                }
            };

            return payload;
        }

        protected override async Task afterDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? onChange)
        {
            if (onChange == null)
                return;
            
            await veneers.Machine.Transport.SendAsync("DATA_ARRIVE", veneer, onChange);
        }
        
        public override async Task<dynamic?> OnStrategySweepCompleteAsync(Machine machine, dynamic? beforeSweepComplete)
        {
            dynamic payload = new
            {
                observation = new
                {
                    time =  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                    machine = machine.Id,
                    name = "sweep"
                },
                state = new
                {
                    data = new
                    {
                        online = machine.StrategySuccess,
                        healthy = machine.StrategyHealthy
                    }
                }
            };
            
            return payload;
        }
        
        protected override async Task afterSweepCompleteAsync(Machine machine, dynamic? onSweepComplete)
        {
            await machine.Transport.SendAsync("SWEEP_END", null, onSweepComplete);
        }
    }
}