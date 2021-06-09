
using System.Net.Sockets;
using System.Text;


/// <summary>
/// Объект состояний для асинхронных операций
/// </summary>
public class StateObject
{
    public const int BufferSize = 1024;


    /// <summary>
    /// Буфер приема
    /// </summary>
    public byte[] Buffer = new byte[BufferSize];

    /// <summary>
    /// Структура для считывания
    /// </summary>
    public StringBuilder StringBuilder = new StringBuilder();


    /// <summary>
    /// Рабочий сокет
    /// </summary>
    public Socket WorkSocket = null;
}

