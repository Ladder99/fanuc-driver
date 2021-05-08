using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fanuc
{
    public class Machines
    {
        public List<Machine> All
        {
            get { return _machines; }
        }
        
        private List<Machine> _machines = new List<Machine>();

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
    }
}