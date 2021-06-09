using System;





class Programm
{
    public static void Main()
    {
        int ServerPort = 11000;
        string otherMachineName = "DESKTOP-Q67Q8PO";

        Communication communication = new Communication(ServerPort);
        communication.CreateServer();

        communication.OnClientNameNotFound += i => Console.WriteLine($"[{i}]\tне в сети");
        communication.OnClientTimeout += i => Console.WriteLine($"[{i.RemoteIP}]\tтаймаут"); 

        communication.OnServerRecieveMessage += Print;
        communication.OnStopServer += e => Console.WriteLine("Stop server  " + e != null ? e.Message : "");

        while (true)
        {
            string message = Console.ReadLine();
            communication.SendMessage(message, otherMachineName);
        }
    }




    static void Print(RemotePointInfo remotePoint, string message)
    {
        Console.WriteLine($"[{remotePoint.RemoteIP}]\t" + message);
    }
}