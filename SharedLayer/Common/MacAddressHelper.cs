using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace SharedLayer.Common
{
    public static class MacAddressHelper
    {
        public static string GenerateStableId()
        {
            var mac = NetworkInterface
                .GetAllNetworkInterfaces()
                .FirstOrDefault(nic =>
                    nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    nic.OperationalStatus == OperationalStatus.Up)?
                .GetPhysicalAddress()
                .ToString();

            return string.IsNullOrWhiteSpace(mac)
                ? Guid.NewGuid().ToString()
                : CreateGuidFromString(mac).ToString();
        }

        private static Guid CreateGuidFromString(string input)
        {
            using var provider = System.Security.Cryptography.MD5.Create();
            var hash = provider.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return new Guid(hash);
        }
    }
}
