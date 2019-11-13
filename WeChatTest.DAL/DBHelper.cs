using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeChatTest.DAL
{
    /*...
  命名规则

  所有Get 带 All的都是获取表里的所有数据
  不带 All的 是获取 state为 0的可用数据


  ...*/

    public class DBHelper : IDisposable
    {
        private IDbConnection _conn;
        public IDbConnection conn
        {
            get
            {
                if (_conn == null)
                {
                    try
                    {
                        _conn = new SqlConnection(connString);
                        _conn.Open();
                    }
                    catch (Exception ex)
                    {
                        //LogHelper.ErrorLog("DbConnection Init", ex);
                        throw ex;
                    }

                }
                else
                {
                    if (_conn.State == ConnectionState.Closed)
                    {
                        try
                        {
                            _conn.Open();
                        }
                        catch (Exception ex)
                        {
                            //LogHelper.ErrorLog("DbConnection Init", ex);
                            throw ex;

                        }

                    }
                }

                return _conn;
            }
        }
        public readonly string connString;
        public DBHelper(string connString)
        {
            try
            {
                this.connString = connString;
                ////conn = new SqlConnection(connString);
                //conn.ConnectionString = connString;
                //conn.Open();
            }
            catch (Exception ex)
            {
                //LogHelper.ErrorLog(MethodBase.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

        public void Dispose()
        {
            if (conn != null)
            {
                conn.Dispose();
            }
        }



 
    }
}
