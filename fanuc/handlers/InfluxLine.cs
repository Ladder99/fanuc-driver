using System;
using System.Threading.Tasks;
using InfluxDB.LineProtocol;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.handlers
{
    public class InfluxLine: Handler
    {
        private int _counter = 0;
        
        public InfluxLine(Machine machine) : base(machine)
        {
            
        }

        public override async Task<dynamic?> OnDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? beforeChange)
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

                var payload = new LineProtocolWriter(Precision.Milliseconds)
                    .Measurement(veneer.Name)
                    .Tag("machine_id", veneers.Machine.Id)
                    .Tag("path_no", veneer.Marker[0].path_no.ToString())
                    .Tag("axis_name", (veneer.Marker[1].name + veneer.Marker[1].suff).ToString())
                    .Field("position", (float) veneer.LastArrivedValue.pos.absolute)
                    .Field("feed", (float) veneer.LastArrivedValue.actf);
                
                Console.WriteLine(
                    _counter++ + " > " +
                    payload
                );

                return payload;
            }

            await Task.Yield();
            return null;
        }
        
        protected override async Task afterDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? onChange)
        {
            if (onChange == null)
            {
                await Task.Yield();
                return;
            }
                
            var topic = $"fanuc/{veneers.Machine.Id}/influx";
            string payload = JObject.FromObject(onChange).ToString();
            await veneers.Machine["broker"].PublishChangeAsync(topic, payload);
        }
        
        protected override async Task afterDataErrorAsync(Veneers veneers, Veneer veneer, dynamic? onError)
        {
            /*
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(new
            {
                method = veneer.LastArrivedInput.method, rc = veneer.LastArrivedInput.rc
            });
            */
            
            await Task.Yield();
        }
    }
}