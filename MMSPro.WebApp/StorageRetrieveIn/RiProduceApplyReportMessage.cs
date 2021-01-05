/*------------------------------------------------------------------------------
 * Unit Name：RiProduceApplyReportMessage.cs
 * Description: 回收入库--显示生产组员创建修复申请报告信息的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-09-27
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
    public class RiProduceApplyReportMessage:Page
    {
        private int _taskid, _reportid;
        private SPGridView spgvMaterial;
        private Button btnOK;
        private bool bfinished = false;

        private static string[] ShowTlist = {        
                                                  "财务编号:FinanceCode",                                                  
                                                  "物资名称:MaterialName",
                                                  "规格型号:SpecificationModel",
                                                  "生产厂家:ManufacturerName",                                                          
                                                  "修复(根/台/套/件):RepairGentaojian",
                                                  "回收单号:RetrieveCode",
                                                  "备注:Remark"
                                            };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    _reportid = db.SrinRepairReport.SingleOrDefault(u => u.TaskID.Equals(_taskid)).SrinRepairReportID;
                    if(db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID.Equals(_taskid)).TaskState.Equals("已完成"))//分支流程--任务完成的情况
                        bfinished = true;
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

            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);            

            //初始化spgvMaterial
            this.spgvMaterial = new SPGridView();
            this.spgvMaterial.AutoGenerateColumns = false;
            this.spgvMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");

            BoundField bfColumn;

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
                SrinRepairReport srrp = db.SrinRepairReport.SingleOrDefault(u => u.TaskID.Equals(_taskid));

                ((Label)GetControltByMaster("lblCode")).Text = srrp.SrinRepairReportCode.Trim();
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(srrp.CreateTime.ToLongDateString(), srrp.CreateTime.ToLongTimeString());
                ((Label)GetControltByMaster("lblProduce")).Text = srrp.EmpInfo.EmpName;
                (GetControltByMaster("lblRemark") as Label).Text = srrp.Remark.Trim();

                //初始化质检待修复物资
                spgvMaterial.DataSource = from a in db.SrinInspectorVerifyDetails
                                          where a.SrinInspectorVerifyTransferID == srrp.SrinInspectorVerifyTransferID
                                             && a.RepairGentaojian != 0
                                          select new
                                          {
                                              a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.FinanceCode,                                             
                                              a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                              a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                              a.SrinMaterialVerifyDetails.Manufacturer.ManufacturerName,
                                              a.RepairGentaojian,
                                              a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.RetrieveCode,                                             
                                              a.Remark,                                              
                                          };
                this.spgvMaterial.DataBind();

            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            if (bfinished)
            {
                btnOK.Visible = false;
                (GetControltByMaster("ltrInfo") as Literal).Visible = true;
            }

        }

        #endregion

        #region 控件事件方法

        void btnRefresh_Click(object sender, EventArgs e)
        {

        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            if (bfinished)
                Response.Redirect("../../default-old.aspx", false);
            else
                Response.Redirect(string.Format("RiProduceApplyReport.aspx?TaskID={0}", _taskid), false);
        }

        void btnOK_Click(object sender, EventArgs e)
        {                           
            Response.Redirect(string.Format("CreateSrinTask.aspx?TaskID={0}&TaskType=检验员检验修复物资&WorkID={1}", _taskid, _reportid), false);           
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
