using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using fanuc.collectors;
using fanuc.handlers;
using fanuc.veneers;

namespace fanuc
{
    public class Machine
    {
        public override string ToString()
        {
            return new
            {
                Id,
                _focasEndpoint.IPAddress,
                _focasEndpoint.Port,
                _focasEndpoint.ConnectionTimeout
            }.ToString();
        }

        public dynamic Info
        {
            get
            {
                return new
                {
                    _id,
                    _focasEndpoint.IPAddress,
                    _focasEndpoint.Port,
                    _focasEndpoint.ConnectionTimeout
                };
            }
        }

        public Machines Machines
        {
            get => _machines;
        }
        
        private Machines _machines;
        
        public FocasEndpoint FocasEndpoint
        {
            get => _focasEndpoint;
        }
        
        private FocasEndpoint _focasEndpoint;
        
        public Platform Platform
        {
            get => _platform;
        }
        
        private Platform _platform;
        
        public bool Enabled
        {
            get => _enabled;
        }
        
        private bool _enabled = false;
        
        public string Id
        {
            get => _id;
        }
        
        private string _id = string.Empty;
        
        public Machine(Machines machines, bool enabled, string id, string focasIpAddress, ushort focasPort = 8193, short timeout = 10)
        {
            _machines = machines;
            _enabled = enabled;
            _id = id;
            _focasEndpoint = new FocasEndpoint(focasIpAddress, focasPort, timeout);
            _platform = new Platform(this);
            _veneers = new Veneers(this);
            _propertyBag = new Dictionary<string, dynamic>();
        }

        #region property-bag
        
        private Dictionary<string, dynamic> _propertyBag;
        
        public dynamic? this[string propertyBagKey]
        {
            get
            {
                if (_propertyBag.ContainsKey(propertyBagKey))
                {
                    return _propertyBag[propertyBagKey];
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (_propertyBag.ContainsKey(propertyBagKey))
                {
                    _propertyBag[propertyBagKey] = value;
                }
                else
                {
                    _propertyBag.Add(propertyBagKey, value);
                }
            }
        }
        
        #endregion
        
        #region handler
        
        public Handler Handler
        {
            get => _handler;
        }
        
        private Handler _handler;
        
        public void AddHandler(Type type)
        {
            _handler = (Handler) Activator.CreateInstance(type, new object[] { this });
            _handler.Initialize();
            _veneers.OnDataArrival = _handler.OnDataArrivalInternal;
            _veneers.OnDataChange = _handler.OnDataChangeInternal;
            _veneers.OnError = _handler.OnErrorInternal;
        }
        
        #endregion
        
        #region collector
        
        public bool CollectorSuccess
        {
            get => _collector.LastSuccess;
        }
        
        public Collector Collector
        {
            get => _collector;
        }
        
        private Collector _collector;
        
        public void AddCollector(Type type, int sweepMs = 1000)
        {
            _collector = (Collector) Activator.CreateInstance(type, new object[] { this, sweepMs });
        }

        public void InitCollector()
        {
            _collector.Initialize();
        }

        public void RunCollector()
        {
            _collector.Collect();
        }
        
        #endregion
        
        #region veneeers
        
        public Veneers Veneers
        {
            get => _veneers;
        }
        
        private Veneers _veneers;

        public bool VeneersApplied
        {
            get; 
            set;
        }

        public void ApplyVeneer(Type type, string name, bool isInternal = false)
        {
            _veneers.Add(type, name, isInternal);
        }

        public void SliceVeneer(dynamic split)
        {
            _veneers.Slice(split);
        }
        
        public void SliceVeneer(dynamic sliceKey, dynamic split)
        {
            _veneers.Slice(sliceKey, split);
        }

        public void ApplyVeneerAcrossSlices(Type type, string name, bool isInternal = false)
        {
            _veneers.AddAcrossSlices(type, name, isInternal);
        }
        
        public void ApplyVeneerAcrossSlices(dynamic sliceKey, Type type, string name, bool isInternal = false)
        {
            _veneers.AddAcrossSlices(sliceKey, type, name, isInternal);
        }

        public dynamic PeelVeneer(string name, dynamic input, dynamic? input2 = null)
        {
            return _veneers.Peel(name, input, input2);
        }
        
        public dynamic PeelAcrossVeneer(dynamic split, string name, dynamic input, dynamic? input2 = null)
        {
            return _veneers.PeelAcross(split, name, input, input2);
        }

        public void MarkVeneer(dynamic split, dynamic marker)
        {
            _veneers.Mark(split, marker);
        }
        
        #endregion
    }
}