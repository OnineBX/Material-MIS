
/*------------------------------------------------------------------------------
 * Unit Name：SrinTaskHistoryInfo.cs
 * Description: 显示回收入库任务历史信息
 * Author: Xu Chun Lei
 * Created Date: 2010-09-07
 * ----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Configuration;


namespace MMSPro.WebApp
{
    public class SrinTaskHistoryInfo : Page
    {
        private int _workid;
        private string _taskType;
        private List<TaskStorageIn> srintasklist = new List<TaskStorageIn>();

        private MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {               

                GridView gdvTaskInfo = (GridView)Page.FindControl("gdvTaskInfo");                
                _workid = Convert.ToInt32(Request.QueryString["WorkID"]);
                _taskType = Request.QueryString["TaskType"];

                int taskid = 0;//初始任务ID
                TaskStorageIn tsi;
                switch (_taskType)
                {
                    case "物资组清点":
                        tsi = db.TaskStorageIn.SingleOrDefault(u => u.StorageInType.Equals("回收入库") && u.TaskType.Equals(_taskType) && u.StorageInID.Equals(_workid) && u.PreviousTaskID == -1);
                        if (tsi != null)
                        {
                            taskid = tsi.TaskStorageID;
                            srintasklist.Add(tsi);
                            GetTaskHistoryInfo(srintasklist, taskid);//,-1);
                        }                                                
                        break;                      
                    case "生产组安排质检":
                        taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
                        tsi = db.TaskStorageIn.SingleOrDefault(u => u.PreviousTaskID.Equals(taskid) && u.StorageInID.Equals(_workid) && u.TaskType.Equals(_taskType));                        
                        if (tsi != null)
                        {
                            taskid = tsi.TaskStorageID;
                            srintasklist.Add(tsi);
                            GetTaskHistoryInfo(srintasklist, taskid);//, _workid);
                        }
                        break;
                                                       
                }

                gdvTaskInfo.DataSource = from a in srintasklist
                                         orderby a.TaskStorageID descending
                                         select new
                                         {
                                             任务 = a.TaskTitle,
                                             发起者 = a.EmpInfo.EmpName,
                                             执行者 = a.EmpInfo1.EmpName,
                                             类型 = a.TaskType,
                                             创建时间 = a.CreateTime,
                                             任务状态 = a.TaskState,
                                         };
                gdvTaskInfo.DataBind();
                
            }   
            catch (Exception ex)
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", ex.Message));
            }           
        }

        /// <summary>
        /// 递归获得任务历史信息
        /// </summary>
        /// <param name="tasklist">保存历史任务的列表</param>
        /// <param name="parentid">父任务ID</param>
        /// <param name="workid">表单ID</param>
        private void GetTaskHistoryInfo(List<TaskStorageIn> tasklist,int parentid)//,int workid)
        {
            
            var tlist = from a in db.TaskStorageIn
                        where a.PreviousTaskID == parentid
                           //&& a.StorageInID == (workid == -1?a.StorageInID:workid)
                        select a;
            if (tlist == null)
                return;
            else
            {
                tasklist.AddRange(tlist);
                foreach (TaskStorageIn tsi in tlist)
                    GetTaskHistoryInfo(tasklist, tsi.TaskStorageID);//,workid);
            }
            
        }        
          
    }
}
