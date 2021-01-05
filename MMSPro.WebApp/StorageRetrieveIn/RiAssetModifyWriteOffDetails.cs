/*------------------------------------------------------------------------------
 * Unit Name：RiAssetModifyWriteoffDetails.cs
 * Description: 回收入库--资产组修改回收合格物资冲销的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-1-01
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
    public class RiAssetModifyWriteoffDetails : Page
    {
        private int _taskid, _receiptid;
        private SPGridView spgvMaterial;
        private Button btnOK;
        private CustomValidator vldQuantity;

        private static string[] ShowTlist = {
                                                "出库调拨单号:StorageOutNoticeCode", 
                                                "物资编号:MaterialCode",
                                                "生产厂家:ManufacturerName",
                                                "出库时间:CreateTime",
                                                "状态:Status",
                                                "根/台/套/件(冲销前):RealGentaojian",                                                
                                                "米(冲销前):RealMetre",                                                
                                                "吨(冲销前):RealTon",                                                
                                                "单价:UnitPrice",                                                 
                                                "金额:RealAmount",
                                                "计量单位:CurUnit",
                                                "IsSelected:IsSelected",
                                                "DetailsID:DetailsID",
                                                "SrinAssetQualifiedDetailsID:SrinAssetQualifiedDetailsID"
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

            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);

            //初始化spgvWriteOffMaterial
            this.spgvMaterial = new SPGridView();
            this.spgvMaterial.AutoGenerateColumns = false;
            this.spgvMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");
            this.spgvMaterial.AllowGrouping = true;
            this.spgvMaterial.AllowGroupCollapse = true;
            this.spgvMaterial.GroupDescriptionField = "Description";
            this.spgvMaterial.GroupField = "MaterialName";
            this.spgvMaterial.GroupFieldDisplayName = "回收合格物资";

            BoundField bfColumn;
            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvMaterial.Columns.Add(bfColumn);
            }

            //加入冲销数量--根/台/套/件列
            TemplateField tfGentaojian = new TemplateField();
            tfGentaojian.HeaderText = "冲销数量(根/台/套/件)";
            tfGentaojian.ItemTemplate = new TextBoxTemplate("Gentaojian", "Gentaojian", "^(-?\\d+)(\\.\\d+)?$", "0", 80);
            this.spgvMaterial.Columns.Insert(6, tfGentaojian);

            //加入冲销数量--米列
            TemplateField tfMetre = new TemplateField();
            tfMetre.HeaderText = "冲销数量(米)";
            tfMetre.ItemTemplate = new TextBoxTemplate("Metre", "Metre", "^(-?\\d+)(\\.\\d+)?$", "0", 80);
            this.spgvMaterial.Columns.Insert(8, tfMetre);

            //加入回收数量--吨列
            TemplateField tfQuantityTon = new TemplateField();
            tfQuantityTon.HeaderText = "冲销数量(吨)";
            tfQuantityTon.ItemTemplate = new TextBoxTemplate("Ton", "Ton", "^(-?\\d+)(\\.\\d+)?$", "0", 80);
            this.spgvMaterial.Columns.Insert(10, tfQuantityTon);

            //加入备注列            
            TemplateField tfRemark = new TemplateField();
            tfRemark.HeaderText = "备注";
            tfRemark.ItemTemplate = new TextBoxTemplate("备注", DataControlRowType.DataRow, "Remark");
            this.spgvMaterial.Columns.Insert(14, tfRemark);
        }

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息
                SrinAChiefQReceiptConfirm smqc = db.SrinAChiefQReceiptConfirm.SingleOrDefault(u => u.SrinQualifiedReceiptID.Equals(_receiptid));

                ((Label)GetControltByMaster("lblProject")).Text = smqc.SrinQualifiedReceipt.SrinInspectorVerifyTransfer.SrinProduceVerifyTransfer.SrinVerifyTransfer.SrinReceipt.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.ProjectInfo.ProjectName;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(smqc.SrinQualifiedReceipt.CreateTime.ToLongDateString(), smqc.SrinQualifiedReceipt.CreateTime.ToLongTimeString());
                ((Label)GetControltByMaster("lblCode")).Text = smqc.SrinQualifiedReceipt.SrinQualifiedReceiptCode;
                int projectid = smqc.SrinQualifiedReceipt.SrinInspectorVerifyTransfer.SrinProduceVerifyTransfer.SrinVerifyTransfer.SrinReceipt.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.Project;

                //初始化冲销物资明细--同一项目的出库物资中包含的回收合格物资
                
                var Details = (from a in db.SrinWriteOffDetails.AsEnumerable()
                                 join w in db.WriteOffDetails on new { a.StorageOutRealDetailsID } equals new { w.StorageOutRealDetailsID }
                                 where a.SrinQualifiedReceiptID.Equals(_receiptid)
                                 select new
                                 {
                                     MaterialName = string.Format("{0}--规格型号：{1}", w.MaterialName, w.SpecificationModel),
                                     a.StorageOutRealDetails.StorageOutNotice.StorageOutNoticeCode,
                                     w.MaterialCode,
                                     w.ManufacturerName,
                                     a.StorageOutRealDetails.CreateTime,
                                     w.Status,
                                     RealGentaojian = w.RealGentaojian + a.Gentaojian,
                                     RealMetre = w.RealMetre + a.Metre,
                                     RealTon = w.RealTon + a.Ton,
                                     a.Gentaojian,
                                     a.Metre,
                                     a.Ton,
                                     w.CurUnit,
                                     w.UnitPrice,
                                     w.RealAmount,
                                     a.Remark,
                                     IsSelected = true,
                                     DetailsID = a.SrinWriteOffDetailsID,
                                     a.SrinAssetQualifiedDetailsID,
                                     Description = string.Format("财务编码：{0}--待冲销数量(根台套件/米/吨)：{1}/{2}/{3}", w.FinanceCode,
                                                                 a.SrinAssetQualifiedDetails.Gentaojian, a.SrinAssetQualifiedDetails.Metre, a.SrinAssetQualifiedDetails.Ton)
                                 }).Union(
                                                 from a in db.WriteOffDetails.AsEnumerable()
                                                 where a.ProjectID == projectid
                                                    && (from d in db.SrinAssetQualifiedDetails//冲销表中所包含的检验合格的物资
                                                        where d.SrinQualifiedReceiptID == _receiptid
                                                        select d.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialID).Contains(a.MaterialID)
                                                    && !(from c in db.SrinWriteOffDetails//排除已经选过的物资
                                                         where c.SrinQualifiedReceiptID == _receiptid
                                                         select c.StorageOutRealDetailsID).Contains(a.StorageOutRealDetailsID)
                                                 join b in db.SrinAssetQualifiedDetails on new { a.MaterialID, ReceiptID = _receiptid } equals new { b.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialID, ReceiptID = b.SrinQualifiedReceiptID }
                                                 orderby a.MaterialID
                                                 select new
                                                 {
                                                     MaterialName = string.Format("{0}--规格型号：{1}", a.MaterialName, a.SpecificationModel),
                                                     a.StorageOutNoticeCode,
                                                     a.MaterialCode,
                                                     a.ManufacturerName,
                                                     a.CreateTime,
                                                     a.Status,
                                                     a.RealGentaojian,
                                                     a.RealMetre,
                                                     a.RealTon,
                                                     Gentaojian = Decimal.Zero,
                                                     Metre = Decimal.Zero,
                                                     Ton = Decimal.Zero,
                                                     a.CurUnit,
                                                     a.UnitPrice,
                                                     a.RealAmount,
                                                     b.Remark,
                                                     IsSelected = false,
                                                     DetailsID = a.StorageOutRealDetailsID,
                                                     b.SrinAssetQualifiedDetailsID,
                                                     Description = string.Format("财务编码：{0}--待冲销数量(根台套件/米/吨)：{1}/{2}/{3}", a.FinanceCode, b.Gentaojian, b.Metre, b.Ton)
                                                 }
                                               );

                this.spgvMaterial.DataSource = from a in Details
                                               orderby a.MaterialName
                                               select a;
                this.spgvMaterial.DataBind();

                //初始化表尾信息
                (GetControltByMaster("lblAsset") as Label).Text = smqc.SrinQualifiedReceipt.EmpInfo.EmpName;
                (GetControltByMaster("lblAChief") as Label).Text = smqc.EmpInfo.EmpName;
                (GetControltByMaster("lblAChiefDate") as Label).Text = string.Concat(smqc.ConfirmTime.ToLongDateString(), smqc.ConfirmTime.ToLongTimeString());
                (GetControltByMaster("lblRemark") as Label).Text = smqc.Remark;

            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            this.spgvMaterial.Columns[16].Visible = false;
            this.spgvMaterial.Columns[17].Visible = false;
            this.spgvMaterial.Columns[18].Visible = false;

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
                //将确认结果保存到数据库
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    SrinWriteOffDetails swod;
                    decimal dQuantity = 0;//计量单位对应的数量
                    bool bSelected;
                    int iDetailsID,iPricingIndex;
                    foreach (GridViewRow gvr in this.spgvMaterial.Rows)
                    {
                        bSelected = Convert.ToBoolean(gvr.Cells[16].Text);
                        iDetailsID = Convert.ToInt32(gvr.Cells[17].Text);
                        iPricingIndex = GetPricingIndex(gvr.Cells[14].Text);                        
                        if (bSelected)//已经选择过该物资的情况
                        {
                            swod = db.SrinWriteOffDetails.SingleOrDefault(u => u.SrinWriteOffDetailsID.Equals(iDetailsID));
                            swod.Gentaojian = Convert.ToDecimal((gvr.Cells[7].Controls[0] as TextBox).Text.Trim());
                            if (swod.Gentaojian == 0)//冲销数量为0的情况--在冲销表删除该条物资
                                db.SrinWriteOffDetails.DeleteOnSubmit(swod);
                            else//不为0则修改该条物资数据
                            {
                                dQuantity = Convert.ToDecimal((gvr.Cells[iPricingIndex].Controls[0] as TextBox).Text.Trim());

                                swod.Gentaojian = Convert.ToDecimal((gvr.Cells[7].Controls[0] as TextBox).Text.Trim());
                                swod.Metre = Convert.ToDecimal((gvr.Cells[9].Controls[0] as TextBox).Text.Trim());
                                swod.Ton = Convert.ToDecimal((gvr.Cells[11].Controls[0] as TextBox).Text.Trim());
                                swod.Amount = Convert.ToDecimal(gvr.Cells[12].Text) * dQuantity;
                                swod.Remark = (gvr.Cells[15].Controls[0] as TextBox).Text.Trim();
                                swod.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                            }                                                       
                        }
                        else//没有选择过的物资的情况
                        {
                            swod = new SrinWriteOffDetails();
                            swod.Gentaojian = Convert.ToDecimal((gvr.Cells[7].Controls[0] as TextBox).Text.Trim());
                            if (swod.Gentaojian == 0)//冲销数量为0则跳过不处理
                                continue;

                            dQuantity = Convert.ToDecimal((gvr.Cells[GetPricingIndex(gvr.Cells[14].Text)].Controls[0] as TextBox).Text.Trim());

                            swod.SrinQualifiedReceiptID = _receiptid;
                            swod.StorageOutRealDetailsID = iDetailsID;
                            swod.SrinAssetQualifiedDetailsID = Convert.ToInt32(gvr.Cells[18].Text);
                            swod.Metre = Convert.ToDecimal((gvr.Cells[9].Controls[0] as TextBox).Text.Trim());
                            swod.Ton = Convert.ToDecimal((gvr.Cells[11].Controls[0] as TextBox).Text.Trim());
                            swod.Amount = Convert.ToDecimal(gvr.Cells[12].Text) * dQuantity;
                            swod.Remark = (gvr.Cells[15].Controls[0] as TextBox).Text.Trim();
                            swod.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                            swod.Creator = db.EmpInfo.SingleOrDefault(u => u.Account == SPContext.Current.Web.CurrentUser.LoginName).EmpID;
                            db.SrinWriteOffDetails.InsertOnSubmit(swod);
                        }
                        
                    }
                    db.SubmitChanges();
                }
                Response.Redirect(string.Format("RiAssetWriteoffDetailsMessage.aspx?TaskID={0}", _taskid), false);
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
                    return 7;
                case "米":
                    return 9;
                case "吨":
                    return 11;
                default:
                    return -1;
            }
        }

        #endregion
    }
}
