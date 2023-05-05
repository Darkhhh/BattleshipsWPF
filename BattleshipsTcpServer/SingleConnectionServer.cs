using System.Net;
using System.Net.Sockets;

namespace BattleshipsTcpServer;

public class SingleConnectionServer
{
    private readonly TcpListener _tcpListener;
    private TcpClient? _tcpClient;
    private NetworkStream? _playerStream;
    
    public SingleConnectionServer(string ip = "127.0.0.1", int port = 8888)
    {
        _tcpListener = new TcpListener(IPAddress.Parse(ip), port);
    }

    #region Starting And Stopping Server

    public async Task WaitForClientAsync()
    {
        while (true)
        {
            _tcpClient = await _tcpListener.AcceptTcpClientAsync();
            if (_tcpClient.Connected)
            {
                _playerStream = _tcpClient.GetStream();
                break;
            }
        }
    }

    public void Stop() => _tcpListener.Stop();

    public void Start() => _tcpListener.Start();

    #endregion


    #region Senders

    public async Task SendTurnAsync(Turn val)
    {
        var turn = new[] { (byte)Indicators.TurnIndicator, (byte)val };
        await _playerStream!.WriteAsync(turn);
    }

    public async Task SendResultAsync(ShotResult result)
    {
        var message = new[] { (byte)Indicators.ResultIndicator, (byte)result };
        await _playerStream!.WriteAsync(message);
    }

    public async Task SendCoordinates((int, int) coordinates)
    {
        var message = new []{(byte)coordinates.Item1, (byte)coordinates.Item2};
        await _playerStream!.WriteAsync(message);
    }

    #endregion
    

    #region Getters

    public async Task<(int, int)> GetCoordinates()
    {
        var responseData = new byte[2];
        while (true)
        {
            var bytes = await _playerStream!.ReadAsync(responseData);
            if (bytes > 0) break;
        }
        return (responseData[0], responseData[1]);
    }

    public async Task<ShotResult> GetShotResult()
    {
        var responseData = new byte[2];
        var bytes = await _playerStream!.ReadAsync(responseData);
        if (responseData[0] != (byte)Indicators.ResultIndicator)
            throw new Exception("Некорректный формат ответа от сервера");
        return (ShotResult)responseData[1];
    }

    #endregion


    #region Static

    public static async Task Play()
    {
        var server = new SingleConnectionServer();
        var randomGenerator = new Random();
        var gameBoard = new GameBoard();

        var counter = 0;
        try
        {
            gameBoard.AutomaticGeneration();
            server.Start();
            Console.WriteLine("Сервер запущен. Ожидание подключений... ");
            await server.WaitForClientAsync();
            Console.WriteLine("Клиент подключен. Отправляем очередность...");
            await server.SendTurnAsync(Turn.First);
            Console.WriteLine("Очередность доставлена. Ожидание координат...\n");
        
            while (true)
            {
                var coordinates = await server.GetCoordinates();
                Console.WriteLine($"Получены координаты: {coordinates}");
                Thread.Sleep(500);
                var shotResult = gameBoard.Shot(coordinates, out var killed);
                Console.WriteLine($"Результат выстрела игрока: {shotResult.ToString()}");
        
                var res = ShotResult.Missed;
                if (shotResult && killed) res = ShotResult.Killed;
                if (shotResult && !killed) res = ShotResult.Shotted;
        
                await server.SendResultAsync(res);

                if (!gameBoard.IsAnyActiveShipsLeft || counter > 19) break;
        
                if (shotResult) continue;
        
                while (true)
                {
                    var computerShot = (randomGenerator.Next(10), randomGenerator.Next(10));
                    await server.SendCoordinates(computerShot);
        
                    var playerShotResult = await server.GetShotResult();
                    Console.WriteLine($"Результат выстрела компьютера: {playerShotResult.ToString()}");

                    if (playerShotResult is ShotResult.Killed or ShotResult.Shotted)
                    {
                        counter++;
                        continue;
                    }
                    
                    break;
                }
            }

            if (!gameBoard.IsAnyActiveShipsLeft)
            {
                Console.WriteLine("Сервер проиграл!");
            }

            if (counter > 19)
            {
                Console.WriteLine("Сервер Выиграл!");
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