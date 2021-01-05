/*------------------------------------------------------------------------------
 * Unit Name：CommitOutAssetAuditInfo.cs
 * Description: 委外出库--资产管理员处理资产组长审核信息的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-07-08
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
    public class CommitOutAssetAuditInfo : System.Web.UI.Page
    {
        private int _taskid,_noticeid;      

        private SPGridView spgvMaterial;
        private Button btnOK;
        private Label  lblProduceAuditTitle, lblMaterialAuditTitle;        

        private static string[] ShowTlist =  {          
                                                 "物资编码:MaterialCode",                                                                     
                                                 "生产厂家:ManufacturerName",
                                                 "所属仓库:StorageName",
                                                 "所在垛位:PileName",
                                                 "到库日期:StorageTime",
                                                 "批次:BatchIndex",
                                                 "状态:Status",
                                                 "库存(根/台/套/件):StocksGentaojian",                                                 
                                                 "库存(米):StocksMetre",                                                 
                                                 "库存(吨):StocksTon",                                                 
                                                 "单价:UnitPrice",
                                                 "计量单位:CurUnit",
                                                 "IsSelect:IsSelect",
                                                 "DetailsID:StorageCommitOutDetailsID",
                                                 "ID:ID"
                                             };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                _taskid = Convert.ToInt32(Request.QueryString["TaskID"]);

                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    StorageOutTask sot = db.StorageOutTask.SingleOrDefault(u => u.TaskID == this._taskid);
                    if (sot.TaskState.Equals("已完成"))
                    {
                        Response.Redirect(string.Format("CommitOutAssetDetailsMessage.aspx?TaskID={0}", _taskid), false);
                        return;
                    }
                    _noticeid = sot.NoticeID;                    
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

        #region 初始化和绑定函数

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

        private void InitializeCustomControls()
        {
            InitBar();

            this.spgvMaterial = new SPGridView();
            this.spgvMaterial.AutoGenerateColumns = false;
            this.spgvMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");
            this.spgvMaterial.AllowGrouping = true;
            this.spgvMaterial.AllowGroupCollapse = true;
            this.spgvMaterial.GroupDescriptionField = "Description";
            this.spgvMaterial.GroupField = "MaterialName";
            this.spgvMaterial.GroupFieldDisplayName = "出库物资";
            
            BoundField bfColumn;

            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvMaterial.Columns.Add(bfColumn);
            }

            //加入实际出库数量(根/台/套/件)列
            TemplateField tfGentaojian = new TemplateField();
            tfGentaojian.HeaderText = "出库(根/台/套/件)";
            tfGentaojian.ItemTemplate = new TextBoxTemplate("Gentaojian", "RealGentaojian", "^(-?\\d+)(\\.\\d+)?$", 60);
            this.spgvMaterial.Columns.Insert(8, tfGentaojian);

            //加入实际出库数量(米)列
            TemplateField tfMetre = new TemplateField();
            tfMetre.HeaderText = "出库(米)";
            tfMetre.ItemTemplate = new TextBoxTemplate("Metre", "RealMetre", "^(-?\\d+)(\\.\\d+)?$", 60);
            this.spgvMaterial.Columns.Insert(10, tfMetre);

            //加入实际出库数量(根/台/套/件)列
            TemplateField tfTon = new TemplateField();
            tfTon.HeaderText = "出库(吨)";
            tfTon.ItemTemplate = new TextBoxTemplate("Ton", "RealTon", "^(-?\\d+)(\\.\\d+)?$", 60);
            this.spgvMaterial.Columns.Insert(12, tfTon);

            //加入备注列            
            TemplateField tfRemark = new TemplateField();
            tfRemark.HeaderText = "备注";
            tfRemark.ItemTemplate = new TextBoxTemplate("备注", DataControlRowType.DataRow, "Remark");
            this.spgvMaterial.Columns.Insert(14, tfRemark);

            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);
                       
            lblProduceAuditTitle = (Label)GetControltByMaster("lblProduceAuditTitle");
            lblMaterialAuditTitle = (Label)GetControltByMaster("lblMaterialAuditTitle");

        }        

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息
                StorageCommitOutAssetAudit scoaa = db.StorageCommitOutAssetAudit.SingleOrDefault(u => u.TaskID.Equals(GetPreviousTaskID(0,_taskid)));

                ((Label)GetControltByMaster("lblReceiver")).Text = scoaa.StorageCommitOutNotice.BusinessUnitInfo.BusinessUnitName;
                ((Label)GetControltByMaster("lblNoticeCode")).Text = scoaa.StorageCommitOutNotice.StorageCommitOutNoticeCode;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(scoaa.StorageCommitOutNotice.CreateTime.ToLongDateString(),scoaa.StorageCommitOutNotice.CreateTime.ToLongTimeString());
                
                //初始化物资出库明细列表                
                var Details = (from a in db.StorageCommitOutRealDetails.AsEnumerable()
                               join b in db.StorageStocks.AsEnumerable() on new { a.StocksID, Status = a.MaterialStatus } equals new { b.StocksID, b.Status }
                               where a.StorageCommitOutNoticeID == _noticeid
                               orderby b.MaterialID, b.StorageTime ascending
                               let v1 = (from e in db.StorageCommitOutRealDetails.AsEnumerable()
                                         where e.StorageCommitOutNoticeID == _noticeid && e.StorageCommitOutDetails.MaterialID == b.MaterialID
                                         select new { e.RealGentaojian }).Sum(u => u.RealGentaojian)
                               let v2 = (from e in db.StorageCommitOutRealDetails.AsEnumerable()
                                         where e.StorageCommitOutNoticeID == _noticeid && e.StorageCommitOutDetails.MaterialID == b.MaterialID
                                         select new { e.RealMetre }).Sum(u => u.RealMetre)
                               let v3 = (from e in db.StorageCommitOutRealDetails.AsEnumerable()
                                         where e.StorageCommitOutNoticeID == _noticeid && e.StorageCommitOutDetails.MaterialID == b.MaterialID
                                         select new { e.RealTon }).Sum(u => u.RealTon)
                               let v = string.Format("财务编码：{0}--根台套件/米/吨(总库存)：{1}/{2}/{3}--根台套件/米/吨(调拨)：{4}/{5}/{6}", b.FinanceCode,
                                                                                                                                             (from c in db.StorageStocks
                                                                                                                                              where c.MaterialID == b.MaterialID
                                                                                                                                              select c).Sum(u => u.StocksGenTaojian) + v1,
                                                                                                                                              (from c in db.StorageStocks
                                                                                                                                               where c.MaterialID == b.MaterialID
                                                                                                                                               select c).Sum(u => u.StocksMetre) + v2,
                                                                                                                                              (from c in db.StorageStocks
                                                                                                                                               where c.MaterialID == b.MaterialID
                                                                                                                                               select c).Sum(u => u.StocksTon) + v3,
                                                                                                                                              a.StorageCommitOutDetails.Gentaojian,
                                                                                                                                              a.StorageCommitOutDetails.Metre,
                                                                                                                                              a.StorageCommitOutDetails.Ton)
                               select new
                               {
                                   MaterialName = string.Format("{0}--规格型号：{1}", b.MaterialName, b.SpecificationModel),
                                   b.MaterialCode,
                                   b.ManufacturerName,
                                   b.StorageName,
                                   b.PileName,
                                   b.BatchIndex,
                                   Status = a.MaterialStatus,
                                   b.StorageTime,
                                   StocksGenTaojian = b.StocksGenTaojian + a.RealGentaojian,
                                   StocksMetre = b.StocksMetre + a.RealMetre,
                                   StocksTon = b.StocksTon + a.RealTon,
                                   a.RealGentaojian,
                                   a.RealMetre,
                                   a.RealTon,
                                   b.UnitPrice,
                                   b.CurUnit,
                                   ID = a.StorageCommitOutRealDetailsID,
                                   Description = v,
                                   a.Remark,
                                   IsSelect = true,
                                   a.StorageCommitOutDetailsID
                               }).AsEnumerable().Union(
                                               from a in db.StorageStocks.AsEnumerable()
                                               where (from b in db.StorageCommitOutDetails.AsEnumerable()
                                                      where b.StorageCommitOutNoticeID.Equals(_noticeid)
                                                      select b.MaterialID).AsEnumerable().Contains(a.MaterialID)
                                                      && !(from c in db.StorageCommitOutRealDetails.AsEnumerable()
                                                           where c.StorageCommitOutNoticeID.Equals(_noticeid)
                                                           select new { c.StocksID, Status = c.MaterialStatus }).AsEnumerable().Contains(new { a.StocksID,a.Status })
                                               join b in db.StorageCommitOutDetails.AsEnumerable() on new { a.MaterialID, StorageCommitOutNoticeID = _noticeid } equals new { b.MaterialID, b.StorageCommitOutNoticeID }
                                               orderby a.MaterialID, a.StorageTime ascending
                                               let v1 = (from e in db.StorageCommitOutRealDetails.AsEnumerable()
                                                         where e.StorageCommitOutNoticeID == _noticeid && e.StorageCommitOutDetails.MaterialID == b.MaterialID
                                                         select new { e.RealGentaojian }).Sum(u => u.RealGentaojian)
                                               let v2 = (from e in db.StorageCommitOutRealDetails.AsEnumerable()
                                                         where e.StorageCommitOutNoticeID == _noticeid && e.StorageCommitOutDetails.MaterialID == b.MaterialID
                                                         select new { e.RealMetre }).Sum(u => u.RealMetre)
                                               let v3 = (from e in db.StorageCommitOutRealDetails.AsEnumerable()
                                                         where e.StorageCommitOutNoticeID == _noticeid && e.StorageCommitOutDetails.MaterialID == b.MaterialID
                                                         select new { e.RealTon }).Sum(u => u.RealTon)                                               
                                               let v5 = string.Format("财务编码：{0}--根台套件/米/吨(总库存)：{1}/{2}/{3}--根台套件/米/吨(调拨)：{4}/{5}/{6}", a.FinanceCode,
                                                                                                                                                             (from c in db.StorageStocks
                                                                                                                                                              where c.MaterialID == a.MaterialID
                                                                                                                                                              select c).Sum(u => u.StocksGenTaojian) + v1,
                                                                                                                                                              (from c in db.StorageStocks
                                                                                                                                                               where c.MaterialID == a.MaterialID
                                                                                                                                                               select c).Sum(u => u.StocksMetre) + v2,
                                                                                                                                                              (from c in db.StorageStocks
                                                                                                                                                               where c.MaterialID == a.MaterialID
                                                                                                                                                               select c).Sum(u => u.StocksTon) + v3,
                                                                                                                                                              b.Gentaojian, b.Metre, b.Ton)
                                               select new
                                               {
                                                   MaterialName = string.Format("{0}--规格型号：{1}", a.MaterialName, a.SpecificationModel),
                                                   a.MaterialCode,
                                                   a.ManufacturerName,
                                                   a.StorageName,
                                                   a.PileName,
                                                   a.BatchIndex,
                                                   a.Status,
                                                   a.StorageTime,
                                                   a.StocksGenTaojian,
                                                   a.StocksMetre,
                                                   a.StocksTon,
                                                   RealGentaojian = Decimal.Zero,
                                                   RealMetre = Decimal.Zero,
                                                   RealTon = Decimal.Zero,
                                                   a.UnitPrice,
                                                   a.CurUnit,
                                                   ID = a.StocksID,
                                                   Description = v5,
                                                   Remark = String.Empty,
                                                   IsSelect = false,
                                                   b.StorageCommitOutDetailsID
                                               }
                                               ).AsEnumerable();
                this.spgvMaterial.DataSource = from a in Details
                                               orderby a.MaterialName, a.StorageTime ascending
                                               select a;
                this.spgvMaterial.DataBind();

                //初始化物资组长审核信息                
                (GetControltByMaster("txtAssetOpinion") as TextBox).Text = scoaa.AuditOpinion;
                (GetControltByMaster("lblAssetResult") as Label).Text = scoaa.AuditStatus;
                (GetControltByMaster("lblAssetChief") as Label).Text = scoaa.EmpInfo.EmpName;
                (GetControltByMaster("lblAssetTime") as Label).Text = string.Concat(scoaa.AuditTime.ToLongDateString(), scoaa.AuditTime.ToLongTimeString());

                //初始化生产组长审核信息                
                (GetControltByMaster("lblProduceOpinion") as Label).Text = scoaa.StorageCommitOutProduceAudit.AuditOpinion;
                (GetControltByMaster("lblProduceResult") as Label).Text = scoaa.StorageCommitOutProduceAudit.AuditStatus;
                (GetControltByMaster("lblProduceChief") as Label).Text = scoaa.StorageCommitOutProduceAudit.EmpInfo.EmpName;
                (GetControltByMaster("lblProduceTime") as Label).Text = string.Concat(scoaa.StorageCommitOutProduceAudit.AuditTime.ToLongDateString(),scoaa.StorageCommitOutProduceAudit.AuditTime.ToLongTimeString());                
            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);
            
            this.spgvMaterial.Columns[16].Visible = false;
            this.spgvMaterial.Columns[17].Visible = false;
            this.spgvMaterial.Columns[18].Visible = false;
            this.spgvMaterial.Columns[19].Visible = false;           
            
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
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    StorageCommitOutRealDetails scord;
                    int ID,iDetailsID;
                    decimal iPricingQuantity;
                    bool bSelect;
                    foreach (GridViewRow gvr in this.spgvMaterial.Rows)
                    {
                        iPricingQuantity = Convert.ToDecimal((gvr.Cells[GetPricingIndex(gvr.Cells[16].Text)].Controls[0] as TextBox).Text.Trim());
                        bSelect = Convert.ToBoolean(gvr.Cells[17].Text);
                        iDetailsID = Convert.ToInt32(gvr.Cells[18].Text);
                        ID = Convert.ToInt32(gvr.Cells[19].Text);
                        if (bSelect)//来自出库明细表
                            scord = db.StorageCommitOutRealDetails.SingleOrDefault(u => u.StorageCommitOutRealDetailsID.Equals(ID));
                        else
                            scord = new StorageCommitOutRealDetails();
                        if (iPricingQuantity == 0)//计量单位所设置的出库数量为0的情况
                        {
                            if (bSelect)
                                db.StorageCommitOutRealDetails.DeleteOnSubmit(scord);
                            continue;
                        }

                        if (!bSelect)//没有在出库明细表的情况
                        {
                            scord.StorageCommitOutNoticeID = _noticeid;
                            scord.StorageCommitOutDetailsID = iDetailsID;
                            scord.StocksID = ID;                            
                            scord.Creator = (db.EmpInfo.SingleOrDefault(u => u.EmpName == SPContext.Current.Web.CurrentUser.Name)).EmpID;
                        }
                        scord.MaterialStatus = gvr.Cells[7].Text;
                        scord.RealGentaojian = Convert.ToDecimal(((TextBox)gvr.Cells[9].Controls[0]).Text);
                        scord.RealMetre = Convert.ToDecimal(((TextBox)gvr.Cells[11].Controls[0]).Text);
                        scord.RealTon = Convert.ToDecimal(((TextBox)gvr.Cells[13].Controls[0]).Text);
                        scord.RealAmount = iPricingQuantity * Convert.ToDecimal(gvr.Cells[14].Text);
                        scord.Remark = (gvr.Cells[15].Controls[0] as TextBox).Text.Trim();
                        scord.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                        if (!bSelect)
                            db.StorageCommitOutRealDetails.InsertOnSubmit(scord);
                    }
                    db.SubmitChanges();

                }

                Response.Redirect(string.Format("CommitOutAssetDetailsMessage.aspx?TaskID={0}", _taskid), false);
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

        #region 辅助函数

        private int GetPricingIndex(string curunit)
        {
            switch (curunit)
            {
                case "根/台/套/件":
                    return 9;
                case "米":
                    return 11;
                case "吨":
                    return 13;
                default:
                    return -1;
            }
        }

        private int GetPreviousTaskID(int step, int taskid)
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                int tid = db.StorageOutTask.SingleOrDefault(u => u.TaskID == taskid).PreviousTaskID;
                if (step == 0)
                    return tid;
                return GetPreviousTaskID(--step, tid);
            }
        }

        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }
        
        #endregion
    }
}

