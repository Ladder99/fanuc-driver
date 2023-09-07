using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.handlers;

// ReSharper disable once UnusedType.Global
public class FanucOne : Handler
{

    //get the time when the machine starts running
    private DateTime _startTime = DateTime.Now;
    
    public FanucOne(Machine machine) : base(machine)
    {
        if (!machine.Configuration.handler.ContainsKey("iso8601"))
            machine.Configuration.handler.Add("iso8601", false);
        
        if (!machine.Configuration.handler.ContainsKey("change_only"))
            machine.Configuration.handler.Add("change_only", true);

        if (!machine.Configuration.handler.ContainsKey("skip_internal"))
            machine.Configuration.handler.Add("skip_internal", true);
    }

    protected override async Task<dynamic?> OnDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? beforeArrival)
    {
        if (veneer.GetType().Name == "FocasPerf")
        {
            // always allow perf
        }
        else if (Machine.Configuration.handler["change_only"] == true)
        {
            // change only
            return null;
        }
        else
        {
            if (Machine.Configuration.handler["skip_internal"] == true && veneer.IsInternal)
                // all data, but skip internals
                return null;
        }

        dynamic ts = Machine.Configuration.handler["iso8601"] == true ? DateTime.UtcNow.ToString("o") : new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        
        dynamic payload = new
        {
            observation = new
            {
                time = ts,
                machine = veneers.Machine.Id,
                name = veneer.Name,
                marker = veneer.Marker ?? new dynamic[]{}
            },
            state = new
            {
                time = veneer.ArrivalDelta.TotalMilliseconds,
                data = veneer.LastArrivedValue
            }
        };

        return payload;
    }

    protected override async Task AfterDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? onArrival)
    {
        if (onArrival == null)
            return;

        await veneers.Machine.Transport.SendAsync("DATA_ARRIVE", veneer, onArrival);
    }

    protected override async Task<dynamic?> OnDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? beforeChange)
    {
        if (Machine.Configuration.handler["change_only"] == false)
            return null;

        if (Machine.Configuration.handler["skip_internal"] == true && veneer.IsInternal)
            return null;

        dynamic ts = Machine.Configuration.handler["iso8601"] == true ? DateTime.UtcNow.ToString("o") : new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        
        dynamic payload = new
        {
            observation = new
            {
                time = ts,
                machine = veneers.Machine.Id,
                name = veneer.Name,
                marker = veneer.Marker ?? new dynamic[]{}
            },
            state = new
            {
                time = veneer.ChangeDelta.TotalMilliseconds,
                data = veneer.LastChangedValue
            }
        };

        return payload;
    }

    protected override async Task AfterDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? onChange)
    {
        if (onChange == null)
            return;

        await veneers.Machine.Transport.SendAsync("DATA_ARRIVE", veneer, onChange);
    }

    protected override async Task<dynamic?> OnStrategySweepCompleteAsync(Machine machine, dynamic? beforeSweepComplete)
    {
        dynamic ts = Machine.Configuration.handler["iso8601"] == true ? DateTime.UtcNow.ToString("o") : new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        
        dynamic payload = new
        {
            observation = new
            {
                time = ts,
                machine = machine.Id,
                name = "sweep"
            },
            state = new
            {
                data = new
                {
                    online = machine.StrategySuccess,
                    healthy = machine.StrategyHealthy,
                    uptime_ms = (DateTime.Now - _startTime).TotalMilliseconds
                }
            }
        };

        return payload;
    }

    protected override async Task AfterSweepCompleteAsync(Machine machine, dynamic? onSweepComplete)
    {
        await machine.Transport.SendAsync("SWEEP_END", null, onSweepComplete);
    }
}