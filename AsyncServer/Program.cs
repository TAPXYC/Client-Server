using System;

class Programm
{
    public  static void Main()
    {
        ScanWeb scanWeb = new ScanWeb();
        scanWeb.OnChangeScanProgress += p => Console.WriteLine($"Scan {p * 100}%");

        int ServerPort = 11000;

        Communication communication = new Communication(ServerPort, 3);
        //communication.CreateServer();

        //communication.OnClientNameNotFound += i => Console.WriteLine($"[{i}]\tне в сети");
        //communication.OnClientTimeout += i => Console.WriteLine($"[{i.RemoteIP}]\tтаймаут");
        //communication.OnClientRecieveAnswer += (i,s) => Console.WriteLine($"[{i.RemoteIP}]\t {s}");


        //communication.OnServerRecieveMessage += Print;
        //communication.OnStopServer += e => Console.WriteLine("Stop server  " + e != null ? e.Message : "");

        var names = scanWeb.ScanByIP(WebInfo.GetGates(WebInfo.GetIP()), WebInfo.GetSubnetMask(WebInfo.GetIP()), communication);

        names.Wait();

        Console.WriteLine("Scan  Complete");
        foreach (var item in names.Result)
        {
            Console.WriteLine(item.Name + "      " + item.IP);
        }

        
    }






    static void Print(RemotePointInfo remotePoint, string message)
    {
        Console.WriteLine($"[{remotePoint.RemoteIP}]\t" + message);
    }
}