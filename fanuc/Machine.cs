using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
                IPAddress,
                Port,
                ConnectionTimeout
            }.ToString();
        }

        public dynamic Info
        {
            get
            {
                return new
                {
                    Id,
                    IPAddress,
                    Port,
                    ConnectionTimeout
                };
            }
        }
        
        public Platform Platform
        {
            get { return _platform; }
        }
        
        private Platform _platform;
        
        private bool _enabled = false;
        
        private string _id = string.Empty;
        
        public string IPAddress
        {
            get { return _focasIpAddress; }
        }
        
        private string _focasIpAddress = "127.0.0.1";
        
        public ushort Port
        {
            get { return _focasPort; }
        }
        
        private ushort _focasPort = 8193;
        
        public short ConnectionTimeout
        {
            get { return _connectionTimeout; }
        }
        
        private short _connectionTimeout = 3;

        public Veneers Veneers
        {
            get { return _veneers; }
        }
        
        private Veneers _veneers;
        
        public bool Enabled
        {
            get { return _enabled; }
        }
        
        public string Id
        {
            get { return _id; }
        }
        
        public Machine(bool enabled, string id, string focasIpAddress, ushort focasPort = 8193, short timeout = 10)
        {
            _enabled = enabled;
            _id = id;
            _focasIpAddress = focasIpAddress;
            _focasPort = focasPort;
            _connectionTimeout = timeout;
            _platform = new Platform(this);
            _veneers = new Veneers(this);
        }
        
        public bool VeneersCreated { get; set; }

        public void AddVeneer(Type type, string note)
        {
            _veneers.Add(type, note);
        }

        public void SliceVeneer(dynamic split)
        {
            _veneers.Slice(split);
        }

        public void AddVeneerAcrossSlices(Type type, string note)
        {
            _veneers.AddAcrossSlices(type, note);
        }

        public dynamic PeelVeneer(string note, dynamic input)
        {
            return _veneers.Peel(note, input);
        }
        
        public dynamic PeelAcrossVeneer(dynamic split, string note, dynamic input)
        {
            return _veneers.PeelAcross(split, note, input);
        }

        public void MarkVeneer(dynamic split, dynamic marker)
        {
            _veneers.Mark(split, marker);
        }
    }
}