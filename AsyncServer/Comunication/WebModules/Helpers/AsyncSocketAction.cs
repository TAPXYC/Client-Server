using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class AsyncSocketAction
{



    #region Send region

    /// <summary>
    /// Информация об отправке сообщения
    /// </summary>
    struct SendState
    {
        /// <summary>
        /// Сигнальное событие потоку извне
        /// </summary>
        public ManualResetEvent ResetEvent;
        /// <summary>
        /// Сокет получателя (удаленный)
        /// </summary>
        public Socket WorkSocket;
        /// <summary>
        /// Нужно ли закрывать поток
        /// </summary>
        public bool NeedCloseHandler;

        public SendState(Socket socket, ManualResetEvent resetEvent, bool needCloseHandler)
        {
            WorkSocket = socket;
            ResetEvent = resetEvent;
            NeedCloseHandler = needCloseHandler;
        }
    }




    /// <summary>
    /// Отправка данных 
    /// </summary>
    /// <param name="remoteSocket">Удаленный сокет</param>
    /// <param name="data">Данные</param>
    /// <param name="resetEvent">Сигнальное событие потоку извне</param>
    /// <param name="needCloseHandler">Нужно ли закрывать поток</param>
    public static void Send(Socket remoteSocket, String data, ManualResetEvent resetEvent = null, bool needCloseHandler = false)
    {
        // Конвертируем сообщение в байтовый вид 
        byte[] byteData = Encoding.UTF8.GetBytes(data);

        SendState sendState = new SendState(remoteSocket, resetEvent, needCloseHandler);

        // Начинаем отправлять данные клиенту  
        remoteSocket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), sendState);
    }








    /// <summary>
    /// Асинхронная отправка сообщения
    /// </summary>
    /// <param name="ar">Состояние асинхронной операции</param>
    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            SendState sendState = (SendState)ar.AsyncState;

            // Получаем сокет от состояния аснхронной операции
            Socket handler = sendState.WorkSocket;

            // Завершаем отправку данных на удаленого клиента 
            int bytesSent = handler.EndSend(ar);

            if(sendState.ResetEvent != null)
                sendState.ResetEvent.Set();

            //Закрываем поток и очищаем его
            if (sendState.NeedCloseHandler)
            {
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    #endregion
}

