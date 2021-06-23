namespace l99.driver.fanuc
{
    public class FocasEndpoint
    {
        public string IPAddress => _focasIpAddress;

        private string _focasIpAddress = "127.0.0.1";
        
        public ushort Port => _focasPort;

        private ushort _focasPort = 8193;
        
        public short ConnectionTimeout => _connectionTimeout;

        private short _connectionTimeout = 3;

        public FocasEndpoint(string focasIpAddress, ushort focasPort, short connectionTimeout)
        {
            _focasIpAddress = focasIpAddress;
            _focasPort = focasPort;
            _connectionTimeout = connectionTimeout;
        }
    }
}