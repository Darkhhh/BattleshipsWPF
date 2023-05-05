using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace BattleshipsWpf.GameSource;


public class PlayerTcpClient
{
    #region Private Values

    private readonly TcpClient _tcpClient = new TcpClient();

    private NetworkStream? _stream;

    private const int MessageSize = 2;

    #endregion


    #region Connection With Server

    public async Task ConnectToServerAsync(string ip = "127.0.0.1", int port = 8888)
    {
        await _tcpClient.ConnectAsync(ip, port);
        if (_tcpClient.Connected)
        {
            Console.WriteLine($"Подключение с {_tcpClient.Client.RemoteEndPoint} установлено");
            _stream = _tcpClient.GetStream();
        }
        else
        {
            throw new Exception($"Подключение к серверу не удалось {ip}:{port}");
        }
    }

    public async Task ConnectToServerAsync(IPAddress ip, int port) => await ConnectToServerAsync(ip.ToString(), port);

    public void DisconnectFromServer()
    {
        _tcpClient.Dispose();
        _tcpClient.Close();
    }

    #endregion


    #region Send To Server

    public void SendToServer((int, int) coordinates)
    {
        var message = new byte[MessageSize];
        message[0] = (byte)coordinates.Item1;
        message[1] = (byte)coordinates.Item2;
        SendToServer(message);
    }

    public void SendToServer(ShotResult result)
    {
        var message = new byte[MessageSize];
        message[0] = (byte)Indicators.ResultIndicator;
        message[1] = (byte)result;
        SendToServer(message);
    }

    private async void SendToServer(byte[] message)
    {
        await _stream!.WriteAsync(message);
        Console.WriteLine($"Серверу {_tcpClient.Client.RemoteEndPoint} отправлены данные");
    }

    #endregion


    #region Get From Server

    public async Task<ShotResult> GetResultAsync()
    {
        var responseData = new byte[MessageSize];
        var bytes = await _stream!.ReadAsync(responseData);
        if (responseData[0] != (byte)Indicators.ResultIndicator)
            throw new Exception("Некорректный формат ответа от сервера");
        Console.WriteLine($"Полученный результат выстрела: {(ShotResult)responseData[1]}");
        return (ShotResult)responseData[1];
    }

    public async Task<(int, int)> GetCoordinatesAsync()
    {
        var responseData = new byte[MessageSize];
        var bytes = await _stream!.ReadAsync(responseData);
        if (responseData[0] == (byte)Indicators.ResultIndicator)
            throw new Exception("Некорректный формат ответа от сервера");
        Console.WriteLine($"Полученные координаты: {responseData}");
        return (responseData[0], responseData[1]);
    }

    public async Task<Turn> GetTurnAsync()
    {
        var responseData = new byte[MessageSize];
        var bytes = await _stream!.ReadAsync(responseData);
        if (responseData[0] != (byte)Indicators.TurnIndicator)
            throw new Exception("Некорректный формат ответа от сервера");
        Console.WriteLine($"Получен ход: {(Turn)responseData[1]}");
        return (Turn)responseData[1];
    }

    #endregion

}
