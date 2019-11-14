using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;
using WeChatTest.DAL;
using WeChatTest.DAL.Models;
using WeChatTest.Models;
using static Tencent.WXBizMsgCrypt;

namespace WeChatTest.Controllers
{
    public class ValuesController : ApiController
    {

        public static List<BaseMessage> queue;

        string sToken = "promtheusx";
        string sAppID = "wx5ab6d8d11a1d7ddb";
        string sAppSecret = "12e170f15835269b303613bda37e06b9";
        string sEncodingAESKey = "kqwYzXZSs7wFWVGNmyWBztIBPyktOlIwXDKSOEciM0s";
        readonly string connString = "Data Source=xuyuex.date,1433;Initial Catalog=StarrySkyDB;Persist Security Info=True;User ID=sa;Password=Nich0las";

        public static string inputValue;
        [HttpGet]
        [ActionName("GetValue")]
        public string GetValue()
        {
            return inputValue;
        }

        [HttpGet]
        [ActionName("Index")]
        public HttpResponseMessage Get(string signature, string timestamp, string nonce, string echostr)
        {
            ArrayList AL = new ArrayList();
            AL.Add(sToken);
            AL.Add(timestamp);
            AL.Add(nonce);
            AL.Sort(new DictionarySort());
            string raw = "";
            for (int i = 0; i < AL.Count; ++i)
            {
                raw += AL[i];
            }

            SHA1 sha;
            ASCIIEncoding enc;
            string hash = "";
            try
            {
                sha = new SHA1CryptoServiceProvider();
                enc = new ASCIIEncoding();
                byte[] dataToHash = enc.GetBytes(raw);
                byte[] dataHashed = sha.ComputeHash(dataToHash);
                hash = BitConverter.ToString(dataHashed).Replace("-", "");
                hash = hash.ToLower();

                if (hash == signature)
                {
                    HttpResponseMessage msg = new HttpResponseMessage(HttpStatusCode.OK);
                    msg.Content = new StringContent(echostr, System.Text.Encoding.UTF8, "text/html");

                    return msg;
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            //return 0;
        }

        // POST api/<controller>
        [HttpPost]
        [ActionName("Index")]
        public HttpResponseMessage Post(string signature, string timestamp, string nonce)
        {
            inputValue= string.Format("{0}--{1}--{2}", signature, timestamp, nonce);
            string postString;

            HttpResponseMessage msg = new HttpResponseMessage(HttpStatusCode.OK);
            msg.Content = new StringContent("success", System.Text.Encoding.UTF8, "text/html");

            using (Stream stream = HttpContext.Current.Request.InputStream)
            {
                Byte[] postBytes = new Byte[stream.Length];
                stream.Read(postBytes, 0, (Int32)stream.Length);
                postString = Encoding.UTF8.GetString(postBytes);
                Trace.WriteLine(postString);

                XElement xdoc = XElement.Parse(postString);
                var msgtype = xdoc.Element("MsgType").Value.ToUpper();
                MsgType type = (MsgType)Enum.Parse(typeof(MsgType), msgtype);

                //BaseMessage baseMsg = ConvertObj<BaseMessage>(postString);
                if (queue == null)
                {
                    queue = new List<BaseMessage>();
                }
                else if (queue.Count >= 150)
                {
                    queue.RemoveRange(0, 100);
                }

                switch (type)
                {
                    case MsgType.TEXT:
                        TextMessage textMsg = ConvertObj<TextMessage>(postString);
                        break;
                    case MsgType.IMAGE:
                        ImgMessage imgMsg = ConvertObj<ImgMessage>(postString);

                        SQLHelper db = new SQLHelper(connString);
                        bool msgExists = db.CheckMsgExists(imgMsg.MsgId);
                        if (msgExists)
                        {
                            return msg;
                        }
                        else
                        {
                            string passCode = db.InsertPicMsg(imgMsg);

                            string response = CreateTextReponse(imgMsg.FromUserName, imgMsg.ToUserName, passCode);

                            Trace.WriteLine(response);

                            msg.Content = new StringContent(response, System.Text.Encoding.UTF8, "text/html");
                            return msg;
                        }

                        break;
                    case MsgType.EVENT:
                        msg.Content = new StringContent("", System.Text.Encoding.UTF8, "text/html");

                        EventMessage eventMsg = ConvertObj<EventMessage>(postString);

                        if (eventMsg == null)
                            return msg;

                        //break;
                        if (eventMsg.Event == Event.SCAN)
                        {

                            if (queue.Where(v => v.CreateTime == eventMsg.CreateTime && v.FromUserName == v.FromUserName).Count() > 0)
                            {
                                return msg;
                            }
                            else
                            {
                                queue.Add(eventMsg);
                                Console.WriteLine(eventMsg.FromUserName);

                                string sceneStr = eventMsg.EventKey;

                                //扫码消息格式 ScanQR OpenID SenceStr
                                SendMsg("118.190.45.106", 8712, "ScanQR " + eventMsg.FromUserName + " " + sceneStr);
                            }
                        }

                        return msg;
                    //break;
                    default:
                        break;
                }
            }

            return msg;
        }

        [HttpPost]
        [ActionName("CreateMenu")]
        public HttpResponseMessage CreateMenu(System.Net.Http.Formatting.FormDataCollection form)
        {
            HttpResponseMessage msg = new HttpResponseMessage(HttpStatusCode.OK);
            try
            {
                string json = form.Get("menu");
                string token = GetAccessToken(sAppID);
                string apiUri = string.Format("https://api.weixin.qq.com/cgi-bin/menu/create?access_token={0}", token);

                HttpClient client = new HttpClient();
                string requestJson = json;
                StringContent content = new StringContent(requestJson, Encoding.UTF8);
                var response = client.PostAsync(apiUri, content);
                string tickJson = response.Result.Content.ReadAsStringAsync().Result;


                msg.Content = new StringContent(tickJson, System.Text.Encoding.UTF8, "text/html");


            }
            catch (Exception ex)
            {
                msg.Content = new StringContent(ex.Message, System.Text.Encoding.UTF8, "text/html");
            }
            return msg;
        }

        [HttpGet]
        [ActionName("GetAccessToken")]
        public string GetAccessToken(string appId)
        {
            if (appId != sAppID)
                return "error";

            SQLHelper db = new SQLHelper(connString);
            string token = db.GetTokenByAppID(appId);
            if (string.IsNullOrEmpty(token))
            {
                string apiUri = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", appId, sAppSecret);
                HttpClient client = new HttpClient();
                var reponse = client.GetStringAsync(apiUri);

                var aToken = Newtonsoft.Json.JsonConvert.DeserializeObject<AccessToken>(reponse.Result);
                if (aToken.access_token != null)
                {
                    db.SetTokenByAppID(appId, aToken.expires_in, aToken.access_token);
                    return aToken.access_token;
                }
                else
                {
                    return "error";
                }

                //var jtoken = Newtonsoft.Json.Linq.JObject.Parse(reponse.Result).GetValue("access_token");
                //if (jtoken != null)
                //{
                //    jtoken.Value<string>(
                //}
                //else
                //{
                //    return "error";
                //}
            }
            else
            {
                return token;
            }
        }

        [HttpGet]
        [ActionName("GetQRCode")]
        public HttpResponseMessage GetQRCode()
        {
            string token = GetAccessToken(sAppID);
            string apiUri = string.Format("https://api.weixin.qq.com/cgi-bin/qrcode/create?access_token={0}", token);

            HttpClient client = new HttpClient();
            string requestJson = "{\"expire_seconds\": 604800, \"action_name\": \"QR_SCENE\", \"action_info\": {\"scene\": {\"scene_id\": 1234567890}}}";
            StringContent content = new StringContent(requestJson, Encoding.UTF8);
            var response = client.PostAsync(apiUri, content);
            string tickJson = response.Result.Content.ReadAsStringAsync().Result;

            AccessTokenTicket ticket = Newtonsoft.Json.JsonConvert.DeserializeObject<AccessTokenTicket>(tickJson);
            apiUri = string.Format("https://mp.weixin.qq.com/cgi-bin/showqrcode?ticket={0}", ticket.ticket);
            response = client.GetAsync(apiUri);
            return response.Result;
        }

        //获取永久型二维码
        [HttpGet]
        [ActionName("GetQRCodeLimit")]
        public HttpResponseMessage GetQRCodeLimit(string sceneStr)
        {
            SQLHelper db = new SQLHelper(connString);
            byte[] qrData = db.GetSceneQR(sceneStr);
            if (qrData == null)
            {
                string token = GetAccessToken(sAppID);
                string apiUri = string.Format("https://api.weixin.qq.com/cgi-bin/qrcode/create?access_token={0}", token);

                HttpClient client = new HttpClient();
                string requestJson = "{\"action_name\": \"QR_LIMIT_STR_SCENE\", \"action_info\": {\"scene\": {\"scene_str\": \"" + sceneStr + "\"}}}";
                StringContent content = new StringContent(requestJson, Encoding.UTF8);
                var response = client.PostAsync(apiUri, content);
                string tickJson = response.Result.Content.ReadAsStringAsync().Result;

                AccessTokenTicket ticket = Newtonsoft.Json.JsonConvert.DeserializeObject<AccessTokenTicket>(tickJson);
                apiUri = string.Format("https://mp.weixin.qq.com/cgi-bin/showqrcode?ticket={0}", ticket.ticket);
                response = client.GetAsync(apiUri);


                db.InsertSceneQR(sceneStr, sAppID, response.Result.Content.ReadAsByteArrayAsync().Result);


                return response.Result;

            }
            else
            {
                HttpResponseMessage msg = new HttpResponseMessage(HttpStatusCode.OK);

                //ObjectContent content = new  ObjectContent(
                ByteArrayContent cont = new ByteArrayContent(qrData);
                MemoryStream ms = new MemoryStream(qrData, 0, qrData.Length);
                StreamContent stream = new StreamContent(ms);
                msg.Content = stream;
                HttpContext.Current.Response.ContentType = "image/jpeg";

                //Request.CreateResponse<byte[]>(HttpStatusCode.OK, qrData, "text/html");
                return msg;
            }


        }

        private string CreateTextReponse(string from, string to, string content)
        {
            string response = string.Format(@"<xml>
                                                            <ToUserName><![CDATA[{0}]]></ToUserName>
                                                            <FromUserName><![CDATA[{1}]]></FromUserName>
                                                            <CreateTime>{2}</CreateTime>
                                                            <MsgType><![CDATA[text]]></MsgType>
                                                            <Content><![CDATA[{3}]]></Content>
                                                            </xml>",
                                                      from, to, DateTime2Int(DateTime.Now), content);

            return response;
        }

        private static int DateTime2Int(DateTime dt)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(dt - startTime).TotalSeconds;
        }

        private static T ConvertObj<T>(string xmlstr)
        {
            try
            {
                XElement xdoc = XElement.Parse(xmlstr);
                var type = typeof(T);
                var t = Activator.CreateInstance<T>();
                foreach (XElement element in xdoc.Elements())
                {
                    var pr = type.GetProperty(element.Name.ToString());
                    if (element.HasElements)
                    {//这里主要是兼容微信新添加的菜单类型。nnd，竟然有子属性，所以这里就做了个子属性的处理
                        foreach (var ele in element.Elements())
                        {
                            pr = type.GetProperty(ele.Name.ToString());
                            pr.SetValue(t, Convert.ChangeType(ele.Value, pr.PropertyType), null);
                        }
                        continue;
                    }
                    if (pr.PropertyType.Name == "MsgType")//获取消息模型
                    {
                        pr.SetValue(t, (MsgType)Enum.Parse(typeof(MsgType), element.Value.ToUpper()), null);
                        continue;
                    }
                    if (pr.PropertyType.Name == "Event")//获取事件类型。
                    {
                        pr.SetValue(t, (Event)Enum.Parse(typeof(Event), element.Value.ToUpper()), null);
                        continue;
                    }
                    pr.SetValue(t, Convert.ChangeType(element.Value, pr.PropertyType), null);
                }
                return t;
            }
            catch (Exception)
            {

                return default(T);
            }

        }
        private Boolean SendMsg(string ip, int port, string message)
        {
            try
            {
                System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient();
                client.Connect(ip, port);

                Byte[] data = new Byte[256];
                //System.Text.Encoding.ASCII.GetBytes(message);
                System.Net.Sockets.NetworkStream stream = client.GetStream();

                Int32 bytes = stream.Read(data, 0, data.Length);
                String responseData = String.Empty;
                responseData = System.Text.Encoding.UTF8.GetString(data, 0, bytes);
                if (responseData == "welcome\r\n")
                {
                    data = System.Text.Encoding.UTF8.GetBytes(message + "\r\n");
                    stream.Write(data, 0, data.Length);


                    //bytes = stream.Read(data, 0, data.Length);
                    //responseData = System.Text.Encoding.UTF8.GetString(data, 0, bytes);
                    //String[] strAry = responseData.Split(' ');

                }
                stream.Close();
                client.Close();
                return true;
            }
            catch (ArgumentNullException e)
            {
                return false;
            }
            catch (System.Net.Sockets.SocketException e)
            {
                return false;
            }
        }
    }


}