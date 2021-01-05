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
    /// <summary>
    /// 创建出库流程任务页面
    /// </summary>
    public class StockTransferCreateTask : System.Web.UI.Page
    {
        private int _noticeid;
        private string _storageouttasktype;
        private int _auditid;
        private int _taskid;
        private int _audittype;
        private string strBackUrl;
        
        private TextBox txtExecutor;        
        Button btnSave;
        Button btnQuit;
        Button btnBack;
        Label lblMessages;
        Literal L1;


        protected void Page_Load(object sender, EventArgs e)
        {
            strBackUrl = Request.QueryString["BackUrl"];
            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnBack = (Button)GetControltByMaster("btnBack");
            this.lblMessages = (Label)this.GetControltByMaster("lblMessages");
            this.lblMessages.Visible = false;
            _noticeid = Convert.ToInt32(Request.QueryString["StockTransferID"]);
            bool bolEdite = false;
            //如果已经发送则不允许再次发送
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var n = db.StockTransferTask.Where(a => a.StockTransferID == _noticeid && a.TaskInType == "移库任务").OrderByDescending(a=>a.StockTransferTaskID);
                if (n .ToList().Count>0)
                {
                    
                    //已经有记录
                    if (n.First().TaskType != Request.QueryString["TaskType"] )
                    {
                        if (n.First().TaskType == "物资组长" && n.First().TaskState == "已完成" && n.First().AuditStatus == "审核通过")
                        {
                            bolEdite = false;
                            Response.Redirect(string.Format("../PublicPage/ErrorInfo.aspx?ErrorInfo=该任务已经结束！&DisposeUrl={0}&BackUrl={1}", strBackUrl, strBackUrl), false);
                            return;
                        }
                        else
                        {
                            //可改
                            bolEdite = true;
                        }
                    }
                    if (strBackUrl.Contains("StockTransferManager.aspx"))
                    {
                        bolEdite = false;
                        Response.Redirect(string.Format("../PublicPage/ErrorInfo.aspx?ErrorInfo=该任务已进入流程不能重复发送！&DisposeUrl={0}&BackUrl={1}", strBackUrl, strBackUrl), false);
                        return;
                    }
                }
                else
                {
                    //可改
                    bolEdite = true;
                }

              //判断是否有详表
                if (db.StockTransferDetail.Count(a => a.StockTransferID == _noticeid) <= 0)
                {
                    Response.Redirect(string.Format("../PublicPage/ErrorInfo.aspx?ErrorInfo=该调拨详单没有记录！&DisposeUrl={0}&BackUrl={1}", strBackUrl, strBackUrl), false); 
                    return;
                }

              

            }

            


            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
            this.btnBack.Click += new EventHandler(btnQuit_Click);
            this.InitControl();
            //显示和隐藏
            if(!bolEdite)
                Response.Redirect(string.Format("../PublicPage/ErrorInfo.aspx?ErrorInfo=该任务已经发送！&DisposeUrl={0}&BackUrl={1}", strBackUrl, strBackUrl), false);
            

            ((Panel)this.GetControltByMaster("panelHide")).Visible = bolEdite;
            ((Panel)this.GetControltByMaster("panelSec")).Visible = !bolEdite;
            //if (!IsPostBack)
            //{
            //    ViewState["BackUrl"] = Request.UrlReferrer.ToString();
            //}

        }

        #region 初始化和数据绑定方法

        private void InitControl()
        {            


            ((Label)this.GetControltByMaster("lblCreator")).Text = SPContext.Current.Web.CurrentUser.LoginName;
           
            txtExecutor = (TextBox)this.GetControltByMaster("txtExecutor");
            //如果是发起人确认就自动填写
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {

                if (Request.QueryString["TaskType"] == "发起人确认")
                {
                    var t = db.StockTransferTask.Where(a => a.StockTransferID == _noticeid && a.TaskInType == "移库任务").OrderBy(a => a.StockTransferTaskID).First();
                    txtExecutor.Enabled = false;
                    txtExecutor.Text = t.EmpInfo.Account;
                }
                else
                {

                    selUser(txtExecutor);
                }

            }
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {

                //初始化调拨通知单号
                ((Label)this.GetControltByMaster("lblStorageInNoticeCode")).Text = db.StockTransfer.SingleOrDefault(u => u.StockTransferID == _noticeid).StockTransferNum;

                //初始化任务发起者
                ((Label)this.GetControltByMaster("lblCreator")).Text = SPContext.Current.Web.CurrentUser.LoginName;
                                              
            }

        }

        #endregion        

        #region 控件事件方法

        public void btnSave_Click(object sender, EventArgs e)
        {
            using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //修改上一条记录
                StockTransferTask st = dc.StockTransferTask.SingleOrDefault(a => a.StockTransferID == Convert.ToInt32(Request.QueryString["StockTransferID"]) && a.TaskInType == "移库任务"&& a.TaskState == "未完成");
                if(st!=null)
                    st.TaskState = "已完成";

                //创建下一条记录
                StockTransferTask stt = new StockTransferTask();
                stt.Remark = ((TextBox)this.GetControltByMaster("txtRemark")).Text.Trim();
                stt.StockTransferID = Convert.ToInt32(Request.QueryString["StockTransferID"]);
                stt.TaskCreaterID = reEmpId(((Label)this.GetControltByMaster("lblCreator")).Text.Trim());
                stt.TaskInType = "移库任务";
                stt.TaskState = "未完成";
                stt.TaskTargetID = reEmpId(txtExecutor.Text.Trim());
                stt.TaskTitle = ((TextBox)this.GetControltByMaster("txtTaskTitle")).Text.Trim();
                stt.TaskType = Request.QueryString["TaskType"];
                stt.CreateTime = dc.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                dc.StockTransferTask.InsertOnSubmit(stt);
                dc.SubmitChanges();
                Response.Redirect(strBackUrl);
            }

        }

        public void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect(strBackUrl);
        }   

#endregion

        #region 辅助方法

        private int reEmpId(string Emp)
        {
            int valueEmp = 0;
            using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                EmpInfo EI = dc.EmpInfo.SingleOrDefault(u => u.Account == Emp);
                if (EI != null)
                {
                    valueEmp = EI.EmpID;
                }

            }

            return valueEmp;
        }

        private void selUser(TextBox tbox_W)
        {
            L1 = (Literal)GetControltByMaster("L1");
            L1.Text = JSDialogAid.GetJSForDialog(tbox_W.ClientID, "../StorageAndPile/SelectUser.aspx");
        }

        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }

        #endregion           
    }
}