// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.utils;

public class FocasEndpoint
{
    public FocasEndpoint(string focasIpAddress, ushort focasPort, short connectionTimeout)
    {
        IPAddress = focasIpAddress;
        Port = focasPort;
        ConnectionTimeout = connectionTimeout;
    }

    // ReSharper disable once InconsistentNaming
    public string IPAddress { get; }

    public ushort Port { get; }

    public short ConnectionTimeout { get; }
}