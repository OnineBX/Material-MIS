/*------------------------------------------------------------------------------
 * Unit Name：SrinDispatchCenter.cs
 * Description: 回收入库--任务发送处理中心
 * Author: Xu Chun Lei
 * Created Date: 2010-08-19
 * ----------------------------------------------------------------------------*/
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

namespace MMSPro.WebApp
{
    public class SrinDispatchCenter:Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                string strBackUrl, strDisposeUrl;
                int formid = Convert.ToInt32(Request.QueryString["FormID"]);               
                
                int taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
                string tasktype = Request.QueryString["TaskType"];
                switch (tasktype)
                {
                    case "物资组清点":
                        strBackUrl = "../StorageRetrieveIn/ManageSrinSubDoc.aspx";
                        //回收入库单中没有物资的情况
                        if (db.SrinSubDetails.Count(u => u.SrinSubDocID == formid) == 0)
                        {
                            strDisposeUrl = string.Format("../StorageRetrieveIn/SelectSrinSubDetails.aspx?SubDocID={0}", formid);
                            Response.Redirect(string.Format("../PublicPage/ErrorInfo.aspx?ErrorInfo=该回收分单中没有物资明细，不能提交清点！&DisposeUrl={0}&BackUrl={1}", HttpUtility.UrlEncode(strDisposeUrl), strBackUrl), false);
                            return;
                        }
                        //已经发送物资组清点的情况
                        int icount = db.TaskStorageIn.Count(u => u.PreviousTaskID == -1 && u.StorageInID == formid && u.TaskType.Equals(tasktype));//已经发送物资组清点的任务
                        if (icount != 0)
                        {
                            Response.Redirect(string.Format("../PublicPage/ErrorInfo.aspx?ErrorInfo=该回收分单已经提交物资组清点，不能重复提交！&DisposeUrl={0}&BackUrl={1}", strBackUrl, strBackUrl), false);
                            return;
                        }
                        int executor = Convert.ToInt32(Request.QueryString["Executor"]);
                        Response.Redirect(string.Format("CreateSrinTask.aspx?WorkID={0}&TaskType={1}&Executor={2}", formid, tasktype, executor), false);
                        break;
                    case "维修保养物资组长审核":
                        //维修保养计划表中没有物资的情况
                        strBackUrl = string.Format("../StorageRetrieveIn/ManageRepairAndVerify.aspx?TaskID={0}", taskid);
                        if (db.SrinMaterialRepairDetails.Count(u => u.SrinRepairPlanID == formid) == 0)
                        {                           
                            strDisposeUrl = HttpUtility.UrlEncode(string.Format("../StorageRetrieveIn/SelectRepairOrVerifyDetails.aspx?FormID={0}&Type=维修保养", formid));
                            string strUrl = string.Format("../PublicPage/ErrorInfo.aspx?ErrorInfo=该维修保养计划表中没有物资明细，不能发送物资组！&DisposeUrl={0}&BackUrl={1}", strDisposeUrl, strBackUrl);
                            Response.Redirect(strUrl, false);
                            return;
                        }
                        //已经发送审核的情况
                        if (db.TaskStorageIn.Count(u => u.PreviousTaskID.Equals(taskid) && u.TaskType.Equals(tasktype) && u.StorageInID.Equals(formid)) != 0)
                        {                            
                            string strUrl = string.Format("../PublicPage/ErrorInfo.aspx?ErrorInfo=该维修保养计划表已经提交物资组长审核，不能重复提交！&DisposeUrl={0}&BackUrl={1}", strBackUrl, strBackUrl);
                            Response.Redirect(strUrl, false);
                            return;
                        }
                        Response.Redirect(string.Format("CreateSrinTask.aspx?TaskID={0}&WorkID={1}&TaskType={2}&IsFirst=True", taskid, formid, tasktype));
                        break;
                    case "生产组安排质检":
                        strBackUrl = string.Format("../StorageRetrieveIn/ManageRepairAndVerify.aspx?TaskID={0}", taskid);
                        //回收检验传递表未完成检验准备工作的情况
                        if (!db.SrinVerifyTransfer.SingleOrDefault(u => u.SrinVerifyTransferID.Equals(formid)).ReadyWorkIsFinished)
                        {
                            string strUrl = string.Format("../PublicPage/ErrorInfo.aspx?ErrorInfo=该回收检验传递表尚未完成检验准备工作，不能提交生产组！&DisposeUrl={0}&BackUrl={1}", strBackUrl, strBackUrl);
                            Response.Redirect(strUrl, false);
                            return;
                        }
                        //回收检验传递表中没有物资的情况                        
                        if (db.SrinMaterialVerifyDetails.Count(u => u.SrinVerifyTransferID == formid) == 0)
                        {
                            strDisposeUrl = HttpUtility.UrlEncode(string.Format("../StorageRetrieveIn/SelectRepairOrVerifyDetails.aspx?FormID={0}&Type=回收检验", formid));
                            string strUrl = string.Format("../PublicPage/ErrorInfo.aspx?ErrorInfo=该回收检验传递表中没有物资明细，不能发送生产组！&DisposeUrl={0}&BackUrl={1}", strDisposeUrl, strBackUrl);
                            Response.Redirect(strUrl, false);
                            return;
                        }
                        //已经发送审核的情况
                        if (db.TaskStorageIn.Count(u => u.PreviousTaskID.Equals(taskid) && u.TaskType.Equals(tasktype) && u.StorageInID.Equals(formid))!= 0)
                        {
                            string strUrl = string.Format("../PublicPage/ErrorInfo.aspx?ErrorInfo=该回收检验传递表已经提交生产组，不能重复提交！&DisposeUrl={0}&BackUrl={1}", strBackUrl, strBackUrl);
                            Response.Redirect(strUrl, false);
                            return;
                        }
                        Response.Redirect(string.Format("CreateSrinTask.aspx?TaskID={0}&WorkID={1}&TaskType={2}", taskid, formid, tasktype));
                        break;
                }
            }
        }        
    }
}
