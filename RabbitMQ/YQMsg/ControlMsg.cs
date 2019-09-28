using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.YQMsg
{
    /// <summary>
    /// 主控下发的设备控制类消息
    /// </summary>
    public class ControlMsg : MsgBase
    {
        public ControlMsg()
        {
            this.MESSAGE_TYPE = "control";
        }
        /// <summary>
        /// 控制命令
        /// </summary>
        public int COMMAND_ID { get; set; }
    }
}
