using System;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.handlers
{
    public class FanucOne: Handler
    {
        private readonly Formatting jsonFormatting = Formatting.None;

        public FanucOne(Machine machine, object cfg) : base(machine, cfg)
        {
            
        }

        private string topicEval(Veneer veneer)
        {
            return $"fanuc/{machine.Id}/{veneer.Name}{(veneer.SliceKey == null ? string.Empty : "/" + veneer.SliceKey.ToString())}";
        }
        
        public override async Task<dynamic?> OnDataArrivalAsync(Veneers veneers, Veneer veneer, dynamic? beforeArrival)
        {
            // only allow focas performance
            if (veneer.Name != "focas_perf")
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
            
            var topic = topicEval(veneer);
            string payload = JObject.FromObject(onArrival).ToString(jsonFormatting);
            await veneers.Machine.Transport.SendAsync(topic, payload, true);
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
            
            var topic = topicEval(veneer);
            string payload = JObject.FromObject(onChange).ToString(jsonFormatting);
            await veneers.Machine.Transport.SendAsync(topic, payload, true);
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
            string topic = $"fanuc/{machine.Id}/sweep";
            string payload = JObject.FromObject(onSweepComplete).ToString(jsonFormatting);
            await machine.Transport.SendAsync(topic, payload, true);
        }

        public override async Task OnGenerateIntermediateModel(string json)
        {
            var topic = $"fanuc/{machine.Id}/$model";
            await machine.Transport.SendAsync(topic, json, true);
        }
    }
}