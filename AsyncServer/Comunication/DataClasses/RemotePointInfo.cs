using System.Net;



public class RemotePointInfo
{
    /// <summary>
    /// Удаленная точка, куда отправляется сообщение
    /// </summary>
    public IPAddress RemoteIP;

    /// <summary>
    /// Имя удаленного компьютера
    /// </summary>
    public string MachineName => Dns.GetHostEntry(RemoteIP.ToString()).HostName;
}

