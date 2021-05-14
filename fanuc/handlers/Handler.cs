using System;
using fanuc.veneers;

namespace fanuc.handlers
{
    public class Handler
    {
        protected Machine machine;
        protected Func<Veneers, Veneer, dynamic?> beforeArrival;
        protected Action<Veneers, Veneer, dynamic?> afterArrival;
        protected Func<Veneers, Veneer, dynamic?> beforeChange;
        protected Action<Veneers, Veneer, dynamic?> afterChange;
        protected Func<Veneers, Veneer, dynamic?> beforeError;
        protected Action<Veneers, Veneer, dynamic?> afterError;
        
        public Handler(Machine machine, 
            Func<Veneers, Veneer, dynamic?> beforeArrival = null, 
            Action<Veneers, Veneer, dynamic?> afterArrival = null,
            Func<Veneers, Veneer, dynamic?> beforeChange = null, 
            Action<Veneers, Veneer, dynamic?> afterChange = null,
            Func<Veneers, Veneer, dynamic?> beforeError = null, 
            Action<Veneers, Veneer, dynamic?> afterError = null)
        {
            this.machine = machine;
            this.beforeArrival = beforeArrival;
            this.afterArrival = afterArrival;
            this.beforeChange = beforeChange;
            this.afterChange = afterChange;
            this.beforeError = beforeError;
            this.afterError = afterError;
        }
        
        public void OnDataArrivalInternal(Veneers veneers, Veneer veneer)
        {
            dynamic? beforeRet = null;
            if (beforeArrival != null)
                beforeRet = beforeArrival(veneers, veneer);
            
            dynamic? onRet = OnDataArrival(veneers, veneer, beforeRet);

            if (afterArrival != null)
                afterArrival(veneers, veneer, onRet);
        }
        
        public virtual dynamic? OnDataArrival(Veneers veneers, Veneer veneer, dynamic? beforeArrival)
        {
            return null;
        }
        
        public virtual void OnDataChangeInternal(Veneers veneers, Veneer veneer)
        {
            dynamic? beforeRet = null;
            if (beforeChange != null)
                beforeRet = beforeChange(veneers, veneer);
            
            dynamic? onRet = OnDataChange(veneers, veneer, beforeRet);

            if (afterChange != null)
                afterChange(veneers, veneer, onRet);
        }
        
        public virtual dynamic? OnDataChange(Veneers veneers, Veneer veneer, dynamic? beforeChange)
        {
            return null;
        }
        
        public virtual void OnErrorInternal(Veneers veneers, Veneer veneer)
        {
            dynamic? beforeRet = null;
            if (beforeError != null)
                beforeRet = beforeError(veneers, veneer);
            
            dynamic? onRet = OnError(veneers, veneer, beforeRet);

            if (afterError != null)
                afterError(veneers, veneer, onRet);
        }
        
        public virtual dynamic? OnError(Veneers veneers, Veneer veneer, dynamic? beforeError)
        {
            return null;
        }
    }
}