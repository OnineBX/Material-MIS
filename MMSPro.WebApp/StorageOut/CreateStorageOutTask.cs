/*------------------------------------------------------------------------------
 * Unit Name：CreateStorageOutTask.cs
 * Description: 创建正常出库任务页面
 * Author: Xu Chun Lei
 * Created Date: 2010-10-28
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
using System.Reflection;

namespace MMSPro.WebApp
{
    public class CreateStorageOutTask:System.Web.UI.Page
    {
        private int _taskid, _noticeid;
        private string _tasktype,_executor;
        private string strBackUrl,strFinishUrl;
        private Label lblCreatorName, lblExecutorName;

        private TextBox txtExecutor;
        private Button btnSave;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                
                string strTaskID = Request.QueryString["TaskID"];                                
                _taskid = string.IsNullOrEmpty(strTaskID) ? -1 : Convert.ToInt32(strTaskID);

                _tasktype = Request.QueryString["TaskType"];

                string strNoticeID = Request.QueryString["NoticeID"];
                _noticeid = string.IsNullOrEmpty(strNoticeID) ? -1 : Convert.ToInt32(strNoticeID);

                _executor = Request.QueryString["Executor"];
                strFinishUrl = "../../default-old.aspx";

                InitControl();
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));
            }            

        }       

        private void InitControl()
        {
            InitBar();

            this.lblCreatorName = GetControltByMaster("lblCreatorName") as Label;
            this.lblExecutorName = GetControltByMaster("lblExecutorName") as Label;
            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnSave.Click += new EventHandler(btnSave_Click);

            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化调拨通知单号
                (this.GetControltByMaster("lblNoticeCode") as Label).Text = db.StorageOutNotice.SingleOrDefault(u => u.StorageOutNoticeID == _noticeid).StorageOutNoticeCode;
            }

            //根据任务类型，设置页面显示元素
            switch (_tasktype)
            {
                case "物资出库审核":
                    lblCreatorName.Text = "资产管理员";
                    lblExecutorName.Text = "资产组长";
                    this.strBackUrl = string.Format("NormalOutAssetDetailsMessage.aspx?TaskID={0}", _taskid);                                     
                    break;
                case "物资调拨审核":
                    lblCreatorName.Text = "生产技术员";
                    lblExecutorName.Text = "生产组长";
                    if (_taskid == -1)//分支流程--没有任务的情况
                    {
                        this.strBackUrl = "ManageStorageOutNotice.aspx";
                        this.strFinishUrl = this.strBackUrl;
                    }
                    else
                        this.strBackUrl = string.Format("NormalOutProduceDetailsMessage.aspx?TaskID={0}", _taskid);                    
                    break;
                case "物资调拨审核信息":
                    lblCreatorName.Text = "生产组长";
                    lblExecutorName.Text = "生产技术员";
                    this.strBackUrl = string.Format("NormalOutProduceAuditMessage.aspx?TaskID={0}", _taskid);
                    break;
                case "物资出库审核信息":
                    lblCreatorName.Text = "资产组长";
                    lblExecutorName.Text = "资产管理员";
                    this.strBackUrl = string.Format("NormalOutAssetAuditMessage.aspx?TaskID={0}", _taskid);
                    break;
                case "物资出库":
                    lblCreatorName.Text = "生产技术员";
                    lblExecutorName.Text = "资产管理员";
                    this.strBackUrl = string.Format("NormalOutProduceAuditMessage.aspx?TaskID={0}", this._taskid);                    
                    break;
                case "主任审批":
                    lblCreatorName.Text = "物资管理员";
                    lblExecutorName.Text = "主任";
                    this.strBackUrl = string.Format("NormalOutAssetAuditInfo.aspx?TaskID={0}", _taskid);                    
                    break;
            }

            (GetControltByMaster("lblCreator") as Label).Text = SPContext.Current.Web.CurrentUser.LoginName;

            txtExecutor = GetControltByMaster("txtExecutor") as TextBox;
            //初始化选择用户脚本
            if (string.IsNullOrEmpty(_executor))
                (GetControltByMaster("ltrJS") as Literal).Text = JSDialogAid.GetJSForDialog(txtExecutor.ClientID, "../StorageAndPile/SelectUser.aspx");
            else
            {
                txtExecutor.Text = _executor;
                txtExecutor.ReadOnly = true;
            }
            
        }

        private void InitBar()
        {
            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");

            //返回
            ToolBarButton tbarbtnBack = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnBack.ID = "btnBack";
            tbarbtnBack.Text = "返回";
            tbarbtnBack.ImageUrl = "/_layouts/images/BACK.GIF";
            tbarbtnBack.Click += new EventHandler(tbarbtnBack_Click);
            tbarTop.Buttons.Controls.Add(tbarbtnBack);

            ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnRefresh.ID = "btnRefresh";
            btnRefresh.Text = "刷新";
            btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
            btnRefresh.Padding = "0,5,0,0";
            btnRefresh.Click += new EventHandler(btnRefresh_Click);
            tbarTop.RightButtons.Controls.Add(btnRefresh);
        }       

        #region 控件事件方法

        void btnRefresh_Click(object sender, EventArgs e)
        {
            
        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect(strBackUrl, false);
        }  

        public void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    //判断是否已经创建任务
                    StorageOutTask sotc = db.StorageOutTask.SingleOrDefault(u => u.NoticeID.Equals(_noticeid) && u.PreviousTaskID.Equals(_taskid) && u.Process.Equals("正常出库"));
                    if (sotc != null)
                    {
                        Response.Redirect(strFinishUrl, false);
                        return;
                    }

                    //修改完成状态                    
                    StorageOutTask soto = db.StorageOutTask.SingleOrDefault(u => u.TaskID == _taskid);
                    if (soto != null)
                    {
                        if (soto.TaskState.Equals("已完成"))//分支流程--处理IE回退(当前任务已完成并且未产生新任务的情况)
                        {
                            Response.Redirect(strFinishUrl, false);
                            return;
                        }
                        else
                            soto.TaskState = "已完成";
                    }

                    //判断用户存在与否
                    if (db.EmpInfo.Count(u => u.Account.Equals(txtExecutor.Text.Trim())) == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", string.Format("<script>alert('不存在{0}用户，请同步AD账户 ')</script>", ((Label)this.GetControltByMaster("lblExecutorName")).Text));
                        return;
                    }                    


                    //发送新任务
                    StorageOutTask sotn = new StorageOutTask();
                    sotn.Process = "正常出库";
                    sotn.TaskCreaterID = db.EmpInfo.SingleOrDefault(u => u.Account.Equals((GetControltByMaster("lblCreator") as Label).Text)).EmpID;
                    sotn.NoticeID = _noticeid;
                    sotn.TaskTargetID = db.EmpInfo.SingleOrDefault(u => u.Account.Equals(txtExecutor.Text.Trim())).EmpID;
                    sotn.TaskTitle = (this.GetControltByMaster("txtTaskTitle") as TextBox).Text.Trim();
                    sotn.TaskState = "未完成";
                    sotn.TaskDispose = "未废弃";
                    sotn.TaskType = _tasktype;
                    sotn.Remark = (GetControltByMaster("txtRemark") as TextBox).Text.Trim();
                    sotn.PreviousTaskID = _taskid;
                    sotn.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();

                    db.StorageOutTask.InsertOnSubmit(sotn);
                    db.SubmitChanges();                                        

                }

                Response.Redirect(strFinishUrl, false);
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
            }               

        }       

        #endregion

        #region 辅助方法

        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }        

        #endregion
    }
}
