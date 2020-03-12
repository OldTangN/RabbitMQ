using MyLogLib;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
namespace RabbitMQ
{
    public abstract class RabbitMQBase
    {
        /// <summary>
        /// 接收消息回调
        /// </summary>
        public event Action<string> OnReceived;

        protected readonly IConnection connection;
        protected readonly IModel channel;

        public string MqHost { get; private set; }
        public string MqUserName { get; private set; }
        public string MqPassword { get; private set; }
       
        protected RabbitMQBase(string mqHost, string mqUserName, string mqPwd)
        {
            this.MqHost = mqHost;
            this.MqUserName = mqUserName;
            this.MqPassword = mqPwd;
            
           
            connection = CreateConnection();
            channel = connection.CreateModel();
        }

        /// <summary>
        /// 创建一个IConnection。
        /// </summary>
        /// <returns></returns>
        internal IConnection CreateConnection()
        {
            const ushort hearbeat = 60;
            var factory = new ConnectionFactory()
            {
                HostName = MqHost,
                UserName = MqUserName,
                Password = MqPassword,
                //Port = int.Parse(MQConfig.MqPort),
                //心跳超时时间
                RequestedHeartbeat = hearbeat,
                AutomaticRecoveryEnabled = true//自动重连

            };
            IConnection connection = null;
            try
            {
                connection = factory.CreateConnection();
                return connection;
            }
            catch (Exception e)
            {
                MyLog.WriteLog("创建MQ连接失败！", e.Message);
                Thread.Sleep(2000);
                CreateConnection();
            }
            //创建连接对象
            return connection;
        }

        /// <summary>
        /// 创建一个Model
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        internal IModel CreateModel(IConnection connection)
        {
            return connection.CreateModel();
        }


        /// <summary>
        /// 客户端发送上行消息
        /// </summary>
        public abstract void SendMessage(string message);

        /// <summary>
        /// 服务端发送下行消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="routing_key">路由键</param>
        public abstract void SendMessage(string message, string routing_key);

        /// <summary>
        /// 客户端读取下行消息 or 服务端读取上行消息
        /// </summary>
        /// <param name="queue"></param>
        public abstract void ReceiveMessage();

        protected void consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;
            var message = Encoding.UTF8.GetString(body);
            try
            {
                channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                MyLog.WriteLog("BasicAck异常！", ex);
            }
            try
            {
                OnReceived?.Invoke(message);
            }
            catch (Exception ex)
            {
                MyLog.WriteLog("OnReceived回调异常！", ex);
            }
        }

        /// <summary>
        /// 成功发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Channel_BasicAcks(object sender, BasicAckEventArgs e)
        {
            //e.DeliveryTag是一个整数类型的消息传递标记，在同一个channel中每次psuh消息后该值都会自增
            //Trace.WriteLine("消息已成功送达：" + e.DeliveryTag);
        }
        /// <summary>
        /// 发送失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Model_BasicNacks(object sender, BasicNackEventArgs e)
        {
            //Trace.WriteLine("消息已送达失败：" + e.DeliveryTag);
        }
        /// <summary>
        /// 关闭通道 
        /// </summary>
        public void Close()
        {
            channel.Dispose();
            channel.Close();
            connection.Close();
            connection.Dispose();
        }
    }
}
