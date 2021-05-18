using System;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.handlers
{
    public class Native: Handler
    {
        public Native(Machine machine) : base(machine)
        {
            
        }

        public override void Initialize()
        {
            
        }
        
        protected override dynamic? beforeDataArrival(Veneers veneers, Veneer veneer)
        {
            veneers.Machine["broker"]["disco"].Add(veneers.Machine.Id, veneers.Machine["broker"]);
            
            return null;
        }
        
        public override dynamic? OnDataArrival(Veneers veneers, Veneer veneer, dynamic? beforeArrival)
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
                source = new
                {
                    method = veneer.IsInternal ? "" : veneer.LastArrivedInput.method,
                    invocationMs = veneer.IsInternal ? -1 : veneer.LastArrivedInput.invocationMs,
                    data = veneer.IsInternal ? new { } : veneer.LastArrivedInput.request.GetType().GetProperty(veneer.LastArrivedInput.method).GetValue(veneer.LastArrivedInput.request, null)
                },
                delta = new
                {
                    time = veneer.ArrivalDelta,
                    data = veneer.LastArrivedValue
                }
            };

            return payload;
        }
        
        protected override void afterDataArrival(Veneers veneers, Veneer veneer, dynamic? onArrival)
        {
            var topic = $"fanuc/{veneers.Machine.Id}-all/{veneer.Name}{(veneer.SliceKey == null ? string.Empty : "/" + veneer.SliceKey.ToString())}";
            string payload = JObject.FromObject(onArrival).ToString();
            veneers.Machine["broker"].PublishArrival(topic, payload);
        }
        
        protected override dynamic? beforeDataChange(Veneers veneers, Veneer veneer)
        {
            veneers.Machine["broker"]["disco"].Add(veneers.Machine.Id, veneers.Machine["broker"]);
            
            return null;
        }
        
        public override dynamic? OnDataChange(Veneers veneers, Veneer veneer, dynamic? beforeChange)
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
                source = new
                {
                    method = veneer.LastChangedInput.method,
                    veneer.LastChangedInput.invocationMs,
                    data = veneer.LastChangedInput.request.GetType().GetProperty(veneer.LastChangedInput.method).GetValue(veneer.LastChangedInput.request, null)
                },
                delta = new
                {
                    time = veneer.ChangeDelta,
                    data = veneer.LastChangedValue
                }
            };

            return payload;
        }
        
        protected override void afterDataChange(Veneers veneers, Veneer veneer, dynamic? onChange)
        {
            var topic = $"fanuc/{veneers.Machine.Id}/{veneer.Name}{(veneer.SliceKey == null ? string.Empty : "/" + veneer.SliceKey.ToString())}";
            string payload = JObject.FromObject(onChange).ToString();
            veneers.Machine["broker"].PublishChange(topic, payload);
        }
        
        protected override dynamic? beforeDataError(Veneers veneers, Veneer veneer)
        {
            return null;
        }
        
        public override dynamic? OnError(Veneers veneers, Veneer veneer, dynamic? beforeError)
        {
            return null;
        }
        
        protected override void afterDataError(Veneers veneers, Veneer veneer, dynamic? onError)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(new
            {
                method = veneer.LastArrivedInput.method, rc = veneer.LastArrivedInput.rc
            });
        }
        
        protected override dynamic? beforeSweepComplete(Machine machine)
        {
            return null;
        }
        
        public override dynamic? OnCollectorSweepComplete(Machine machine, dynamic? beforeSweepComplete)
        {
            dynamic payload = new
            {
                observation = new
                {
                    time =  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                    machine = machine.Id,
                    name = "PING"
                },
                source = new
                {
                    data = machine.Info
                },
                delta = new
                {
                    data = machine.CollectorSuccess ? "OK" : "NOK"
                }
            };

            return payload;
        }
        
        protected override void afterSweepComplete(Machine machine, dynamic? onSweepComplete)
        {
            string topic_all = $"fanuc/{machine.Id}-all/PING";
            string topic = $"fanuc/{machine.Id}/PING";
            string payload = JObject.FromObject(onSweepComplete).ToString();
            
            machine["broker"].PublishArrivalStatus(topic_all, payload);
            machine["broker"].PublishChangeStatus(topic, payload);
        }
    }
}