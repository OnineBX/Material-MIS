//***********************************************************
//--Description:代理处理                                    *
//--Created By: adonis                                      *
//--Date:2010.9.16                                          *
//--*********************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;

namespace MMSPro.WebApp
{
    public static class Proxy
    {

        public  struct MyProxy
        {
            public static int _Fiduciary;
            public static int _ProxID;
            public static int _Principal;
        }
      
        /// <summary>
        /// 如果是登录用户是代理人，则执行代理任务
        /// </summary>
        /// <param name="Principal">委托人</param>
        /// <param name="proxyType">委托任务类型,如:正常出库</param>
        /// <returns>返回代理人ID</returns>
        public static int send(int Principal,int proxyType)
        {
            DateTime dateNow = Convert.ToDateTime("1900-1-1 00:00:00");
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {

                TaskProxy TP = db.TaskProxy.SingleOrDefault(u => u.ProxyPrincipal == Principal && u.ProxyTaskType == proxyType && u.TaskDispose!="已完成");
                MyProxy._Principal = Principal;
                if (TP != null)
                {
                     MyProxy._Fiduciary = TP.ProxyFiduciary;
                     MyProxy._ProxID = TP.TaskProxyID;
                     
                     TP.TaskDispose = "处理中";
                     db.SubmitChanges();
                    //服务器时间
                    var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                    dateNow = SevTime.First();
                    //代理人是登录用户，且在代理任务期
                    if (dateNow >= TP.StartTime && dateNow <= TP.EndTime)
                    {


                        return MyProxy._Fiduciary;
                    }
                    else
                    {
                        TP.TaskDispose = "已过期";
                        db.SubmitChanges();
                        return MyProxy._Principal;
                    }
                }
                
                          
            }
            return MyProxy._Principal;

        }
        /// <summary>
        /// 返回代理任务ID
        /// </summary>
        /// <param name="Principal">委托人</param>
        /// <param name="proxyType">委托任务类型</param>
        /// <returns></returns>
        public static int getProxyTaskId(int Principal, int proxyType)
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                 TaskProxy TP = db.TaskProxy.SingleOrDefault(u => u.ProxyPrincipal == Principal && u.ProxyTaskType == proxyType && u.TaskDispose != "已过期");
                 if (TP != null)
                 {
                     MyProxy._ProxID = TP.TaskProxyID;
                     return MyProxy._ProxID;
                 }
                 return 0;

            }

        }

        /// <summary>
        /// 保存到主任委托任务关系表
        /// </summary>
        /// <param name="taskId">当前发送任务ID</param>
        /// <param name="proxyId">代理任务ID</param>
        /// <returns></returns>
        public static bool saveTask(int taskId,int proxyId)
        {
            using (MMSProDBDataContext data = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                try
                {
                    if (proxyId != 0)
                    {
                        ProxyDirector PD = new ProxyDirector();
                        PD.TaskID = taskId;
                        PD.TaskProxyID = proxyId;

                        data.ProxyDirector.InsertOnSubmit(PD);
                        data.SubmitChanges();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch(Exception ex)
                {
                    return false;
                }
               
            }
           
        }
    }
}
