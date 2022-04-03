using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;
using JsonFlatten;
using MoreLinq;
using Newtonsoft.Json.Linq;
using SparkplugNet.VersionB.Data;
using SpBB = SparkplugNet.VersionB;

namespace l99.driver.fanuc.transports
{
    public class SpB : Transport
    {
        private dynamic _config;

        private Dictionary<string, dynamic> _previous = new Dictionary<string, dynamic>();
        private Dictionary<string, dynamic> _current = new Dictionary<string, dynamic>();

        private SpBB.SparkplugNode _node;
        
        public SpB(Machine machine, object cfg) : base(machine, cfg)
        {
            _config = cfg;
        }

        public override async Task<dynamic?> CreateAsync()
        {
            // create spb client
            // add device
            List<SpBB.Data.Metric> metrics = new List<Metric>()
            {
                
            };
            _node = new SpBB.SparkplugNode(metrics, null);

            return null;
        }

        public override async Task ConnectAsync()
        {
            // track broker connection status
            
            //  disconnected => connected
            //      _broker_connection_status_change = true
            //      NBIRTH
            
            //  connected => disconnected
            //      LWT NDEATH
            //      _broker_connection_status_change = true
        }

        public override async Task SendAsync(params dynamic[] parameters)
        {
            var @event = parameters[0];
            var veneer = parameters[1];
            var data = parameters[2];

            switch (@event)
            {
                case "DATA_ARRIVE":

                    //  collect metrics
                    //  current[veneer] = flatten(data)
                    
                    var prefix = $"{veneer.Name}{(veneer.SliceKey == null ? null : "/"+veneer.SliceKey)}";
                    processIncoming(prefix, data);
                    
                    break;
                    
                case "SWEEP_END":
                    
                    //  make diff
                    //      current = dict<veneer,data>
                    //      previous = dict<veneer, data>
                    //      diff = current,previous
                    
                    // track cnc online status
                    
                    //  offline => online
                    //      broker.isConnected
                    //          DBIRTH  current
                    
                    //  
                    //  online => offline
                    //      broker.isConnected
                    //          LWT DDEATH
                    
                    //  online
                    //      broker.isConnected
                    //          DDATA   diff
                    
                    //  broker disconnected => connected
                    //      _broker_connection_status_change = true && broker.isConnected
                    //          DBIRTH  current
                    
                    //  previous = current
                    //  current = null
                    
                    processIncoming("sweep", data);
                    
                    //var diff1 = _current
                    //    .Except(_previous)
                    //    .ToDictionary(
                    //        kvp => kvp.Key, 
                    //        kvp => kvp.Value);

                    Console.WriteLine(" ");
                    Console.WriteLine($"========== {DateTime.Now.ToString()}");
                    Console.WriteLine($"current count = {_current.Count()}");
                    Console.WriteLine(" ");

                    var diff = new Dictionary<string, object>();
                    
                    _current    // iterate current and compare to previous
                        .ForEach(x =>
                        {
                            if (!_previous.ContainsKey(x.Key))              // new key added
                            {
                                diff.Add(x.Key, x.Value);
                                Console.WriteLine($"A {x.Key} = {x.Value.ToString()}");
                            }
                            else if (!_previous[x.Key].Equals(x.Value))     // value changed
                            {
                                Console.WriteLine($"C {x.Key} = {x.Value.ToString()}");
                            }
                        });
                    
                    //diff1
                    //    .ForEach(x =>
                    //    {
                    //        // maintain current as master
                    //        _current[x.Key] = x.Value;
                    //        Console.WriteLine($"{x.Key} = {x.Value.ToString()}");
                    //    });
                    
                    //var diff2 = _previous
                    //    .Except(_current)
                    //    .ToDictionary(
                    //        kvp => kvp.Key, 
                    //        kvp => kvp.Value);;
                    
                    _previous = _current
                        .ToDictionary(
                            kvp => kvp.Key, 
                            kvp => kvp.Value);
                    //_current.Clear();
                    
                    break;
                
                case "INT_MODEL":

                    break;
            }
        }

        void processIncoming(string prefix, dynamic data)
        {
            // flatten incoming data
            JObject jc = JObject.FromObject(data.state.data);
            var fc = jc.Flatten();
            
            // massage keys
            var dict = fc.Select(x=>
                {
                    return new KeyValuePair<string, object>(
                        //$"{prefix.Replace('/','.')}.{x.Key.Replace('[','_').Replace("]", string.Empty)}",
                        $"{prefix.Replace('/','.')}.{x.Key}",
                        x.Value);
                })
                .ToDictionary(
                    x=> x.Key,
                    x=> x.Value);
               
            // remove arrays for now, we need to deal with those differently
            var keys = dict.Keys.Where(k => k.Contains('[')).ToList();
            foreach (var key in keys)
            {
                dict.Remove(key);
            }
            
            dict    // iterate incoming data
                .ForEach(x =>
                {
                    if (!_current.ContainsKey(x.Key))
                    {
                        // add if doesn't exist
                        _current.Add(x.Key, x.Value);
                    }
                    else
                    {
                        // assign if exists
                        _current[x.Key] = x.Value;
                    }
                });
            
            // merge data into current
            //_current = _current
            //    .Union(dict)
            //    .ToDictionary(
            //        kvp => kvp.Key, 
            //        kvp => kvp.Value);
        }

    }
}
