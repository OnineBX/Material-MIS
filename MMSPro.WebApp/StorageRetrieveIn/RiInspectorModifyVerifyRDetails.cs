/*------------------------------------------------------------------------------
 * Unit Name：RiInspectorVerifyRDetails.cs
 * Description: 回收入库--检验员检验修复物资的页面
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
    public class RiInspectorModifyVerifyRDetails:Page
    {
        private int _taskid, _reportid;
        private SPGridView spgvMaterial;
        private Button btnOK;

        private static string[] ShowTlist = {                                                                                                                                  
                                                          "物资名称:MaterialName",
                                                          "规格型号:SpecificationModel",                                     
                                                          "生产厂家:ManufacturerName",  
                                                          "修复(根/台/套/件):RepairGentaojian",
                                                          "回收单号:RetrieveCode",
                                                          "SrinInspectorVerifyRDetailsID:SrinInspectorVerifyRDetailsID"
                                                     };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    _reportid = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID.Equals(_taskid)).StorageInID;
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

            //加入合格数量列
            TemplateField tfQualified = new TemplateField();
            tfQualified.HeaderText = "合格数量";
            tfQualified.ItemTemplate = new TextBoxTemplate("Qualified", "QualifiedGentaojian", "^(-?\\d+)(\\.\\d+)?$", 80);
            this.spgvMaterial.Columns.Insert(4, tfQualified);

            //加入待报废列
            TemplateField tfReject = new TemplateField();
            tfReject.HeaderText = "待报废数量";
            tfReject.ItemTemplate = new TextBoxTemplate("Reject", "RejectGentaojian", "^(-?\\d+)(\\.\\d+)?$", 80);
            this.spgvMaterial.Columns.Insert(5, tfReject);

            //加入质检报告号列
            TemplateField tfRetrieveCode = new TemplateField();
            tfRetrieveCode.HeaderText = "质检报告号";
            tfRetrieveCode.ItemTemplate = new TextBoxTemplate("VerifyCode", DataControlRowType.DataRow, "VerifyCode");
            this.spgvMaterial.Columns.Insert(6, tfRetrieveCode);           

            //加入质检日期列
            TemplateField tfRealVerifyDate = new TemplateField();
            tfRealVerifyDate.HeaderText = "质检日期";
            tfRealVerifyDate.ItemTemplate = new DateTimeTemplate("VerifyTime");
            this.spgvMaterial.Columns.Insert(7, tfRealVerifyDate);

            //加入备注列            
            TemplateField tfRemark = new TemplateField();
            tfRemark.HeaderText = "备注";
            tfRemark.ItemTemplate = new TextBoxTemplate("备注", DataControlRowType.DataRow, "Remark");
            this.spgvMaterial.Columns.Insert(8, tfRemark);


        }        

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息
                SrinRepairReport srrp = db.SrinRepairReport.SingleOrDefault(u => u.TaskID.Equals(GetPreviousTaskID(0, _taskid)));

                ((Label)GetControltByMaster("lblCode")).Text = srrp.SrinRepairReportCode.Trim();
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(srrp.CreateTime.ToLongDateString(), srrp.CreateTime.ToLongTimeString());
                ((Label)GetControltByMaster("lblProduce")).Text = srrp.EmpInfo.EmpName;
                (GetControltByMaster("lblRemark") as Label).Text = srrp.Remark.Trim();

                //初始化待修复明细
                spgvMaterial.DataSource = from a in db.SrinInspectorVerifyRDetails
                                          where a.SrinInspectorVerifyDetails.SrinInspectorVerifyTransferID == srrp.SrinInspectorVerifyTransferID                                             
                                          select new
                                          {
                                              a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                              a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                              a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.Manufacturer.ManufacturerName,
                                              a.SrinInspectorVerifyDetails.RepairGentaojian,
                                              a.QualifiedGentaojian,
                                              a.RejectGentaojian,                                              
                                              a.VerifyCode,
                                              a.VerifyTime,
                                              a.Remark,
                                              a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.RetrieveCode,
                                              a.SrinInspectorVerifyRDetailsID
                                          };
                this.spgvMaterial.DataBind();

            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            spgvMaterial.Columns[10].Visible = false;

        }

        #endregion

        #region 控件事件方法

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

                //将质检结果保存到数据库
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    int creatorid = db.EmpInfo.SingleOrDefault(u => u.Account == SPContext.Current.Web.CurrentUser.LoginName).EmpID;
                    SrinInspectorVerifyRDetails sivrd;
                    foreach (GridViewRow gvr in spgvMaterial.Rows)
                    {
                        sivrd = db.SrinInspectorVerifyRDetails.SingleOrDefault(u => u.SrinInspectorVerifyRDetailsID.Equals(Convert.ToInt32(gvr.Cells[10].Text)));                        
                        sivrd.QualifiedGentaojian = Convert.ToDecimal((gvr.Cells[4].Controls[0] as TextBox).Text.Trim());
                        sivrd.RejectGentaojian = Convert.ToDecimal((gvr.Cells[5].Controls[0] as TextBox).Text.Trim());
                        sivrd.VerifyCode = (gvr.Cells[6].Controls[0] as TextBox).Text.Trim();
                        sivrd.VerifyTime = (gvr.Cells[7].Controls[0] as DateTimeControl).SelectedDate;
                        sivrd.Remark = (gvr.Cells[8].Controls[0] as TextBox).Text.Trim();
                        sivrd.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();                                         
                    }
                    db.SubmitChanges();     
                    Response.Redirect(string.Format("RiInspectorVerifyRDetailsMessage.aspx?TaskID={0}", _taskid), false);

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
