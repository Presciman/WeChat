using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using WeChatTest.DAL.Models;
using WeChatTest.DAL.DataModels;

namespace WeChatTest.DAL
{
    public class SQLHelper : DBHelper
    {
        public SQLHelper(string connString) : base(connString)
        {

        }

        /// <summary>
        /// 检查该消息是否处理过
        /// </summary>
        /// <param name="msgid"></param>
        /// <returns></returns>
        public bool CheckMsgExists(string MsgId)
        {
            try
            {
                string sqlStr = "select MsgId from WC_PicMsg where MsgId=@MsgId";
                var result = conn.Query<string>(sqlStr, new { MsgId = MsgId });

                return result.Count() > 0;
            }
            catch (Exception ex)
            {

                return false;
            }
            finally
            {
                conn.Close();
            }

        }

        public string InsertPicMsg(ImgMessage model)
        {
            try
            {
                string sqlStr = "insert into WC_PicMsg values (@MsgId, @PicUrl, @MediaId, @FromUserName, @CreateDate) ";
                conn.Execute(sqlStr, new { MsgId = model.MsgId, PicUrl = model.PicUrl, MediaId = model.MediaId, FromUserName = model.FromUserName, CreateDate = DateTime.Now });

                sqlStr = "select PassCode from WC_PassCode where FromUserName=@FromUserName";
                var result = conn.Query<string>(sqlStr, new { FromUserName = model.FromUserName });
                if (result.Count() > 0)
                {
                    return result.SingleOrDefault();
                }
                else
                {
                    Random rd = new Random(~unchecked((int)DateTime.Now.Ticks));
                    int code = rd.Next(100000, 1000000);

                    sqlStr = "insert into WC_PassCode values (@FromUserName, @PassCode)";
                    conn.Execute(sqlStr, new { FromUserName = model.FromUserName, PassCode = code });

                    return code.ToString();
                }
            }
            catch (Exception ex)
            {

                return string.Empty;
            }
            finally
            {
                conn.Close();
            }
        }

        public IEnumerable<string> GetPicList(string passCode)
        {
            List<string> list = new List<string>();
            try
            {
                string sqlStr = "select FromUserName from WC_PassCode where PassCode=@PassCode";
                var result = conn.Query<string>(sqlStr, new { PassCode = passCode });
                if (result.Count() == 0)
                {
                    return list;
                }
                else
                {
                    string openId = result.FirstOrDefault();

                    sqlStr = "select PicUrl from WC_PicMsg where FromUserName=@FromUserName";
                    return conn.Query<string>(sqlStr, new { FromUserName = openId });

                }
            }
            catch (Exception ex)
            {

                return list;
            }
            finally
            {
                conn.Close();
            }
        }

        public IEnumerable<string> GetPicListByOpenID(string openid)
        {
            List<string> list = new List<string>();
            try
            {

                string sqlStr = "select PicUrl from WC_PicMsg where FromUserName=@FromUserName and CreateDate > convert(char(10),GetDate(),120)";
                return conn.Query<string>(sqlStr, new { FromUserName = openid });

            }
            catch (Exception ex)
            {

                return list;
            }
            finally
            {
                conn.Close();
            }
        }

        public string GetTokenByAppID(string AppID)
        {
            try
            {
                string sqlStr = "select * from WC_Config where AppID=@AppID";
                var result = conn.QuerySingleOrDefault<WC_Config>(sqlStr, new { AppID = AppID });

                if (string.IsNullOrEmpty(result.AccessToken))
                {
                    return string.Empty;
                }
                else
                {
                    TimeSpan tSpan = DateTime.Now - result.TokenUpdateTime;
                    if (result.ExpiresIn - (int)tSpan.TotalSeconds > 60)
                    {
                        return result.AccessToken;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
            finally
            {
                conn.Close();
            }
        }

        public bool SetTokenByAppID(string AppID, string ExpiresIn, string AccessToken)
        {
            try
            {
                string sqlStr = "update WC_Config set AccessToken=@AccessToken , ExpiresIn=@ExpiresIn , TokenUpdateTime=@TokenUpdateTime  where AppID=@AppID";
                int result = conn.Execute(sqlStr, new { AccessToken = AccessToken, ExpiresIn = ExpiresIn, TokenUpdateTime = DateTime.Now, AppID = AppID });

                return result > 1;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                conn.Close();
            }
        }

        public bool InsertSceneQR(string sceneStr, string appid, byte[] qrcode)
        {
            try
            {
                string sqlStr = "insert into WC_SceneQR values (@SceneStr, @AppID, @QRCode, @CreateDate) ";
                var result = conn.Execute(sqlStr, new { SceneStr = sceneStr, AppID = appid, QRCode = qrcode, CreateDate = DateTime.Now });

                return result > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                conn.Close();
            }
        }

        public byte[] GetSceneQR(string sceneStr)
        {
            try
            {
                string sqlStr = "select * from WC_SceneQR where  SceneStr=@SceneStr";
                var result = conn.QuerySingleOrDefault<WC_SceneQR>(sqlStr, new { SceneStr = sceneStr });

                return result.QRCode;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
