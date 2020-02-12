using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.YQMsg
{
    public class HeartBeatMsg : MsgBase
    {
        public HeartBeatMsg()
        {
            this.MESSAGE_TYPE = "heart";
        }
        /// <summary>
        /// 设备状态
        /// </summary>
        public string STATUS { get; set; }
    }

    public enum HeartStatus
    {
        /// <summary>
        /// 启动中（初始化中）
        /// </summary>
       Initializing = 0,
        /// <summary>
        /// 启动完毕（初始化完毕）
        /// </summary>
        Init_Complete = 1,
        /// <summary>
        /// 工作中（检测中）
        /// </summary>
        Working = 2,
        /// <summary>
        /// 空闲（检测完毕）
        /// </summary>
        Finished = 3,
        /// <summary>
        /// 故障
        /// </summary>
        Error = 4,
        /// <summary>
        /// 维修
        /// </summary>
        Maintenance = 5
    }
}
