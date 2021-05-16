using System;
using fanuc.veneers;
using Newtonsoft.Json.Linq;

namespace fanuc.handlers
{
    public class SplunkMetric: Handler
    {
        private int _counter = 0;
        
        public SplunkMetric(Machine machine) : base(machine)
        {
            
        }

        public override void Initialize()
        {
           
        }
        
        protected override dynamic? beforeDataArrival(Veneers veneers, Veneer veneer)
        {
            return null;
        }
        
        public override dynamic? OnDataArrival(Veneers veneers, Veneer veneer, dynamic? beforeArrival)
        {
            return null;
        }
        
        protected override void afterDataArrival(Veneers veneers, Veneer veneer, dynamic? onArrival)
        {
            
        }
        
        protected override dynamic? beforeDataChange(Veneers veneers, Veneer veneer)
        {
            return null;
        }
        
        public override dynamic? OnDataChange(Veneers veneers, Veneer veneer, dynamic? beforeChange)
        {
            if (veneer.Name == "axis_data")
            {
                switch (veneer.Marker[1].name.ToString())
                {
                    case "X":
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case "Y":
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case "Z":
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }

                var payload = new
                {
                    time = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                    @event = "metric",
                    host = veneers.Machine.Id,
                    fields = new
                    {
                        _value = (float) veneer.LastArrivedValue.pos.absolute,
                        path_no = veneer.Marker[0].path_no.ToString(),
                        axis_name = (veneer.Marker[1].name + veneer.Marker[1].suff).ToString(),
                        metric_name = "position"
                    }
                };
                
                Console.WriteLine(
                    _counter++ + " > " +
                    JObject.FromObject(payload).ToString()
                );

                return payload;
            }

            return null;
        }
        
        protected override void afterDataChange(Veneers veneers, Veneer veneer, dynamic? onChange)
        {
            if (onChange == null)
                return;
            
            var topic = $"fanuc/{veneers.Machine.Id}/splunk";
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
            /*
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(new
            {
                method = veneer.LastArrivedInput.method, rc = veneer.LastArrivedInput.rc
            });
            */
        }
        
        protected override dynamic? beforeSweepComplete(Machine machine)
        {
            return null;
        }
        
        public override dynamic? OnCollectorSweepComplete(Machine machine, dynamic? beforeSweepComplete)
        {
            return null;
        }
        
        protected override void afterSweepComplete(Machine machine, dynamic? onSweepComplete)
        {
            
        }
    }
}