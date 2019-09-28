using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.YQMsg
{
    public class PLCMsg : MsgBase
    {
        public PLCMsg()
        {
            this.MESSAGE_TYPE = "plc";            
        }
        public int PALLET_COUNT { get; set; }

        public int STATUS { get; set; }
    }
}
