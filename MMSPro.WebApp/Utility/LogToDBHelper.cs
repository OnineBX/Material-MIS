/*------------------------------------------------------------------------------
 * Unit Name：LogToDBHelper.cs
 * Description: 用于向数据库创建日志
 * Author: Xu Chun Lei
 * Created Date: 2010-07-14
 * ----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Microsoft.SharePoint;

namespace MMSPro.WebApp
{  
    public sealed class LogToDBHelper
     {
        private static readonly LogToDBHelper instance = new LogToDBHelper();

        public static readonly string LOG_MSG_LOADERROR = "载入数据出现错误，请重新打开该页面！";
        public static readonly string LOG_MSG_QUERYERROR = "不存在您要获取的信息，请核实后重新打开该页面！";
        public static readonly string LOG_MSG_INSERTERROR = "您试图添加错误数据，请核实后再执行添加操作！";
        public static readonly string LOG_MSG_UPDATEERROR = "您正在尝试更新错误数据，请核实后再执行更新操作！";
        public static readonly string LOG_MSG_DELETEERROR = "您正在删除的数据存在错误，请核实后再执行删除操作！";

        private LogToDBHelper()
        {
            
        }

        public static LogToDBHelper Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// 向数据库写入日志
        /// </summary>
        /// <param name="message">日志内容,非空</param>
        /// <param name="type">日志类型：错误或信息，非空</param>
        /// <param name="source">日志产生来源，来自哪个类方法，非空</param>
        public void WriteLog(string message,string type,string source)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    LogInfo linfo = new LogInfo();
                    linfo.LogMessage = message;
                    linfo.LogSource = source;
                    linfo.LogType = type;
                    linfo.LogDateTime = DateTime.Now;
                    linfo.LogUser = db.EmpInfo.SingleOrDefault(u=> u.Account == SPContext.Current.Web.CurrentUser.LoginName).EmpID;
                    db.LogInfo.InsertOnSubmit(linfo);
                    db.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message, "错误", ex.Source);
            }
        }
    }   


}
