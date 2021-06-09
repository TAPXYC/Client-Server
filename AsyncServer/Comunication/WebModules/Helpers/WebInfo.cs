using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

class WebInfo
{
    public static IPAddress GetIP()
    {
        return Dns.GetHostEntry(Dns.GetHostName()).AddressList.Last(ip => ip.AddressFamily == AddressFamily.InterNetwork);
    }


    public static IPAddress GetIP(string name)
    {
        return Dns.GetHostEntry(name).AddressList.Last(ip => ip.AddressFamily == AddressFamily.InterNetwork);
    }




    /// <summary>
    /// Возвращает имя машины по IP
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    public static string GetNameByIp(IPAddress ip)
    {
        return Dns.GetHostEntry(ip).HostName;
    }





    /// <summary>
    /// Возвращает маску подсистемы
    /// </summary>
    /// <param name="address">IP подсистемы</param>
    /// <returns>Маска подсистемы</returns>
    public static IPAddress GetSubnetMask(IPAddress address)
    {
        foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
        {
            foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
            {
                if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (address.Equals(unicastIPAddressInformation.Address))
                    {
                        return unicastIPAddressInformation.IPv4Mask;
                    }
                }
            }
        }
        throw new ArgumentException($"Can't find subnetmask for IP address '{address}'");
    }





    /// <summary>
    /// Возвращает шлюз
    /// </summary>
    /// <param name="ip">IP устройства</param>
    public static IPAddress GetGates(IPAddress ip)
    {
        IPAddress currentGates = null;

        var targetInterface = NetworkInterface.GetAllNetworkInterfaces()
            .SingleOrDefault(ni => ni.GetIPProperties().UnicastAddresses.OfType<UnicastIPAddressInformation>()
                                                                    .Any(x => x.Address.Equals(ip)));

        if (targetInterface != null)
        {
            var gates = targetInterface.GetIPProperties().GatewayAddresses;

            if (gates.Count != 0)
            {
                foreach (var gateAddress in gates)
                    currentGates = gateAddress.Address;
            }
        }

        return currentGates;
    }




    /// <summary>
    /// Определяет по маске максимальное количество подключенных устройств
    /// </summary>
    /// <param name="mask"></param>
    /// <returns></returns>
    public static int MaskToInt(IPAddress mask)
    {
        int zeroCount = 0;

        foreach (byte b in mask.GetAddressBytes())
        {
            byte cloneB = b;

            while (cloneB % 2 == 0 && cloneB != 0)
            {
                zeroCount++;
                cloneB /= 2;
            }
            if (b == 0)
                zeroCount += 8;
        }

        return (int)Math.Pow(2, zeroCount);
    }





    /// <summary>
    /// Возвращает следующий адрес за указанным
    /// </summary>
    /// <param name="IP">Изначальный IP</param>
    /// <returns>Следующий IP</returns>
    public static IPAddress IncrementIP(IPAddress IP)
    {
        byte[] byteIP = IP.GetAddressBytes();


        for (int i = 3; i >= 0; i--)
        {
            if (byteIP[i] < 255)
            {
                byteIP[i]++;
                break;
            }
            else
                byteIP[i]++;
        }

        return new IPAddress(byteIP);
    }

}
