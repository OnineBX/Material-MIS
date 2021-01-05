/*------------------------------------------------------------------------------
 * Unit Name：CreateRepairOrVerify.cs
 * Description: 回收入库--物资管理员创建维修保养表和回收检验表的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-08-13
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
using System.Data.Linq.SqlClient;

namespace MMSPro.WebApp
{
    public class CreateRepairOrVerify:Page
    {        
        private int _taskid,_receiptid,_workid;
        private string _type;
        private Button btnSave;
        private CustomValidator vldCode;
        private TextBox txtCode,txtRemark;
        private DateTimeControl dtcCreateTime;
        private CheckBox chbReadWork;

        protected void Page_Load(object sender, EventArgs e)
        {
            _taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
            _type = Request.QueryString["Type"];
            _workid = Convert.ToInt32(Request.QueryString["WorkID"]);

            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    _receiptid = db.SrinReceipt.SingleOrDefault(u => u.SrinStocktakingConfirm.SrinStocktaking.TaskID.Equals(_taskid)).SrinReceiptID;

                }

                InitializeCustomControls();
                BindDataToCustomControls();
                ShowCustomControls();
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
        }

        #region 初始化和数据绑定方法

        private void InitializeCustomControls()
        {
            InitToolBar();

            chbReadWork = GetControltByMaster("chbReadWork") as CheckBox;

            btnSave = (Button)GetControltByMaster("btnSave");
            btnSave.Click += new EventHandler(btnSave_Click);

            vldCode = (CustomValidator)GetControltByMaster("vldCode");
            vldCode.ServerValidate += new ServerValidateEventHandler(vldCode_ServerValidate);

            txtCode = GetControltByMaster("txtCode") as TextBox;
            txtRemark = GetControltByMaster("txtRemark") as TextBox;
            dtcCreateTime = GetControltByMaster("dtcCreateTime") as DateTimeControl;

            switch (_type)
            {
                case "维修保养表":
                    ((Label)GetControltByMaster("lblCodeName")).Text = "维修保养计划表编号";
                    break;
                case"回收检验表":
                    ((Label)GetControltByMaster("lblCodeName")).Text = "回收检验传递表编号";
                    chbReadWork.Visible = true;
                    break;
            }
        }        
        

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                ((Label)GetControltByMaster("lblCode")).Text = db.SrinReceipt.SingleOrDefault(u => u.SrinReceiptID == _receiptid).SrinReceiptCode;
                ((Label)GetControltByMaster("lblCreator")).Text = SPContext.Current.Web.CurrentUser.LoginName;
                dtcCreateTime.SelectedDate = DateTime.Now;

                if (!Page.IsPostBack && _workid != 0)
                {
                    switch (_type)
                    {
                        case "维修保养表":
                            SrinRepairPlan srp = db.SrinRepairPlan.SingleOrDefault(u => u.SrinRepairPlanID == _workid);
                            txtCode.Text = srp.SrinRepairPlanCode;
                            txtRemark.Text = srp.Remark;
                            dtcCreateTime.SelectedDate = srp.CreateTime;
                            break;
                        case "回收检验表":
                            SrinVerifyTransfer svt = db.SrinVerifyTransfer.SingleOrDefault(u => u.SrinVerifyTransferID == _workid);
                            txtCode.Text = svt.SrinVerifyTransferCode;
                            txtRemark.Text = svt.Remark;
                            dtcCreateTime.SelectedDate = svt.CreateTime;
                            chbReadWork.Checked = svt.ReadyWorkIsFinished;
                            break;
                    }
                }
            }
            
        }

        private void ShowCustomControls()
        {
            if (_workid != 0)
                btnSave.Text = "修改";
        }

        private void InitToolBar()
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


        #endregion

        #region 控件事件方法

        void btnRefresh_Click(object sender, EventArgs e)
        {
            
        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("ManageRepairAndVerify.aspx?TaskID={0}",_taskid),false);
        }

        void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid)
                {                    
                    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        if (_workid == 0)//新建情况
                        {
                            switch (_type)
                            {
                                case "维修保养表":
                                    SrinRepairPlan srp = new SrinRepairPlan();
                                    srp.SrinRepairPlanCode = txtCode.Text.Trim();
                                    srp.SrinReceiptID = _receiptid;
                                    srp.Remark = txtRemark.Text.Trim();
                                    srp.CreateTime = dtcCreateTime.SelectedDate;
                                    srp.Creator = db.EmpInfo.SingleOrDefault(u => u.Account.Equals(SPContext.Current.Web.CurrentUser.LoginName)).EmpID;
                                    srp.TaskID = _taskid;
                                    db.SrinRepairPlan.InsertOnSubmit(srp);
                                    break;
                                case "回收检验表":
                                    SrinVerifyTransfer svt = new SrinVerifyTransfer();
                                    svt.SrinVerifyTransferCode = txtCode.Text.Trim();
                                    svt.SrinReceiptID = _receiptid;
                                    svt.Remark = txtRemark.Text.Trim();
                                    svt.ReadyWorkIsFinished = chbReadWork.Checked;
                                    svt.CreateTime = dtcCreateTime.SelectedDate;
                                    svt.Creator = db.EmpInfo.SingleOrDefault(u => u.Account.Equals(SPContext.Current.Web.CurrentUser.LoginName)).EmpID;
                                    svt.TaskID = _taskid;
                                    db.SrinVerifyTransfer.InsertOnSubmit(svt);
                                    break;
                            }
                        }
                        else//修改情况
                        {
                            switch (_type)
                            {
                                case "维修保养表":
                                    SrinRepairPlan srp = db.SrinRepairPlan.SingleOrDefault(u => u.SrinRepairPlanID == _workid);
                                    srp.SrinRepairPlanCode = txtCode.Text.Trim();                                    
                                    srp.Remark = txtRemark.Text.Trim();
                                    srp.CreateTime = dtcCreateTime.SelectedDate;                                   
                                    break;
                                case "回收检验表":
                                    SrinVerifyTransfer svt = db.SrinVerifyTransfer.SingleOrDefault(u => u.SrinVerifyTransferID == _workid);
                                    svt.SrinVerifyTransferCode = txtCode.Text.Trim();                                    
                                    svt.Remark = txtRemark.Text.Trim();
                                    svt.CreateTime = dtcCreateTime.SelectedDate;
                                    svt.ReadyWorkIsFinished = chbReadWork.Checked;
                                    break;
                            }
                        }                        

                        db.SubmitChanges();
                    }
                    Response.Redirect(string.Format("ManageRepairAndVerify.aspx?TaskID={0}&ReceiptID={1}", _taskid,_receiptid), false);
                }
               
                
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
            }
        }

        void vldCode_ServerValidate(object source, ServerValidateEventArgs args)
        {
            string strCode = txtCode.Text.Trim();
            if (string.IsNullOrEmpty(strCode))
            {
                args.IsValid = false;
                vldCode.Text = "编号不能为空！";
                return;
            }
            if (_workid == 0)
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    switch (_type)
                    {
                        case "维修保养表":
                            if (db.SrinRepairPlan.Count(u => u.SrinRepairPlanCode.Equals(strCode)) != 0)
                            {
                                args.IsValid = false;
                                vldCode.Text = "维修保养计划表编号已存在，请重新输入！";
                                txtCode.Text = string.Empty;
                            }
                            break;
                        case "回收检验表":
                            if (db.SrinVerifyTransfer.Count(u => u.SrinVerifyTransferCode.Equals(strCode)) != 0)
                            {
                                vldCode.Text = "回收检验传递表编号已存在，请重新输入！";
                                txtCode.Text = string.Empty;
                                args.IsValid = false;
                            }
                            break;
                    }
                }
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
