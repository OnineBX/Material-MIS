/*------------------------------------------------------------------------------
 * Unit Name：ManageRepairOrVerifyDetails.cs
 * Description: 回收入库--管理维修保养计划表或回收检验传递表的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-08-16
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
    public class ManageRepairOrVerifyDetails:Page
    {
        private int _formid;
        private string _type;

        private SPGridView spgvMaterial;
        private static string[] ShowRepairTlist = {                                                                                                                        
                                                      "物资名称:MaterialName",
                                                      "规格型号:SpecificationModel",                                                     
                                                      "回收数量:RetrieveInQuantity",
                                                      "SrinMaterialRepairDetailsID:SrinMaterialRepairDetailsID"
                                                   };

        private static string[] ShowVerifyTlist = {                                                                                                                        
                                                      "物资名称:MaterialName",
                                                      "规格型号:SpecificationModel",                                                      
                                                      "回收数量:TotleGentaojian",
                                                      "仓库:StorageName",
                                                      "垛位:PileName",                                                      
                                                      "SrinMaterialVerifyDetailsID:SrinMaterialVerifyDetailsID"
                                                   };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                _formid = Convert.ToInt32(Request.QueryString["FormID"]);
                _type = Request.QueryString["Type"];
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    bool bSubmit = false;
                    //已经发送审核的情况
                    switch (_type)
                    {
                        case "维修保养":
                            if (db.TaskStorageIn.Count(u => u.TaskType.Equals("维修保养物资组长审核") && u.StorageInID.Equals(_formid)) != 0)
                                bSubmit = true;
                            break;
                        case "回收检验":
                            if (db.TaskStorageIn.Count(u => u.TaskType.Equals("生产组安排质检") && u.StorageInID.Equals(_formid)) != 0)
                                bSubmit = true;
                            break;
                    }
                    if (bSubmit)
                    {
                        Response.Redirect(string.Format("ViewRepairOrVerifyDetails.aspx?FormID={0}&Type={1}", _formid, _type), false);
                        return;
                    }
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

            //初始化物资列表
            this.spgvMaterial = new SPGridView();
            this.spgvMaterial.AutoGenerateColumns = false;
            this.spgvMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");
            this.spgvMaterial.RowDataBound += new GridViewRowEventHandler(spgvMaterial_RowDataBound);

            //添加选择列
            TemplateField tfSelect = new TemplateField();
            tfSelect.ItemTemplate = new MulCheckBoxTemplate("Select", DataControlRowType.DataRow,false);
            tfSelect.HeaderTemplate = new MulCheckBoxTemplate("Select", DataControlRowType.Header);
            this.spgvMaterial.Columns.Insert(0, tfSelect);

            BoundField bfColumn;

            switch (_type)
            {
                case "维修保养":
                    foreach (var kvp in ShowRepairTlist)
                    {
                        bfColumn = new BoundField();
                        bfColumn.HeaderText = kvp.Split(':')[0];
                        bfColumn.DataField = kvp.Split(':')[1];
                        this.spgvMaterial.Columns.Add(bfColumn);
                    }
                    ////加入生产厂家列            
                    //TemplateField tfManufacture = new TemplateField();
                    //tfManufacture.HeaderText = "生产厂家";
                    //tfManufacture.ItemTemplate = new MulDropDownListTemplate(DataControlRowType.DataRow, "DDLManufacture", "ManufactureID", 150);
                    //this.spgvMaterial.Columns.Insert(3, tfManufacture);

                    //加入进库时间列
                    TemplateField tfArrivalTime = new TemplateField();
                    tfArrivalTime.HeaderText = "进库时间";
                    tfArrivalTime.ItemTemplate = new DateTimeTemplate("ArrivalTime");
                    this.spgvMaterial.Columns.Insert(3, tfArrivalTime);

                    //加入维修保养数量列
                    //"维修保养数量(根/台/套/件):QuantityGentaojian",
                    TemplateField tfRepairQuantity = new TemplateField();
                    tfRepairQuantity.HeaderText = "维修/保养数量";
                    tfRepairQuantity.ItemTemplate = new TextBoxTemplate("Gtj", "Gentaojian", "^(-?\\d+)(\\.\\d+)?$",100);
                    this.spgvMaterial.Columns.Insert(5, tfRepairQuantity);

                    //加入维修保养原因列
                    TemplateField tfRepairReason = new TemplateField();
                    tfRepairReason.HeaderText = "维修/保养原因";
                    tfRepairReason.ItemTemplate = new TextBoxTemplate("维修/保养原因", DataControlRowType.DataRow,"RepairReason");
                    this.spgvMaterial.Columns.Insert(6, tfRepairReason);

                    //加入计划完成时间列
                    TemplateField tfPlanTime = new TemplateField();
                    tfPlanTime.HeaderText = "计划完成时间";
                    tfPlanTime.ItemTemplate = new DateTimeTemplate("PlanTime");
                    this.spgvMaterial.Columns.Insert(7, tfPlanTime);

                    //加入实际维修数量保养列
                    TemplateField tfRealGtj = new TemplateField();
                    tfRealGtj.HeaderText = "实际维修/保养数量";
                    tfRealGtj.ItemTemplate = new TextBoxTemplate("RealGtj", "RealGentaojian", "^(-?\\d+)(\\.\\d+)?$",100);
                    this.spgvMaterial.Columns.Insert(8, tfRealGtj);

                    //加入实际完成时间列
                    TemplateField tfRealTime = new TemplateField();
                    tfRealTime.HeaderText = "实际完成时间";
                    tfRealTime.ItemTemplate = new DateTimeTemplate("RealTime");
                    this.spgvMaterial.Columns.Insert(9, tfRealTime);

                    //加入备注列            
                    TemplateField tfRepairRemark = new TemplateField();
                    tfRepairRemark.HeaderText = "备注";
                    tfRepairRemark.ItemTemplate = new TextBoxTemplate("备注", DataControlRowType.DataRow,"Remark");
                    this.spgvMaterial.Columns.Insert(10, tfRepairRemark);

                    break;
                case "回收检验":
                    foreach (var kvp in ShowVerifyTlist)
                    {
                        bfColumn = new BoundField();
                        bfColumn.HeaderText = kvp.Split(':')[0];
                        bfColumn.DataField = kvp.Split(':')[1];
                        this.spgvMaterial.Columns.Add(bfColumn);
                    }
                    
                    //加入回收日期列
                    TemplateField tfRetrieveDate = new TemplateField();
                    tfRetrieveDate.HeaderText = "回收日期";
                    tfRetrieveDate.ItemTemplate = new DateTimeTemplate("RetrieveTime");
                    this.spgvMaterial.Columns.Insert(4, tfRetrieveDate);                   

                    //加入备注列            
                    TemplateField tfVerifyRemark = new TemplateField();
                    tfVerifyRemark.HeaderText = "备注";
                    tfVerifyRemark.ItemTemplate = new TextBoxTemplate("备注", DataControlRowType.DataRow, "Remark");
                    this.spgvMaterial.Columns.Insert(7, tfVerifyRemark);
                    
                    break;
            }

            //加入生产厂家列            
            TemplateField tfManufacture = new TemplateField();
            tfManufacture.HeaderText = "生产厂家";
            tfManufacture.ItemTemplate = new MulDropDownListTemplate(DataControlRowType.DataRow, "DDLManufacture", "ManufactureID", 150);
            this.spgvMaterial.Columns.Insert(3, tfManufacture);
            
        }        

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                switch (_type)
                {
                    case "维修保养":
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
                                                      a.RealTime,
                                                      a.RealGentaojian,
                                                      a.Remark,
                                                      a.SrinMaterialRepairDetailsID
                                                  };
                        break;
                    case"回收检验":
                        spgvMaterial.DataSource = from a in db.SrinMaterialVerifyDetails
                                                  where a.SrinVerifyTransferID == _formid
                                                  select new
                                                  {   
                                                      a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                                      a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                                      a.ManufactureID,
                                                      a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.TotleGentaojian,
                                                      a.SrinDetails.SrinStocktakingDetails.StorageInfo.StorageName,
                                                      a.SrinDetails.SrinStocktakingDetails.PileInfo.PileName,
                                                      a.RetrieveTime,                                                      
                                                      a.Remark,
                                                      a.SrinMaterialVerifyDetailsID
                                                  };
                        break;
                }
                          
                spgvMaterial.DataBind();
            }
        }


        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);
            switch (_type)
            {
                case "维修保养":
                    spgvMaterial.Columns[12].Visible = false;
                    break;
                case "回收检验":
                    spgvMaterial.Columns[9].Visible = false;
                    break;
            }            
        }

        private void InitToolBar()
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
            tbarbtnBack.Text = "确认并返回";
            tbarbtnBack.ImageUrl = "/_layouts/images/BACK.GIF";
            tbarbtnBack.Click += new EventHandler(tbarbtnBack_Click);
            tbarbtnBack.OnClientClick = "return VerifyDDL()";            
            (GetControltByMaster("ltrJS") as Literal).Text = JSDialogAid.GetVerifyDDLJSForBtn("--请选择--", "请为维修保养的物资选择生产厂商！");
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

        void spgvMaterial_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    DropDownList ddlManufacture = e.Row.Cells[3].Controls[0] as DropDownList;
                    ddlManufacture.Items.Clear();
                    switch(_type)
                    {
                        case"维修保养":                            
                            ddlManufacture.DataSource = (from s in db.StorageStocks 
                                                         where (from a in db.SrinMaterialRepairDetails 
                                                                where a.SrinMaterialRepairDetailsID == Convert.ToInt32(e.Row.Cells[12].Text)
                                                                select a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialID).Contains(s.MaterialID)                                                                                                          
                                                         select new
                                                         {
                                                             s.ManufacturerName,
                                                             s.ManufacturerID
                                                         }).Distinct();
                            break;
                        case "回收检验":
                            ddlManufacture.DataSource = (from s in db.StorageStocks
                                                         where (from a in db.SrinMaterialVerifyDetails
                                                                where a.SrinMaterialVerifyDetailsID == Convert.ToInt32(e.Row.Cells[9].Text)
                                                                select a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialID).Contains(s.MaterialID)
                                                         select new
                                                         {
                                                             s.ManufacturerName,
                                                             s.ManufacturerID
                                                         }).Distinct();
                            break;
                    }                    

                    //绑定仓库
                    
                    ddlManufacture.DataTextField = "ManufacturerName";
                    ddlManufacture.DataValueField = "ManufacturerID";
                    ddlManufacture.DataBind();
                    ddlManufacture.Items.Insert(0, new ListItem("--请选择--", "0"));

                }

            }
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {
            
        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            try
            {
                int taskid = ModifyDetails();
                Response.Redirect(string.Format("ManageRepairAndVerify.aspx?TaskID={0}", taskid), false);
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_UPDATEERROR));                
            }
        }

        void tbarbtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    int ichecked = 0;                    
                    CheckBox chb;  
                    int idetailsid;
                    foreach (GridViewRow gvr in spgvMaterial.Rows)
                    {
                        chb = (CheckBox)gvr.Cells[0].Controls[0];
                        if (!chb.Checked)
                            continue;
                        ichecked++;
                        switch (_type)
                        {
                            case "维修保养":
                                idetailsid = Convert.ToInt32(gvr.Cells[12].Text);
                                SrinMaterialRepairDetails smrd = db.SrinMaterialRepairDetails.SingleOrDefault(a => a.SrinMaterialRepairDetailsID == idetailsid);
                                db.SrinMaterialRepairDetails.DeleteOnSubmit(smrd);
                                break;
                            case "回收检验":
                                idetailsid = Convert.ToInt32(gvr.Cells[9].Text);
                                SrinMaterialVerifyDetails smvd = db.SrinMaterialVerifyDetails.SingleOrDefault(a => a.SrinMaterialVerifyDetailsID == idetailsid);
                                db.SrinMaterialVerifyDetails.DeleteOnSubmit(smvd);
                                break;
                        }                                                       

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
            Response.Redirect(string.Format("SelectRepairOrVerifyDetails.aspx?FormID={0}&Type={1}", _formid,_type), false);
        }              

        #endregion

        #region 辅助方法

        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }

        private int ModifyDetails()
        {
            int taskid = 0;
            
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {                    
                switch (_type)
                {
                    case "维修保养":
                        taskid = db.SrinRepairPlan.SingleOrDefault(u => u.SrinRepairPlanID == _formid).TaskID;
                        SrinMaterialRepairDetails smrd;
                        DateTimeControl dtcPlanTime,dtcRealTime;
                        foreach (GridViewRow gvr in this.spgvMaterial.Rows)
                        {
                            smrd = db.SrinMaterialRepairDetails.SingleOrDefault(u => u.SrinMaterialRepairDetailsID == Convert.ToInt32(gvr.Cells[12].Text));
                            smrd.ArrivalTime = ((DateTimeControl)gvr.Cells[4].Controls[0]).SelectedDate;
                            smrd.ManufactureID = Convert.ToInt32((gvr.Cells[3].Controls[0] as DropDownList).SelectedValue);
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
                        break;
                    case "回收检验":
                        taskid = db.SrinVerifyTransfer.SingleOrDefault(u => u.SrinVerifyTransferID == _formid).TaskID;
                        SrinMaterialVerifyDetails smvd;
                        foreach (GridViewRow gvr in this.spgvMaterial.Rows)
                        {
                            smvd = db.SrinMaterialVerifyDetails.SingleOrDefault(u => u.SrinMaterialVerifyDetailsID == Convert.ToInt32(gvr.Cells[9].Text));
                            smvd.ManufactureID = Convert.ToInt32((gvr.Cells[3].Controls[0] as DropDownList).SelectedValue);
                            smvd.RetrieveTime = ((DateTimeControl)gvr.Cells[5].Controls[0]).SelectedDate.Date;
                            smvd.Remark = ((TextBox)gvr.Cells[8].Controls[0]).Text;
                            smvd.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();                                
                        }
                        break;
                }
                db.SubmitChanges();
                
            }
            return taskid;                                                               

        }

        #endregion

    }
}
