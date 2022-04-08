using l99.driver.@base;

namespace l99.driver.fanuc.transports;

public class Null : Transport
{
    private dynamic _config;
    private dynamic _model;
    
    public Null(Machine machine, object cfg) : base(machine, cfg)
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