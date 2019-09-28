using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.YQMsg
{
    public class TaskMsg : MsgBase
    {
        public TaskMsg()
        {
            this.MESSAGE_TYPE = "task";
        }
        public List<TaskData> DATA { get; set; } = new List<TaskData>();
    }
}
