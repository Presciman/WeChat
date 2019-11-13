using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeChatTest.DAL.DataModels
{
    public class WC_SceneQR
    {
        public string SceneStr { get; set; }
        public string AppID { get; set; }
        public byte[] QRCode { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
