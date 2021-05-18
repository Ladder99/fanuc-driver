using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
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
        
        public override void Initialize()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(nextSequence() + " > " + string.Format(_topicFormat, "NBIRTH"));
            // MQTT LWT NDEATH
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
            
            return null;
        }
        
        protected override void afterDataChange(Veneers veneers, Veneer veneer, dynamic? onChange)
        {
            
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
            Console.WriteLine(nextSequence() + " > " + string.Format(_topicFormat, "NDATA"));
            // veneer.LastArrivedInput.method, rc = veneer.LastArrivedInput.rc
        }
        
        protected override dynamic? beforeSweepComplete(Machine machine)
        {
            return null;
        }
        
        public override dynamic? OnCollectorSweepComplete(Machine machine, dynamic? beforeSweepComplete)
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

            return null;
        }
        
        protected override void afterSweepComplete(Machine machine, dynamic? onSweepComplete)
        {
            
        }
    }
}