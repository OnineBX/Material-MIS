/*------------------------------------------------------------------------------
 * Unit Name：RiAssetQualifiedReceipt.cs
 * Description: 回收入库--物资组处理合格物资的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-09-17
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
    public class RiAssetQualifiedReceipt:Page
    {
        private int _taskid, _transferid;
        private string _tasktype;
        private SPGridView spgvMaterial;
        private Button btnOK;
        private CustomValidator vldCode;
        private TextBox txtCode;

        private static string[] ShowTlist = {                                                          
                                                  "生产厂家:ManufacturerName",                                                   
                                                  "出库单价(原):UnitPrice",
                                                  "回收单号:RetrieveCode",
                                                  "检验报告号:VerifyCode",
                                                  "计量单位:CurUnit",
                                                  "ManufactureID:ManufacturerID",
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
                        Response.Redirect(string.Format("RiAssetStorageDetails.aspx?TaskID={0}", _taskid), false);
                        return;
                    }

                    //分支流程--已经发送确认任务的情况
                    if (db.TaskStorageIn.Count(u => u.PreviousTaskID.Equals(_taskid) && u.TaskType.Equals("资产组长确认合格物资")) != 0)
                    {
                        Response.Redirect(string.Format("RiAssetQualifiedReceiptMessage.aspx?TaskID={0}", _taskid), false);
                        return;
                    }

                    //分支流程--已经生成回收入库单(合格)的情况
                    if (db.SrinQualifiedReceipt.Count(u => u.TaskID.Equals(_taskid)) != 0)
                    {
                        Response.Redirect(string.Format("RiAssetModifyQualifiedReceipt.aspx?TaskID={0}", _taskid), false);
                        return;
                    }
                    _transferid = tsi.StorageInID;
                    _tasktype = tsi.TaskType;
                    
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
            this.spgvMaterial.AllowGrouping = true;
            this.spgvMaterial.AllowGroupCollapse = true;
            this.spgvMaterial.GroupDescriptionField = "Description";
            this.spgvMaterial.GroupField = "MaterialName";
            this.spgvMaterial.GroupFieldDisplayName = "回收检验合格物资";    

            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvMaterial.Columns.Add(bfColumn);
            }

            //加入回收数量--根/台/套/件列
            TemplateField tfGentaojian = new TemplateField();
            tfGentaojian.HeaderText = "根/台/套/件";
            tfGentaojian.ItemTemplate = new TextBoxTemplate("QualifiedGentaojian", string.Empty, "^(-?\\d+)(\\.\\d+)?$","0", 80);
            this.spgvMaterial.Columns.Insert(1, tfGentaojian);

            //加入回收数量--米列
            TemplateField tfMetre = new TemplateField();
            tfMetre.HeaderText = "米";
            tfMetre.ItemTemplate = new TextBoxTemplate("QualifiedMetre", string.Empty, "^(-?\\d+)(\\.\\d+)?$","0", 80);
            this.spgvMaterial.Columns.Insert(2, tfMetre);

            //加入回收数量--吨列
            TemplateField tfTon = new TemplateField();
            tfTon.HeaderText = "吨";
            tfTon.ItemTemplate = new TextBoxTemplate("QualifiedTon", string.Empty, "^(-?\\d+)(\\.\\d+)?$","0", 80);
            this.spgvMaterial.Columns.Insert(3, tfTon);

            //加入单价(入库)列
            TemplateField tfUnitPrice = new TemplateField();
            tfUnitPrice.HeaderText = "入库单价(新)";
            tfUnitPrice.ItemTemplate = new TextBoxTemplate("InUnitPrice",string.Empty,"^(-?\\d+)(\\.\\d+)?$","0", 80);
            this.spgvMaterial.Columns.Insert(5, tfUnitPrice);

            //加入备注列            
            TemplateField tfRemark = new TemplateField();
            tfRemark.HeaderText = "备注";
            tfRemark.ItemTemplate = new TextBoxTemplate("备注", DataControlRowType.DataRow, "Remark");
            this.spgvMaterial.Columns.Insert(9, tfRemark);

        }        

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息
                SrinInspectorVerifyTransfer sivt = db.SrinInspectorVerifyTransfer.SingleOrDefault(u => u.SrinInspectorVerifyTransferID.Equals(_transferid));

                ((Label)GetControltByMaster("lblProject")).Text = sivt.SrinProduceVerifyTransfer.SrinVerifyTransfer.SrinReceipt.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.ProjectInfo.ProjectName;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(sivt.CreateTime.ToLongDateString(), sivt.CreateTime.ToLongTimeString());
                ((Label)GetControltByMaster("lblInspector")).Text = sivt.EmpInfo.EmpName; 

                //初始化质检合格物资
                if (_tasktype.Equals("资产组处理合格物资"))
                {
                    this.spgvMaterial.DataSource = (from a in db.StorageOutRealDetails
                                                   where a.StorageOutNotice.ProjectID == sivt.SrinProduceVerifyTransfer.SrinVerifyTransfer.SrinReceipt.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.ProjectInfo.ProjectID
                                                   && (from b in db.SrinInspectorVerifyDetails
                                                       where b.SrinInspectorVerifyTransferID.Equals(_transferid) && b.QualifiedGentaojian != 0
                                                       select b.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialID).Contains(a.StorageOutDetails.MaterialID)
                                                   join c in db.SrinInspectorVerifyDetails on new { a.StorageOutDetails.MaterialID, TransferID = _transferid } equals new { c.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialID, TransferID = c.SrinInspectorVerifyTransferID }
                                                    join d in db.StorageStocks on new { a.StorageOutDetails.MaterialID,Status = a.MaterialStatus, a.StocksID } equals new { d.MaterialID,d.Status, d.StocksID }
                                                   orderby a.StorageOutDetails.MaterialID, a.CreateTime ascending
                                                   select new
                                                   {
                                                       MaterialName = string.Format("{0}--规格型号：{1}", d.MaterialName, d.SpecificationModel),
                                                       d.ManufacturerName,                                                       
                                                       d.UnitPrice,
                                                       d.CurUnit,
                                                       c.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.RetrieveCode,
                                                       c.VerifyCode,
                                                       c.SrinInspectorVerifyDetailsID,
                                                       c.Remark,                                                       
                                                       d.ManufacturerID,
                                                       Description = string.Format("财务编码：{0}--回收合格数量(根/台/套/件)：{1}",d.FinanceCode,c.QualifiedGentaojian)

                                                   }).Distinct();
                }
                else
                {
                    spgvMaterial.DataSource = (from a in db.StorageOutRealDetails
                                               where a.StorageOutNotice.ProjectID == sivt.SrinProduceVerifyTransfer.SrinVerifyTransfer.SrinReceipt.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.ProjectInfo.ProjectID
                                               && (from b in db.SrinInspectorVerifyDetails
                                                   where b.SrinInspectorVerifyTransferID.Equals(_transferid) && b.QualifiedGentaojian != 0
                                                   select b.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialID).Contains(a.StorageOutDetails.MaterialID)
                                               join c in db.SrinInspectorVerifyRDetails on new { a.StorageOutDetails.MaterialID, TransferID = _transferid } equals new { c.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialID, TransferID = c.SrinInspectorVerifyDetails.SrinInspectorVerifyTransferID}
                                               join d in db.StorageStocks on new { a.StorageOutDetails.MaterialID, Status = a.MaterialStatus, a.StocksID } equals new { d.MaterialID, d.Status, d.StocksID }
                                               orderby a.StorageOutDetails.MaterialID, a.CreateTime ascending
                                               select new
                                               {
                                                   MaterialName = string.Format("{0}--规格型号：{1}", d.MaterialName, d.SpecificationModel),
                                                   d.ManufacturerName,
                                                   d.UnitPrice,
                                                   d.CurUnit,
                                                   c.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.RetrieveCode,
                                                   c.VerifyCode,
                                                   c.SrinInspectorVerifyDetailsID,
                                                   c.Remark,
                                                   d.ManufacturerID,
                                                   Description = string.Format("财务编码：{0}--回收合格数量(根/台/套/件)：{1}", d.FinanceCode, c.QualifiedGentaojian)

                                               }).Distinct();
                }
                
                this.spgvMaterial.DataBind();                                                               
            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            spgvMaterial.Columns[11].Visible = false;
            spgvMaterial.Columns[12].Visible = false;

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
                    vldCode.Text = "回收入库单编号不能为空！";
                    return;
                }

                //数据库中存在相同NoticeCode的情况
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    if (db.SrinQualifiedReceipt.Count(u => u.SrinQualifiedReceiptCode.Equals(strCode)) != 0)
                    {
                        args.IsValid = false;
                        vldCode.Text = "回收入库单编号已存在！";
                        return;
                    }
                }

                args.IsValid = true;
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));
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

                        //生成回收物资设备入库单(合格)
                        SrinQualifiedReceipt sqrp = new SrinQualifiedReceipt();
                        sqrp.SrinQualifiedReceiptCode = txtCode.Text.Trim();                        
                        sqrp.Remark = ((TextBox)GetControltByMaster("txtRemark")).Text.Trim();
                        sqrp.NeedWriteOff = (GetControltByMaster("chbWriteOff") as CheckBox).Checked;
                        sqrp.SrinInspectorVerifyTransferID = _transferid;
                        sqrp.TaskID = _taskid;
                        sqrp.CreateTime = ((DateTimeControl)GetControltByMaster("dtcCreateTime")).SelectedDate;
                        sqrp.Creator = db.EmpInfo.SingleOrDefault(u => u.Account.Equals(SPContext.Current.Web.CurrentUser.LoginName)).EmpID;
                        db.SrinQualifiedReceipt.InsertOnSubmit(sqrp);
                        db.SubmitChanges();

                        //添加物资明细
                        int iDetailsID,iPricingIndex;
                        SrinAssetQualifiedDetails saqd;                        
                        foreach (GridViewRow gvr in spgvMaterial.Rows)
                        {
                            saqd = new SrinAssetQualifiedDetails();
                            saqd.Gentaojian = Convert.ToDecimal((gvr.Cells[2].Controls[0] as TextBox).Text.Trim());
                            if (saqd.Gentaojian == 0)
                                continue;

                            iDetailsID = Convert.ToInt32(gvr.Cells[12].Text);
                            iPricingIndex = GetPricingIndex(gvr.Cells[9].Text);
                            
                            saqd.SrinQualifiedReceiptID = sqrp.SrinQualifiedReceiptID;
                            saqd.SrinInspectorVerifyDetailsID = iDetailsID;
                            
                            saqd.Metre = Convert.ToDecimal((gvr.Cells[3].Controls[0] as TextBox).Text.Trim());
                            saqd.Ton = Convert.ToDecimal((gvr.Cells[4].Controls[0] as TextBox).Text.Trim());
                            saqd.OutUnitPrice = Convert.ToDecimal(gvr.Cells[5].Text);
                            saqd.InUnitPrice = Convert.ToDecimal((gvr.Cells[6].Controls[0] as TextBox).Text.Trim());                            
                            saqd.CurUnit = gvr.Cells[9].Text;
                            saqd.ManufactureID = Convert.ToInt32(gvr.Cells[11].Text);
                            saqd.Remark = (gvr.Cells[10].Controls[0] as TextBox).Text.Trim();
                            saqd.Amount = Convert.ToDecimal((gvr.Cells[iPricingIndex].Controls[0] as TextBox).Text.Trim()) * Convert.ToDecimal(gvr.Cells[5].Text);
                            saqd.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                            saqd.Creator = db.EmpInfo.SingleOrDefault(u => u.Account == SPContext.Current.Web.CurrentUser.LoginName).EmpID;
                            db.SrinAssetQualifiedDetails.InsertOnSubmit(saqd);
                        }
                        db.SubmitChanges();

                        Response.Redirect(string.Format("RiAssetQualifiedReceiptMessage.aspx?TaskID={0}", _taskid), false);

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

        private int GetPricingIndex(string curunit)
        {
            switch (curunit)
            {
                case "根/台/套/件":
                    return 2;
                case "米":
                    return 3;
                case "吨":
                    return 4;
                default:
                    return -1;
            }
        }

        #endregion
    }
}
