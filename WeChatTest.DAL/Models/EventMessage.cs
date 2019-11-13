using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeChatTest.DAL.Models
{
    //已关注的扫码事件
    public class EventMessage : BaseMessage
    {
        public Event Event { get; set; }
        /// <summary>
        /// 事件KEY值，是一个32位无符号整数，即创建二维码时的二维码scene_id
        /// </summary>
        public string EventKey { get; set; }

        public string Ticket { get; set; }
    }
}
