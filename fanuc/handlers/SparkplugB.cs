using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.handlers
{
    public class SparkplugB: Handler
    {
        private int _sequence = 0;
        private string _topicFormat;
        private bool _isDeviceAlive = false;
        private List<dynamic> _ddata = new List<dynamic>();
        
        public SparkplugB(Machine machine) : base(machine)
        {
            _topicFormat = $"spBv1.0/fanuc/{{0}}/{IPGlobalProperties.GetIPGlobalProperties().HostName}/{this.machine.Id}";
        }

        private int nextSequence()
        {
            if (_sequence > 255)
                _sequence = 0;

            int ns = _sequence;
            _sequence++;
            return ns;
        }

        private int currentSequence()
        {
            return _sequence - 1;
        }
        
        public override async Task InitializeAsync()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(nextSequence() + " > " + string.Format(_topicFormat, "NBIRTH"));
            // MQTT LWT NDEATH
            
            await Task.Yield();
        }
        
        public override async Task<dynamic?> OnDataChangeAsync(Veneers veneers, Veneer veneer, dynamic? beforeChange)
        {
            if (veneer.Name == "connect")
            {
                if (veneer.LastChangedValue.success == true)
                {
                    _isDeviceAlive = true;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(nextSequence() + " > " + string.Format(_topicFormat, "DBIRTH"));
                }
                else
                {
                    _isDeviceAlive = true;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(nextSequence() + " > " + string.Format(_topicFormat, "DDEATH"));
                }
            }

            if (veneer.Name == "axis_data" && _isDeviceAlive == true)
            {
                _ddata.Add(new
                {
                    name= $"{veneer.Name}/{veneer.SliceKey}",
                    timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                    dataType = "Float",
                    value = (float) veneer.LastArrivedValue.pos.absolute
                });
            }
            
            await Task.Yield();
            return null;
        }
        
        protected override async Task afterDataErrorAsync(Veneers veneers, Veneer veneer, dynamic? onError)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(nextSequence() + " > " + string.Format(_topicFormat, "NDATA"));
            // veneer.LastArrivedInput.method, rc = veneer.LastArrivedInput.rc
            
            await Task.Yield();
        }
        
        public override async Task<dynamic?> OnCollectorSweepCompleteAsync(Machine machine, dynamic? beforeSweepComplete)
        {
            if (_ddata.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(nextSequence() + " > " + string.Format(_topicFormat, "DDATA"));

                dynamic payload = new
                {
                    timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                    metrics = _ddata,
                    seq = currentSequence()
                };
                
                Console.WriteLine(currentSequence() + " > " + JObject.FromObject(payload).ToString());
                
                _ddata.Clear();
            }

            await Task.Yield();
            return null;
        }
    }
}