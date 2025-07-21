using System.Net.NetworkInformation;

namespace SharedLayer.Common;

public static class MacAddressHelper
{
    public static string GenerateGuid()
    {
        var mac = NetworkInterface.GetAllNetworkInterfaces()
            .FirstOrDefault(_ => _.OperationalStatus == OperationalStatus.Up)?
            .GetPhysicalAddress()
            .ToString();

        return Guid.NewGuid().ToString() + mac;
    }
}