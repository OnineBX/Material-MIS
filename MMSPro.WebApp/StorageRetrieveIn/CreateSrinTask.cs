/*------------------------------------------------------------------------------
 * Unit Name：CreateSrinTask.cs
 * Description: 创建回收入库任务页面
 * Author: Xu Chun Lei
 * Created Date: 2010-07-28
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
    public class CreateSrinTask: System.Web.UI.Page
    {
        private int _taskid, _workid,_executorid;
        private string _tasktype,strBackUrl,strFinishUrl;

        private Button btnSave;
        private TextBox txtExecutor,txtRemark;
        private Label lblName,lblCode, lblCreatorName, lblExecutorName;

        //private static string strUnFinishTaskType = "物资组清点,维修保养物资组长审核,生产组安排质检";//保存并发任务：在创建这些任务时，只创建任务而不结束当前任务

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                //包括主流程和分支流程--流程初始任务发起时没有任务ID的情况
                string strTaskID = Request.QueryString["TaskID"];                
                _taskid = string.IsNullOrEmpty(strTaskID) ? -1 : Convert.ToInt32(strTaskID);

                _executorid = Convert.ToInt32(Request.QueryString["Executor"]);

                _tasktype = Request.QueryString["TaskType"];
                strFinishUrl = "../../default-old.aspx";  
             
                string strWorkID = Request.QueryString["WorkID"];
                _workid = string.IsNullOrEmpty(strWorkID) ? -1 : Convert.ToInt32(strWorkID);                    
                   

                this.InitControl();
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));
            }

        }

        #region 初始化和数据绑定方法
        private void InitControl()
        {
            InitBar();

            lblName = (Label)this.GetControltByMaster("lblName");
            lblCode = (Label)this.GetControltByMaster("lblCode");
            lblCreatorName = (Label)this.GetControltByMaster("lblCreatorName");
            lblExecutorName = (Label)this.GetControltByMaster("lblExecutorName");
            txtExecutor = (TextBox)this.GetControltByMaster("txtExecutor");
            txtRemark = (TextBox)this.GetControltByMaster("txtRemark");

            this.btnSave = (Button)GetControltByMaster("btnSave");            
            this.btnSave.Click += new EventHandler(btnSave_Click);           

            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                if (_executorid != 0)//执行者已经确定的情况
                {
                    txtExecutor.ReadOnly = true;
                    txtExecutor.Text = db.EmpInfo.SingleOrDefault(u => u.EmpID == _executorid).Account;
                }
                //根据任务类型，设置页面显示元素
                switch (_tasktype)
                {
                    case "物资组清点":
                        lblCode.Text = "N/A";
                        lblCreatorName.Text = "配送组员";
                        lblExecutorName.Text = "物资组员";                        
                        if (_taskid == -1)//此时为还未存在任务，发送任务后返回上一页
                        {
                            this.strBackUrl = "ManageSrinSubDoc.aspx";
                            this.strFinishUrl = this.strBackUrl;
                        }
                        else
                            this.strBackUrl = string.Format("RiDeliverySubDetailsMessage.aspx?TaskID={0}", _taskid);                        
                        break;
                    case "物资组长确认清点结果":
                        lblCode.Text = "N/A";
                        lblCreatorName.Text = "物资组员";
                        lblExecutorName.Text = "物资组长";
                        this.strBackUrl = string.Format("RiMaterialStocktakingMessage.aspx?TaskID={0}", _taskid);
                        this.strFinishUrl = this.strBackUrl;
                        break;
                    case "资产组办理回收":
                        lblCode.Text = "N/A";
                        lblCreatorName.Text = "物资组员";
                        lblExecutorName.Text = "资产组员";
                        this.strBackUrl = string.Format("RiMChiefConfirmStocktakingMessage.aspx?TaskID={0}", _taskid);
                        break;
                    case "处理清点问题":
                        lblCode.Text = "N/A";
                        lblCreatorName.Text = "物资组员";
                        lblExecutorName.Text = "配送组员";                        
                        this.strBackUrl = string.Format("RiMaterialStocktakingMessage.aspx?TaskID={0}",_taskid);
                        txtRemark.Text = Request.QueryString["TaskInfo"];
                        break;
                    case "回收入库单资产组长确认":
                        lblCode.Text = db.SrinReceipt.SingleOrDefault(u => u.SrinReceiptID == _workid).SrinReceiptCode;
                        lblCreatorName.Text = "资产组员";
                        lblExecutorName.Text = "资产组长";
                        this.strBackUrl = string.Format("RiAssetReceiptMessage.aspx?TaskID={0}", _taskid);
                        this.strFinishUrl = this.strBackUrl;
                        break;                                          
                    case "维修保养物资组长审核":
                        lblCode.Text = db.SrinRepairPlan.SingleOrDefault(u => u.SrinRepairPlanID == _workid).SrinRepairPlanCode;
                        lblName.Text = "维修保养计划表编号";
                        lblCreatorName.Text = "物资组员";
                        lblExecutorName.Text = "物资组长";
                        bool isfirst = Convert.ToBoolean(Request.QueryString["IsFirst"]);
                        if (isfirst)
                        {
                            this.strBackUrl = string.Format("ManageRepairAndVerify.aspx?TaskID={0}", _taskid);
                            this.strFinishUrl = this.strBackUrl;
                        }
                        else
                            this.strBackUrl = string.Format("RiMaterialRepairDetailsMessage.aspx?TaskID={0}",_taskid);
                        break;
                    case "处理物资组长审核问题":
                        lblCode.Text = db.SrinRepairPlan.SingleOrDefault(u => u.SrinRepairPlanID == _workid).SrinRepairPlanCode;
                        lblName.Text = "维修保养计划表编号";
                        lblCreatorName.Text = "物资组长";
                        lblExecutorName.Text = "物资组员";                        
                        this.strBackUrl = string.Format(" RiMaterialRepairAuditMessage.aspx?TaskID={0}", _taskid);                        
                        break;                    
                    case "生产组安排质检":
                        lblCode.Text = db.SrinVerifyTransfer.SingleOrDefault(u => u.SrinVerifyTransferID == _workid).SrinVerifyTransferCode;
                        lblName.Text = "回收检验传递表编号";
                        lblCreatorName.Text = "物资组员";
                        lblExecutorName.Text = "生产组员";                         
                        this.strBackUrl = string.Format("ManageRepairAndVerify.aspx?TaskID={0}",_taskid);
                        this.strFinishUrl = this.strBackUrl;
                        break;                   
                    case "检验员质检":
                        lblCode.Text = db.SrinVerifyTransfer.SingleOrDefault(u => u.SrinVerifyTransferID == _workid).SrinVerifyTransferCode;
                        lblName.Text = "回收检验传递表编号";
                        lblCreatorName.Text = "生产组员";
                        lblExecutorName.Text = "质检员";
                        this.strBackUrl = string.Format("RiProduceArrangeVerifyMessage.aspx?TaskID={0}", _taskid);
                        break;
                    case "资产组处理修复合格物资":
                        lblCode.Text = "N/A";
                        lblName.Text = "编号";
                        lblCreatorName.Text = "检验员";
                        lblExecutorName.Text = "资产管理员";
                        this.strBackUrl = string.Format("RiInspectorVerifyRDetailsMessage.aspx?TaskID={0}", _taskid);
                        this.strFinishUrl = strBackUrl;
                        break;
                    case "资产组处理合格物资":
                        lblCode.Text = db.SrinInspectorVerifyTransfer.SingleOrDefault(u => u.TaskID == _taskid).SrinProduceVerifyTransfer.SrinVerifyTransfer.SrinVerifyTransferCode;
                        lblName.Text = "回收检验传递表编号";
                        lblCreatorName.Text = "检验员";
                        lblExecutorName.Text = "资产管理员";
                        this.strBackUrl = string.Format("RiInspectorVerifyDetailsMessage.aspx?TaskID={0}", _taskid);
                        this.strFinishUrl = strBackUrl;
                        break;
                    case "资产组长确认合格物资":
                        lblCode.Text = db.SrinQualifiedReceipt.SingleOrDefault(u => u.SrinQualifiedReceiptID == _workid).SrinQualifiedReceiptCode;
                        lblName.Text = "回收入库单(合格)编号";
                        lblCreatorName.Text = "资产管理员";
                        lblExecutorName.Text = "资产组长";
                        this.strBackUrl = string.Format("RiAssetQualifiedReceiptMessage.aspx?TaskID={0}", _taskid);
                        this.strFinishUrl = this.strBackUrl;
                        break;                    
                    case "生产组申请维修":
                        lblCode.Text = db.SrinInspectorVerifyTransfer.SingleOrDefault(u => u.TaskID == _taskid).SrinProduceVerifyTransfer.SrinVerifyTransfer.SrinVerifyTransferCode;
                        lblName.Text = "回收检验传递表编号";
                        lblCreatorName.Text = "检验员";
                        lblExecutorName.Text = "生产组员";
                        this.strBackUrl = string.Format("RiInspectorVerifyDetailsMessage.aspx?TaskID={0}", _taskid);
                        this.strFinishUrl = strBackUrl;
                        break;
                    case "检验员检验修复物资":
                        lblCode.Text = db.SrinRepairReport.SingleOrDefault(u => u.SrinRepairReportID.Equals(_workid)).SrinRepairReportCode;
                        lblName.Text = "申请修复报告编号";
                        lblCreatorName.Text = "生产组员";
                        lblExecutorName.Text = "检验员";
                        this.strBackUrl = string.Format("RiProduceApplyReportMessage.aspx?TaskID={0}", _taskid);
                        break;                          
                }
                               
            }

            //初始化任务发起者
            (GetControltByMaster("lblCreator") as Label).Text = SPContext.Current.Web.CurrentUser.LoginName;

            if(_executorid == 0)//当没有指定任务执行者的情况
                selUser(txtExecutor);            

        }       

        #endregion

        #region 控件事件方法

        void btnRefresh_Click(object sender, EventArgs e)
        {

        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect(strBackUrl, false);
        }       

        void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    //判断是否已经创建任务
                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.StorageInID.Equals(_workid) && u.TaskType.Equals(this._tasktype) && u.TaskState.Equals("未完成"));
                    if (tsi != null)
                    {
                        Response.Redirect("../../default-old.aspx", false);
                        return;
                    }

                    //修改完成状态
                    if (strFinishUrl.Equals("../../default-old.aspx"))//对于任务完成后的页面为default-old.aspx的（非并发任务），修改当前完成任务状态
                    {                        
                        TaskStorageIn tsio = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskid);
                        if (tsio != null)
                        {
                            if (tsio.TaskState.Equals("已完成"))//分支流程--处理IE回退(当前任务已完成并且未产生新任务的情况)
                            {
                                Response.Redirect("../../default-old.aspx", false);
                                return;
                            }
                            else
                                tsio.TaskState = "已完成";
                        }       
                    }                                 

                    //发送新任务
                    TaskStorageIn tsin = new TaskStorageIn();
                    tsin.StorageInType = "回收入库";
                    tsin.TaskCreaterID = db.EmpInfo.SingleOrDefault(u => u.Account == ((Label)this.GetControltByMaster("lblCreator")).Text.Trim()).EmpID;
                    tsin.StorageInID = _workid;
                    tsin.TaskTargetID = db.EmpInfo.SingleOrDefault(u => u.Account == txtExecutor.Text.Trim()).EmpID;
                    tsin.TaskTitle = ((TextBox)this.GetControltByMaster("txtTaskTitle")).Text.Trim();
                    tsin.TaskState = "未完成";
                    tsin.TaskDispose = "未废弃";
                    tsin.TaskType = _tasktype;
                    tsin.Remark = txtRemark.Text.Trim();
                    tsin.PreviousTaskID = _taskid;
                    tsin.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();

                    db.TaskStorageIn.InsertOnSubmit(tsin);
                    db.SubmitChanges();

                    //支线流程--存在主任代理的情况
                    //未完成

                }                
                Response.Redirect(strFinishUrl, false);//任务完成后显示的页面

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
        private void InitBar()
        {
            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");

            //返回
            ToolBarButton tbarbtnBack = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnBack.ID = "backRow";
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

        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }

        private void selUser(TextBox tbox_W)
        {
            ((Literal)GetControltByMaster("L1")).Text = JSDialogAid.GetJSForDialog(tbox_W.ClientID, "../StorageAndPile/SelectUser.aspx");
        }

        #endregion
    }
}
