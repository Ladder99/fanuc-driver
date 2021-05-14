using System;
using fanuc.veneers;

namespace fanuc.handlers
{
    public class Handler
    {
        protected Machine machine;
        
        public Handler(Machine machine)
        {
            this.machine = machine;
        }
        
        public virtual void Initialize(dynamic config)
        {
            
        }
        
        public void OnDataArrivalInternal(Veneers veneers, Veneer veneer)
        {
            dynamic? beforeRet = beforeDataArrival(veneers, veneer);
            dynamic? onRet = OnDataArrival(veneers, veneer, beforeRet);
            afterDataArrival(veneers, veneer, onRet);
        }

        protected virtual dynamic? beforeDataArrival(Veneers veneers, Veneer veneer)
        {
            return null;
        }
        
        public virtual dynamic? OnDataArrival(Veneers veneers, Veneer veneer, dynamic? beforeArrival)
        {
            return null;
        }
        
        protected virtual void afterDataArrival(Veneers veneers, Veneer veneer, dynamic? onArrival)
        {
            
        }
        
        public virtual void OnDataChangeInternal(Veneers veneers, Veneer veneer)
        {
            dynamic? beforeRet = beforeDataChange(veneers, veneer);
            dynamic? onRet = OnDataChange(veneers, veneer, beforeRet);
            afterDataChange(veneers, veneer, onRet);
        }
        
        protected virtual dynamic? beforeDataChange(Veneers veneers, Veneer veneer)
        {
            return null;
        }
        
        public virtual dynamic? OnDataChange(Veneers veneers, Veneer veneer, dynamic? beforeChange)
        {
            return null;
        }
        
        protected virtual void afterDataChange(Veneers veneers, Veneer veneer, dynamic? onChange)
        {
            
        }
        
        public virtual void OnErrorInternal(Veneers veneers, Veneer veneer)
        {
            dynamic? beforeRet = beforeDataError(veneers, veneer);
            dynamic? onRet = OnError(veneers, veneer, beforeRet);
            afterDataError(veneers, veneer, onRet);
        }
        
        protected virtual dynamic? beforeDataError(Veneers veneers, Veneer veneer)
        {
            return null;
        }
        
        public virtual dynamic? OnError(Veneers veneers, Veneer veneer, dynamic? beforeError)
        {
            return null;
        }
        
        protected virtual void afterDataError(Veneers veneers, Veneer veneer, dynamic? onError)
        {
            
        }

        public virtual void OnCollectorSweepCompleteInternal()
        { 
            dynamic? beforeRet = beforeSweepComplete(machine);
            dynamic? onRet = OnCollectorSweepComplete(machine, beforeRet);
            afterSweepComplete(machine, onRet);
        }

        protected virtual dynamic? beforeSweepComplete(Machine machine)
        {
            return null;
        }
        
        public virtual dynamic? OnCollectorSweepComplete(Machine machine, dynamic? beforeSweepComplete)
        {
            return null;
        }

        protected virtual void afterSweepComplete(Machine machine, dynamic? onSweepComplete)
        {
            
        }
    }
}