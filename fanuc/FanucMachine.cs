using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc;

// ReSharper disable once ClassNeverInstantiated.Global
public class FanucMachine : Machine
{
    public FanucMachine(Machines machines, object config) : base(machines, config)
    {
        this["platform"] = new Platform(this);

        //TODO: validate config
        FocasEndpoint = new FocasEndpoint(
            Configuration.type["net"]["ip"],
            (ushort) Configuration.type["net"]["port"],
            (short) Configuration.type["net"]["timeout_s"]);
    }

    public override dynamic Info =>
        new
        {
            _id = Id,
            FocasEndpoint.IPAddress,
            FocasEndpoint.Port,
            FocasEndpoint.ConnectionTimeout
        };

    public FocasEndpoint FocasEndpoint { get; }

    public override string ToString()
    {
        return new
        {
            Id,
            FocasEndpoint.IPAddress,
            FocasEndpoint.Port,
            FocasEndpoint.ConnectionTimeout
        }.ToString()!;
    }
}