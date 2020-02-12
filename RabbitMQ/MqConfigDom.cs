using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ
{
    public class MqConfigDom
    {
        /// <summary>
        /// 消息队列的地址
        /// </summary>
        public string MqHost { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string MqUserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string MqPassword { get; set; }
        /// <summary>
        /// 端口号
        /// </summary>
        public string MqPort { get; set; }


        /// <summary>
        /// 上行数据交换机
        /// </summary>
        public string MqUpExchange { get; set; }
        /// <summary>
        /// 下行数据交换机
        /// </summary>
        public string MqDownExchange { get; set; }
        /// <summary>
        /// 上行队列名称
        /// </summary>
        public string MqUpQueueName { get; set; }
        /// <summary>
        /// 下行队列名称
        /// </summary>
        public string MqDownQueueName { get; set; }
        /// <summary>
        /// 路由键
        /// </summary>
        public string RoutingKey { get; set; }
    }
}
