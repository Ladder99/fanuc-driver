using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.transports;

// ReSharper disable once UnusedType.Global
public class Null : Transport
{
    // ReSharper disable once NotAccessedField.Local
    private dynamic _model = null!;
    
    public Null(Machine machine, object cfg) : base(machine, cfg)
    {
        
    }

    public override async Task<dynamic?> CreateAsync()
    {
        return await Task.FromResult(new {});
    }

    public override async Task ConnectAsync()
    {
        await Task.FromResult(0);
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
        
        await Task.FromResult(0);
    }
    
    public override async Task OnGenerateIntermediateModelAsync(dynamic model)
    {
        _model = model;
        await Task.FromResult(0);
    }
}