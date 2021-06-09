using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Timers;
using System.Threading.Tasks;





public class AsynchronousClient
{
    /// <summary>
    /// Событие таймаута отправки собщения
    /// </summary>
    public event Action<RemotePointInfo> OnSendTimeout;


    /// <summary>
    /// Событие когда ДНС не может дать IP по имень устройства
    /// </summary>
    public event Action<string> OnNameNotFound;


    /// <summary>
    /// Событие получения ответа
    /// </summary>
    public event Action<RemotePointInfo, string> OnRecieveAnswer;


    /// <summary>
    /// Информация о конечной точке (туда, куда отправляем сообщение) 
    /// </summary>
    public RemotePointInfo remotePointInfo = new RemotePointInfo();


    #region Private

    /// <summary>
    /// Номер порта удаленного устройства.
    /// </summary>
    private int _port;

    /// <summary>
    /// Время в секундах до отмены отправки исходящего сообщения
    /// </summary>
    private float _timeToCancel = 3;

    /// <summary>
    /// Имеется ли подключение
    /// </summary>
    private bool _hasConnect = false;


    // Экземпляры ManualResetEvent, сигнализирующие о завершении.  
    private ManualResetEvent connectDone = new ManualResetEvent(false);
    private ManualResetEvent sendDone = new ManualResetEvent(false);
    private ManualResetEvent receiveDone = new ManualResetEvent(false);

    private Socket client;
    private IPEndPoint removeIP;

    private string remoteMachineName;
    private bool findRemoteIP = false;

    #endregion




    /// <summary>
    /// 
    /// </summary>
    /// <param name="serverPort">Прослушиваемый порт</param>
    /// <param name="otherMachineName">Имя удаленной машины</param>
    /// <param name="timeToCancel">Время до отмены отправки сообщения в секундах</param>
    public AsynchronousClient(int serverPort, string otherMachineName, float timeToCancel)
    {
        findRemoteIP = false;

        _port = serverPort;
        _timeToCancel = timeToCancel;
        IPAddress ServerIP = new IPAddress(0);

        try
        {
            ServerIP = WebInfo.GetIP(otherMachineName);
            findRemoteIP = true;
        }
        catch
        {
            remoteMachineName = otherMachineName;
        }

        //Запоминаем IP получателя (куда отправляем)
        remotePointInfo.RemoteIP = ServerIP;

        removeIP = new IPEndPoint(ServerIP, _port);
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="serverPort">Прослушиваемый порт</param>
    /// <param name="otherMachineName">IP удаленной машины</param>
    /// <param name="timeToCancel">Время до отмены отправки сообщения в секундах</param>
    public AsynchronousClient(int serverPort, IPAddress otherMachineIP, float timeToCancel)
    {
        findRemoteIP = true;

        _port = serverPort;
        _timeToCancel = timeToCancel;

        //Запоминаем IP получателя (куда отправляем)
        remotePointInfo.RemoteIP = otherMachineIP;

        removeIP = new IPEndPoint(otherMachineIP, _port);
    }







    /// <summary>
    /// Запуск клиента
    /// </summary>
    /// <param name="message">Сообщение для отправки</param>
    public void StartClient(string message)
    {
        if (!findRemoteIP)
        {
            OnNameNotFound?.Invoke(remoteMachineName);
            return;
        }

        // Подключение к удаленному устройству.
        try
        {
            Task.Run(() =>
            {
                // Создание TCP/IP socket.  
                client = new Socket(remotePointInfo.RemoteIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                //Запуск таймера, по истечении которого сообщение будет отклонено
                StartTimer();

                // Подключитесь к удаленной конечной точке. 
                client.BeginConnect(removeIP, ConnectCallback, client);
                connectDone.WaitOne();

                //Если не получилось установить соединение - 
                if (_hasConnect)
                {
                    // Отправьте тестовые данные на удаленное устройство.
                    AsyncSocketAction.Send(client, message + "<EOF>", sendDone);
                    sendDone.WaitOne();

                    // Получить ответ от удаленного устройства.
                    Receive(client);
                    receiveDone.WaitOne();

                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
                else
                {
                    OnSendTimeout?.Invoke(remotePointInfo);
                }
            });

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }



    /// <summary>
    /// Запуск таймера, по истечении которого сообщение будет отклонено
    /// </summary>
    private void StartTimer()
    {
        System.Timers.Timer timer = new System.Timers.Timer(_timeToCancel * 1000);
        timer.Elapsed += SendTimeout;
        timer.AutoReset = false;
        timer.Start();
    }



    /// <summary>
    /// Таймаут отправки сообщения
    /// </summary>
    private void SendTimeout(object sender, ElapsedEventArgs e)
    {
        client.Close();

        if (!_hasConnect)
            connectDone.Set();
    }





    /// <summary>
    /// Асинхронное соединение
    /// </summary>
    /// <param name="client">Сокет клиента (локальный)</param>
    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            // Завершаем запрос на подключение
            client.EndConnect(ar);

            _hasConnect = true;
            // Продолжаем главный поток
            connectDone.Set();
        }
        catch (Exception e)
        {
            //Console.WriteLine(e.ToString());
        }
    }




    #region Получение ответа

    /// <summary>
    /// Прием данных
    /// </summary>
    /// <param name="client">Сокет клиента (локальный)</param>
    private void Receive(Socket client)
    {
        try
        {
            // Create the state object.  
            StateObject state = new StateObject();
            state.WorkSocket = client;

            // Начинаем ассинхронно принимать сообщение  
            client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }







    /// <summary>
    /// Асинхронное получение данных
    /// </summary>
    /// <param name="ar">результат асинхронной операции</param>
    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            // Извлекаем объект состояния и клиентский сокет из объекта асинхронного состояния. 
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.WorkSocket;

            // Получаем количество полученных байт.  
            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.StringBuilder.Append(Encoding.UTF8.GetString(state.Buffer, 0, bytesRead));

                // Продолжаем считывать данные
                client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            else
            {
                // Все данные поступили; помещаем их в ответ.
                if (state.StringBuilder.Length > 1)
                {
                    OnRecieveAnswer?.Invoke(remotePointInfo, state.StringBuilder.ToString());
                }
                // Возобновляем главный поток
                receiveDone.Set();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    #endregion
}