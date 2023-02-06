using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using l99.driver.@base;
using Scriban;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.transports;

// ReSharper disable once InconsistentNaming
// ReSharper disable once UnusedType.Global
public class InfluxLP : Transport
{
    // runtime - veneer name, template
    private readonly Dictionary<string, Template> _templateLookup = new();

    private InfluxDBClient _client = null!;

    // config - veneer type, template text
    private Dictionary<string, string> _transformLookup = new();
    private WriteApiAsync _writeApi = null!;

    public InfluxLP(Machine machine) : base(machine)
    {
        //TODO: make defaults
    }

    public override async Task<dynamic?> CreateAsync()
    {
        _client = InfluxDBClientFactory
            .Create(
                Machine.Configuration.transport["host"], 
                Machine.Configuration.transport["token"]);

        _writeApi = _client.GetWriteApiAsync();

        _transformLookup = (Machine.Configuration.transport["transformers"] as Dictionary<dynamic, dynamic>)
            .ToDictionary(
                kv => (string) kv.Key,
                kv => (string) kv.Value);

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

                if (HasTransform(veneer))
                {
                    string lp = _templateLookup[veneer.Name]
                        .Render(new {data.observation, data.state.data});

                    if (!string.IsNullOrEmpty(lp))
                    {
                        Logger.Info($"[{Machine.Id}] {lp}");
                        _writeApi
                            .WriteRecordAsync(
                                lp,
                                WritePrecision.Ms,
                                Machine.Configuration.transport["bucket"],
                                Machine.Configuration.transport["org"]);
                    }
                }

                break;

            case "SWEEP_END":

                if (HasTransform("SWEEP_END"))
                {
                    var lp = _templateLookup["SWEEP_END"]
                        .Render(new {data.observation, data.state.data});

                    Logger.Info($"[{Machine.Id}] {lp}");

                    _writeApi
                        .WriteRecordAsync(
                            lp,
                            WritePrecision.Ms,
                            Machine.Configuration.transport["bucket"],
                            Machine.Configuration.transport["org"]);
                }

                break;

            case "INT_MODEL":

                break;
        }
    }

    private bool HasTransform(string templateName, string transformName = null!)
    {
        if (transformName == null)
            transformName = templateName;

        // template exists and has been cached
        if (_templateLookup.ContainsKey(templateName)) return true;

        // transform exists in config, create a template and cache it
        if (_transformLookup.ContainsKey(transformName))
        {
            var transform = _transformLookup[transformName];
            var template = Template.Parse(transform);
            if (template.HasErrors) Logger.Error($"[{Machine.Id}] '{templateName}' template transform has errors");
            _templateLookup.Add(templateName, template);
            return true;
        }

        return false;
    }

    private bool HasTransform(Veneer veneer)
    {
        return HasTransform(veneer.Name,
            $"{veneer.GetType().FullName}, {veneer.GetType().Assembly.GetName().Name}");
    }
}

/*

from(bucket: "fanuc")
  |> range(start: v.timeRangeStart, stop: v.timeRangeStop)
  |> filter(fn: (r) => r["_measurement"] == "gcode")
  |> map(fn: (r) => ({r with _value: string(v: r._value)}))
  |> group(columns: ["machine", "path", "_measurement", "_time"])
  |> pivot(rowKey:["_time"], columnKey: ["_field"], valueColumn: "_value")
  |> group()
  |> drop(columns: ["_measurement"])
  |> yield()

from(bucket: "fanuc")
  |> range(start: v.timeRangeStart, stop: v.timeRangeStop)
  |> filter(fn: (r) => r["_measurement"] == "state")
  |> filter(fn: (r) => r["_field"] == "execution")
  |> stateDuration(
    fn: (r) => r._value == "READY",
    column: "ready_duration",
    unit: 1s)
  |> stateDuration(
    fn: (r) => r._value == "ACTIVE",
    column: "active_duration",
    unit: 1s)
  |> yield()
  
*/