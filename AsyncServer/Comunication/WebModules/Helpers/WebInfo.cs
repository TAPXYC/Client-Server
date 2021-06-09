using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

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
}
