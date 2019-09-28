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
    public class ServerMQ:RabbitMQBase
    {
        public override void SentMessage(string message)
        {
            SentMessage(message, "");
        }

        /// <summary>
        /// 服务器下发给各个专机的指令，要带有专机的消息队列路由键
        /// </summary>
        /// <param name="eventMessage">消息</param>
        /// <param name="routing_key">路由键</param>
        public override void SentMessage(string eventMessage, string routing_key)
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
                channel1.ExchangeDeclare(exchange: MQConfig.MqDownExchange, durable: true, type: "direct");
                //表示持久化
                var properties = channel1.CreateBasicProperties();
                properties.DeliveryMode = deliveryMode;
                var body = Encoding.UTF8.GetBytes(eventMessage);
                try
                {
                    //推送消息
                    channel1.BasicPublish(exchange: MQConfig.MqDownExchange,
                                         routingKey: routing_key,//带有路由键
                                         basicProperties: null,
                                         body: body);
                }
                catch (Exception e)
                {

                    Trace.WriteLine(e.Message);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queue">接收的队列</param>
        public override void ReceiveMessage()
        {
            try
            {
                //生命队列持久化
                channel.QueueDeclare(MQConfig.MqUpQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                // 告知 RabbitMQ，在未收到当前 Worker 的消息确认信号时，不再分发给消息，确保公平调度。
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                //构建消费者实例
                var consumer = new EventingBasicConsumer(channel);
                //绑定接收消息
                consumer.Received += consumer_Received;
                channel.BasicConsume(MQConfig.MqUpQueueName, autoAck: false, consumer: consumer);
            }
            catch(Exception e)
            {
                Log.Error(e.Message);
            }
        }

        
    }
}
