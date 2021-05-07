using System;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace fanuc.veneers
{
    public class Veneer
    {
        public TimeSpan ChangeDelta
        {
            get { return _stopwatch.Elapsed; }
        }
        
        protected Stopwatch _stopwatch = new Stopwatch();

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
        
        public dynamic LastInput
        {
            get { return _lastInput; }
        }
        
        protected dynamic _lastInput = new { };

        public dynamic DataDelta
        {
            get { return _lastValue; }
        }
        
        protected dynamic _lastValue = new { };

        protected bool _isFirstCall = true;
        
        public Action<Veneer> OnError = (veneer) => { };

        public Action<Veneer> OnChange =  (veneer) => { };

        protected void writeJsonArrayToConsole(dynamic d)
        {
            Console.WriteLine(JArray.FromObject(d).ToString());
        }
        
        public Veneer(string name = "")
        {
            _name = name;
            _stopwatch.Start();
        }

        protected void dataChanged(dynamic input, dynamic current_value)
        {
            this._lastInput = input;
            this._lastValue = current_value;
            this.OnChange(this);
            _stopwatch.Restart();
        }

        protected void onError(dynamic input)
        {
            this._lastInput = input;
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
            return new { };
        }

        protected virtual dynamic Next(dynamic input)
        {
            return new { };
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