using Battleships;
using BattleshipsWpf.GameSource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BattleshipsWpf.Pages;


public partial class GamePage : UserControl
{
    #region Private Values

    private GameBoard _playerBoard = new GameBoard();

    private const int BoardSize = 10;

    private Dictionary<(int, int), Button> _boardButtons = new Dictionary<(int, int), Button>(BoardSize * BoardSize);

    private bool _showingPlayerBoard = false;

    private bool _playerTurn = false;

    private int _successfulShots = 0;

    #endregion


    #region Public Values

    public Action<bool>? GameOver;

    #endregion


    #region Constructors

    public GamePage()
    {
        InitializeComponent();        
        _playerBoard.AutomaticGeneration();
        CreateBoard();

        WaitForTheTurn();
    }

    private async void WaitForTheTurn()
    {
        await MainWindow.TcpClient.ConnectToServerAsync();

        var turn = await MainWindow.TcpClient.GetTurnAsync();
        if (turn == Turn.First)
        {
            _playerTurn = true;
            gameStatusTextBlock.Text = "Ваш ход!";
        }
        else
        {
            _playerTurn = false;
            gameStatusTextBlock.Text = "Ожидание хода противника...";
            WaitForTheOpponent();
        }
    }

    #endregion


    #region Board Handling

    private void CreateBoard()
    {
        var str = "ABCDEFGHIJ";

        foreach (var chr in str)
        {
            var textBlock = new TextBlock() { Text = chr.ToString(), Style = FindResource("BoardText") as Style };
            boardUniformGrid.Children.Add(textBlock);
        }

        for (int i = 0; i < BoardSize; i++)
        {
            boardUniformGrid.Children.Add(new TextBlock() { Text = i.ToString(), Style = FindResource("BoardText") as Style });
            for (int j = 0; j < BoardSize; j++)
            {
                var button = new Button() { Style = FindResource("MaterialDesignOutlinedButton") as Style, Content = " ", Width = 40 };
                button.Click += OpponentBoardButtonClick;
                _boardButtons.Add((i, j), button);
                boardUniformGrid.Children.Add(button);
            }
        }
    }

    private void switchBoardsButton_Click(object sender, RoutedEventArgs e)
    {
        BoardValue[,] vals;
        if (!_showingPlayerBoard)
        {
            vals = _playerBoard.GetBoardValues();
            switchBoardsButton.Content = "Доска противника";
        }
        else
        {
            vals = _playerBoard.OpponentsBoard.Board;
            switchBoardsButton.Content = "Моя доска";
        }
        RefreshBoard(vals, !_showingPlayerBoard);
        _showingPlayerBoard = !_showingPlayerBoard;
    }

    private void OpponentBoardButtonClick(object sender, EventArgs e)
    {
        if (!_playerTurn) return;

        var clickedButton = (Button)sender;

        (int, int) clickedPosition = (-1, -1);

        foreach (var item in _boardButtons)
        {
            if (clickedButton != item.Value) continue;

            clickedPosition = item.Key;
            break;
        }

        HandleClickedPosition(clickedPosition);
    }

    private void RefreshBoard(BoardValue[,] vals, bool disableButtons)
    {
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                string s = "";
                if (vals[i, j] == BoardValue.None) s = "";
                if (vals[i, j] == BoardValue.ActiveShipPart) s = "S";
                if (vals[i, j] == BoardValue.Missed) s = "o";
                if (vals[i, j] == BoardValue.DestroyedShipPart) s = "$";

                //if (s == "$" || s == "o" || disableButtons) _boardButtons[(i, j)].IsEnabled = false;
                if (s == "$" || s == "o" || s == "S" || disableButtons) _boardButtons[(i, j)].IsEnabled = false;
                else _boardButtons[(i, j)].IsEnabled = true;
                _boardButtons[(i, j)].Content = s;
            }
        }
    }

    #endregion


    #region Server Communication

    private async void HandleClickedPosition((int, int) clickedPosition)
    {
        MainWindow.TcpClient.SendToServer(clickedPosition);
        var result = await MainWindow.TcpClient.GetResultAsync();

        if (result == ShotResult.Missed)
        {
            _playerBoard.OpponentsBoard.MissedShot(clickedPosition);
            RefreshBoard(_playerBoard.OpponentsBoard.Board, false);
            gameStatusTextBlock.Text = "Мимо! Ход противника...";
            _playerTurn = false;
            WaitForTheOpponent();
        }
        else
        {
            _successfulShots++;
            if (_successfulShots > 19) GameOver?.Invoke(true);

            _playerBoard.OpponentsBoard.InTargetShot(clickedPosition);
            RefreshBoard(_playerBoard.OpponentsBoard.Board, false);
            var resultString = result == ShotResult.Shotted ? "Попал" : "Убил";
            gameStatusTextBlock.Text = $"{resultString}! Ваш ход!";
            _playerTurn = true;
        }
    }


    private async void WaitForTheOpponent()
    {
        Thread.Sleep(200);
        var shotCoordinates = await MainWindow.TcpClient.GetCoordinatesAsync();
        var shot = _playerBoard.Shot(shotCoordinates, out var killed);
        //RefreshBoard(_playerBoard.GetBoardValues(), false);

        if (!shot)
        {
            MainWindow.TcpClient.SendToServer(ShotResult.Missed);
            gameStatusTextBlock.Text = "Ваш ход!";
            _playerTurn = true;
        }
        else
        {
            if (!killed) MainWindow.TcpClient.SendToServer(ShotResult.Shotted);
            else MainWindow.TcpClient.SendToServer(ShotResult.Killed);

            if(!_playerBoard.IsAnyActiveShipsLeft) GameOver?.Invoke(false);

            gameStatusTextBlock.Text = "Ожидание хода противника...";
            _playerTurn = false;
            if (_playerBoard.IsAnyActiveShipsLeft) WaitForTheOpponent();
        }
    }

    #endregion
}
