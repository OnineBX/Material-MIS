/*------------------------------------------------------------------------------
 * Unit Name：ViewRepairAndVerifyInfo.cs
 * Description: 回收入库--物资管理员管理维修保养表和回收检验表的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-10-21
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
using System.Data.Linq.SqlClient;

namespace MMSPro.WebApp
{
    public class ViewRepairAndVerifyInfo:Page
    {
        private int _taskid, _receiptid;
        private SPGridView spgvRepair, spgvVerify;        

        private static string[] ShowRepairTlist =  {                                                                                                                                              
                                                       "创建时间:CreateTime",
                                                       "包含物资:MaterialCount",
                                                       "备注:Remark", 
                                                       "审核:MaterialChief",
                                                       "SrinRepairPlanID:SrinRepairPlanID"
                                                  };
        private static string[] ShowVerifyTList = {
                                                      "创建时间:CreateTime",
                                                      "包含物资:MaterialCount",
                                                      "完成检验准备工作:Ready",
                                                      "备注:Remark",
                                                      "SrinVerifyTransferID:SrinVerifyTransferID"
                                                  };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    SrinReceipt srp = db.SrinReceipt.SingleOrDefault(u => u.SrinStocktakingConfirm.SrinStocktaking.TaskID.Equals(_taskid));
                    _receiptid = srp.SrinReceiptID;
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

        private void InitializeCustomControls()
        {
            InitToolBar();           

            //初始化维修保养表
            this.spgvRepair = new SPGridView();
            this.spgvRepair.AutoGenerateColumns = false;
            this.spgvRepair.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");

            BoundField bfColumn;
            foreach (var kvp in ShowRepairTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvRepair.Columns.Add(bfColumn);
            }

            //添加选择列
            TemplateField tfChooseRepair = new TemplateField();
            tfChooseRepair.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "SrinRepairPlanID");
            tfChooseRepair.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
            this.spgvRepair.Columns.Insert(0, tfChooseRepair);

            SPMenuField colRepairMenu = new SPMenuField();
            colRepairMenu.HeaderText = "维修保养计划表编号";
            colRepairMenu.TextFields = "SrinRepairPlanCode";
            colRepairMenu.MenuTemplateId = "mtRepair";

            colRepairMenu.NavigateUrlFields = "SrinRepairPlanID"; //定义方式:"列名1,列名2..."
            colRepairMenu.NavigateUrlFormat = "ManageRepairOrVerifyDetails.aspx?FormID={0}&Type=维修保养";
            colRepairMenu.TokenNameAndValueFields = "curID=SrinRepairPlanID";//定义方式:"别名1=列名1,别名2=列名2...."

            MenuTemplate mtRepair = new MenuTemplate();
            mtRepair.ID = "mtRepair";

            MenuItemTemplate mitMaterial = new MenuItemTemplate("物资组长审核", "/_layouts/images/newitem.gif");
            mitMaterial.ClientOnClickNavigateUrl = string.Format("SrinDispatchCenter.aspx?TaskType=维修保养物资组长审核&TaskID={0}&FormID=%curID%", _taskid);
            mtRepair.Controls.Add(mitMaterial);

            this.Controls.Add(mtRepair);
            this.spgvRepair.Columns.Insert(1, colRepairMenu);            


            //初始化回收检验表
            this.spgvVerify = new SPGridView();
            this.spgvVerify.AutoGenerateColumns = false;
            this.spgvVerify.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");

            foreach (var kvp in ShowVerifyTList)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvVerify.Columns.Add(bfColumn);
            }

            //添加选择列
            TemplateField tfChooseVerify = new TemplateField();
            tfChooseVerify.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "SrinVerifyTransferID");
            tfChooseVerify.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
            this.spgvVerify.Columns.Insert(0, tfChooseVerify);

            SPMenuField colVerifyMenu = new SPMenuField();
            colVerifyMenu.HeaderText = "回收检验传递表编号";
            colVerifyMenu.TextFields = "SrinVerifyTransferCode";
            colVerifyMenu.MenuTemplateId = "mtVerify";

            colVerifyMenu.NavigateUrlFields = "SrinVerifyTransferID"; //定义方式:"列名1,列名2..."
            colVerifyMenu.NavigateUrlFormat = "ManageRepairOrVerifyDetails.aspx?FormID={0}&Type=回收检验";
            colVerifyMenu.TokenNameAndValueFields = "curID=SrinVerifyTransferID";//定义方式:"别名1=列名1,别名2=列名2...."

            MenuTemplate mtVerify = new MenuTemplate();
            mtVerify.ID = "mtVerify";

            MenuItemTemplate mitVerify = new MenuItemTemplate("提交生产组", "/_layouts/images/newitem.gif");
            mitVerify.ClientOnClickNavigateUrl = string.Format("SrinDispatchCenter.aspx?TaskType=生产组安排质检&TaskID={0}&FormID=%curID%", _taskid);
            mtVerify.Controls.Add(mitVerify);

            this.Controls.Add(mtVerify);
            this.spgvVerify.Columns.Insert(1, colVerifyMenu);

            //添加任务详情列
            HyperLinkField hlfVerifyTask = new HyperLinkField();
            hlfVerifyTask.HeaderText = "任务详情";
            this.spgvVerify.Columns.Insert(7, hlfVerifyTask);

            this.spgvVerify.RowDataBound += new GridViewRowEventHandler(spgvVerify_RowDataBound);
        }


        private void BindDataToCustomControls()
        {            
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化清点信息
                SrinStocktaking sst = db.SrinStocktaking.SingleOrDefault(u => u.TaskID == _taskid);
                (GetControltByMaster("lblMaterial") as Label).Text = sst.EmpInfo.EmpName;
                (GetControltByMaster("lblResult") as Label).Text = sst.StocktakingResult;
                (GetControltByMaster("lblInventoryDate") as Label).Text = string.Concat(sst.StocktakingDate.ToLongDateString(), sst.StocktakingDate.ToLongTimeString());
                (GetControltByMaster("lblOpinion") as Label).Text = sst.StocktakingProblem;  

                //初始化回收入库单信息
                SrinReceipt srp = db.SrinReceipt.SingleOrDefault(u => u.SrinReceiptID == _receiptid);
                ((Label)GetControltByMaster("lblProject")).Text = srp.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.ProjectInfo.ProjectName;
                ((Label)GetControltByMaster("lblAsset")).Text = srp.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblCode")).Text = srp.SrinReceiptCode;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(srp.CreateTime.ToLongDateString(), srp.CreateTime.ToLongTimeString());
                ((Label)GetControltByMaster("lblRemark")).Text = srp.Remark;

                this.spgvRepair.DataSource = from a in db.SrinRepairPlan
                                             join b in db.TaskStorageIn on new { TaskID = a.TaskID, RepairPlanID = a.SrinRepairPlanID, TaskType = "维修保养物资组长审核" } equals new { TaskID = b.PreviousTaskID.Value, RepairPlanID = b.StorageInID, b.TaskType }
                                             into c
                                             from d in c.DefaultIfEmpty()
                                             where a.SrinReceiptID == _receiptid
                                             select new
                                             {
                                                 a.Remark,
                                                 MaterialCount = a.SrinMaterialRepairDetails.Count,
                                                 a.CreateTime,
                                                 a.SrinRepairPlanCode,
                                                 a.SrinRepairPlanID,
                                                 MaterialChief = d == null ? "N/A" : d.EmpInfo1.EmpName,
                                             };
                this.spgvRepair.DataBind();

                this.spgvVerify.DataSource = from v in db.SrinVerifyTransfer
                                             where v.SrinReceiptID == _receiptid
                                             select new
                                             {
                                                 v.Remark,
                                                 MaterialCount = v.SrinMaterialVerifyDetails.Count,
                                                 v.CreateTime,
                                                 v.SrinVerifyTransferCode,
                                                 Ready = v.ReadyWorkIsFinished ? "已完成" : "未完成",
                                                 v.SrinVerifyTransferID
                                             };
                this.spgvVerify.DataBind();
            }
        }

        private void ShowCustomControls()
        {
            Panel p2 = (Panel)GetControltByMaster("Panel2");
            p2.Controls.Add(this.spgvRepair);
            this.spgvRepair.Columns[6].Visible = false;

            Panel p3 = (Panel)GetControltByMaster("Panel3");
            p3.Controls.Add(this.spgvVerify);
            this.spgvVerify.Columns[6].Visible = false;

        }

        private void InitToolBar()
        {
            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");

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


        #endregion

        #region 控件事件方法

        void btnOK_Click(object sender, EventArgs e)
        {
            try
            {

                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskid);
                    tsi.TaskState = "已完成";
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

        void btnRefresh_Click(object sender, EventArgs e)
        {

        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("../../default-old.aspx", false);
        }

        void spgvVerify_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
                e.Row.Cells[7].Text = string.Format("<a href=\"javaScript:onClick=window.showModalDialog(encodeURI('SrinTaskHistoryInfo.aspx?WorkID={0}&&TaskType=生产组安排质检&TaskID={1}'),'0','resizable:yes;dialogWidth:968px;dialogHeight:545px')\">任务详情</a>", int.Parse(e.Row.Cells[6].Text), _taskid);
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
