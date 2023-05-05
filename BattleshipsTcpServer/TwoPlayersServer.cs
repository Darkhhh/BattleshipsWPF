using System.Net;
using System.Net.Sockets;

namespace BattleshipsTcpServer;

public class TwoPlayersServer
{
    private readonly TcpListener _tcpListener;
    private TcpClient? _tcpClient1, _tcpClient2;
    public NetworkStream? PlayerStream1 { get; set; }
    public NetworkStream? PlayerStream2 { get; set; }
    
    public TwoPlayersServer(string ip = "127.0.0.1", int port = 8888)
    {
        _tcpListener = new TcpListener(IPAddress.Parse(ip), port);
    }

    #region Starting And Stopping Server

    public async Task WaitForClientAsync()
    {
        while (true)
        {
            _tcpClient1 = await _tcpListener.AcceptTcpClientAsync();
            _tcpClient2 = await _tcpListener.AcceptTcpClientAsync();
            if (_tcpClient1.Connected) PlayerStream1 = _tcpClient1.GetStream();
            if (_tcpClient2.Connected) PlayerStream2 = _tcpClient2.GetStream();

            if (_tcpClient1.Connected && _tcpClient2.Connected) break;
        }
    }

    public void Stop() => _tcpListener.Stop();

    public void Start() => _tcpListener.Start();

    #endregion
    
    
    #region Senders

    public async Task SendTurnAsync(Turn val, NetworkStream playerStream)
    {
        var turn = new[] { (byte)Indicators.TurnIndicator, (byte)val };
        await playerStream!.WriteAsync(turn);
    }

    public async Task SendResultAsync(ShotResult result, NetworkStream playerStream)
    {
        var message = new[] { (byte)Indicators.ResultIndicator, (byte)result };
        await playerStream!.WriteAsync(message);
    }

    public async Task SendCoordinates((int, int) coordinates, NetworkStream playerStream)
    {
        var message = new []{(byte)coordinates.Item1, (byte)coordinates.Item2};
        await playerStream!.WriteAsync(message);
    }

    #endregion
    

    #region Getters

    public async Task<(int, int)> GetCoordinates(NetworkStream playerStream)
    {
        var responseData = new byte[2];
        while (true)
        {
            var bytes = await playerStream!.ReadAsync(responseData);
            if (bytes > 0) break;
        }
        return (responseData[0], responseData[1]);
    }

    public async Task<ShotResult> GetShotResult(NetworkStream playerStream)
    {
        var responseData = new byte[2];
        var bytes = await playerStream!.ReadAsync(responseData);
        if (responseData[0] != (byte)Indicators.ResultIndicator)
            throw new Exception("Некорректный формат ответа от сервера");
        return (ShotResult)responseData[1];
    }
    
    #endregion
    
    
    #region Static

    public static async Task Play()
    {
        var server = new TwoPlayersServer();
        var counter1 = 0; 
        var counter2 = 0;
        try
        {
            server.Start();
            Console.WriteLine("Сервер запущен. Ожидание подключений... ");
            await server.WaitForClientAsync();
            Console.WriteLine("Клиенты подключены. Отправляем очередность...");
            if (server.PlayerStream1 != null) await server.SendTurnAsync(Turn.First, server.PlayerStream1);
            else throw new Exception("First player stream is not available");
            if (server.PlayerStream2 != null) await server.SendTurnAsync(Turn.Second, server.PlayerStream2);
            else throw new Exception("Second player stream is not available");
            Console.WriteLine("Очередность доставлена. Ожидание координат...\n");

            var actualPlayerStream = server.PlayerStream1;
            var actualOpponentStream = server.PlayerStream2;
        
            while (true)
            {
                var coordinates = await server.GetCoordinates(actualPlayerStream);
                Console.WriteLine($"Получены координаты: {coordinates}");
                Thread.Sleep(500);
                await server.SendCoordinates(coordinates, actualOpponentStream);
                var shotResult = await server.GetShotResult(actualOpponentStream);
                Console.WriteLine($"Результат выстрела игрока: {shotResult.ToString()}");

                await server.SendResultAsync(shotResult, actualPlayerStream);

                // Check for end game
                if (counter1 > 19 || counter2 > 19) break;

                if (shotResult != ShotResult.Missed)
                {
                    counter1++;
                    continue;
                }
        
                while (true)
                {
                    actualPlayerStream = server.PlayerStream2;
                    actualOpponentStream = server.PlayerStream1;
                    
                    coordinates = await server.GetCoordinates(actualPlayerStream);
                    Console.WriteLine($"Получены координаты: {coordinates}");
                    Thread.Sleep(500);
                    await server.SendCoordinates(coordinates, actualOpponentStream);
                    shotResult = await server.GetShotResult(actualOpponentStream);
                    Console.WriteLine($"Результат выстрела игрока: {shotResult.ToString()}");

                    await server.SendResultAsync(shotResult, actualPlayerStream);

                    if (shotResult is ShotResult.Killed or ShotResult.Shotted)
                    {
                        counter2++;
                        continue;
                    }
                    
                    actualPlayerStream = server.PlayerStream1;
                    actualOpponentStream = server.PlayerStream2;
                    
                    break;
                }
            }

            if (counter1 > 19)
            {
                Console.WriteLine("Первый игрок Выиграл!");
            }

            if (counter2 > 19)
            {
                Console.WriteLine("Второй игрок Выиграл!");
            }
        }
        catch
        {
            Console.WriteLine("Произошла ошибка...");
        }
        finally
        {
            Console.WriteLine("Сервер остановлен.");
            server.Stop();
        }
    }

    #endregion
}