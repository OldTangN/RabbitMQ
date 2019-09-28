using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.YQMsg
{
    public class RltData
    {
        /// <summary>
        /// 工位
        /// </summary>
        public string WORK_ID { get; set; } = "0";

        /// <summary>
        /// 条码
        /// </summary>
        public string BAR_CODE { get; set; } = "";

        /// <summary>
        /// 0=合格,1=不合格
        /// </summary>
        public string result { get; set; } = "1";
    }
}
