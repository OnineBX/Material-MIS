/*------------------------------------------------------------------------------
 * Unit Name：CommitOutDirectorMessage.cs
 * Description: 委外出库--主任确认后，显示审批信息的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-07-07
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
    public class CommitOutDirectorMessage:System.Web.UI.Page
    {    
        private int _taskid,_noticeid;

        private SPGridView spgvMaterial;
        private Button btnOK;            

        private static string[] ShowTlist =  {          
                                                 "物资编码:MaterialCode",                                                                     
                                                 "生产厂家:ManufacturerName",
                                                 "所属仓库:StorageName",
                                                 "所在垛位:PileName",
                                                 "到库日期:StorageTime",
                                                 "批次:BatchIndex",
                                                 "状态:MaterialStatus",
                                                 "库存(根/台/套/件):StocksGentaojian",
                                                 "出库(根/台/套/件):RealGentaojian",
                                                 "库存(米):StocksMetre",
                                                 "出库(米):RealMetre",
                                                 "库存(吨):StocksTon",
                                                 "出库(吨):RealTon",
                                                 "单价:UnitPrice",
                                                 "金额:RealAmount",
                                                 "备注:Remark"
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

            //刷新
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

            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);                                            

        }
        
        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息
                StorageCommitOutNotice scon = db.StorageCommitOutNotice.SingleOrDefault(u => u.StorageCommitOutNoticeID == this._noticeid);

                ((Label)GetControltByMaster("lblReceiver")).Text = db.BusinessUnitInfo.SingleOrDefault(u => u.BusinessUnitID == scon.Receiver).BusinessUnitName;
                (GetControltByMaster("lblNoticeCode") as Label).Text = scon.StorageCommitOutNoticeCode;
                ((Label)GetControltByMaster("lblDate")).Text = scon.CreateTime.ToLongDateString();

                //初始化出库物资明细
                this.spgvMaterial.DataSource = from a in db.StorageCommitOutRealDetails
                                               join b in db.StorageStocks on new { a.StocksID, Status = a.MaterialStatus } equals new { b.StocksID,b.Status }
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
                                               select new
                                               {
                                                   MaterialName = string.Format("{0}--规格型号：{1}", b.MaterialName, b.SpecificationModel),
                                                   b.MaterialCode,
                                                   b.ManufacturerName,
                                                   b.StorageName,
                                                   b.PileName,
                                                   b.BatchIndex,
                                                   a.MaterialStatus,
                                                   b.StorageTime,
                                                   StocksGenTaojian = b.StocksGenTaojian + a.RealGentaojian,
                                                   StocksMetre = b.StocksMetre + a.RealMetre,
                                                   StocksTon = b.StocksTon + a.RealTon,
                                                   a.RealGentaojian,
                                                   a.RealMetre,
                                                   a.RealTon,
                                                   b.UnitPrice,
                                                   a.RealAmount,
                                                   a.Remark,
                                                   Description = string.Format("财务编码：{0}--根台套件/米/吨(总库存)：{1}/{2}/{3}--根台套件/米/吨(调拨)：{4}/{5}/{6}",  b.FinanceCode,
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
                                               };
                this.spgvMaterial.DataBind();

                ////初始化主任审批信息
                StorageCommitOutDirectorConfirm scodc = db.StorageCommitOutDirectorConfirm.SingleOrDefault(u => u.TaskID.Equals(_taskid));
                (GetControltByMaster("lblDirector") as Label).Text = scodc.EmpInfo.EmpName;
                (GetControltByMaster("lblDirectorTime") as Label).Text = string.Concat(scodc.ConfirmTime.ToLongDateString(), scodc.ConfirmTime.ToLongTimeString());


                //初始化资产组长审核信息                
                (GetControltByMaster("lblAssetOpinion") as Label).Text = scodc.StorageCommitOutAssetAudit.AuditOpinion;
                (GetControltByMaster("lblAssetResult") as Label).Text = scodc.StorageCommitOutAssetAudit.AuditStatus;
                (GetControltByMaster("lblAssetChief") as Label).Text = scodc.StorageCommitOutAssetAudit.EmpInfo.EmpName;
                (GetControltByMaster("lblAssetTime") as Label).Text = string.Concat(scodc.StorageCommitOutAssetAudit.AuditTime.ToLongDateString(), scodc.StorageCommitOutAssetAudit.AuditTime.ToLongTimeString());

                //初始化生产组长审核信息                
                (GetControltByMaster("lblProduceOpinion") as Label).Text = scodc.StorageCommitOutAssetAudit.StorageCommitOutProduceAudit.AuditOpinion;
                (GetControltByMaster("lblProduceResult") as Label).Text = scodc.StorageCommitOutAssetAudit.StorageCommitOutProduceAudit.AuditStatus;
                (GetControltByMaster("lblProduceChief") as Label).Text = scodc.StorageCommitOutAssetAudit.StorageCommitOutProduceAudit.EmpInfo.EmpName;
                (GetControltByMaster("lblProduceTime") as Label).Text = string.Concat(scodc.StorageCommitOutAssetAudit.StorageCommitOutProduceAudit.AuditTime.ToLongDateString(), scodc.StorageCommitOutAssetAudit.StorageCommitOutProduceAudit.AuditTime.ToLongTimeString());

            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            //分支流程--任务已完成的情况
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                StorageOutTask sot = db.StorageOutTask.SingleOrDefault(u => u.TaskID == this._taskid);
                if (sot.TaskState.Equals("已完成"))
                {                                
                    (GetControltByMaster("lblProduceAuditTitle") as Label).ForeColor = System.Drawing.Color.Blue;
                    (GetControltByMaster("lblMaterialAuditTitle") as Label).ForeColor = System.Drawing.Color.Blue;
                    (GetControltByMaster("lblDirectorAuditTitle") as Label).ForeColor = System.Drawing.Color.Blue;
                    (GetControltByMaster("ltrInfo") as Literal).Visible = true;
                    btnOK.Visible = false;                                       
                }
            }

        }

        #endregion

        #region 控件事件

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
                    db.StorageOutTask.SingleOrDefault(u => u.TaskID.Equals(_taskid)).TaskState = "已完成";
                    db.SubmitChanges();
                    
                }
                Response.Redirect("../../default-old.aspx", false);
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }                                                              
        }
        
        #endregion

        #region 辅助函数
        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }

        private int reEmpId(string Emp)
        {
            int valueEmp = 0;
            try
            {
                using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    EmpInfo EI = dc.EmpInfo.SingleOrDefault(u => u.Account == Emp);
                    if (EI != null)
                        valueEmp = EI.EmpID;

                }

            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));  
            }              
            
            return valueEmp;
        }
        #endregion
    }
}
