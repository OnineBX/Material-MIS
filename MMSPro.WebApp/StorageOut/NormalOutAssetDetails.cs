/*------------------------------------------------------------------------------
 * Unit Name：NormalOutAssetDetails.cs
 * Description: 正常出库--资产管理员确定出库明细的页面
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
    public class NormalOutAssetDetails:System.Web.UI.Page
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
                                                 "DetailsID:StorageOutDetailsID",
                                                 "ID:StocksID"
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

                    if (sot.TaskState.Equals("已完成"))//分支流程--任务已经完成的情况
                    {
                        Response.Redirect(string.Format("NormalOutAssetDetailsMessage.aspx?TaskID={0}", _taskid),false);
                        return;
                    }
                }

                //分支流程--已经生成出库明细的情况
                if (RealDetailsIsDone())
                {
                    Response.Redirect(string.Format("NormalOutAssetModifyDetails.aspx?TaskID={0}", _taskid), false);
                    return;
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
            tfGentaojian.ItemTemplate = new TextBoxTemplate("Gentaojian", string.Empty, "^(-?\\d+)(\\.\\d+)?$", "0", 60);
            this.spgvMaterial.Columns.Insert(8, tfGentaojian);

            //加入实际出库数量(米)列
            TemplateField tfMetre = new TemplateField();
            tfMetre.HeaderText = "出库(米)";
            tfMetre.ItemTemplate = new TextBoxTemplate("Metre", string.Empty, "^(-?\\d+)(\\.\\d+)?$", "0", 60);
            this.spgvMaterial.Columns.Insert(10, tfMetre);

            //加入实际出库数量(根/台/套/件)列
            TemplateField tfTon = new TemplateField();
            tfTon.HeaderText = "出库(吨)";
            tfTon.ItemTemplate = new TextBoxTemplate("Ton", string.Empty, "^(-?\\d+)(\\.\\d+)?$", "0", 60);
            this.spgvMaterial.Columns.Insert(12, tfTon);

            //加入备注列            
            TemplateField tfRemark = new TemplateField();
            tfRemark.HeaderText = "备注";
            tfRemark.ItemTemplate = new TextBoxTemplate("备注", DataControlRowType.DataRow);
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

                //初始化物资调拨明细列表                
                this.spgvMaterial.DataSource = from a in db.StorageStocks
                                                where (from b in db.StorageOutDetails
                                                       where b.StorageOutNoticeID.Equals(_noticeid)
                                                       select b.MaterialID).Contains(a.MaterialID)
                                                join b in db.StorageOutDetails on new { a.MaterialID, StorageOutNoticeID =_noticeid } equals new { b.MaterialID, b.StorageOutNoticeID }
                                                orderby a.MaterialID,a.StorageTime ascending
                                                select new
                                                {
                                                    MaterialName = string.Format("{0}--规格型号：{1}",a.MaterialName,a.SpecificationModel),
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
                                                    a.UnitPrice,
                                                    a.CurUnit,
                                                    b.StorageOutDetailsID,
                                                    a.StocksID,
                                                    Description = string.Format("财务编码：{0}--根台套件/米/吨(总库存)：{1}/{2}/{3}--根台套件/米/吨(调拨)：{4}/{5}/{6}", a.FinanceCode,
                                                                                                                                                                         (from c in db.StorageStocks
                                                                                                                                                                          where c.MaterialID == a.MaterialID
                                                                                                                                                                          select c).Sum(u => u.StocksGenTaojian),
                                                                                                                                                                         (from c in db.StorageStocks
                                                                                                                                                                          where c.MaterialID == a.MaterialID
                                                                                                                                                                          select c).Sum(u => u.StocksMetre),
                                                                                                                                                                          (from c in db.StorageStocks
                                                                                                                                                                          where c.MaterialID == a.MaterialID
                                                                                                                                                                          select c).Sum(u => u.StocksTon),
                                                                                                                                                                          b.Gentaojian,b.Metre,b.Ton)                                                    

                                                };
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
                    decimal iPricingQuantity;
                    foreach (GridViewRow gvr in this.spgvMaterial.Rows)
                    {
                        iPricingQuantity = Convert.ToDecimal((gvr.Cells[GetPricingIndex(gvr.Cells[16].Text)].Controls[0] as TextBox).Text.Trim());
                        if (iPricingQuantity == 0)
                            continue;
                        sord = new StorageOutRealDetails();                       
                        sord.StorageOutNoticeID = _noticeid;
                        sord.StorageOutDetailsID = Convert.ToInt32(gvr.Cells[17].Text);
                        sord.StocksID = Convert.ToInt32(gvr.Cells[18].Text);
                        sord.MaterialStatus = gvr.Cells[7].Text;
                        sord.RealGentaojian = Convert.ToDecimal(((TextBox)gvr.Cells[9].Controls[0]).Text);
                        sord.RealMetre = Convert.ToDecimal(((TextBox)gvr.Cells[11].Controls[0]).Text);
                        sord.RealTon = Convert.ToDecimal(((TextBox)gvr.Cells[13].Controls[0]).Text);
                        sord.RealAmount = iPricingQuantity * Convert.ToDecimal(gvr.Cells[14].Text);
                        sord.Remark = (gvr.Cells[15].Controls[0] as TextBox).Text.Trim();
                        sord.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                        sord.Creator = (db.EmpInfo.SingleOrDefault(u => u.EmpName == SPContext.Current.Web.CurrentUser.Name)).EmpID;
                        
                        db.StorageOutRealDetails.InsertOnSubmit(sord);
                    }
                    db.SubmitChanges();                   
                    
                }

                Response.Redirect(string.Format("NormalOutAssetDetailsMessage.aspx?TaskID={0}", _taskid),false);
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

        private bool RealDetailsIsDone()
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    if (db.StorageOutRealDetails.Count(u => u.StorageOutNoticeID.Equals(_noticeid)) != 0)
                        return true;
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));  
                return false;
            }               
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
