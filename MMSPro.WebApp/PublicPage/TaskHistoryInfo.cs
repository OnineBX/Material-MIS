using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Configuration;


namespace MMSPro.WebApp
{
    public class TaskHistoryInfo : System.Web.UI.Page
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
                        gdvTaskInfo.DataSource = from a in db.TaskStorageIn
                                                 where a.StorageInID == _noticeid && a.StorageInType == _taskType
                                                 orderby a.StorageInID descending
                                                 select new
                                                 {
                                                     任务=a.TaskTitle,
                                                     发起者= a.EmpInfo.EmpName,
                                                     执行者= a.EmpInfo1.EmpName,
                                                     类型=a.TaskType,
                                                     创建时间=a.CreateTime,
                                                     任务状态 = a.TaskState,
                                                     所属批次 = a.QCBatch
                                                                                                         
                                                 };
                        gdvTaskInfo.DataBind();
                    }
                }

                if (_taskType.IndexOf("出库") != -1)
                {
                    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        gdvTaskInfo.DataSource = from a in db.StorageOutTask
                                                 where a.NoticeID == _noticeid && a.Process == _taskType
                                                 orderby a.TaskID descending
                                                 select new
                                                 {
                                                     任务=a.TaskTitle,
                                                     发起者= a.EmpInfo.EmpName,
                                                     执行者= a.EmpInfo1.EmpName,
                                                     类型=a.TaskType,
                                                     创建时间=a.CreateTime,
                                                     任务状态 = a.TaskState,
       
                                                 };
                        gdvTaskInfo.DataBind();
                    }
                }
                if (_taskType.IndexOf("移库") != -1)
                {

                    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        gdvTaskInfo.DataSource = from a in db.StockTransferTask
                                                 where a.StockTransferID == _noticeid && a.TaskInType == _taskType
                                                 orderby a.StockTransferTaskID descending
                                                 select new
                                                 {
                                                     任务 = a.TaskTitle,
                                                     发起者 = a.EmpInfo.EmpName,
                                                     执行者 = a.EmpInfo1.EmpName,
                                                     类型= a.TaskType,
                                                     创建时间 = a.CreateTime,   
                                                     任务状态 = a.TaskState,
                                                     备注 = a.Remark,
                                                     审核意见 = a.AuditOpinion
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
