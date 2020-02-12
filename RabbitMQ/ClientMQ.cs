using System;
using System.Diagnostics;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ
{
    /// <summary>
    /// 客户端MQ
    /// </summary>
    public class ClientMQ : RabbitMQBase
    {
        public override void SentMessage(string eventMessage)
        {
            SentMessage(eventMessage, "");
        }
        public override void SentMessage(string message, string routing_key)
        {
            const byte deliveryMode = 2;

            //获取通道
            var channel1 = CreateModel(connection);
            using (channel1)
            {
                channel1.ConfirmSelect();//开启Confirm模式
                channel1.BasicAcks += Channel_BasicAcks;//通过事件回调
                channel1.BasicNacks += Model_BasicNacks;
                // 声明队列，通过指定 durable 参数为 true，对消息进行持久化处理。 
                //channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: null);

                channel1.ExchangeDeclare(exchange: MQConfig.MqUpExchange, durable: true, type: "fanout");
                //表示持久化
                var properties = channel1.CreateBasicProperties();
                properties.DeliveryMode = deliveryMode;
                var body = Encoding.UTF8.GetBytes(message);
                try
                {
                    //推送消息
                    channel1.BasicPublish(exchange: MQConfig.MqUpExchange,
                                         routingKey: routing_key,
                                         basicProperties: null,
                                         body: body);
                }
                catch (Exception e)
                {

                    Trace.WriteLine(e.Message);
                }
            }
        }

        public override void ReceiveMessage()
        {
            try
            {
                channel.QueueDeclare(queue: MQConfig.MqDownQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                // 告知 RabbitMQ，在未收到当前 Worker 的消息确认信号时，不再分发给消息，确保公平调度。
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                channel.ExchangeDeclare(exchange: MQConfig.MqDownExchange, durable: true, type: "direct");
                channel.QueueBind(MQConfig.MqDownQueueName, exchange: MQConfig.MqDownExchange, routingKey: ""/* MQConfig.RoutingKey*/);
                //构建消费者实例
                var consumer = new EventingBasicConsumer(channel);
                //绑定接收消息
                consumer.Received += consumer_Received;
                channel.BasicConsume(MQConfig.MqDownQueueName, autoAck: false, consumer: consumer);
            }
            catch (Exception e)
            {
                Log.Error("ReceiveMessage异常：" + e.Message + "\r\n" + e.StackTrace);
            }
        }
    }
}
