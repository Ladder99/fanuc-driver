using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.strategies;

public class FanucStrategy : Strategy
{
    protected FanucStrategy(Machine machine, object configuration) : base(machine, configuration)
    {
        Platform = Machine["platform"]!;
        Platform.StartupProcess(3, "focas2.log");
    }

    public dynamic Platform { get; }

    ~FanucStrategy()
    {
        // TODO: verify invocation on linux
        Platform.ExitProcess();
    }

    protected dynamic PathMarker(short number)
    {
        return new
        {
            type = "path",
            number
        };
    }

    protected dynamic AxisName(dynamic axis)
    {
        return ((char) axis.name).AsAscii() +
               ((char) axis.suff).AsAscii();
    }

    protected dynamic SpindleName(dynamic spindle)
    {
        return ((char) spindle.name).AsAscii() +
               ((char) spindle.suff1).AsAscii() +
               ((char) spindle.suff2).AsAscii();
        // ((char) spindle.suff3).AsAscii(); reserved
    }

    protected dynamic SpindleMarker(short number, string name)
    {
        return new
        {
            type = "spindle",
            number,
            name
        };
    }

    protected dynamic AxisMarker(short number, string name)
    {
        return new
        {
            type = "axis",
            number,
            name
        };
    }
}