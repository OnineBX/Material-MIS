/*------------------------------------------------------------------------------
 * Unit Name：RiAssetModifyQualifiedReceipt.cs
 * Description: 回收入库--物资组修改回收入库单(合格)物资的页面
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
    public class RiAssetModifyQualifiedReceipt:Page
    {
        private int _taskid, _receiptid;
        private SPGridView spgvMaterial;
        private Button btnOK;
        private CustomValidator vldCode;
        private TextBox txtCode,txtRemark;
        private DateTimeControl dtcCreateTime;
        private CheckBox chbWriteOff;

        private static string[] ShowTlist = {                                                          
                                                  "生产厂家:ManufacturerName",                                                   
                                                  "出库单价(原):OutUnitPrice",
                                                  "回收单号:RetrieveCode",
                                                  "检验报告号:VerifyCode",
                                                  "计量单位:CurUnit",
                                                  "Selected:Selected",
                                                  "ManufacturerID:MID",
                                                  "DetailsID:DetailsID"
                                            };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    _receiptid = db.SrinQualifiedReceipt.SingleOrDefault(u => u.TaskID.Equals(_taskid)).SrinQualifiedReceiptID;
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
            txtRemark = GetControltByMaster("txtRemark") as TextBox;
            dtcCreateTime = GetControltByMaster("dtcCreateTime") as DateTimeControl;
            chbWriteOff = GetControltByMaster("chbWriteOff") as CheckBox;

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
            tfGentaojian.ItemTemplate = new TextBoxTemplate("Gentaojian", "Gentaojian", "^(-?\\d+)(\\.\\d+)?$", "0", 80);
            this.spgvMaterial.Columns.Insert(1, tfGentaojian);

            //加入回收数量--米列
            TemplateField tfQuantityMetre = new TemplateField();
            tfQuantityMetre.HeaderText = "米";
            tfQuantityMetre.ItemTemplate = new TextBoxTemplate("Metre", "Metre", "^(-?\\d+)(\\.\\d+)?$", 80);
            this.spgvMaterial.Columns.Insert(2, tfQuantityMetre);

            //加入回收数量--吨列
            TemplateField tfQuantityTon = new TemplateField();
            tfQuantityTon.HeaderText = "吨";
            tfQuantityTon.ItemTemplate = new TextBoxTemplate("Ton", "Ton", "^(-?\\d+)(\\.\\d+)?$", 80);
            this.spgvMaterial.Columns.Insert(3, tfQuantityTon);

            //加入单价(入库)列
            TemplateField tfUnitPrice = new TemplateField();
            tfUnitPrice.HeaderText = "入库单价(新)";
            tfUnitPrice.ItemTemplate = new TextBoxTemplate("InUnitPrice", "InUnitPrice", "^(-?\\d+)(\\.\\d+)?$",80);
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
                SrinQualifiedReceipt sqrp = db.SrinQualifiedReceipt.SingleOrDefault(u => u.SrinQualifiedReceiptID == _receiptid);

                ((Label)GetControltByMaster("lblProject")).Text = sqrp.SrinInspectorVerifyTransfer.SrinProduceVerifyTransfer.SrinVerifyTransfer.SrinReceipt.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.ProjectInfo.ProjectName;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(sqrp.SrinInspectorVerifyTransfer.CreateTime.ToLongDateString(), sqrp.SrinInspectorVerifyTransfer.CreateTime.ToLongTimeString());
                ((Label)GetControltByMaster("lblInspector")).Text = sqrp.SrinInspectorVerifyTransfer.EmpInfo.EmpName;

                //初始化质检合格物资
                //初始化物资出库明细列表                
                var Details = (from a in db.SrinAssetQualifiedDetails.AsEnumerable()//已加入到出库表中的物资                               
                               where a.SrinQualifiedReceiptID == _receiptid
                               orderby a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialID
                               let mname = a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName
                               let fcode = a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.FinanceCode
                               let smodel = a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel
                               select new
                               {
                                   MaterialName = string.Format("{0}--规格型号：{1}", mname, smodel),
                                   a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.Manufacturer.ManufacturerName,
                                   a.Gentaojian,
                                   a.Metre,
                                   a.Ton,
                                   a.OutUnitPrice,
                                   a.InUnitPrice,
                                   a.CurUnit,
                                   a.Amount,
                                   a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.RetrieveCode,
                                   a.SrinInspectorVerifyDetails.VerifyCode,
                                   a.Remark,
                                   MID = a.ManufactureID,
                                   Selected = true,
                                   DetailsID = a.SrinAssetQualifiedDetailsID,
                                   Description = string.Format("财务编码：{0}--回收合格数量(根/台/套/件)：{1}", fcode, a.SrinInspectorVerifyDetails.QualifiedGentaojian)
                               }).AsEnumerable().Union(
                                               (from a in db.StorageOutRealDetails.AsEnumerable()
                                                join d in db.StorageStocks.AsEnumerable() on new { a.StorageOutDetails.MaterialID, Status = a.MaterialStatus, a.StocksID } equals new { d.MaterialID, d.Status, d.StocksID }
                                                where a.StorageOutNotice.ProjectID == sqrp.SrinInspectorVerifyTransfer.SrinProduceVerifyTransfer.SrinVerifyTransfer.SrinReceipt.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.ProjectInfo.ProjectID
                                                && !(from e in db.SrinAssetQualifiedDetails.AsEnumerable()
                                                     where e.SrinQualifiedReceiptID == _receiptid
                                                     select e.OutUnitPrice).Contains(d.UnitPrice)//不包含当前选中物资的单价
                                                && (from b in db.SrinInspectorVerifyDetails.AsEnumerable()
                                                    where b.SrinInspectorVerifyTransferID.Equals(sqrp.SrinInspectorVerifyTransferID) && b.QualifiedGentaojian != 0
                                                    select b.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialID).Contains(a.StorageOutDetails.MaterialID)//回收物资中的物资
                                                join c in db.SrinInspectorVerifyDetails.AsEnumerable() on new { a.StorageOutDetails.MaterialID, TransferID = sqrp.SrinInspectorVerifyTransferID } equals new { c.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialID, TransferID = c.SrinInspectorVerifyTransferID }
                                               orderby a.StorageOutDetails.MaterialID
                                               select new
                                               {
                                                   MaterialName = string.Format("{0}--规格型号：{1}", d.MaterialName,d.SpecificationModel),
                                                   d.ManufacturerName,                          
                                                   Gentaojian = Decimal.Zero,
                                                   Metre = Decimal.Zero,
                                                   Ton = Decimal.Zero,
                                                   OutUnitPrice = d.UnitPrice,
                                                   InUnitPrice = Decimal.Zero,
                                                   d.CurUnit,                                                   
                                                   Amount = Decimal.Zero,
                                                   c.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.RetrieveCode,                                                   
                                                   c.VerifyCode,                                                   
                                                   c.Remark,
                                                   MID = d.ManufacturerID,                                                   
                                                   Selected = false,
                                                   DetailsID = c.SrinInspectorVerifyDetailsID,
                                                   Description = string.Format("财务编码：{0}--回收合格数量(根/台/套/件)：{1}", d.FinanceCode, c.QualifiedGentaojian)                                                  
                                               }).Distinct()
                                               ).AsEnumerable();
                this.spgvMaterial.DataSource = from a in Details
                                               orderby a.MaterialName
                                               select a;
                
                this.spgvMaterial.DataBind();

                //初始化回收入库单(合格)信息
                if (!Page.IsPostBack)
                {                    
                    txtCode.Text = sqrp.SrinQualifiedReceiptCode;
                    txtRemark.Text = sqrp.Remark;
                    dtcCreateTime.SelectedDate = sqrp.CreateTime;
                    chbWriteOff.Checked = sqrp.NeedWriteOff;
                }
            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            spgvMaterial.Columns[11].Visible = false;
            spgvMaterial.Columns[12].Visible = false;
            spgvMaterial.Columns[13].Visible = false;

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
                    if (db.SrinQualifiedReceipt.Count(u => u.SrinQualifiedReceiptCode.Equals(strCode) && !u.SrinQualifiedReceiptID.Equals(_receiptid)) != 0)
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
                        SrinQualifiedReceipt sqrp = db.SrinQualifiedReceipt.SingleOrDefault(u => u.SrinQualifiedReceiptID.Equals(_receiptid));
                        sqrp.SrinQualifiedReceiptCode = txtCode.Text.Trim();
                        sqrp.NeedWriteOff = chbWriteOff.Checked;
                        sqrp.Remark = ((TextBox)GetControltByMaster("txtRemark")).Text.Trim();                        
                        sqrp.CreateTime = ((DateTimeControl)GetControltByMaster("dtcCreateTime")).SelectedDate;

                        //修改合格物资明细

                        
                        bool bselected;//标识物资是否已经被选择过
                        int iDetailsID, iPricingIndex;//物资ID,计量单位对应的索引：根台套件、米或吨
                        SrinAssetQualifiedDetails saqd;
                        decimal dQuantity = 0;
                        foreach (GridViewRow gvr in spgvMaterial.Rows)
                        {
                            iPricingIndex = GetPricingIndex(gvr.Cells[9].Text);
                            bselected = Convert.ToBoolean(gvr.Cells[11].Text);
                            iDetailsID = Convert.ToInt32(gvr.Cells[13].Text);

                            if (bselected)//修改原有物资的情况
                            {
                                saqd = db.SrinAssetQualifiedDetails.SingleOrDefault(u => u.SrinAssetQualifiedDetailsID.Equals(iDetailsID));
                                dQuantity = Convert.ToDecimal((gvr.Cells[iPricingIndex].Controls[0] as TextBox).Text.Trim());
                                saqd.Gentaojian = Convert.ToDecimal((gvr.Cells[2].Controls[0] as TextBox).Text.Trim());

                                if (saqd.Gentaojian == 0)//回收（跟台套件）数量为0的情况--则删除此项
                                    db.SrinAssetQualifiedDetails.DeleteOnSubmit(saqd);
                                else//不为0则修改此物资
                                {                                    
                                    saqd.Metre = Convert.ToDecimal((gvr.Cells[3].Controls[0] as TextBox).Text.Trim());
                                    saqd.Ton = Convert.ToDecimal((gvr.Cells[4].Controls[0] as TextBox).Text.Trim());
                                    saqd.InUnitPrice = Convert.ToDecimal((gvr.Cells[6].Controls[0] as TextBox).Text.Trim());
                                    saqd.Remark = ((TextBox)gvr.Cells[10].Controls[0]).Text.Trim();
                                    saqd.Amount = dQuantity * saqd.OutUnitPrice;
                                    saqd.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                                }
                            }
                            else//加入新物资的情况
                            {
                                saqd = new SrinAssetQualifiedDetails();                                
                                saqd.Gentaojian = Convert.ToDecimal((gvr.Cells[2].Controls[0] as TextBox).Text.Trim());
                                if (saqd.Gentaojian == 0)
                                    continue;                                

                                saqd.SrinQualifiedReceiptID = sqrp.SrinQualifiedReceiptID;
                                saqd.SrinInspectorVerifyDetailsID = iDetailsID;

                                saqd.Metre = Convert.ToDecimal((gvr.Cells[3].Controls[0] as TextBox).Text.Trim());
                                saqd.Ton = Convert.ToDecimal((gvr.Cells[4].Controls[0] as TextBox).Text.Trim());
                                saqd.OutUnitPrice = Convert.ToDecimal(gvr.Cells[5].Text);
                                saqd.InUnitPrice = Convert.ToDecimal((gvr.Cells[6].Controls[0] as TextBox).Text.Trim());
                                saqd.CurUnit = gvr.Cells[9].Text;
                                saqd.ManufactureID = Convert.ToInt32(gvr.Cells[12].Text);
                                saqd.Remark = (gvr.Cells[10].Controls[0] as TextBox).Text.Trim();
                                saqd.Amount = Convert.ToDecimal((gvr.Cells[iPricingIndex].Controls[0] as TextBox).Text.Trim()) * Convert.ToDecimal(gvr.Cells[5].Text);
                                saqd.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                                saqd.Creator = db.EmpInfo.SingleOrDefault(u => u.Account == SPContext.Current.Web.CurrentUser.LoginName).EmpID;
                                db.SrinAssetQualifiedDetails.InsertOnSubmit(saqd);
                            }                                                                                                                                                                
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
