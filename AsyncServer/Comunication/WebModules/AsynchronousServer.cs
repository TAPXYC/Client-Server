using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class AsynchronousServer
{
    /// <summary>
    /// Событие запуска сервера
    /// </summary>
    public event Action OnStartServer;

    /// <summary>
    /// Событие получения сообщения
    /// </summary>
    public event Action<RemotePointInfo, string> OnRecieveMessage;

    /// <summary>
    /// Событие остановки сервера
    /// </summary>
    public event Action<Exception> OnStopServer;


    /// <summary>
    /// Сигнал потоков 
    /// </summary>
    ManualResetEvent AcceptConnection = new ManualResetEvent(false);

    private IPEndPoint LocalEndPoint;
    private Socket Listener;

    private bool _isActive;



    public AsynchronousServer(int port)
    {
        //Создаем локальную точку доступа
        IPAddress ipAddress = WebInfo.GetIP();
        LocalEndPoint = new IPEndPoint(ipAddress, port);

        // Создаем TCP/IP сокет  
        Listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        Listener.Bind(LocalEndPoint);
    }





    /// <summary>
    /// Начало прослушки
    /// </summary>
    public void StartListening()
    {
        _isActive = true;
        Exception stopServerEx = null;

        Task.Run(() =>
        {
            OnStartServer?.Invoke();

            // Привязываем сокет к локальной точке для входящих подключений  
            try
            {
                Listener.Listen(100);

                while (_isActive)
                {
                    // Ставим событие в несигнализированное состояние 
                    AcceptConnection.Reset();

                    // Ставим сокет в состояние ожидания подключений 
                    Listener.BeginAccept(new AsyncCallback(AcceptCallback), Listener);

                    // Останавливаем поток, пока никто не подключился  
                    AcceptConnection.WaitOne();
                }

            }
            catch (Exception e)
            {
                stopServerEx = e;
            }

            OnStopServer?.Invoke(stopServerEx);
        });
    }




    /// <summary>
    /// Останавливает сервер
    /// </summary>
    public void StopServer()
    {
        _isActive = false;
        AcceptConnection.Set();

        Listener.Shutdown( SocketShutdown.Both);
        Listener.Close();
    }





    /// <summary>
    /// Прием входящего подключения
    /// </summary>
    /// <param name="ar">Состояние асинхронной операции</param>
    private void AcceptCallback(IAsyncResult ar)
    {
        // Говорим главному потоку о том, что приняли подключение, и возобновляем его  
        AcceptConnection.Set();

        // Получаем через асинхронный результат сокет прослушки и сокет для работы с клиентом
        Socket listener = (Socket)ar.AsyncState;

        //Console.WriteLine(listener.RemoteEndPoint.ToString());

        Socket handler = listener.EndAccept(ar);

        // Создаем объект состояния  
        StateObject state = new StateObject();

        //Рабочий сокет - подключившийся клиент. Запоминаем его для дальнейших операций
        state.WorkSocket = handler;

        handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
    }





    /// <summary>
    /// Асинхронный прием данных
    /// </summary>
    /// <param name="ar">Состояние асинхронной операции</param>
    private void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;

        // Снова получаем клиента из объекта состояний 
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.WorkSocket;

        // Считываем данные с клиента
        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0)
        {
            // Сохраняем все данные в стрингбилдер 
            state.StringBuilder.Append(Encoding.UTF8.GetString(state.Buffer, 0, bytesRead));

            // Проверяем, закончилось ли сообщение
            content = state.StringBuilder.ToString();

            //Если есть слово окончания, то прекращаем прием данных и отправляем ответ
            if (content.IndexOf("<EOF>") > -1)
            {
                var RemotePointInfo = new RemotePointInfo();
                RemotePointInfo.RemoteIP = (handler.RemoteEndPoint as IPEndPoint).Address;

                OnRecieveMessage?.Invoke(RemotePointInfo, content);

                // Отправляем ответ клиенту 
                string Answer = "Ok";

                AsyncSocketAction.Send(handler, Answer, needCloseHandler: true);
            }
            else
            {
                // Иначе не все данные приняты, продолжаем прием  
                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
        }
        else
        {
            Console.WriteLine("ПОСТАВЬТЕ КТО-НИБУДЬ СИМВОЛ ОКОНЧАНИЯ ЭТОМУ КЛИЕНТУ!");
        }
    }
}
