// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc
{
    public class FocasEndpoint
    {
        // ReSharper disable once InconsistentNaming
        public string IPAddress { get; }

        public ushort Port { get; }

        public short ConnectionTimeout { get; }

        public FocasEndpoint(string focasIpAddress, ushort focasPort, short connectionTimeout)
        {
            IPAddress = focasIpAddress;
            Port = focasPort;
            ConnectionTimeout = connectionTimeout;
        }
    }
}