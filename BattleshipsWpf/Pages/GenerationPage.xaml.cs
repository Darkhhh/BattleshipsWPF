using Battleships;
using BattleshipsWpf.GameSource;
using System.Windows.Controls;
using System.Windows.Media;

namespace BattleshipsWpf.Pages;


public partial class GenerationPage : UserControl
{
    #region Action

    public Action<GameBoard>? GameStarted { get; set; }

    #endregion



    #region Private Values

    private const int BoardSize = 10;

    private Dictionary<(int, int), TextBlock> _boardBlocks = new Dictionary<(int, int), TextBlock>(BoardSize * BoardSize);

    private GameBoard _gameBoard = new GameBoard();

    #endregion



    #region Constructors

    public GenerationPage()
    {
        InitializeComponent();
        GenerateBoard();
        RegenerateBoard();
    }

    #endregion



    #region BoardGeneration

    private void GenerateBoard()
    {
        var str = "ABCDEFGHIJ";

        foreach (var chr in str)
        {
            var textBlock = new TextBlock() { Text = chr.ToString(), Style = FindResource("BoardText") as Style };
            boardGrid.Children.Add(textBlock);
        }

        for (int i = 0; i < BoardSize; i++)
        {
            boardGrid.Children.Add(new TextBlock() { Text = i.ToString(), Style = FindResource("BoardText") as Style });
            for (int j = 0; j < BoardSize; j++)
            {
                Border borderTextBlock = new Border() { BorderThickness = new Thickness(0.5), BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0)) };
                var textBlock = new TextBlock() { Text = "_", Style = FindResource("BoardText") as Style };
                _boardBlocks.Add((i, j), textBlock);
                borderTextBlock.Child = textBlock;
                boardGrid.Children.Add(borderTextBlock);
            }
        }
    }

    private void RegenerateBoard()
    {
        _gameBoard.AutomaticGeneration();

        var vals = _gameBoard.GetBoardValues();

        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                string s = "";
                if (vals[i, j] == BoardValue.None) s = "";
                if (vals[i, j] == BoardValue.ActiveShipPart) s = "S";

                _boardBlocks[(i, j)].Text = s;
            }
        }
    }

    #endregion



    #region ClickHandlers

    private void generateButton_Click(object sender, RoutedEventArgs e) => RegenerateBoard();

    private void playButton_Click(object sender, RoutedEventArgs e) => GameStarted?.Invoke(_gameBoard);

    #endregion
}
