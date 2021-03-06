﻿/*------------------------------------------------------------------------------
 * Unit Name：NormalOutModifyAssetDetails.cs
 * Description: 正常出库--资产管理员修改出库明细页面
 * Author: Xu Chun Lei
 * Created Date: 2010-10-28
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
    public class NormalOutAssetModifyDetails : System.Web.UI.Page
    {
        private int _noticeid;
        private int _taskid;        

        private SPGridView spgvMaterial;
        private Button btnOK;

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
                                                 "DetailsID:StorageOutDetailsID",
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
            this.spgvMaterial = new SPGridView();
            this.spgvMaterial.AutoGenerateColumns = false;
            this.spgvMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");
            this.spgvMaterial.AllowGrouping = true;
            this.spgvMaterial.AllowGroupCollapse = true;
            this.spgvMaterial.GroupDescriptionField = "Description";
            this.spgvMaterial.GroupField = "MaterialName";
            this.spgvMaterial.GroupFieldDisplayName = "调拨物资";
            
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
            tfRemark.ItemTemplate = new TextBoxTemplate("备注", DataControlRowType.DataRow,"Remark");
            this.spgvMaterial.Columns.Insert(14, tfRemark);

            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);             

        }

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息
                StorageOutProduceAudit sopa = db.StorageOutProduceAudit.SingleOrDefault(u => u.TaskID.Equals(GetPreviousTaskID(0, _taskid)));

                (GetControltByMaster("lblConstructor") as Label).Text = sopa.StorageOutNotice.BusinessUnitInfo1.BusinessUnitName;
                (GetControltByMaster("lblProprietor") as Label).Text = sopa.StorageOutNotice.BusinessUnitInfo.BusinessUnitName;
                (GetControltByMaster("lblProject") as Label).Text = string.Format("{0}({1}阶段)", sopa.StorageOutNotice.ProjectInfo.ProjectName, sopa.StorageOutNotice.ProjectStage);
                (GetControltByMaster("lblNoticeCode") as Label).Text = sopa.StorageOutNotice.StorageOutNoticeCode;
                (GetControltByMaster("lblProperty") as Label).Text = sopa.StorageOutNotice.ProjectInfo.ProjectProperty;
                (GetControltByMaster("lblDate") as Label).Text = sopa.StorageOutNotice.CreateTime.ToLongDateString(); 
                
                //初始化物资出库明细列表                
                var Details = (from a in db.StorageOutRealDetails.AsEnumerable()//已加入到出库表中的物资
                               join b in db.StorageStocks.AsEnumerable() on new { a.StocksID, Status = a.MaterialStatus } equals new { b.StocksID, b.Status }
                               join d in db.StorageOutDetails.AsEnumerable() on new { a.StorageOutNoticeID, b.MaterialID } equals new { d.StorageOutNoticeID, d.MaterialID }
                               where a.StorageOutNoticeID == _noticeid
                               orderby b.MaterialID, b.StorageTime ascending
                               let v1 = (from e in db.StorageOutRealDetails.AsEnumerable()
                                         where e.StorageOutNoticeID == _noticeid && e.StorageOutDetails.MaterialID == b.MaterialID
                                         select new { e.RealGentaojian }).Sum(u => u.RealGentaojian)
                               let v2 = (from e in db.StorageOutRealDetails.AsEnumerable()
                                         where e.StorageOutNoticeID == _noticeid && e.StorageOutDetails.MaterialID == b.MaterialID
                                         select new { e.RealMetre }).Sum(u => u.RealMetre)
                               let v3 = (from e in db.StorageOutRealDetails.AsEnumerable()
                                         where e.StorageOutNoticeID == _noticeid && e.StorageOutDetails.MaterialID == b.MaterialID
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
                                                                                                                                              d.Gentaojian, d.Metre, d.Ton)
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
                                   ID = a.StorageOutRealDetailsID,
                                   Description = v,
                                   a.Remark,
                                   IsSelect = true,
                                   a.StorageOutDetailsID
                               }).AsEnumerable().Union(
                                               from a in db.StorageStocks.AsEnumerable()
                                               where (from b in db.StorageOutDetails.AsEnumerable()
                                                      where b.StorageOutNoticeID.Equals(_noticeid)
                                                      select b.MaterialID).AsEnumerable().Contains(a.MaterialID)
                                                      && !(from c in db.StorageOutRealDetails.AsEnumerable()
                                                           where c.StorageOutNoticeID.Equals(_noticeid)
                                                           select new { c.StocksID, Status = c.MaterialStatus }).AsEnumerable().Contains(new { a.StocksID, a.Status })
                                               join b in db.StorageOutDetails.AsEnumerable() on new { a.MaterialID, StorageOutNoticeID = _noticeid } equals new { b.MaterialID, b.StorageOutNoticeID }
                                               orderby a.MaterialID, a.StorageTime ascending
                                               let v1 = (from e in db.StorageOutRealDetails.AsEnumerable()
                                                         where e.StorageOutNoticeID == _noticeid && e.StorageOutDetails.MaterialID == b.MaterialID
                                                         select new { e.RealGentaojian }).Sum(u => u.RealGentaojian)
                                               let v2 = (from e in db.StorageOutRealDetails.AsEnumerable()
                                                         where e.StorageOutNoticeID == _noticeid && e.StorageOutDetails.MaterialID == b.MaterialID
                                                         select new { e.RealMetre }).Sum(u => u.RealMetre)
                                               let v3 = (from e in db.StorageOutRealDetails.AsEnumerable()
                                                         where e.StorageOutNoticeID == _noticeid && e.StorageOutDetails.MaterialID == b.MaterialID
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
                                                   b.StorageOutDetailsID
                                               }
                                               ).AsEnumerable();
                this.spgvMaterial.DataSource = from a in Details
                                               orderby a.MaterialName, a.StorageTime ascending
                                               select a;
                this.spgvMaterial.DataBind();

                //初始化生产技术部审核信息                
                ((Label)GetControltByMaster("lblOpinion")).Text = sopa.AuditOpinion;
                ((Label)GetControltByMaster("lblResult")).Text = sopa.AuditStatus;
                ((Label)GetControltByMaster("lblProduceChief")).Text = sopa.EmpInfo.EmpName;
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
                    StorageOutRealDetails sord;
                    int ID, iDetailsID;
                    decimal iPricingQuantity;
                    bool bSelect;
                    foreach (GridViewRow gvr in this.spgvMaterial.Rows)
                    {
                        iPricingQuantity = Convert.ToDecimal((gvr.Cells[GetPricingIndex(gvr.Cells[16].Text)].Controls[0] as TextBox).Text.Trim());
                        bSelect = Convert.ToBoolean(gvr.Cells[17].Text);
                        iDetailsID = Convert.ToInt32(gvr.Cells[18].Text);
                        ID = Convert.ToInt32(gvr.Cells[19].Text);
                        if (bSelect)//来自出库明细表
                            sord = db.StorageOutRealDetails.SingleOrDefault(u => u.StorageOutRealDetailsID.Equals(ID));
                        else
                            sord = new StorageOutRealDetails();
                        if (iPricingQuantity == 0)//计量单位所设置的出库数量为0的情况
                        {
                            if (bSelect)
                                db.StorageOutRealDetails.DeleteOnSubmit(sord);
                            continue;
                        }

                        if (!bSelect)//没有在出库明细表的情况
                        {
                            sord.StorageOutNoticeID = _noticeid;
                            sord.StorageOutDetailsID = iDetailsID;
                            sord.StocksID = ID;
                            sord.MaterialStatus = gvr.Cells[7].Text;
                            sord.Creator = (db.EmpInfo.SingleOrDefault(u => u.EmpName == SPContext.Current.Web.CurrentUser.Name)).EmpID;
                        }
                        sord.RealGentaojian = Convert.ToDecimal(((TextBox)gvr.Cells[9].Controls[0]).Text);
                        sord.RealMetre = Convert.ToDecimal(((TextBox)gvr.Cells[11].Controls[0]).Text);
                        sord.RealTon = Convert.ToDecimal(((TextBox)gvr.Cells[13].Controls[0]).Text);
                        sord.RealAmount = iPricingQuantity * Convert.ToDecimal(gvr.Cells[14].Text);
                        sord.Remark = (gvr.Cells[15].Controls[0] as TextBox).Text.Trim();
                        sord.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                        if (!bSelect)
                            db.StorageOutRealDetails.InsertOnSubmit(sord);
                    }
                    db.SubmitChanges();

                }

                Response.Redirect(string.Format("NormalOutAssetDetailsMessage.aspx?TaskID={0}", _taskid), false);
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

        #endregion
    }
}
