using MyLogLib;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.Text;

namespace RabbitMQ
{
    /// <summary>
    /// 客户端MQ
    /// </summary>
    public class ClientMQ : RabbitMQBase
    {
        /// <summary>
        /// 上行交换机
        /// </summary>
        public string MqUpExchange { get; private set; }
        /// <summary>
        /// 下行队列
        /// </summary>
        public string MqDownQueueName { get; private set; }
        /// <summary>
        /// 下行交换机
        /// </summary>
        public string MqDownExchange { get; private set; }   

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mqHost">地址</param>
        /// <param name="mqUserName">账号</param>
        /// <param name="mqPwd">密码</param>
        /// <param name="mqUpExchange">上行交换机</param>
        /// <param name="mqDownExchange">下行交换机</param>
        /// <param name="mqDownQueueName">下行消息队列，既是专机自身队列</param>
        public ClientMQ(string mqHost, string mqUserName, string mqPwd, string mqUpExchange, string mqDownExchange, string mqDownQueueName)
            : base(mqHost, mqUserName, mqPwd)
        {
            this.MqUpExchange = mqUpExchange;
            this.MqDownExchange = mqDownExchange;
            this.MqDownQueueName = mqDownQueueName;
            MyLog.WriteLog("12");
        }

        /// <summary>
        /// 发送消息至服务端（上行）
        /// </summary>
        /// <param name="eventMessage"></param>
        public override void SendMessage(string eventMessage)
        {
            SendMessage(eventMessage, "");
        }

        /// <summary>
        /// 发送消息至服务端（上行）
        /// </summary>
        /// <param name="message"></param>
        /// <param name="routing_key">此参数在客户端调用时不适用</param>
        public override void SendMessage(string message, string routing_key = "")
        {
            routing_key = "";
            const byte deliveryMode = 2;//消息持久化
            //获取通道
            var channel1 = CreateModel(connection);
            using (channel1)
            {
                channel1.ConfirmSelect();//开启Confirm模式
                channel1.BasicAcks += Channel_BasicAcks;//通过事件回调
                channel1.BasicNacks += Model_BasicNacks;
                // 声明队列，通过指定 durable 参数为 true，对消息进行持久化处理。 
                //channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: null);

                channel1.ExchangeDeclare(exchange: MqUpExchange, durable: true, type: "fanout");
                //表示持久化
                var properties = channel1.CreateBasicProperties();
                properties.DeliveryMode = deliveryMode;
                var body = Encoding.UTF8.GetBytes(message);
                try
                {
                    //推送消息
                    channel1.BasicPublish(exchange: MqUpExchange,
                                         routingKey: routing_key,
                                         basicProperties: null,
                                         body: body);
                }
                catch (Exception e)
                {
                    MyLog.WriteLog("BasicPublish异常！", e);
                }
            }
        }

        /// <summary>
        /// 接收服务端的下行消息
        /// </summary>
        public override void ReceiveMessage()
        {
            try
            {
                channel.QueueDeclare(queue: MqDownQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                // 告知 RabbitMQ，在未收到当前 Worker 的消息确认信号时，不再分发给消息，确保公平调度。
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                channel.ExchangeDeclare(exchange: MqDownExchange, durable: true, type: "direct");
                channel.QueueBind(MqDownQueueName, exchange: MqDownExchange, routingKey: ""/* MQConfig.RoutingKey*/);
                //构建消费者实例
                var consumer = new EventingBasicConsumer(channel);
                //绑定接收消息
                consumer.Received += consumer_Received;
                //channel.BasicConsume(MqDownQueueName, autoAck: false, consumer: consumer);
                channel.BasicConsume(MqDownQueueName, noAck: false, consumer: consumer);
            }
            catch (Exception e)
            {
                MyLog.WriteLog("ReceiveMessage异常！", e);
            }
        }
    }
}
