using System;
using System.Collections.Generic;

namespace l99.driver.@base
{
    public class Machine
    {
        public override string ToString()
        {
            return new
            {
                Id
            }.ToString();
        }

        public virtual dynamic Info
        {
            get
            {
                return new
                {
                    _id
                };
            }
        }

        public Machines Machines
        {
            get => _machines;
        }
        
        protected Machines _machines;
        
        public bool Enabled
        {
            get => _enabled;
        }
        
        protected bool _enabled = false;
        
        public string Id
        {
            get => _id;
        }
        
        protected string _id = string.Empty;
        
        public Machine(Machines machines, bool enabled, string id, dynamic config)
        {
            _machines = machines;
            _enabled = enabled;
            _id = id;
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
        
        protected Handler _handler;
        
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
        
        protected Collector _collector;
        
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
        
        protected Veneers _veneers;

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