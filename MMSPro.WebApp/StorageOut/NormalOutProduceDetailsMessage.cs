/*------------------------------------------------------------------------------
 * Unit Name：NormalOutProduceDetailsMessage.cs
 * Description: 正常出库--显示生产技术员修改调拨明细信息的页面
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
    public class NormalOutProduceDetailsMessage:Page
    {
        private int _taskid,_noticeid;
        private SPGridView spgvMaterial;
        private Button btnOK;
        private bool bfinished = false;

        private static string[] ShowTlist =  { 
                                                 "财务编码:FinanceCode", 
                                                 "物资名称:MaterialName",
                                                 "规格型号:SpecificationModel",                                                                                         
                                                 "库存数量(根/台/套/件):StocksGentaojian",
                                                 "调拨数量(根/台/套/件):Gentaojian",   
                                                 "库存数量(米):StocksMetre",
                                                 "调拨数量(米):Metre",
                                                 "库存数量(吨):StocksTon",
                                                 "调拨数量(吨):Ton",
                                                 "备注:Remark"
                                              };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._taskid = Convert.ToInt32(Request.QueryString["TaskID"]);

                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    StorageOutTask sot = db.StorageOutTask.SingleOrDefault(u => u.TaskID.Equals(_taskid));
                    if (sot.TaskState.Equals("已完成"))
                        bfinished = true;
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
            //初始化ToolBar
            InitToolBar();

            //初始化spgvMaterial
            this.spgvMaterial = new SPGridView();
            this.spgvMaterial.AutoGenerateColumns = false;
            this.spgvMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");

            BoundField bfColumn;

            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvMaterial.Columns.Add(bfColumn);
            }

            btnOK = GetControltByMaster("btnOK") as Button;
            btnOK.Click += new EventHandler(btnOK_Click);

        }        

        private void BindDataToCustomControls()
        {           

            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {

                //初始化表头信息
                StorageOutNotice son = db.StorageOutNotice.SingleOrDefault(u => u.StorageOutNoticeID == this._noticeid);

                (GetControltByMaster("lblConstructor") as Label).Text = son.BusinessUnitInfo1.BusinessUnitName;
                (GetControltByMaster("lblProprietor") as Label).Text = son.BusinessUnitInfo.BusinessUnitName;
                (GetControltByMaster("lblProject") as Label).Text = string.Format("{0}({1}阶段)", son.ProjectInfo.ProjectName, son.ProjectStage);
                (GetControltByMaster("lblNoticeCode") as Label).Text = son.StorageOutNoticeCode;
                (GetControltByMaster("lblProperty") as Label).Text = son.ProjectInfo.ProjectProperty;
                (GetControltByMaster("lblDate") as Label).Text = son.CreateTime.ToLongDateString();

                //初始化调拨明细
                this.spgvMaterial.DataSource = (from a in db.StorageOutDetails
                                                join b in db.StorageStocks on a.MaterialID equals b.MaterialID
                                                where a.StorageOutNoticeID == _noticeid
                                                select new
                                                {
                                                    a.MaterialInfo.FinanceCode,
                                                    a.MaterialInfo.MaterialName,
                                                    a.MaterialInfo.SpecificationModel,
                                                    a.Gentaojian,
                                                    a.Metre,
                                                    a.Ton,
                                                    StocksGenTaojian = (from c in db.StorageStocks
                                                                        where c.MaterialID == a.MaterialID
                                                                        select c).Sum(u => u.StocksGenTaojian),
                                                    StocksMetre = (from c in db.StorageStocks
                                                                   where c.MaterialID == a.MaterialID
                                                                   select c).Sum(u => u.StocksMetre),
                                                    StocksTon = (from c in db.StorageStocks
                                                                 where c.MaterialID == a.MaterialID
                                                                 select c).Sum(u => u.StocksTon),
                                                    a.Remark
                                                }).Distinct();
                this.spgvMaterial.DataBind();
            }


        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            if (bfinished)
            {
                btnOK.Visible = false;
                Panel p2 = (Panel)GetControltByMaster("Panel2");
                p2.Controls.Add(new LiteralControl("<font style=\"font-size:x-small;font-weight:bold;color:green\">该任务已完成，物资调拨单已经发送生产组长审核. . .</font>"));
            }
        }

        #endregion

        #region 控件事件

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            if(bfinished)
                Response.Redirect("../../default-old.aspx", false);
            else
                Response.Redirect(string.Format("NormalOutProduceAuditInfo.aspx?TaskID={0}", _taskid), false);
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {

        }

        void btnOK_Click(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("CreateStorageOutTask.aspx?TaskID={0}&NoticeID={1}&TaskType=物资调拨审核",_taskid,_noticeid));
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
