using System.Net;
using System.Windows.Controls;

namespace BattleshipsWpf.Pages;

public partial class ConnectionPage : UserControl
{
    public Action<IPAddress, int>? StartConnection;


    public ConnectionPage() => InitializeComponent();


    private void connectButton_Click(object sender, RoutedEventArgs e)
    {
        StartConnection?.Invoke(IPAddress.Parse(ipTextBox.Text), Convert.ToInt32(portTextBox.Text));
    }
}
