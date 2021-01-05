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
    public class RiAssetWriteoffDetailsMessage:Page
    {
        private int _taskid, _receiptid;
        private SPGridView spgvMaterial;
        private Button btnOK;
        private bool bfinished = false;        

        private static string[] ShowTlist = {                                                      
                                                "物资编码:MaterialCode",                                                
                                                "生产厂家:ManufacturerName",
                                                "状态:Status",
                                                "根/台/套/件(出库):RealGentaojian",                                                
                                                "根/台/套/件(冲销后):Gentaojian",
                                                "米(出库):RealMetre",                                                
                                                "米(冲销后):Metre",
                                                "吨(出库):RealTon",
                                                "吨(冲销后):Ton",
                                                "单价:UnitPrice",
                                                "金额(出库):RealAmount",
                                                "金额(冲销后):Amount",                                                
                                                "备注:Remark"                                                
                                            };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {                    
                    _receiptid = db.SrinQualifiedReceipt.SingleOrDefault(u => u.TaskID.Equals(_taskid)).SrinQualifiedReceiptID;
                    if (db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID.Equals(_taskid)).TaskState.Equals("已完成"))
                        bfinished = true;
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
            this.spgvMaterial.GroupField = "MaterialName";
            this.spgvMaterial.GroupFieldDisplayName = "回收检验合格物资"; 


            BoundField bfColumn;

            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvMaterial.Columns.Add(bfColumn);
            }

        }

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {

                //初始化冲销明细
                this.spgvMaterial.DataSource = from a in db.SrinWriteOffDetails
                                               join c in db.StorageStocks on new { Status = a.StorageOutRealDetails.MaterialStatus, a.StorageOutRealDetails.StocksID, a.SrinQualifiedReceiptID } equals new { c.Status, c.StocksID, SrinQualifiedReceiptID = _receiptid }
                                               where a.SrinQualifiedReceiptID == _receiptid
                                               select new
                                               {
                                                   MaterialName = string.Format("{0}--规格型号：{1}", c.MaterialName, c.SpecificationModel),
                                                   c.MaterialCode,                                                                                                      
                                                   c.ManufacturerName,
                                                   c.Status,
                                                   a.StorageOutRealDetails.RealGentaojian,
                                                   a.StorageOutRealDetails.RealMetre,
                                                   a.StorageOutRealDetails.RealTon,
                                                   Gentaojian = a.StorageOutRealDetails.RealGentaojian - (from b in db.SrinWriteOffDetails
                                                                                                          where b.StorageOutRealDetailsID == a.StorageOutRealDetailsID
                                                                                                             && b.CreateTime <= a.CreateTime
                                                                                                          select b.Gentaojian).Sum(),
                                                   Metre = a.StorageOutRealDetails.RealMetre - (from b in db.SrinWriteOffDetails
                                                                                                where b.StorageOutRealDetailsID == a.StorageOutRealDetailsID
                                                                                                   && b.CreateTime <= a.CreateTime
                                                                                                select b.Metre).Sum(),
                                                   Ton = a.StorageOutRealDetails.RealTon - (from b in db.SrinWriteOffDetails
                                                                                            where b.StorageOutRealDetailsID == a.StorageOutRealDetailsID
                                                                                               && b.CreateTime <= a.CreateTime
                                                                                            select b.Ton).Sum(),
                                                   c.UnitPrice,
                                                   a.StorageOutRealDetails.RealAmount,
                                                   Amount = a.StorageOutRealDetails.RealAmount - (from b in db.SrinWriteOffDetails
                                                                                                  where b.StorageOutRealDetailsID == a.StorageOutRealDetailsID
                                                                                                     && b.CreateTime <= a.CreateTime
                                                                                                  select b.Amount).Sum(),
                                                   a.Remark,
                                                   Description = string.Format("财务编码：{0}--冲销数量(根台套件/米/吨)：{1}/{2}/{3}", c.FinanceCode, 
                                                                               a.SrinAssetQualifiedDetails.Gentaojian,a.SrinAssetQualifiedDetails.Metre,a.SrinAssetQualifiedDetails.Ton)
                                               };
                this.spgvMaterial.DataBind();                

            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            if (bfinished)//分支流程--任务已经完成的情况
            {
                (GetControltByMaster("ltrInfo") as Literal).Visible = true;
                btnOK.Visible = false;
            }
            
        }

        #endregion

        #region 控件事件方法

        void btnRefresh_Click(object sender, EventArgs e)
        {

        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            if (bfinished)//分支流程--任务已经完成的情况
                Response.Redirect("../../default-old.aspx", false);
            else
            {
                //string strBackUrl = string.Format("../StorageRetrieveIn/RiAssetWriteoffDetailsMessage.aspx?TaskID={0}", _taskid);
                //Response.Redirect(string.Format("../PublicPage/ErrorInfo.aspx?ErrorInfo=冲销操作已经完成，请返回！&DisposeUrl={0}&BackUrl={1}", strBackUrl, strBackUrl), false);
                Response.Redirect(string.Format("RiAssetWriteOffDetails.aspx?TaskID={0}",_taskid),false);
            }
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("RiAssetStorageDetails.aspx?TaskID={0}", _taskid), false);            
        }

        #endregion

        #region 辅助方法

        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }
        
        #endregion
    }
}
