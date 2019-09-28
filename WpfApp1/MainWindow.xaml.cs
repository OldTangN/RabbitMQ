using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using RabbitMQ;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            server?.Close();
        }

        private ServerMQ server;
        private void BtnStartServer_Click(object sender, RoutedEventArgs e)
        {
            server = new ServerMQ();
            server.singleArrivalEvent += Server_singleArrivalEvent;
            server.ReceiveMessage();
        }

        private void Server_singleArrivalEvent(string data)
        {
            rtxtMsg.Dispatcher.Invoke(new Action<string>((str) =>
            {
                rtxtMsg.AppendText(str + "\r");
            }), data);
        }
    }
}
