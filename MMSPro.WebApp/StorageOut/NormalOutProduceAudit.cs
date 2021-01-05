/*------------------------------------------------------------------------------
 * Unit Name：NormalOutProduceAudit.cs
 * Description: 正常出库--生产组长审核的页面
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
    public class NormalOutProduceAudit: System.Web.UI.Page
    {

        private int _noticeid;
        private int _taskid;

        private SPGridView spgvMaterial;
        private CheckBox chbAgree;
        private TextBox txtOpinion;
        private Button btnOK;

        private StorageOutProduceAudit sopa;//生产组长审核信息

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

                    //分支流程--任务已经完成的情况
                    if (sot.TaskState.Equals("已完成"))
                    {
                        Response.Redirect(string.Format("NormalOutProduceAuditMessage.aspx?TaskID={0}", _taskid), false);
                        return;
                    }
                    sopa = db.StorageOutProduceAudit.SingleOrDefault(u => u.TaskID.Equals(_taskid));
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
            
            BoundField bfColumn;

            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvMaterial.Columns.Add(bfColumn);
            }

            chbAgree = (CheckBox)GetControltByMaster("chbAgree");
            chbAgree.CheckedChanged += new EventHandler(chbAgree_CheckedChanged);

            txtOpinion = (TextBox)GetControltByMaster("txtOpinion");
            btnOK = (Button)GetControltByMaster("btnOK");
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
                (GetControltByMaster("lblProject") as Label).Text = string.Format("{0}({1}阶段)", son.ProjectInfo.ProjectName,son.ProjectStage);
                (GetControltByMaster("lblNoticeCode") as Label).Text = son.StorageOutNoticeCode;
                (GetControltByMaster("lblProperty") as Label).Text = son.ProjectInfo.ProjectProperty;
                (GetControltByMaster("lblDate") as Label).Text = son.CreateTime.ToLongDateString();

                //初始化审核列表
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

                //分支流程--已经审核的情况
                if (this.sopa != null)
                {
                    if (!Page.IsPostBack)
                    {                        
                        if (sopa.AuditStatus.Equals("未通过"))
                        {
                            txtOpinion.Text = sopa.AuditOpinion;
                            txtOpinion.Enabled = true;
                            chbAgree.AutoPostBack = false;
                            chbAgree.Checked = false;
                            chbAgree.AutoPostBack = true;
                        }
                    }
                }
            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            //分支流程--已经审核的情况
            if (this.sopa != null)
                btnOK.Text = "修改审核表单";
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

                //将审核结果保存到数据库
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    if (this.sopa == null)//主流程--未审核的情况
                    {
                        sopa = new StorageOutProduceAudit();
                        sopa.StorageOutNoticeID = _noticeid;
                        sopa.AuditStatus = chbAgree.Checked == true ? "通过" : "未通过";
                        sopa.AuditTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                        sopa.AuditOpinion = txtOpinion.Text.Trim();
                        sopa.ProduceChief = (db.EmpInfo.SingleOrDefault(u => u.EmpName == SPContext.Current.Web.CurrentUser.Name)).EmpID;
                        sopa.TaskID = _taskid;
                        db.StorageOutProduceAudit.InsertOnSubmit(sopa);
                        db.SubmitChanges();

                    }
                    else//分支流程--已经审核的情况
                    {
                        sopa = db.StorageOutProduceAudit.SingleOrDefault(u => u.TaskID.Equals(_taskid));
                        sopa.AuditOpinion = txtOpinion.Text.Trim();
                        sopa.AuditStatus = chbAgree.Checked == true ? "通过" : "未通过";
                        sopa.AuditTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                    }

                    db.SubmitChanges();
                }

                //转到审核表单页
                Response.Redirect(string.Format("NormalOutProduceAuditMessage.aspx?TaskID={0}", _taskid),false);
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
            }            
        }

        void chbAgree_CheckedChanged(object sender, EventArgs e)
        {
            if (chbAgree.Checked)
            {
                txtOpinion.Enabled = false;
                txtOpinion.Text = "同意";
            }
            else
            {
                txtOpinion.Enabled = true;

                txtOpinion.Text = "请在此处填写审核意见...";
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
