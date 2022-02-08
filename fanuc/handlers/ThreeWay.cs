using System;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.handlers
{
    public class ThreeWay: Handler
    {
        private readonly Formatting jsonFormatting = Formatting.None;

        public ThreeWay(Machine machine, object cfg) : base(machine, cfg)
        {
            
        }

        public override async Task<dynamic?> OnDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? beforeChange)
        {
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
            var topic = $"fanuc/{veneers.Machine.Id}/{veneer.Name}{(veneer.SliceKey == null ? string.Empty : "/" + veneer.SliceKey.ToString())}";
            string payload = JObject.FromObject(onChange).ToString(jsonFormatting);
            await veneers.Machine.Transport.SendAsync(topic, payload, true);
        }
        
        public override async Task<dynamic?> OnCollectorSweepCompleteAsync(Machine machine, dynamic? beforeSweepComplete)
        {
            dynamic payload = new
            {
                observation = new
                {
                    time =  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                    machine = machine.Id,
                    name = "ping"
                },
                state = new
                {
                    data = machine.CollectorSuccess ? "OK" : "NOK"
                }
            };
            
            return payload;
        }
        
        protected override async Task afterSweepCompleteAsync(Machine machine, dynamic? onSweepComplete)
        {
            string topic = $"fanuc/{machine.Id}/ping";
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