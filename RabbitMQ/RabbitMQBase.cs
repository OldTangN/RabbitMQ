using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
namespace RabbitMQ
{
    public abstract class RabbitMQBase
    {
        //委托
        public delegate void CodeArrivalhandle(string data);
        /// <summary>
        /// 接受数据
        /// </summary>
        public event CodeArrivalhandle CodeArrivalEvent;

        /// <summary>
        /// 单一队列
        /// </summary>
        /// <param name="data"></param>
        public delegate void SingleArrivalhandle(string data);
        /// <summary>
        /// 单一队列
        /// </summary>
        public event SingleArrivalhandle singleArrivalEvent;

        protected readonly IConnection connection;
        protected readonly IModel channel;

        protected static MqConfigDom MQConfig;

        protected RabbitMQBase()
        {
            connection = CreateConnection();
            channel = connection.CreateModel();
        }

        /// <summary>
        /// 创建一个IConnection。
        /// </summary>
        /// <returns></returns>
        internal static IConnection CreateConnection()
        {
            //获取MQ的配置
            MQConfig = CreateConfigDomInstance();
            const ushort hearbeat = 60;
            var factory = new ConnectionFactory()
            {
                HostName = MQConfig.MqHost,
                UserName = MQConfig.MqUserName,
                Password = MQConfig.MqPassword,
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
                Trace.WriteLine(e.Message);
                Log.Error(e.Message);
                Thread.Sleep(2000);
                CreateConnection();
            }
            //创建连接对象
            return connection;
        }
        /// <summary>
        /// 创建MqConfigDom一个实例。
        /// </summary>
        /// <returns>MqConfigDom</returns>
        internal static MqConfigDom CreateConfigDomInstance()
        {
            return GetConfigFormAppStting();
        }
        /// <summary>
        ///  获取物理配置文件中的配置项目。
        /// </summary>
        /// <returns></returns>
        private static MqConfigDom GetConfigFormAppStting()
        {
            var result = new MqConfigDom();
            IniFiles ifs = new IniFiles("MQSet.ini");
            var mqHost = ifs.ReadString("mqdata", "MqHost", "");
            result.MqHost = mqHost;

            var mqUserName = ifs.ReadString("mqdata", "MqUserName", "");
            result.MqUserName = mqUserName;

            var mqPassword = ifs.ReadString("mqdata", "MqPassword", "");
            result.MqPassword = mqPassword;

            var mqPort = ifs.ReadString("mqdata", "Mqport", "");
            result.MqPort = mqPort;

            var MqUpQueueName = ifs.ReadString("mqdata", "MqUpQueueName", "");
            result.MqUpQueueName = MqUpQueueName;
            var MqDownQueueName = ifs.ReadString("mqdata", "MqDownQueueName", "");
            result.MqDownQueueName = MqDownQueueName;

            var MqUpExchange = ifs.ReadString("mqdata", "MqUpExchange", "");
            result.MqUpExchange = MqUpExchange;

            var MqDownExchange = ifs.ReadString("mqdata", "MqDownExchange", "");
            result.MqDownExchange = MqDownExchange;

            return result;
        }
        /// <summary>
        /// 创建一个Model
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        internal static IModel CreateModel(IConnection connection)
        {
            return connection.CreateModel();
        }


        /// <summary>
        /// 发送信息 纯虚
        /// </summary>
        public abstract void SentMessage(string message);
        public abstract void SentMessage(string message, string routing_key);

        /// <summary>
        /// 读取数据 纯虚
        /// </summary>
        /// <param name="queue"></param>
        public abstract void ReceiveMessage();

        protected void consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var body = e.Body;
                var message = Encoding.UTF8.GetString(body);
                singleArrivalEvent(message);
                //Trace.WriteLine(message);
                channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
            }
            catch (Exception ea)
            {
                Trace.WriteLine(ea.Message);
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
