using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.YQMsg
{
    public class TaskData
    {
        /// <summary>
        /// 生产制令编号
        /// </summary>
        public string WORKORDER_CODE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MATERIEL_CODE { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string ORDER_NO { get; set; }
    }
}
