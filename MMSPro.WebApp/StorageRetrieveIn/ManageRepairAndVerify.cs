/*------------------------------------------------------------------------------
 * Unit Name：ManageRepairAndVerify.cs
 * Description: 回收入库--物资管理员管理维修保养表和回收检验表的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-08-13
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
    public class ManageRepairAndVerify:Page
    {
        private int _taskid,_receiptid;
        private SPGridView spgvRepair, spgvVerify;
        private Button btnOK;               

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

            btnOK = GetControltByMaster("btnOK") as Button;
            btnOK.Click += new EventHandler(btnOK_Click);
            btnOK.OnClientClick = "return confirm('请确认所有维修和质检任务都已发出！')";

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
            //绑定维修保养表
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化回收入库单信息
                SrinReceipt srp = db.SrinReceipt.SingleOrDefault(u => u.SrinReceiptID == _receiptid);
                ((Label)GetControltByMaster("lblProject")).Text = srp.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.ProjectInfo.ProjectName;
                ((Label)GetControltByMaster("lblMaterial")).Text = srp.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblCode")).Text = srp.SrinReceiptCode;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(srp.CreateTime.ToLongDateString(), srp.CreateTime.ToLongTimeString());
                ((Label)GetControltByMaster("lblRemark")).Text = srp.Remark;

                this.spgvRepair.DataSource = from a in db.SrinRepairPlan
                                             join b in db.TaskStorageIn on new { TaskID = a.TaskID, RepairPlanID = a.SrinRepairPlanID, TaskType = "维修保养物资组长审核" } equals new { TaskID=b.PreviousTaskID.Value, RepairPlanID = b.StorageInID, b.TaskType }                                                
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
                                                 Ready = v.ReadyWorkIsFinished?"已完成":"未完成",
                                                 v.SrinVerifyTransferID
                                             };
                this.spgvVerify.DataBind();
            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvRepair);
            this.spgvRepair.Columns[6].Visible = false;

            Panel p2 = (Panel)GetControltByMaster("Panel2");
            p2.Controls.Add(this.spgvVerify);
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
            //if (bfromtitle)
            //{
            //    ToolBarButton tbarbtnView = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            //    tbarbtnView.ID = "viewRow";
            //    tbarbtnView.Text = "查看";
            //    tbarbtnView.ImageUrl = "/_layouts/images/edit.GIF";
            //    tbarbtnView.Click += new EventHandler(tbarbtnView_Click);
            //    tbarTop.Buttons.Controls.Add(tbarbtnView);
            //}

            ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnRefresh.ID = "btnRefresh";
            btnRefresh.Text = "刷新";
            btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
            btnRefresh.Padding = "0,5,0,0";
            btnRefresh.Click += new EventHandler(btnRefresh_Click);
            tbarTop.RightButtons.Controls.Add(btnRefresh);

            //初始化tbarRepair
            ToolBar tbarRepair = (ToolBar)GetControltByMaster("tbarRepair");

            StringBuilder sbScript = new StringBuilder();
            sbScript.Append("var aa= window.confirm('确认删除所选项?');");
            sbScript.Append("if(aa == false){");
            sbScript.Append("return false;}");
            
            ToolBarButton tbarbtnAddR = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnAddR.ID = "AddRepair";
            tbarbtnAddR.Text = "新建";
            tbarbtnAddR.ImageUrl = "/_layouts/images/newitem.gif";
            tbarbtnAddR.Click += new EventHandler(tbarbtnAddR_Click);
            tbarRepair.Buttons.Controls.Add(tbarbtnAddR);
            
            ToolBarButton tbarbtnEditR = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnEditR.ID = "EditRepair";
            tbarbtnEditR.Text = "修改";
            tbarbtnEditR.ImageUrl = "/_layouts/images/edit.gif";
            tbarbtnEditR.Click += new EventHandler(tbarbtnEditR_Click);
            tbarRepair.Buttons.Controls.Add(tbarbtnEditR);            

            ToolBarButton tbarbtnDelteR = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnDelteR.ID = "DeleteRepair";
            tbarbtnDelteR.Text = "删除";
            tbarbtnDelteR.ImageUrl = "/_layouts/images/delete.gif";
            tbarbtnDelteR.Click += new EventHandler(tbarbtnDelteR_Click);            
            tbarbtnDelteR.OnClientClick = sbScript.ToString();
            tbarRepair.Buttons.Controls.Add(tbarbtnDelteR);


            ToolBarButton btnRefreshR = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnRefreshR.ID = "RefreshRepair";
            btnRefreshR.Text = "刷新";
            btnRefreshR.ImageUrl = "/_layouts/images/refresh.GIF";
            btnRefreshR.Padding = "0,5,0,0";
            btnRefreshR.Click += new EventHandler(btnRefreshR_Click);
            tbarRepair.RightButtons.Controls.Add(btnRefreshR);


            //初始化tbarVerify
            ToolBar tbarVerify = (ToolBar)GetControltByMaster("tbarVerify");

            ToolBarButton tbarbtnAddV = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnAddV.ID = "AddVerify";
            tbarbtnAddV.Text = "新建";
            tbarbtnAddV.ImageUrl = "/_layouts/images/newitem.gif";
            tbarbtnAddV.Click += new EventHandler(tbarbtnAddV_Click);
            tbarVerify.Buttons.Controls.Add(tbarbtnAddV);

            ToolBarButton tbarbtnEditV = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnEditV.ID = "EditVerify";
            tbarbtnEditV.Text = "修改";
            tbarbtnEditV.ImageUrl = "/_layouts/images/edit.gif";
            tbarbtnEditV.Click += new EventHandler(tbarbtnEditV_Click);
            tbarVerify.Buttons.Controls.Add(tbarbtnEditV);

            ToolBarButton tbarbtnDelteV = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnDelteV.ID = "DeleteVerify";
            tbarbtnDelteV.Text = "删除";
            tbarbtnDelteV.ImageUrl = "/_layouts/images/delete.gif";
            tbarbtnDelteV.Click += new EventHandler(tbarbtnDelteV_Click);
            tbarbtnDelteV.OnClientClick = sbScript.ToString();
            tbarVerify.Buttons.Controls.Add(tbarbtnDelteV);


            ToolBarButton btnRefreshV = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnRefreshV.ID = "RefreshVerify";
            btnRefreshV.Text = "刷新";
            btnRefreshV.ImageUrl = "/_layouts/images/refresh.GIF";
            btnRefreshV.Padding = "0,5,0,0";
            btnRefreshV.Click += new EventHandler(btnRefreshV_Click);
            tbarVerify.RightButtons.Controls.Add(btnRefreshV);
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

        //void tbarbtnView_Click(object sender, EventArgs e)
        //{
        //    Response.Redirect(string.Format("RiAssetReceiptMessage.aspx?TaskID={0}",_taskid), false);
        //} 

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("RiMaterialStocktakingMessage.aspx?TaskID={0}", _taskid), false);
        }

        void tbarbtnAddR_Click(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("CreateRepairOrVerify.aspx?TaskID={0}&Type=维修保养表",_taskid), false);
        }  

        void tbarbtnEditR_Click(object sender, EventArgs e)
        {

            try
            {
                CheckBox chb;
                int formid;
                foreach (GridViewRow gvr in this.spgvRepair.Rows)
                {
                    chb = (CheckBox)gvr.Cells[0].Controls[0];
                    formid = Convert.ToInt32(gvr.Cells[6].Text);
                    if (chb.Checked)
                    {
                        using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                        {
                            //分支流程--已经进入流程的情况
                            if (db.TaskStorageIn.Count(u => u.PreviousTaskID.Equals(_taskid) && u.TaskType.Equals("维修保养物资组长审核") && u.StorageInID.Equals(formid)) != 0)
                                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", "<script>alert('维修保养计划表已经提交物资组长审核，不能修改!')</script>");                            
                            else//主流程--没有进入流程的情况
                                Response.Redirect(string.Format("CreateRepairOrVerify.aspx?TaskID={0}&Type=维修保养表&WorkID={1}", _taskid, formid), false);
                        }                        
                        return;
                    }
                }
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择需要修改的记录!')</script>");    
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));
            }            
        }

        void btnRefreshV_Click(object sender, EventArgs e)
        {
            
        }

        void tbarbtnDelteV_Click(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    int ichecked = 0;
                    SrinVerifyTransfer svt;
                    CheckBox chb;
                    int svtid;
                    foreach (GridViewRow gvr in this.spgvVerify.Rows)
                    {
                        chb = (CheckBox)gvr.Cells[0].Controls[0];
                        if (!chb.Checked)
                            continue;
                        ichecked++;
                        svtid = int.Parse(gvr.Cells[6].Text);

                        //分支流程--已经进入流程的情况
                        if (db.TaskStorageIn.Count(u => u.StorageInType.Equals("回收入库") && u.TaskType.Equals("生产组安排质检") && u.StorageInID.Equals(svtid)) != 0)
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('编号为{0}的回收检验传递表已进入回收入库流程，不能删除！')</script>", (gvr.Cells[1].Controls[0] as Microsoft.SharePoint.WebControls.Menu).Text));
                            continue;
                        }

                        svt = db.SrinVerifyTransfer.SingleOrDefault(a => a.SrinVerifyTransferID == svtid);
                        db.SrinVerifyTransfer.DeleteOnSubmit(svt);

                    }
                    if (ichecked != 0)
                        db.SubmitChanges();
                    else
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择需要删除的记录!')</script>");
                }
                Response.AddHeader("Refresh", "0");
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));
            }
        }

        void tbarbtnEditV_Click(object sender, EventArgs e)
        {
            try
            {
                CheckBox chb;
                int formid;                                                               

                foreach (GridViewRow gvr in this.spgvVerify.Rows)
                {
                    chb = (CheckBox)gvr.Cells[0].Controls[0];
                    formid = Convert.ToInt32(gvr.Cells[6].Text);
                    if (chb.Checked)
                    {
                        using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                        {
                            //分支流程--已经进入流程的情况
                            if (db.TaskStorageIn.Count(u => u.PreviousTaskID.Equals(_taskid) && u.TaskType.Equals("生产组安排质检") && u.StorageInID.Equals(formid)) != 0)
                                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", "<script>alert('回收检验传递表已经提交生产组，不能修改!')</script>");
                            else//主流程--没有进入流程的情况
                                Response.Redirect(string.Format("CreateRepairOrVerify.aspx?TaskID={0}&Type=回收检验表&WorkID={1}", _taskid, formid), false);
                        }
                        return;
                    }
                }
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择需要修改的记录!')</script>");
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));
            }            
                       
        }

        void tbarbtnAddV_Click(object sender, EventArgs e)
        {            
            Response.Redirect(string.Format("CreateRepairOrVerify.aspx?TaskID={0}&Type=回收检验表", _taskid), false);
        }

        void btnRefreshR_Click(object sender, EventArgs e)
        {
            
        }

        void tbarbtnDelteR_Click(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    int ichecked = 0;
                    SrinRepairPlan srp;
                    CheckBox chb;
                    int srpid;
                    foreach (GridViewRow gvr in this.spgvRepair.Rows)
                    {
                        chb = (CheckBox)gvr.Cells[0].Controls[0];
                        if (!chb.Checked)
                            continue;
                        ichecked++;
                        srpid = int.Parse(gvr.Cells[6].Text);

                        //分支流程--已经进入流程的情况
                        if (db.TaskStorageIn.Count(u => u.StorageInType.Equals("回收入库") && u.TaskType.Equals("维修保养物资组长审核") && u.StorageInID.Equals(srpid)) != 0)
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('编号为{0}维修保养计划表已进入回收入库流程，不能删除！')</script>", (gvr.Cells[1].Controls[0] as Microsoft.SharePoint.WebControls.Menu).Text));
                            continue;
                        }

                        srp = db.SrinRepairPlan.SingleOrDefault(a => a.SrinRepairPlanID == srpid);
                        db.SrinRepairPlan.DeleteOnSubmit(srp);

                    }
                    if (ichecked != 0)
                        db.SubmitChanges();
                    else
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择需要删除的记录!')</script>");
                }
                Response.AddHeader("Refresh", "0");
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));
            }
        }       

        void spgvVerify_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
                e.Row.Cells[7].Text = string.Format("<a href=\"javaScript:onClick=window.showModalDialog(encodeURI('SrinTaskHistoryInfo.aspx?WorkID={0}&&TaskType=生产组安排质检&TaskID={1}'),'0','resizable:yes;dialogWidth:968px;dialogHeight:545px')\">任务详情</a>", int.Parse(e.Row.Cells[6].Text),_taskid);
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
