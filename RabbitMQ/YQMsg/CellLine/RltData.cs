using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.YQMsg.CellLine
{
    public class RltData
    {
        public string WORK_ID { get; set; }//设备序号

        /// <summary>
        /// 表壳码
        /// </summary>
        public string Bar_code { get; set; }

        /// <summary>
        /// 厂内码
        /// </summary>
        public string Factory_code { get; set; }

        /// <summary>
        /// 铭牌码
        /// </summary>
        public string Nameplate { get; set; }


        /// <summary>
        /// 上一道工序的结果 0 为合格 1为不合格，如果上道工序就不合格那么本工序也不用做了
        /// </summary>
        public string result { get; set; }
    }
}
