/*------------------------------------------------------------------------------
 * Unit Name：RiProduceApplyReport.cs
 * Description: 回收入库--生产组员创建修复申请报告的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-09-26
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
    public class RiProduceApplyReport:Page
    {
        private int _taskid, _transferid;
        private SPGridView spgvMaterial;
        private Button btnOK;
        private CustomValidator vldCode;
        private TextBox txtCode,txtRemark;
        private DateTimeControl dtcCreateTime;
        private bool bcreated;

        private static string[] ShowTlist = {        
                                                  "财务编号:FinanceCode",                                                  
                                                  "物资名称:MaterialName",
                                                  "规格型号:SpecificationModel",                                     
                                                  "生产厂家:ManufacturerName",                                                          
                                                  "待修复(根/台/套/件):RepairGentaojian",
                                                  "回收单号:RetrieveCode",
                                                  "检验报告号:VerifyCode",                                                  
                                                  "SrinInspectorVerifyDetailsID:SrinInspectorVerifyDetailsID"
                                            };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskid);
                    //分支流程--任务已经完成的情况
                    if (tsi.TaskState.Equals("已完成"))
                    {
                        Response.Redirect(string.Format("RiProduceApplyReportMessage.aspx?TaskID={0}", _taskid), false);
                        return;
                    }

                    //分支流程--已经生成回收入库单(合格)的情况
                    if (db.SrinRepairReport.Count(u => u.TaskID.Equals(_taskid)) != 0)
                        bcreated = true;
                    _transferid = tsi.StorageInID;

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

        private void InitializeCustomControls()
        {
            InitToolBar();

            dtcCreateTime = GetControltByMaster("dtcCreateTime") as DateTimeControl;
            txtRemark = GetControltByMaster("txtRemark") as TextBox;
            txtCode = GetControltByMaster("txtCode") as TextBox;

            vldCode = GetControltByMaster("vldCode") as CustomValidator;
            vldCode.ServerValidate += new ServerValidateEventHandler(vldCode_ServerValidate);

            btnOK = GetControltByMaster("btnOK") as Button;
            btnOK.Click += new EventHandler(btnOK_Click);

            BoundField bfColumn;

            //初始化spgvQualifiedMaterial
            this.spgvMaterial = new SPGridView();
            this.spgvMaterial.AutoGenerateColumns = false;
            this.spgvMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");

            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvMaterial.Columns.Add(bfColumn);
            }                       
        }

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息
                SrinInspectorVerifyTransfer sivt = db.SrinInspectorVerifyTransfer.SingleOrDefault(u => u.TaskID == GetPreviousTaskID(0, _taskid));

                ((Label)GetControltByMaster("lblProject")).Text = sivt.SrinProduceVerifyTransfer.SrinVerifyTransfer.SrinReceipt.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.ProjectInfo.ProjectName;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(sivt.CreateTime.ToLongDateString(), sivt.CreateTime.ToLongTimeString());
                ((Label)GetControltByMaster("lblInspector")).Text = sivt.EmpInfo.EmpName;

                //初始化质检待修复物资
                spgvMaterial.DataSource = from a in db.SrinInspectorVerifyDetails
                                          where a.SrinInspectorVerifyTransferID == _transferid
                                             && a.RepairGentaojian != 0
                                          select new
                                          {
                                              a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.FinanceCode,                                              
                                              a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                              a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                              a.SrinMaterialVerifyDetails.Manufacturer.ManufacturerName,
                                              a.RepairGentaojian,                                              
                                              a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.RetrieveCode,
                                              a.VerifyCode,
                                              a.Remark,
                                              a.SrinInspectorVerifyDetailsID
                                          };
                this.spgvMaterial.DataBind();

                if (bcreated)//分支流程--已经生成报告的情况
                {
                    if (!Page.IsPostBack)
                    {
                        SrinRepairReport srrp = db.SrinRepairReport.SingleOrDefault(u => u.TaskID.Equals(_taskid));
                        txtCode.Text = srrp.SrinRepairReportCode;
                        txtRemark.Text = srrp.Remark;
                        dtcCreateTime.SelectedDate = srrp.CreateTime;
                    }
                }
            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            spgvMaterial.Columns[7].Visible = false;

            if (bcreated)//分支流程--已经生成报告的情况
                btnOK.Text = "修改申请报告";

        }

        #endregion

        #region 控件事件方法

        void vldCode_ServerValidate(object source, ServerValidateEventArgs args)
        {
            try
            {
                string strCode = this.txtCode.Text.Trim();

                //NoticeID为空的情况
                if (string.IsNullOrEmpty(strCode))
                {
                    args.IsValid = false;
                    vldCode.Text = "修复报告编号不能为空！";
                    return;
                }

                //数据库中存在相同NoticeCode的情况
                if (!bcreated)
                {
                    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        if (db.SrinRepairReport.Count(u => u.SrinRepairReportCode.Equals(strCode)) != 0)
                        {
                            args.IsValid = false;
                            vldCode.Text = "修复单编号已存在！";
                            return;
                        }
                    }
                }

                args.IsValid = true;                   

            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
            }

        }

        void btnRefresh_Click(object sender, EventArgs e)
        {

        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("../../default-old.aspx", false);
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid)
                {
                    //将确认结果保存到数据库
                    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        SrinRepairReport srrp;
                        //生成修复申请报告
                        if (bcreated)//分支流程--已经生成报告的情况
                        {
                            srrp = db.SrinRepairReport.SingleOrDefault(u => u.TaskID.Equals(_taskid));
                            srrp.CreateTime = dtcCreateTime.SelectedDate;
                            srrp.Remark = txtRemark.Text.Trim();
                            srrp.SrinRepairReportCode = txtCode.Text.Trim();
                        }
                        else
                        {
                            srrp = new SrinRepairReport();
                            srrp.SrinInspectorVerifyTransferID = _transferid;
                            srrp.Remark = txtRemark.Text.Trim();
                            srrp.SrinRepairReportCode = txtCode.Text.Trim();
                            srrp.TaskID = _taskid;
                            srrp.CreateTime = dtcCreateTime.SelectedDate;
                            srrp.Creator = db.EmpInfo.SingleOrDefault(u => u.Account.Equals(SPContext.Current.Web.CurrentUser.LoginName)).EmpID;
                            db.SrinRepairReport.InsertOnSubmit(srrp);
                        }
                        db.SubmitChanges();                        

                        Response.Redirect(string.Format("RiProduceApplyReportMessage.aspx?TaskID={0}", _taskid), false);

                    }
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

        #endregion

        #region 辅助方法

        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }

        private int GetPreviousTaskID(int step, int taskid)
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                int tid = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == taskid).PreviousTaskID.Value;
                if (step == 0)
                    return tid;
                return GetPreviousTaskID(--step, tid);
            }
        }        

        #endregion
    }
}
