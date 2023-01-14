#pragma warning disable CS1998

using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.transports;

public class Null : Transport
{
    // ReSharper disable once NotAccessedField.Local
    private dynamic _config;
    // ReSharper disable once NotAccessedField.Local
    private dynamic _model;
    
#pragma warning disable CS8618
    public Null(Machine machine, object cfg) : base(machine, cfg)
#pragma warning restore CS8618
    {
        _config = cfg;
    }

    public override async Task<dynamic?> CreateAsync()
    {
        
        return null;
    }

    public override async Task ConnectAsync()
    {
        
    }

    public override async Task SendAsync(params dynamic[] parameters)
    {
        var @event = parameters[0];
        var veneer = parameters[1];
        var data = parameters[2];

        switch (@event)
        {
            case "DATA_ARRIVE":

                break;
                
            case "SWEEP_END":
                
                break;
            
            case "INT_MODEL":

                break;
        }
    }
    
    public override async Task OnGenerateIntermediateModelAsync(dynamic model)
    {
        _model = model;
    }
}
#pragma warning restore CS1998