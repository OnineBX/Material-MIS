/*------------------------------------------------------------------------------
 * Unit Name：RiAssetWriteoffDetails.cs
 * Description: 回收入库--资产组进行回收合格物资冲销的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-09-19
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
    public class RiAssetStorageDetails:Page
    {
        private int _taskid, _receiptid;
        private SPGridView spgvMaterial;
        private Button btnOK;       

        private int projectid;//回收项目ID
        private bool bWriteOff;//标识物资是否执行了冲销
        private bool bFinished = false;//任务是否结束


        private static string[] ShowTlist = {                                                      
                                                "生产厂家:ManufacturerName",
                                                "仓库:StorageName",
                                                "剁位:PileName",
                                                "根/台/套/件:Gentaojian",
                                                "米:Metre",
                                                "吨:Ton",                                                
                                                "单价:InUnitPrice",
                                                "计量单位:CurUnit",
                                                "入库时间:CreateTime",
                                                "回收时间:RetrieveTime",
                                                "Status:Status",
                                                "StorageID:StorageID",
                                                "PileID:PileID",
                                                "ManufactureID:ManufactureID",
                                                "MaterialID:MaterialID"
                                            };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    SrinQualifiedReceipt sqr = db.SrinQualifiedReceipt.SingleOrDefault(u => u.TaskID.Equals(_taskid));
                    _receiptid = sqr.SrinQualifiedReceiptID;
                    bWriteOff = sqr.NeedWriteOff;

                    if (db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID.Equals(_taskid)).TaskState.Equals("已完成"))
                        bFinished = true;
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
            this.spgvMaterial.AllowGrouping = true;
            this.spgvMaterial.AllowGroupCollapse = true;
            this.spgvMaterial.GroupDescriptionField = "Description";
            if (bWriteOff)
            {
                this.spgvMaterial.GroupField = "StorageType";
                this.spgvMaterial.GroupFieldDisplayName = "待入库物资类型"; 
            }
            else
            {
                this.spgvMaterial.GroupField = "MaterialName";
                this.spgvMaterial.GroupFieldDisplayName = "回收检验合格物资";
            }
            

            BoundField bfColumn;

            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvMaterial.Columns.Add(bfColumn);
            }
            if (bWriteOff)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = "物资名称";
                bfColumn.DataField = "MaterialName";
                this.spgvMaterial.Columns.Insert(0, bfColumn);

                bfColumn = new BoundField();
                bfColumn.HeaderText = "规格型号";
                bfColumn.DataField = "SpecificationModel";
                this.spgvMaterial.Columns.Insert(1, bfColumn);

                bfColumn = new BoundField();
                bfColumn.HeaderText = "财务编码";
                bfColumn.DataField = "FinanceCode";
                this.spgvMaterial.Columns.Insert(2, bfColumn);

                bfColumn = new BoundField();
                bfColumn.HeaderText = "物资编码";
                bfColumn.DataField = "MaterialCode";
                this.spgvMaterial.Columns.Insert(3, bfColumn);
            }
            //加入备注列            
            TemplateField tfRemark = new TemplateField();
            tfRemark.HeaderText = "备注";
            tfRemark.ItemTemplate = new TextBoxTemplate("备注", DataControlRowType.DataRow,"Remark");
            if (bWriteOff)
                this.spgvMaterial.Columns.Insert(14, tfRemark);
            else
                this.spgvMaterial.Columns.Insert(10, tfRemark);   
        }

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息
                SrinAChiefQReceiptConfirm saqc = db.SrinAChiefQReceiptConfirm.SingleOrDefault(u => u.SrinQualifiedReceiptID.Equals(_receiptid));

                ((Label)GetControltByMaster("lblProject")).Text = saqc.SrinQualifiedReceipt.SrinInspectorVerifyTransfer.SrinProduceVerifyTransfer.SrinVerifyTransfer.SrinReceipt.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.ProjectInfo.ProjectName;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(saqc.SrinQualifiedReceipt.CreateTime.ToLongDateString(), saqc.SrinQualifiedReceipt.CreateTime.ToLongTimeString());
                ((Label)GetControltByMaster("lblCode")).Text = saqc.SrinQualifiedReceipt.SrinQualifiedReceiptCode;
                projectid = saqc.SrinQualifiedReceipt.SrinInspectorVerifyTransfer.SrinProduceVerifyTransfer.SrinVerifyTransfer.SrinReceipt.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.Project;

                //初始化回收入库单(合格)明细
                if (bWriteOff)//执行了冲销的情况
                {
                    this.spgvMaterial.DataSource = (from a in db.SrinWriteOffDetails.AsEnumerable()
                                                    join b in db.StorageStocks on new { a.StorageOutRealDetails.StocksID, Status = a.StorageOutRealDetails.MaterialStatus, a.SrinQualifiedReceiptID } equals new { b.StocksID, b.Status, SrinQualifiedReceiptID = _receiptid }
                                                    where a.SrinQualifiedReceiptID == _receiptid
                                                    select new
                                                    {
                                                        StorageType = b.Status.Equals("线上") ? "线上物资" : b.Status.Equals("线下") ? "线下入库" : "回收合格入库",
                                                        b.MaterialName,
                                                        b.MaterialCode,
                                                        b.SpecificationModel,
                                                        b.FinanceCode,
                                                        b.ManufacturerName,
                                                        a.SrinAssetQualifiedDetails.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.StorageInfo.StorageName,
                                                        a.SrinAssetQualifiedDetails.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.PileInfo.PileName,
                                                        a.Gentaojian,
                                                        a.Metre,
                                                        a.Ton,
                                                        a.SrinAssetQualifiedDetails.InUnitPrice,
                                                        a.SrinAssetQualifiedDetails.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.RetrieveTime,
                                                        a.SrinAssetQualifiedDetails.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.CreateTime,
                                                        a.SrinAssetQualifiedDetails.CurUnit,
                                                        a.Remark,
                                                        b.Status,
                                                        a.SrinAssetQualifiedDetails.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.StorageID,
                                                        a.SrinAssetQualifiedDetails.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.PileID,
                                                        a.SrinAssetQualifiedDetails.ManufactureID,
                                                        a.SrinAssetQualifiedDetails.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialID,
                                                        Description = "入库物资详情"

                                                    }).Union(
                                                   from a in db.SrinWriteOffDetails.AsEnumerable()
                                                   where !a.StorageOutRealDetails.MaterialStatus.Equals("回收合格")
                                                         && !a.SrinAssetQualifiedDetails.Gentaojian.Equals((from c in db.SrinWriteOffDetails
                                                                                                           where c.SrinAssetQualifiedDetailsID.Equals(a.SrinAssetQualifiedDetailsID)
                                                                                                           select c).Sum(u => u.Gentaojian))
                                                   join b in db.StorageStocks on new { a.StorageOutRealDetails.StocksID, Status = a.StorageOutRealDetails.MaterialStatus, a.SrinQualifiedReceiptID } equals new { b.StocksID, b.Status, SrinQualifiedReceiptID = _receiptid }
                                                   select new 
                                                   {
                                                       StorageType = "回收合格入库",
                                                       b.MaterialName,
                                                       b.MaterialCode,
                                                       b.SpecificationModel,
                                                       b.FinanceCode,
                                                       b.ManufacturerName,
                                                       a.SrinAssetQualifiedDetails.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.StorageInfo.StorageName,
                                                       a.SrinAssetQualifiedDetails.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.PileInfo.PileName,
                                                       Gentaojian = a.SrinAssetQualifiedDetails.Gentaojian - a.Gentaojian,
                                                       Metre = a.SrinAssetQualifiedDetails.Metre - a.Metre,
                                                       Ton = a.SrinAssetQualifiedDetails.Ton - a.Ton,
                                                       a.SrinAssetQualifiedDetails.InUnitPrice,
                                                       a.SrinAssetQualifiedDetails.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.RetrieveTime,
                                                       a.SrinAssetQualifiedDetails.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.CreateTime,
                                                       a.SrinAssetQualifiedDetails.CurUnit,
                                                       a.Remark,
                                                       Status = "回收合格",
                                                       a.SrinAssetQualifiedDetails.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.StorageID,
                                                       a.SrinAssetQualifiedDetails.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.PileID,
                                                       a.SrinAssetQualifiedDetails.ManufactureID,
                                                       a.SrinAssetQualifiedDetails.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialID,
                                                       Description = "入库物资详情"
                                                   }
                                                   );
                }
                else
                {
                    this.spgvMaterial.DataSource = from a in db.SrinAssetQualifiedDetails
                                                   where a.SrinQualifiedReceiptID == _receiptid
                                                   let fcode = a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.FinanceCode
                                                   let smodel = a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel
                                                   select new
                                                   {
                                                       a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                                       a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.Manufacturer.ManufacturerName,
                                                       a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.StorageInfo.StorageName,
                                                       a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.PileInfo.PileName,
                                                       a.Gentaojian,
                                                       a.Metre,
                                                       a.Ton,
                                                       a.InUnitPrice,
                                                       a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.RetrieveTime,
                                                       a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.CreateTime,
                                                       a.CurUnit,
                                                       a.Remark,
                                                       Status = "回收合格",
                                                       a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.StorageID,
                                                       a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.PileID,
                                                       a.ManufactureID,
                                                       a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialID,
                                                       Description = string.Format("规格型号：{0}--财务编码：{1}--回收合格数量(根/台/套/件)：{2}", smodel, fcode, a.SrinInspectorVerifyDetails.QualifiedGentaojian)
                                                   };
                }
                this.spgvMaterial.DataBind();
               

                //初始化表尾信息
                (GetControltByMaster("lblAsset") as Label).Text = saqc.SrinQualifiedReceipt.EmpInfo.EmpName;
                (GetControltByMaster("lblAChief") as Label).Text = saqc.EmpInfo.EmpName;
                (GetControltByMaster("lblAChiefDate") as Label).Text = string.Concat(saqc.ConfirmTime.ToLongDateString(), saqc.ConfirmTime.ToLongTimeString());
                (GetControltByMaster("lblWriteOff") as Label).Text = saqc.SrinQualifiedReceipt.NeedWriteOff?"执行冲销":"不执行冲销";
                (GetControltByMaster("lblRemark") as Label).Text = saqc.Remark;

            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            if (bWriteOff)
            {
                this.spgvMaterial.Columns[16].Visible = false;
                this.spgvMaterial.Columns[17].Visible = false;
                this.spgvMaterial.Columns[18].Visible = false;
                this.spgvMaterial.Columns[19].Visible = false;
                this.spgvMaterial.Columns[20].Visible = false;
            }
            else
            {
                this.spgvMaterial.Columns[12].Visible = false;
                this.spgvMaterial.Columns[13].Visible = false;
                this.spgvMaterial.Columns[14].Visible = false;
                this.spgvMaterial.Columns[15].Visible = false;
                this.spgvMaterial.Columns[16].Visible = false;
            }

            if (bFinished)
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
            if (bFinished)
                Response.Redirect("../../default-old.aspx", false);
            else
            {
                if (bWriteOff)
                    Response.Redirect(string.Format("RiAssetWriteOffDetailsMessage.aspx?TaskID={0}", _taskid), false);
                else
                    Response.Redirect(string.Format("RiAssetQualifiedReceiptMessage.aspx?TaskID={0}", _taskid), false);
            }
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                //将确认结果保存到数据库
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    int empid = db.EmpInfo.SingleOrDefault(u => u.Account == SPContext.Current.Web.CurrentUser.LoginName).EmpID;
                    foreach (GridViewRow gvr in spgvMaterial.Rows)
                    {
                        if (bWriteOff)
                        {
                            switch (gvr.Cells[16].Text)
                            {
                                case "线上":
                                    StockOnline sol = StorageToOnLine(gvr);
                                    sol.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                                    sol.Creator = empid;
                                    db.StockOnline.InsertOnSubmit(sol);
                                    break;
                                case "线下":
                                    TableOfStocks tos = StorageToOffLine(gvr);
                                    tos.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                                    tos.Creator = empid;
                                    db.TableOfStocks.InsertOnSubmit(tos);
                                    break;
                                case "回收合格":
                                    QualifiedStocks qfs = StorageToQualified(gvr);
                                    qfs.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                                    qfs.Creator = empid;
                                    db.QualifiedStocks.InsertOnSubmit(qfs);
                                    break;
                            }
                        }
                        else
                        {
                            QualifiedStocks qs = new QualifiedStocks();
                            qs.Gentaojian = Convert.ToDecimal(gvr.Cells[4].Text);
                            qs.Metre = Convert.ToDecimal(gvr.Cells[5].Text);
                            qs.Ton = Convert.ToDecimal(gvr.Cells[6].Text);
                            qs.UnitPrice = Convert.ToDecimal(gvr.Cells[7].Text);
                            qs.CurUnit = gvr.Cells[8].Text;
                            qs.Amount = qs.UnitPrice * Convert.ToDecimal(gvr.Cells[GetPricingIndex(qs.CurUnit)].Text);
                            qs.StorageTime = Convert.ToDateTime(gvr.Cells[9].Text);
                            qs.RetrieveTime = Convert.ToDateTime(gvr.Cells[10].Text);
                            qs.Remark = (gvr.Cells[11].Controls[0] as TextBox).Text.Trim();
                            qs.StorageID = Convert.ToInt32(gvr.Cells[13].Text);
                            qs.PileID = Convert.ToInt32(gvr.Cells[14].Text);
                            qs.ManufactureID = Convert.ToInt32(gvr.Cells[15].Text);
                            qs.MaterialID = Convert.ToInt32(gvr.Cells[16].Text);
                            qs.RetrieveProjectID = projectid;
                            qs.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                            qs.Creator = db.EmpInfo.SingleOrDefault(u => u.Account == SPContext.Current.Web.CurrentUser.LoginName).EmpID;
                            db.QualifiedStocks.InsertOnSubmit(qs);
                        }
                    }

                    //结束当前任务
                    db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID.Equals(_taskid)).TaskState = "已完成";

                    db.SubmitChanges();
                }
                Response.Redirect("../../default-old.aspx", false);
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

        private int GetPricingIndex(string curunit)
        {
            if (bWriteOff)
            {
                switch (curunit)
                {
                    case "根/台/套/件":
                        return 8;
                    case "米":
                        return 9;
                    case "吨":
                        return 10;
                    default:
                        return -1;
                }
            }
            else
            {
                switch (curunit)
                {
                    case "根/台/套/件":
                        return 4;
                    case "米":
                        return 5;
                    case "吨":
                        return 6;
                    default:
                        return -1;
                }
            }
        }

        //保存数据到线下库
        private TableOfStocks StorageToOffLine(GridViewRow gvr)
        {
            TableOfStocks tos = new TableOfStocks();
            tos.StorageInType = "回收入库";
            tos.BatchIndex = "N/A";
            tos.MaterialCode = gvr.Cells[4].Text;
            tos.QuantityGentaojian = Convert.ToDecimal(gvr.Cells[8].Text);
            tos.QuantityMetre = Convert.ToDecimal(gvr.Cells[9].Text);
            tos.QuantityTon = Convert.ToDecimal(gvr.Cells[10].Text);
            tos.UnitPrice = Convert.ToDecimal(gvr.Cells[11].Text);
            tos.CurUnit = gvr.Cells[12].Text;
            tos.Amount = tos.UnitPrice * Convert.ToDecimal(gvr.Cells[GetPricingIndex(tos.CurUnit)].Text);
            tos.StorageTime = Convert.ToDateTime(gvr.Cells[13].Text);            
            tos.Remark = (gvr.Cells[15].Controls[0] as TextBox).Text.Trim();
            tos.ExpectedProject = projectid;
            tos.StorageID = Convert.ToInt32(gvr.Cells[17].Text);
            tos.PileID = Convert.ToInt32(gvr.Cells[18].Text);
            tos.ManufacturerID = Convert.ToInt32(gvr.Cells[19].Text);
            tos.MaterialID = Convert.ToInt32(gvr.Cells[20].Text);            
            return tos;
        }

        //保存数据到线上库
        private StockOnline StorageToOnLine(GridViewRow gvr)
        {
            StockOnline sol = new StockOnline();
            sol.StorageInType = "回收入库";
            sol.BatchIndex = "N/A";
            sol.MaterialCode = gvr.Cells[4].Text;
            sol.QuantityGentaojian = Convert.ToDecimal(gvr.Cells[8].Text);
            sol.QuantityMetre = Convert.ToDecimal(gvr.Cells[9].Text);
            sol.QuantityTon = Convert.ToDecimal(gvr.Cells[10].Text);
            sol.UnitPrice = Convert.ToDecimal(gvr.Cells[11].Text);
            sol.CurUnit = gvr.Cells[12].Text;
            sol.Amount = sol.UnitPrice * Convert.ToDecimal(gvr.Cells[GetPricingIndex(sol.CurUnit)].Text);
            sol.StorageTime = Convert.ToDateTime(gvr.Cells[13].Text);
            sol.Remark = (gvr.Cells[15].Controls[0] as TextBox).Text.Trim();
            sol.ExpectedProject = projectid;
            sol.StorageID = Convert.ToInt32(gvr.Cells[17].Text);
            sol.PileID = Convert.ToInt32(gvr.Cells[18].Text);
            sol.ManufacturerID = Convert.ToInt32(gvr.Cells[19].Text);
            sol.MaterialID = Convert.ToInt32(gvr.Cells[20].Text);   

            return sol;
        }

        //保存数据到回收合格库
        private QualifiedStocks StorageToQualified(GridViewRow gvr)
        {
            QualifiedStocks qfs = new QualifiedStocks();            
            qfs.Gentaojian = Convert.ToDecimal(gvr.Cells[8].Text);
            qfs.Metre = Convert.ToDecimal(gvr.Cells[9].Text);
            qfs.Ton = Convert.ToDecimal(gvr.Cells[10].Text);
            qfs.UnitPrice = Convert.ToDecimal(gvr.Cells[11].Text);
            qfs.CurUnit = gvr.Cells[12].Text;
            qfs.Amount = qfs.UnitPrice * Convert.ToDecimal(gvr.Cells[GetPricingIndex(qfs.CurUnit)].Text);
            qfs.StorageTime = Convert.ToDateTime(gvr.Cells[13].Text);
            qfs.RetrieveTime = Convert.ToDateTime(gvr.Cells[14].Text);
            qfs.Remark = (gvr.Cells[15].Controls[0] as TextBox).Text.Trim();
            qfs.RetrieveProjectID = projectid;
            qfs.StorageID = Convert.ToInt32(gvr.Cells[17].Text);
            qfs.PileID = Convert.ToInt32(gvr.Cells[18].Text);
            qfs.ManufactureID = Convert.ToInt32(gvr.Cells[19].Text);
            qfs.MaterialID = Convert.ToInt32(gvr.Cells[20].Text);
            return qfs;
        }

        #endregion
    }
}
