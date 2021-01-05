/*------------------------------------------------------------------------------
 * Unit Name：NormalOutProduceAuditMessage.cs
 * Description: 正常出库--生产组长审核后的信息显示页面
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
    public class NormalOutProduceAuditMessage : System.Web.UI.Page
    {
        private int _taskid, _noticeid;

        private SPGridView spgvMaterial;
        private Button btnOK;
        private Label lblResult,lblAuditTitle;

        private bool bfinished = false;
        private string executor;//不通过时，将发送反馈任务给当前任务发送者
        
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
                _taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    StorageOutTask sot = db.StorageOutTask.SingleOrDefault(u => u.TaskID == this._taskid);
                    _noticeid = sot.NoticeID;
                    if (sot.TaskState.Equals("已完成"))
                        bfinished = true;
                    executor = sot.EmpInfo.Account;
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
                       
            lblResult = (Label)GetControltByMaster("lblResult");
            lblAuditTitle = (Label)GetControltByMaster("lblAuditTitle");


        }
        
        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息                                
                StorageOutProduceAudit sopa = db.StorageOutProduceAudit.SingleOrDefault(u => u.TaskID.Equals(_taskid));

                (GetControltByMaster("lblConstructor") as Label).Text = sopa.StorageOutNotice.BusinessUnitInfo1.BusinessUnitName;
                (GetControltByMaster("lblProprietor") as Label).Text = sopa.StorageOutNotice.BusinessUnitInfo.BusinessUnitName;
                (GetControltByMaster("lblProject") as Label).Text = string.Format("{0}({1}阶段)", sopa.StorageOutNotice.ProjectInfo.ProjectName, sopa.StorageOutNotice.ProjectStage);
                (GetControltByMaster("lblNoticeCode") as Label).Text = sopa.StorageOutNotice.StorageOutNoticeCode;
                (GetControltByMaster("lblProperty") as Label).Text = sopa.StorageOutNotice.ProjectInfo.ProjectProperty;
                (GetControltByMaster("lblDate") as Label).Text = sopa.StorageOutNotice.CreateTime.ToLongDateString();                

                //初始化调拨明细列表
                this.spgvMaterial.DataSource = from a in db.StorageOutDetails                                                
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
                                                };
                this.spgvMaterial.DataBind();

                //初始化审核信息                
                (GetControltByMaster("lblOpinion") as Label).Text = sopa.AuditOpinion;
                lblResult.Text = sopa.AuditStatus;
                (GetControltByMaster("lblProduceChief") as Label).Text = sopa.EmpInfo.EmpName;

            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            //分支流程--任务已完成的情况

            if (bfinished)
            {
                lblAuditTitle.Text = "<font style=\"color:blue;\">生产技术部审核信息</font>";
                Panel p3 = (Panel)GetControltByMaster("Panel3");
                btnOK.Visible = false;
                p3.Controls.AddAt(0, new LiteralControl("<BR/><font style=\"color:green;font-weight:bold;font-size:x-small\" >信息：该任务已完成,您正在查看审核清单...</font><BR/><BR/>"));
            }
            else
            {
                if (lblResult.Text.Equals("通过"))
                    btnOK.Text = "提交资产组";
            }
        }

        #endregion

        #region 控件事件方法
        
        void btnRefresh_Click(object sender, EventArgs e)
        {
            
        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            if(bfinished)
                Response.Redirect("../../default-old.aspx", false);
            else
                Response.Redirect(string.Format("NormalOutProduceAudit.aspx?TaskID={0}", _taskid), false);
        }       
        
        void btnOK_Click(object sender, EventArgs e)
        {
            try
            {                
                              
                if (lblResult.Text.Equals("未通过"))                
                    Response.Redirect(string.Format("CreateStorageOutTask.aspx?TaskID={0}&NoticeID={1}&TaskType=物资调拨审核信息&Executor={2}", _taskid, _noticeid, executor), false); 
                else
                    Response.Redirect(string.Format("CreateStorageOutTask.aspx?TaskID={0}&NoticeID={1}&TaskType=物资出库", _taskid, _noticeid), false); 
                                            
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
        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }
        
        #endregion
    }
}
