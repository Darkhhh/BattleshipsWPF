using System.Windows.Controls;

namespace BattleshipsWpf.Pages;


public partial class EndGamePage : UserControl
{
    #region Actions

    public Action? GameRestarted;

    public Action? BackToMenu;

    #endregion


    public EndGamePage() => InitializeComponent();

    public EndGamePage(string result)
    {
        InitializeComponent();
        endGameTextBlock.Text = result;
    }


    #region Button Handlers

    private void restartButton_Click(object sender, RoutedEventArgs e) => GameRestarted?.Invoke();

    private void backToMenuButton_Click(object sender, RoutedEventArgs e) => BackToMenu?.Invoke();

    #endregion
}
