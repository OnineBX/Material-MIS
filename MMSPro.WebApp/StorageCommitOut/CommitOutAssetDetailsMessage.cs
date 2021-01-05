/*------------------------------------------------------------------------------
 * Unit Name：CommitOutAssetDetailsMessage.cs
 * Description: 委外出库--物资管理员设置和修改实际出库数量后的信息显示页面
 * Author: Xu Chun Lei
 * Created Date: 2010-07-09
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
    public class CommitOutAssetDetailsMessage:System.Web.UI.Page
    {
        private int _noticeid;
        private int _taskid;

        private SPGridView spgvMaterial;
        private Button btnOK;
        private Label lblAuditTitle;
        private Literal ltrInfo;
        private bool bfinished = false;
        private bool baaudited = false;//资产组长是否审核过

        private static string[] ShowTlist =  {          
                                                 "物资编码:MaterialCode",                                                                     
                                                 "生产厂家:ManufacturerName",
                                                 "所属仓库:StorageName",
                                                 "所在垛位:PileName",
                                                 "到库日期:StorageTime",
                                                 "批次:BatchIndex",
                                                 "状态:Status",
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
                    if (sot.TaskState.Equals("已完成"))
                        bfinished = true;

                    if(db.StorageCommitOutAssetAudit.Count(u => u.TaskID.Equals(GetPreviousTaskID("物资出库审核",_taskid))) != 0)
                        baaudited = true;
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

            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);           

            lblAuditTitle = (Label)GetControltByMaster("lblAuditTitle");
            ltrInfo = GetControltByMaster("ltrInfo") as Literal;

        }

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息
                StorageCommitOutProduceAudit scopa = db.StorageCommitOutProduceAudit.SingleOrDefault(u => u.TaskID.Equals(GetPreviousTaskID("物资调拨审核", _taskid)));
                ((Label)GetControltByMaster("lblReceiver")).Text = scopa.StorageCommitOutNotice.BusinessUnitInfo.BusinessUnitName;
                ((Label)GetControltByMaster("lblNoticeCode")).Text = scopa.StorageCommitOutNotice.StorageCommitOutNoticeCode;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(scopa.StorageCommitOutNotice.CreateTime.ToLongDateString(), scopa.StorageCommitOutNotice.CreateTime.ToLongTimeString());

                //初始化物资调拨明细列表                
                this.spgvMaterial.DataSource = from a in db.StorageCommitOutRealDetails
                                               join b in db.StorageStocks on new { a.StocksID, Status = a.MaterialStatus} equals new {b.StocksID,b.Status}
                                               join d in db.StorageCommitOutDetails on new { a.StorageCommitOutNoticeID, b.MaterialID } equals new { d.StorageCommitOutNoticeID, d.MaterialID }
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
                                                   b.Status,
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
                                                                                                                                                                          d.Gentaojian,
                                                                                                                                                                          d.Metre,
                                                                                                                                                                          d.Ton)

                                               };
                this.spgvMaterial.DataBind();

                //初始化生产技术部审核信息                
                (GetControltByMaster("lblOpinion") as Label).Text = scopa.AuditOpinion;
                (GetControltByMaster("lblResult") as Label).Text = scopa.AuditStatus;
                (GetControltByMaster("lblProduceChief") as Label).Text = scopa.EmpInfo.EmpName;
                (GetControltByMaster("lblProduceTime") as Label).Text = string.Concat(scopa.AuditTime.ToLongDateString(), scopa.AuditTime.ToLongTimeString());
            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            //分支流程--任务已完成的情况
            if (bfinished)
            {
                btnOK.Visible = false;
                ltrInfo.Visible = true;
                lblAuditTitle.ForeColor = System.Drawing.Color.Blue;
                
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
            {
                if (baaudited)
                    Response.Redirect(string.Format("CommitOutAssetAuditInfo.aspx?TaskID={0}", _taskid), false);
                else
                    Response.Redirect(string.Format("CommitOutAssetDetails.aspx?TaskID={0}", _taskid), false);
            }                
        }             

        void btnOK_Click(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("CreateCommitOutTask.aspx?TaskID={0}&NoticeID={1}&TaskType=物资出库审核", _taskid,_noticeid),false);
        }
        #endregion                        

        #region 辅助函数

        private int GetPreviousTaskID(string tasktype, int taskid)
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                StorageOutTask sot = db.StorageOutTask.SingleOrDefault(u => u.TaskID == taskid);
                int tid = sot.PreviousTaskID;
                if (sot.TaskType.Equals(tasktype))
                    return taskid;
                if (tid == -1)
                    return 0;
                return GetPreviousTaskID(tasktype, tid);
            }
        }

        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }
        #endregion
    }
}
