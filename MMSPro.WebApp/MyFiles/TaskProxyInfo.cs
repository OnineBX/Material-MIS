using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Configuration;


namespace MMSPro.WebApp
{
    public class TaskProxyInfo: System.Web.UI.Page
    {
        private int _noticeid;
        private string _taskType;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {               

                GridView gdvTaskInfo = (GridView)Page.FindControl("gdvTaskInfo");
                _noticeid = Convert.ToInt32(Request.QueryString["NoticeID"]);
                _taskType = Request.QueryString["TaskType"];                                
          
                if (_taskType.IndexOf("入库") != -1)
                {
                    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        gdvTaskInfo.DataSource = from a in db.ProxyDirector
                                                 join b in db.TaskStorageIn on a.TaskID equals b.TaskStorageID
                                                 where a.TaskProxy.TaskProxyType.TaskProxyTypeName == _taskType
                                                 orderby a.ProxyDirectorID descending
                                                 select new
                                                 {
                                                     任务=b.TaskTitle,
                                                     受托人= (db.EmpInfo.SingleOrDefault(u=>u.EmpID==a.TaskProxy.ProxyFiduciary)).EmpName,
                                                     类型=a.TaskProxy.TaskProxyType.TaskProxyTypeName,
                                                     完成状态 = b.TaskState,                                          
                                                 };
                        gdvTaskInfo.DataBind();
                    }
                }

                if (_taskType.IndexOf("出库") != -1)
                {
                    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {

                        gdvTaskInfo.DataSource = from a in db.ProxyDirector
                                                 join b in db.StorageOutTask on a.TaskID equals b.TaskID
                                                 where a.TaskProxy.TaskProxyType.TaskProxyTypeName == _taskType
                                                 orderby a.ProxyDirectorID descending
                                                 select new
                                                 {
                                                     任务 = b.TaskTitle,
                                                     受托人 = (db.EmpInfo.SingleOrDefault(u => u.EmpID == a.TaskProxy.ProxyFiduciary)).EmpName,
                                                     类型 = a.TaskProxy.TaskProxyType.TaskProxyTypeName,
                                                     完成状态 = b.TaskState,
                                                 };
                        gdvTaskInfo.DataBind();
                    }
                }
                
                
            }   
            catch (Exception ex)
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", ex.Message));
            }           
        }
          
    }
}
