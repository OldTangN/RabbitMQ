using Newtonsoft.Json;
using RabbitMQ;
using RabbitMQ.YQMsg;
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
            Environment.Exit(0);
        }

        private ServerMQ server;
        private void BtnStartServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                server?.Close();
                server = new ServerMQ("47.104.141.161", "admin", "QAZqaz01", "产线数据队列", "生产控制交换机");
                server.OnReceived += Server_OnReceived;
                server.ReceiveMessage();
                ShowLog("服务端已启动！");
            }
            catch (Exception ex)
            {
                ShowLog(ex.Message);
            }           
        }

        private void Server_OnReceived(string data)
        {
            ShowLog("服务端接收：" + data);
            var msg = JsonConvert.DeserializeObject<MsgBase>(data);
            switch (msg.MESSAGE_TYPE)
            {
                case "heart":
                    var heartMsg = JsonConvert.DeserializeObject<HeartBeatMsg>(data);
                    ShowLog($"设备编号:{heartMsg.NO}，心跳状态:{heartMsg.STATUS}，时间戳:{heartMsg.time_stamp}");
                    break;
                default:
                    break;
            }
        }
        private ClientMQ client;
        private void BtnStartClient_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                client?.Close();
                client = new ClientMQ("47.104.141.161", "yuanqimq", "QAZqaz01", "产线数据交换机", "生产控制交换机", "PLC");
                client.OnReceived += Client_OnReceived;
                client.ReceiveMessage();
                ShowLog("客户端已启动！");
            }
            catch (Exception ex)
            {
                ShowLog(ex.Message);
            }
        }

        private void Client_OnReceived(string data)
        {
            ShowLog("客户端接收：" + data);

            var msg = JsonConvert.DeserializeObject<MsgBase>(data);
            switch (msg.MESSAGE_TYPE)
            {
                case "task":
                    var tskMsg = JsonConvert.DeserializeObject<TaskMsg>(data);
                    ShowLog($"设备编号:{tskMsg.NO}，物料号:{tskMsg.DATA.MATERIEL_CODE}，时间戳:{tskMsg.time_stamp}");
                    break;
                default:
                    break;
            }
        }

        private void ShowLog(string log)
        {
            rtxtMsg.Dispatcher.Invoke(new Action<string>((str) =>
            {
                rtxtMsg.AppendText(str + "\r");
            }), log);
        }

        private void BtnServerSend_Click(object sender, RoutedEventArgs e)
        {
            TaskMsg heartMsg = new TaskMsg()
            {
                DEVICE_TYPE = "A",
                //MESSAGE_TYPE = "heart",//
                NO = "A1",
                DATA = new TaskData()
                {
                    MATERIEL_CODE = "EEEEEEE",
                    ORDER_NO = "123123",
                    WORKORDER_CODE = "A2S2D3F4F4"
                },
                time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            string strMsg = JsonConvert.SerializeObject(heartMsg);
            server.SendMessage(strMsg, txtRouteKey.Text);
        }

        private void BtnClientSend_Click(object sender, RoutedEventArgs e)
        {
            HeartBeatMsg heartMsg = new HeartBeatMsg()
            {
                DEVICE_TYPE = "A",
                //MESSAGE_TYPE = "heart",//
                NO = "A1",
                STATUS = "1",
                time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            string strMsg = JsonConvert.SerializeObject(heartMsg);
            client.SendMessage(strMsg);
        }
    }
}
