using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeChatTest.DAL.DataModels
{
    public class WC_Config
    {
        public string AppID { get; set; }
        public string AppSecret { get; set; }
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public DateTime TokenUpdateTime { get; set; }
    }
}
