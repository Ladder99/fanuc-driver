using System;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scriban;

namespace l99.driver.fanuc.handlers
{
    public class FanucOne: Handler
    {
        private dynamic _cfg;
        private Template _topicTemplate;
        public FanucOne(Machine machine, object cfg) : base(machine, cfg)
        {
            _cfg = cfg;
            //TODO: validate config
            _topicTemplate = Template.Parse(_cfg.handler["JSON"]["topic"]);
        }
        
        public override async Task<dynamic?> OnDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? beforeArrival)
        {
            // only allow focas performance
            if (veneer.GetType().Name != "FocasPerf")
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

            switch (_cfg.handler["transfer"])
            {
                case "JSON":
                    var topic = await _topicTemplate.RenderAsync(new { machine, veneer}, member => member.Name);
                    string payload = JObject.FromObject(onArrival).ToString(Formatting.None);
                    await veneers.Machine.Transport.SendAsync(topic, payload, true);
                    break;
                
                default:
                    await veneers.Machine.Transport.SendAsync("DATA_ARRIVE", veneer, onArrival);
                    break;
            }
        }

        public override async Task<dynamic?> OnDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? beforeChange)
        {
            // skip internal veneers
            if (veneer.IsInternal == true)
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
            
            switch (_cfg.handler["transfer"])
            {
                case "JSON":
                    var topic = await _topicTemplate.RenderAsync(new { machine, veneer}, member => member.Name);
                    string payload = JObject.FromObject(onChange).ToString(Formatting.None);
                    await veneers.Machine.Transport.SendAsync(topic, payload, true);
                    break;
                
                default:
                    await veneers.Machine.Transport.SendAsync("DATA_CHANGE", veneer, onChange);
                    break;
            }
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
            switch (_cfg.handler["transfer"])
            {
                case "JSON":
                    string topic = $"fanuc/{machine.Id}/sweep";
                    string payload = JObject.FromObject(onSweepComplete).ToString(Formatting.None);
                    await machine.Transport.SendAsync(topic, payload, true);
                    break;
                
                default:
                    await machine.Transport.SendAsync("SWEEP_END", null, onSweepComplete);
                    break;
            }
        }

        public override async Task OnGenerateIntermediateModel(string json)
        {
            switch (_cfg.handler["transfer"])
            {
                case "JSON":
                    var topic = $"fanuc/{machine.Id}/$model";
                    await machine.Transport.SendAsync(topic, json, true);
                    break;
                
                default:
                    await machine.Transport.SendAsync("INT_MODEL", null, json);
                    break;
            }
        }
    }
}