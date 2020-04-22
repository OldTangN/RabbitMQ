using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.Text;

namespace RabbitMQ
{
    /// <summary>
    /// 服务器MQ
    /// </summary>
    public class ServerMQ : RabbitMQBase
    {
        public string MqDownExchange { get; private set; }
        public string MqUpQueueName { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mqHost">地址</param>
        /// <param name="mqUserName">账号</param>
        /// <param name="mqPwd">密码</param>
        /// <param name="mqUpQueueName">上行消息队列</param>
        /// <param name="mqDownExchange">下行交换机</param>
        public ServerMQ(string mqHost, string mqUserName, string mqPwd, string mqUpQueueName, string mqDownExchange)
            : base(mqHost, mqUserName, mqPwd)
        {
            this.MqUpQueueName = mqUpQueueName;
            this.MqDownExchange = mqDownExchange;
        }

        /// <summary>
        /// 服务第不适应此方法
        /// </summary>
        /// <param name="message"></param>
        public override void SendMessage(string message)
        {
            throw new Exception("服务端发送消息需要指定路由键，请使用SendMessage(eventMessage, routing_key)方法！");
        }

        /// <summary>
        /// 服务器下发给各个专机的指令，要带有专机的消息队列路由键
        /// </summary>
        /// <param name="eventMessage">消息</param>
        /// <param name="routing_key">路由键</param>
        public override void SendMessage(string eventMessage, string routing_key)
        {
            const byte deliveryMode = 2;//消息持久化

            //获取通道
            var channel1 = CreateModel(connection);
            using (channel1)
            {
                channel1.ConfirmSelect();//开启Confirm模式
                channel1.BasicAcks += Channel_BasicAcks;//通过事件回调
                channel1.BasicNacks += Model_BasicNacks;
                // 声明队列，通过指定 durable 参数为 true，对消息进行持久化处理。 
                channel1.ExchangeDeclare(exchange: MqDownExchange, durable: true, type: "direct");
                //表示持久化
                var properties = channel1.CreateBasicProperties();
                properties.DeliveryMode = deliveryMode;
                var body = Encoding.UTF8.GetBytes(eventMessage);
                try
                {
                    //推送消息
                    channel1.BasicPublish(exchange: MqDownExchange,
                                         routingKey: routing_key,//带有路由键
                                         basicProperties: null,
                                         body: body);
                }
                catch (Exception e)
                {
                    MyLogLib.MyLog.WriteLog("BasicPublish异常！", e);
                }
            }
        }
        /// <summary>
        /// 接收客户端的上行消息
        /// </summary>
        public override void ReceiveMessage()
        {
            try
            {
                //生命队列持久化
                channel.QueueDeclare(MqUpQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                // 告知 RabbitMQ，在未收到当前 Worker 的消息确认信号时，不再分发给消息，确保公平调度。
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                //构建消费者实例
                var consumer = new EventingBasicConsumer(channel);
                //绑定接收消息
                consumer.Received += consumer_Received;
                //channel.BasicConsume(MqUpQueueName, autoAck: false, consumer: consumer);
                channel.BasicConsume(MqUpQueueName, noAck: false, consumer: consumer);
            }
            catch (Exception e)
            {
                MyLogLib.MyLog.WriteLog("ReceiveMessage异常!", e);
            }
        }
    }
}
