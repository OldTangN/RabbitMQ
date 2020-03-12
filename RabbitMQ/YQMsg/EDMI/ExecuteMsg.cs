using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.YQMsg.EDMI
{
    public class ExecuteMsg : MsgBase
    {
        public ExecuteMsg()
        {
            this.MESSAGE_TYPE = "execute";
        }
        public List<RltData> DATA { get; set; }
    }
}
