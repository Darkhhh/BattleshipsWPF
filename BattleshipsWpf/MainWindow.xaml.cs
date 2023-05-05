global using BattleshipsWpf.Pages;
global using BattleshipsWpf.GameSource;
global using System.Collections.Generic;
global using System;
global using System.Windows;
global using System.Linq;

using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using Battleships;


namespace BattleshipsWpf;


public partial class MainWindow : Window
{
    #region Pages

    private ConnectionPage _connectionPage = new ConnectionPage();

    private GenerationPage _generationPage = new GenerationPage();

    private EndGamePage _endGamePage = new EndGamePage();

    #endregion


    public static PlayerTcpClient TcpClient = new PlayerTcpClient();


    public MainWindow()
    {
        InitializeComponent();
        SubscribeToEvents();

        //mainWindowPage.Content = _generationPage;
        //mainWindowPage.Content = _connectionPage;
        var gamePage = new GamePage();
        gamePage.GameOver += GameOver;
        mainWindowPage.Content = gamePage;
    }


    #region Event Handlers

    private void SubscribeToEvents()
    {
        _connectionPage.StartConnection += ConnectionRequired;
        _generationPage.GameStarted += GenerationFinished;

        _endGamePage.GameRestarted += RestartGame;
        _endGamePage.BackToMenu += GoBackToMenu;
    }


    #region Connection Page

    private async void ConnectionRequired(IPAddress ip, int port)
    {
        // Get ip and port, connect to the server (ask if available)
        try
        {
            await TcpClient.ConnectToServerAsync(ip, port);

            mainWindowPage.Content = _generationPage;
            _connectionPage.ServerConnectionMessageTextBlock.Visibility = Visibility.Hidden;
        }
        catch
        {
            _connectionPage.ServerConnectionMessageTextBlock.Visibility = Visibility.Visible;
        }       
    }

    #endregion


    #region Generation Page

    private void GenerationFinished(GameBoard board)
    {
        // Save board for next step
    }

    #endregion


    #region Game Page

    private void GameOver(bool playerWin)
    {
        if (playerWin) mainWindowPage.Content = new EndGamePage("Победа!");
        else mainWindowPage.Content = new EndGamePage("Поражение!");
    }

    #endregion


    #region EndGame Page

    private void RestartGame()
    {
        mainWindowPage.Content = _generationPage;
    }

    private void GoBackToMenu()
    {
        // Disconnect from server
        mainWindowPage.Content = _connectionPage;
    }

    #endregion

    #endregion
}
