using System;



/// <summary>
/// Класс для обмена сообщений между двумя машинами
/// </summary>
class Communication
{
    #region события

    public event Action OnStartServer;
    public event Action<RemotePointInfo, string> OnServerRecieveMessage;
    public event Action<Exception> OnStopServer;

    /// <summary>
    /// Событие когда ДНС не может дать IP по имени устройства
    /// </summary>
    public event Action<string> OnClientNameNotFound;
    public event Action<RemotePointInfo> OnClientTimeout;
    public event Action<RemotePointInfo, string> OnClientRecieveAnswer;

    #endregion

    private int Port;
    private float _timeToCancelMessage;


    AsynchronousServer asynchronousServer;



    /// <summary>
    /// 
    /// </summary>
    /// <param name="port">Прослушиваемый порт</param>
    /// <param name="timeToCancelMessage">Время в секундах до отмены отправки сообщения</param>
    public Communication(int port, float timeToCancelMessage = 2)
    {
        Port = port;
        _timeToCancelMessage = timeToCancelMessage;
    }




    #region Client control

    /// <summary>
    /// Отправка сообщения заданной машине
    /// </summary>
    /// <param name="message"></param>
    /// <param name="otherMachineName"></param>
    public void SendMessage(string message, string otherMachineName)
    {
        AsynchronousClient client = new AsynchronousClient(Port, otherMachineName, _timeToCancelMessage);
        client.OnSendTimeout += Client_OnSendTimeout;
        client.OnRecieveAnswer += Client_OnRecieveAnswer;
        client.OnNameNotFound += Client_OnNameNotFound;
        client.StartClient(message);
    }

    #region обработчики событий клиента

    private void Client_OnNameNotFound(string remoteMachineName)
    {
        OnClientNameNotFound?.Invoke(remoteMachineName);
    }

    private void Client_OnSendTimeout(RemotePointInfo remotePointInfo)
    {
        OnClientTimeout?.Invoke(remotePointInfo);
    }


    private void Client_OnRecieveAnswer(RemotePointInfo remotePointInfo, string answer)
    {
        OnClientRecieveAnswer?.Invoke(remotePointInfo, answer);
    }

    #endregion

    #endregion



    #region Server control

    /// <summary>
    /// Создать сервер
    /// </summary>
    public void CreateServer()
    {
        if (asynchronousServer == null)
        {
            asynchronousServer = new AsynchronousServer(Port);
            asynchronousServer.OnStartServer += Server_OnStartServer;
            asynchronousServer.OnRecieveMessage += Server_OnRecieveMessage;
            asynchronousServer.OnStopServer += Server_OnStopServer;
        }

        asynchronousServer.StartListening();
    }


    #region обработчики событий сервера

    private void Server_OnStartServer()
    {
        OnStartServer?.Invoke();
    }



    private void Server_OnStopServer(Exception ex)
    {
        OnStopServer?.Invoke(ex);
    }



    private void Server_OnRecieveMessage(RemotePointInfo remotePointInfo, string message)
    {
        OnServerRecieveMessage?.Invoke(remotePointInfo, message);
    }

    #endregion


    /// <summary>
    /// Отключение сервера
    /// </summary>
    public void ShutdownServer()
    {
        asynchronousServer.StopServer();
    }


    #endregion

}