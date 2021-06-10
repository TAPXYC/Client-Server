using System.Timers;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System;

class Device
{
    public string Name
    {
        get;
        private set;
    }
    public IPAddress IP
    {
        get;
        private set;
    }


    public Device(string name, IPAddress iP)
    {
        Name = name;
        IP = iP;
    }
}






class ScanWeb
{
    public event Action<double> OnChangeScanProgress;



    private Semaphore sem;
    private int maxScanTask = 30;
    private int totalCount;
    private int currentScan;
    private List<Device> Names = new List<Device>();
    private ManualResetEvent scanComplete = new ManualResetEvent(false);


    public async Task<List<Device>> ScanByIP(IPAddress start, IPAddress end, Communicator communication)
    {
        currentScan = 0;
        totalCount = WebInfo.MaskToInt(end);
        sem = new Semaphore(maxScanTask, maxScanTask);
        IPAddress currentScanIP = start;

        communication.OnClientRecieveAnswer += Communication_OnClientRecieveAnswer;
        communication.OnClientTimeout += Communication_OnClientTimeout;

        await Task.Run(() =>
        {
            for (int i = 0; i < totalCount; i++)
            {
                sem.WaitOne();
                IPAddress currentIP = currentScanIP;

                communication.SendMessage("hello", currentIP);

                currentScanIP = WebInfo.IncrementIP(currentScanIP);
            }


            System.Timers.Timer waitTimer = new System.Timers.Timer(5000);
            waitTimer.Elapsed += (s, e) => scanComplete.Set();
            waitTimer.AutoReset = false;
            waitTimer.Start();

            scanComplete.WaitOne();
        });


        communication.OnClientRecieveAnswer -= Communication_OnClientRecieveAnswer;
        communication.OnClientTimeout -= Communication_OnClientTimeout;

        return Names;
    }



    private void Communication_OnClientTimeout(RemotePointInfo obj)
    {
        OneIPScanComplete();
    }

    private void Communication_OnClientRecieveAnswer(RemotePointInfo remoteInfo, string answer)
    {
        Names.Add(new Device(remoteInfo.MachineName, remoteInfo.RemoteIP));
        OneIPScanComplete();
    }


    private void OneIPScanComplete()
    {
        currentScan++;
        OnChangeScanProgress?.Invoke((double)currentScan / totalCount);
        sem.Release();
    }
}

