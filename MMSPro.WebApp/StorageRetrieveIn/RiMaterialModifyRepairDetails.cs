/*------------------------------------------------------------------------------
 * Unit Name：RiMaterialModifyRepairDetails.cs
 * Description: 回收入库--物资管理员修改维修保养物资的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-08-31
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
    public class RiMaterialModifyRepairDetails:Page
    {
        private int _taskid,_pretaskid, _formid;//当前任务ID和维修保养计划表ID
        private SPGridView spgvMaterial;
        private Button btnOK;        

        private static string[] ShowTlist = {                                                                                                                        
                                                  "物资名称:MaterialName",
                                                  "规格型号:SpecificationModel",                                                  
                                                  "回收数量:RetrieveInQuantity",
                                                  "SrinMaterialRepairDetailsID:SrinMaterialRepairDetailsID"
                                               };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                _taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskid);
                    if (tsi.TaskState.Equals("已完成"))//分支流程--任务已经完成的情况
                    {
                        Response.Redirect(string.Format("RiMaterialRepairDetailsMessage.aspx?TaskID={0}", _taskid), false);
                        return;
                    }
                    _formid = tsi.StorageInID;
                    _pretaskid = tsi.PreviousTaskID.Value;
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

        private void InitBar()
        {
            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");
            //新建
            ToolBarButton tbarbtnAdd = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnAdd.ID = "AddNewRow";
            tbarbtnAdd.Text = "新建";
            tbarbtnAdd.ImageUrl = "/_layouts/images/newitem.gif";
            tbarbtnAdd.Click += new EventHandler(tbarbtnAdd_Click);
            tbarTop.Buttons.Controls.Add(tbarbtnAdd);

            //删除
            ToolBarButton tbarbtnDelete = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnDelete.ID = "DeleteRow";
            tbarbtnDelete.Text = "删除";
            tbarbtnDelete.ImageUrl = "/_layouts/images/delete.gif";
            tbarbtnDelete.Click += new EventHandler(tbarbtnDelete_Click);
            StringBuilder sbScript = new StringBuilder();
            sbScript.Append("var aa= window.confirm('确认删除所选项?');");
            sbScript.Append("if(aa == false){");
            sbScript.Append("return false;}");
            tbarbtnDelete.OnClientClick = sbScript.ToString();
            tbarTop.Buttons.Controls.Add(tbarbtnDelete);     

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
            InitBar();

            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);                                   

            spgvMaterial = new SPGridView();
            spgvMaterial.AutoGenerateColumns = false;
            spgvMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");
            this.spgvMaterial.RowDataBound += new GridViewRowEventHandler(spgvMaterial_RowDataBound);
            //添加选择列
            TemplateField tfSelect = new TemplateField();
            tfSelect.ItemTemplate = new MulCheckBoxTemplate("Select", DataControlRowType.DataRow, false);
            tfSelect.HeaderTemplate = new MulCheckBoxTemplate("Select", DataControlRowType.Header);
            this.spgvMaterial.Columns.Insert(0, tfSelect);

            BoundField bfColumn;
            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvMaterial.Columns.Add(bfColumn);
            }

            //加入生产厂家列            
            TemplateField tfManufacture = new TemplateField();
            tfManufacture.HeaderText = "生产厂家";
            tfManufacture.ItemTemplate = new MulDropDownListTemplate(DataControlRowType.DataRow, "ddlManufacture", "ManufactureID",150);
            this.spgvMaterial.Columns.Insert(3, tfManufacture);

            //加入进库时间列
            TemplateField tfArrivalTime = new TemplateField();
            tfArrivalTime.HeaderText = "进库时间";
            tfArrivalTime.ItemTemplate = new DateTimeTemplate("ArrivalTime");
            this.spgvMaterial.Columns.Insert(4, tfArrivalTime);

            //加入维修保养数量列            
            TemplateField tfRepairQuantity = new TemplateField();
            tfRepairQuantity.HeaderText = "维修保养数量(根/台/套/件)";
            tfRepairQuantity.ItemTemplate = new TextBoxTemplate("维修保养数量(根/台/套/件)", "Gentaojian", "^(-?\\d+)(\\.\\d+)?$",100);
            this.spgvMaterial.Columns.Insert(6, tfRepairQuantity);

            //加入维修保养原因列
            TemplateField tfRepairReason = new TemplateField();
            tfRepairReason.HeaderText = "维修/保养原因";
            tfRepairReason.ItemTemplate = new TextBoxTemplate("维修/保养原因", DataControlRowType.DataRow, "RepairReason");
            this.spgvMaterial.Columns.Insert(7, tfRepairReason);

            //加入计划完成时间列
            TemplateField tfPlanTime = new TemplateField();
            tfPlanTime.HeaderText = "计划完成时间";
            tfPlanTime.ItemTemplate = new DateTimeTemplate("PlanTime");
            this.spgvMaterial.Columns.Insert(8, tfPlanTime);

            //加入实际维修数量保养列
            TemplateField tfRealGtj = new TemplateField();
            tfRealGtj.HeaderText = "实际维修/保养数量";
            tfRealGtj.ItemTemplate = new TextBoxTemplate("RealGtj", "RealGentaojian", "^(-?\\d+)(\\.\\d+)?$", 100);
            this.spgvMaterial.Columns.Insert(9, tfRealGtj);

            //加入实际完成时间列
            TemplateField tfRealTime = new TemplateField();
            tfRealTime.HeaderText = "实际完成时间";
            tfRealTime.ItemTemplate = new DateTimeTemplate("RealTime");
            this.spgvMaterial.Columns.Insert(10, tfRealTime);

            //加入备注列            
            TemplateField tfRepairRemark = new TemplateField();
            tfRepairRemark.HeaderText = "备注";
            tfRepairRemark.ItemTemplate = new TextBoxTemplate("备注", DataControlRowType.DataRow, "Remark");
            this.spgvMaterial.Columns.Insert(11, tfRepairRemark);
        }        

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头
                SrinRepairPlan srp = db.SrinRepairPlan.SingleOrDefault(u => u.SrinRepairPlanID == _formid);
                ((Label)GetControltByMaster("lblMaterial")).Text = srp.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(srp.CreateTime.ToLongDateString(), srp.CreateTime.ToLongTimeString());
                ((Label)GetControltByMaster("lblCode")).Text = srp.SrinRepairPlanCode;                

                spgvMaterial.DataSource = from a in db.SrinMaterialRepairDetails
                                          where a.SrinRepairPlanID == _formid
                                          select new
                                          {
                                              a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                              a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                              a.ManufactureID,
                                              a.ArrivalTime,
                                              RetrieveInQuantity = a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.TotleGentaojian,
                                              a.Gentaojian,
                                              a.RepairReason,
                                              a.PlanTime,
                                              a.RealGentaojian,
                                              a.RealTime,
                                              a.Remark,
                                              a.SrinMaterialRepairDetailsID
                                          };
                spgvMaterial.DataBind();

                //初始化审核信息
                SrinMaterialRepairAudit smra = db.SrinMaterialRepairAudit.SingleOrDefault(u => u.TaskID == _pretaskid);
                ((Label)GetControltByMaster("lblMChief")).Text = smra.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblAuditDate")).Text = string.Concat(smra.AuditTime.ToLongDateString(), smra.AuditTime.ToLongTimeString());
                ((Label)GetControltByMaster("lblResult")).Text = smra.AuditResult;
                ((TextBox)GetControltByMaster("txtOpinion")).Text = smra.AuditOpinion;

            }

        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            this.spgvMaterial.Columns[12].Visible = false;
        }

        #endregion

        #region 控件事件方法

        void spgvMaterial_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    DropDownList ddlManufacture = e.Row.Cells[3].Controls[0] as DropDownList;
                    ddlManufacture.Items.Clear();
                    
                    ddlManufacture.DataSource = (from s in db.StorageStocks
                                                 where (from a in db.SrinMaterialRepairDetails
                                                        where a.SrinMaterialRepairDetailsID == Convert.ToInt32(e.Row.Cells[12].Text)
                                                        select a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialID).Contains(s.MaterialID)
                                                 select new
                                                 {
                                                     s.ManufacturerName,
                                                     s.ManufacturerID
                                                 }).Distinct();                   

                    ddlManufacture.DataTextField = "ManufacturerName";
                    ddlManufacture.DataValueField = "ManufacturerID";
                    ddlManufacture.DataBind();
                    ddlManufacture.Items.Insert(0, new ListItem("--请选择--", "0"));

                }

            }
        }

        void tbarbtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    int ichecked = 0;
                    SrinMaterialRepairDetails smrd;
                    CheckBox chb;
                    int idetailsid;
                    foreach (GridViewRow gvr in spgvMaterial.Rows)
                    {
                        chb = (CheckBox)gvr.Cells[0].Controls[0];
                        if (!chb.Checked)
                            continue;
                        ichecked++;

                        idetailsid = Convert.ToInt32(gvr.Cells[12].Text);
                        smrd = db.SrinMaterialRepairDetails.SingleOrDefault(a => a.SrinMaterialRepairDetailsID == idetailsid);
                        db.SrinMaterialRepairDetails.DeleteOnSubmit(smrd);

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

        void tbarbtnAdd_Click(object sender, EventArgs e)
        {
            string strBackUrl = string.Format("RiMaterialModifyRepairDetails.aspx?TaskID={0}",_taskid);
            Response.Redirect(string.Format("SelectRepairOrVerifyDetails.aspx?FormID={0}&Type=维修保养&BackUrl={1}", _formid,HttpUtility.UrlEncode(strBackUrl)), false);
        }

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
                    SrinMaterialRepairDetails smrd;
                    DateTimeControl dtcPlanTime,dtcRealTime;
                    foreach (GridViewRow gvr in this.spgvMaterial.Rows)
                    {
                        smrd = db.SrinMaterialRepairDetails.SingleOrDefault(u => u.SrinMaterialRepairDetailsID == Convert.ToInt32(gvr.Cells[12].Text));
                        smrd.ManufactureID = Convert.ToInt32((gvr.Cells[3].Controls[0] as DropDownList).SelectedValue);
                        smrd.ArrivalTime = ((DateTimeControl)gvr.Cells[4].Controls[0]).SelectedDate;
                        smrd.Gentaojian = Convert.ToDecimal(((TextBox)gvr.Cells[6].Controls[0]).Text);
                        smrd.RepairReason = ((TextBox)gvr.Cells[7].Controls[0]).Text;
                        dtcPlanTime = gvr.Cells[8].Controls[0] as DateTimeControl;
                        smrd.PlanTime = dtcPlanTime.IsDateEmpty ? new Nullable<DateTime>(): dtcPlanTime.SelectedDate;
                        smrd.RealGentaojian = Convert.ToDecimal((gvr.Cells[9].Controls[0] as TextBox).Text.Trim());
                        dtcRealTime = gvr.Cells[10].Controls[0] as DateTimeControl;
                        smrd.RealTime = dtcRealTime.IsDateEmpty ? new Nullable<DateTime>() : dtcRealTime.SelectedDate;
                        smrd.Remark = ((TextBox)gvr.Cells[11].Controls[0]).Text;
                        smrd.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();                                
                    }
                    db.SubmitChanges();   
       
                }
                
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_UPDATEERROR));
            }
            
            Response.Redirect(string.Format("RiMaterialRepairDetailsMessage.aspx?TaskID={0}",_taskid),false);
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
