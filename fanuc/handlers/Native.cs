using System;
using fanuc.veneers;

namespace fanuc.handlers
{
    public class Native: Handler
    {
        public Native(Machine machine,
            Func<Veneers, Veneer, dynamic?> beforeArrival = null,
            Action<Veneers, Veneer, dynamic?> afterArrival = null,
            Func<Veneers, Veneer, dynamic?> beforeChange = null,
            Action<Veneers, Veneer, dynamic?> afterChange = null,
            Func<Veneers, Veneer, dynamic?> beforeError = null,
            Action<Veneers, Veneer, dynamic?> afterError = null) : base(machine, beforeArrival, afterArrival,
            beforeChange, afterChange, beforeError, afterError)
        {
            
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
        
        public override dynamic? OnError(Veneers veneers, Veneer veneer, dynamic? beforeError)
        {
            return null;
        }
    }
}