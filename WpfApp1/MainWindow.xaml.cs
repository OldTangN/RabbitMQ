using CellLine.Msg;
using Newtonsoft.Json;
using RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
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
                server = new ServerMQ("47.104.141.161", "admin", "QAZqaz01", "1线主控管理", "1线下行交换机");
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
                client = new ClientMQ("47.104.141.161", "yuanqimq", "QAZqaz01", "1线上行交换机", "1线下行交换机", "1线PLC管理");
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

            ExecuteMsg execMsg = new ExecuteMsg()
            {
                DATA = new List<RltData>() { new RltData() { Bar_code = "d123" } },
                DEVICE_TYPE = "E015",
                NO = "E01501",
            };
            strMsg = JsonConvert.SerializeObject(execMsg);
            server.SendMessage(strMsg, txtRouteKey.Text);
        }

        private void BtnClientSend_Click(object sender, RoutedEventArgs e)
        {
            //PLCMsg msg = new PLCMsg()
            //{
            //    NO = "DM101",
            //    STATUS = 1,
            //};

            //BarcodeMsg msg = new BarcodeMsg("E00101")
            //{

            //};

            HeartBeatMsg msg = new HeartBeatMsg()
            {
                DEVICE_TYPE = "E003",
                //MESSAGE_TYPE = "heart",//
                NO = "E003011",
                STATUS = 1,
                time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            string strMsg = JsonConvert.SerializeObject(msg);

            client.SendMessage(strMsg);
        }

        private async void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            ShowLog("Call2000");
            await GetRlt2000();
            ShowLog("Call4000");
            await GetRlt4000();
        }

        private async Task<bool> GetRlt4000()
        {
            return await Task.Run<bool>(() =>
            {
                Thread.Sleep(4000);
                ShowLog("4000ms End");
                return true;
            });
        }

        private async Task<bool> GetRlt2000()
        {
            return await Task.Run<bool>(() =>
            {
                Thread.Sleep(2000);
                ShowLog("2000ms End");
                return true;
            });
        }
    }
}
