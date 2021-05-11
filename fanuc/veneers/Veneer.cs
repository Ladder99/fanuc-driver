using System;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace fanuc.veneers
{
    public class Veneer
    {
        public string Name
        {
            get { return _name; }
        }
        
        protected string _name = "";

        public dynamic SliceKey
        {
            get { return _sliceKey; }
        }
        
        protected dynamic? _sliceKey = null;
        
        public dynamic Marker
        {
            get { return _marker; }
        }
        
        protected dynamic _marker = new { };
        
        protected bool _hasMarker = false;
        
        public TimeSpan ArrivalDelta
        {
            get { return _stopwatchDataArrival.Elapsed; }
        }
        
        public dynamic LastArrivedInput
        {
            get { return _lastArrivedInput; }
        }
        
        protected dynamic _lastArrivedInput = new { };
        
        public dynamic LastArrivedValue
        {
            get { return _lastArrivedValue; }
        }
        
        protected dynamic _lastArrivedValue = new { };
        
        protected Stopwatch _stopwatchDataArrival = new Stopwatch();
        
        public dynamic LastChangedInput
        {
            get { return _lastChangedInput; }
        }
        
        protected dynamic _lastChangedInput = new { };
        
        public dynamic LastChangedValue
        {
            get { return _lastChangedValue; }
        }
        
        protected dynamic _lastChangedValue = new { };
        
        public TimeSpan ChangeDelta
        {
            get { return _stopwatchDataChange.Elapsed; }
        }
        
        protected Stopwatch _stopwatchDataChange = new Stopwatch();

        protected bool _isFirstCall = true;
        
        public Action<Veneer> OnError = (veneer) => { };

        public Action<Veneer> OnChange =  (veneer) => { };
        
        public Action<Veneer> OnArrival =  (veneer) => { };
        
        public Veneer(string name = "")
        {
            _name = name;
            _stopwatchDataChange.Start();
        }

        protected void onDataArrived(dynamic input, dynamic current_value)
        {
            this._lastArrivedInput = input;
            this._lastArrivedValue = current_value;
            this.OnArrival(this);
            _stopwatchDataArrival.Restart();
        }
        
        protected void onDataChanged(dynamic input, dynamic current_value)
        {
            this._lastChangedInput = input;
            this._lastChangedValue = current_value;
            this.OnChange(this);
            _stopwatchDataChange.Restart();
        }

        protected void onError(dynamic input)
        {
            this._lastArrivedInput = input;
            // TODO: overwrite last arrived value?
            this.OnError(this);
        }

        public void SetSliceKey(dynamic? sliceKey)
        {
            _sliceKey = sliceKey;
        }
        
        public void Mark(dynamic marker)
        {
            _marker = marker;
            _hasMarker = true;
        }
        
        protected virtual dynamic First(dynamic input)
        {
            return Any(input);
        }

        protected virtual dynamic Any(dynamic input)
        {
            return new { };
        }

        public dynamic Peel(dynamic input)
        {
            if(_isFirstCall)
            {
                _isFirstCall = false;
                return this.First(input);
            }
            else
            {
                return this.Any(input);
            }
        }
    }
}