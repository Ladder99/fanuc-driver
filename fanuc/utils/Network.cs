// ReSharper disable once CheckNamespace

namespace l99.driver.fanuc.utils;

public static class Network
{
    public static List<string> GetAllLocalIPv4()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            //.Where(x => x.NetworkInterfaceType == type && x.OperationalStatus == OperationalStatus.Up)
            .Where(x => x.OperationalStatus == OperationalStatus.Up)
            .SelectMany(x => x.GetIPProperties().UnicastAddresses)
            .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
            .Select(x => x.Address.ToString())
            .ToList();
    }
}