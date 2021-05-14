using System;
using System.Collections.Generic;
using System.Threading;

namespace fanuc
{
    public class Machines
    {
        private List<Machine> _machines = new List<Machine>();

        private int _collectionInterval;
        
        public Machines(int collectionInterval = 1000)
        {
            _collectionInterval = collectionInterval;
        }
        
        public Machine Add(bool enabled, string id, string ip, ushort port = 8193, short timeout = 2)
        {
            var machine = new Machine(enabled, id, ip, port, timeout);
            _machines.Add(machine);
            return machine;
        }

        public IEnumerable<Machine> this[string id]
        {
            get
            {
                if (string.IsNullOrEmpty(id))
                {
                    return _machines.FindAll(x => x.Enabled);
                }
                else
                {
                    return _machines.FindAll(x => x.Id == id && x.Enabled);
                }
            }
        }

        public void Run()
        {
            foreach (var machine in this[null])
            {
                machine.InitCollector();
            }
            
            while (true)
            {
                Thread.Sleep(_collectionInterval);
                
                foreach (var machine in this[null])
                {
                    machine.RunCollector();
                    machine.Handler.OnCollectorSweepCompleteInternal();
                }
            }
        }
    }
}