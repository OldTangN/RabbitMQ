using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.YQMsg.CellLine
{
    public class DataMsg : MsgBase
    {
        public DataMsg()
        {
            this.MESSAGE_TYPE = "data";
        }
        public List<RltData> DATA { get; set; } = new List<RltData>();
    }
}
