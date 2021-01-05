/*------------------------------------------------------------------------------
 * Unit Name：SelectRepairAndVerifyDetails.cs
 * Description: 回收入库--选择和添加维修保养或回收入库物资明细页面
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
using System.Data.Linq.SqlClient;

namespace MMSPro.WebApp
{
    public class SelectRepairOrVerifyDetails:Page
    {
        private int _formid,_receiptid;//_fromid为维修保养计划表或回收检验传递表id，_receiptid为回收入库单id
        private string _type,strBackUrl;
        private Button btnOK;
        private CheckBox chbQuickSet;

        private SPGridView spgvMaterial,spgvExistMaterial;                

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                _formid = Convert.ToInt32(Request.QueryString["FormID"]);
                _type = Request.QueryString["Type"];

                if (string.IsNullOrEmpty(Request.QueryString["BackUrl"]))
                    strBackUrl = string.Format("ManageRepairOrVerifyDetails.aspx?FormID={0}&Type={1}", _formid, _type);
                else
                    strBackUrl = HttpUtility.UrlDecode(Request.QueryString["BackUrl"]);
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    switch (_type)
                    {
                        case "维修保养":
                            _receiptid = db.SrinRepairPlan.SingleOrDefault(u => u.SrinRepairPlanID == _formid).SrinReceiptID;
                            break;
                        case "回收检验":
                            _receiptid = db.SrinVerifyTransfer.SingleOrDefault(u => u.SrinVerifyTransferID == _formid).SrinReceiptID;
                            break;
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

        private void InitBar()
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
            InitBar();

            /*公共代码部分*/

            chbQuickSet = (CheckBox)GetControltByMaster("chbQuickSet");
            chbQuickSet.AutoPostBack = true;
            chbQuickSet.CheckedChanged += new EventHandler(chbQuickSet_CheckedChanged);

            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);            
            
            //***初始化新建物资列表***//
            this.spgvMaterial = new SPGridView();
            this.spgvMaterial.AutoGenerateColumns = false;
            this.spgvMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");
            this.spgvMaterial.RowDataBound += new GridViewRowEventHandler(spgvMaterial_RowDataBound);

            //添加选择列
            TemplateField tfieldCheckbox = new TemplateField();
            tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("选择", DataControlRowType.DataRow);
            tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("选择", DataControlRowType.Header);
            this.spgvMaterial.Columns.Add(tfieldCheckbox);

            //***初始化已加入物资列表***//
            this.spgvExistMaterial = new SPGridView();
            this.spgvExistMaterial.AutoGenerateColumns = false;
            this.spgvExistMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");



            /*分类处理部分*/
            switch (_type)
            {
                case "维修保养":
                    spgvMaterial_InitiRepairDetails();
                    spgvExistMaterial_InitiRepairDetails();
                    break;
                case "回收检验":
                    spgvMaterial_InitiVerifyDetails();
                    spgvExistMaterial_InitiVerifyDetails();
                    break;
            }

            //加入生产厂家列
            TemplateField tfManufacture = new TemplateField();
            tfManufacture.HeaderText = "生产厂家";
            tfManufacture.ItemTemplate = new MulDropDownListTemplate(DataControlRowType.DataRow, "DDLManufacture",150);
            this.spgvMaterial.Columns.Insert(3, tfManufacture);
        }        

        #region 维修保养计划表相关方法
        private void spgvMaterial_InitiRepairDetails()
        {
            string[] ShowTlist = {                                                                                                                        
                                      "物资名称:MaterialName",
                                      "规格型号:SpecificationModel",
                                      "回收数量(根/台/套/件):TotleGentaojian",
                                      "SrinDetailsID:SrinDetailsID"
                                 };
            BoundField bfColumn; 
            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvMaterial.Columns.Add(bfColumn);
            }           

            //加入进库时间列
            TemplateField tfArrivalTime = new TemplateField();
            tfArrivalTime.HeaderText = "进库时间";
            tfArrivalTime.ItemTemplate = new DateTimeTemplate(DataControlRowType.DataRow);
            this.spgvMaterial.Columns.Insert(3, tfArrivalTime);

            //加入维修保养数量列
            //"维修保养数量(根/台/套/件):QuantityGentaojian",
            TemplateField tfRepairQuantity = new TemplateField();
            tfRepairQuantity.HeaderText = "维修保养数量(根/台/套/件)";
            tfRepairQuantity.ItemTemplate = new TextBoxTemplate("维修保养数量(根/台/套/件)", string.Empty, "^(-?\\d+)(\\.\\d+)?$","0");
            this.spgvMaterial.Columns.Insert(5, tfRepairQuantity);

            //加入维修保养原因列
            TemplateField tfRepairReason = new TemplateField();
            tfRepairReason.HeaderText = "维修/保养原因";
            tfRepairReason.ItemTemplate = new TextBoxTemplate("维修/保养原因", DataControlRowType.DataRow);
            this.spgvMaterial.Columns.Insert(6, tfRepairReason);

            //加入备注列            
            TemplateField tfRepairRemark = new TemplateField();
            tfRepairRemark.HeaderText = "备注";
            tfRepairRemark.ItemTemplate = new TextBoxTemplate("备注", DataControlRowType.DataRow,"Remark");
            this.spgvMaterial.Columns.Insert(7, tfRepairRemark);
        }

        private void spgvExistMaterial_InitiRepairDetails()
        {
            string[] ShowTlist = {                                                                                                                        
                                                  "物资名称:MaterialName",
                                                  "规格型号:SpecificationModel",                                     
                                                  "生产厂家:ManufacturerName",
                                                  "进库时间:ArrivalTime",
                                                  "维修保养数量(根/台/套/件):Gentaojian",
                                                  "维修保养原因:RepairReason",                                                  
                                                  "备注:Remark"
                                               };
            BoundField bfColumn;
            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvExistMaterial.Columns.Add(bfColumn);
            }
        }

        #endregion

        private void spgvMaterial_InitiVerifyDetails()
        {
            string[] ShowTlist = {                                                                                                                        
                                      "物资名称:MaterialName",
                                      "规格型号:SpecificationModel",                                      
                                      "回收数量:TotleGentaojian",                                                      
                                      "仓库:StorageName",
                                      "垛位:PileName",                                                      
                                      "SrinDetailsID:SrinDetailsID"
                                  };
            BoundField bfColumn;
            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvMaterial.Columns.Add(bfColumn);
            }           

            //加入回收日期列
            TemplateField tfArrivalTime = new TemplateField();
            tfArrivalTime.HeaderText = "回收日期";
            tfArrivalTime.ItemTemplate = new DateTimeTemplate(DataControlRowType.DataRow);
            this.spgvMaterial.Columns.Insert(3, tfArrivalTime);            

            //加入备注列            
            TemplateField tfRepairRemark = new TemplateField();
            tfRepairRemark.HeaderText = "备注";
            tfRepairRemark.ItemTemplate = new TextBoxTemplate("备注", DataControlRowType.DataRow);
            this.spgvMaterial.Columns.Insert(7, tfRepairRemark);
        }
        private void spgvExistMaterial_InitiVerifyDetails()
        {
            string[] ShowVerifyTlist = {                                                                                                                        
                                                      "物资名称:MaterialName",
                                                      "规格型号:SpecificationModel",                                     
                                                      "生产厂家:ManufacturerName",
                                                      "回收数量:TotleGentaojian",
                                                      "回收日期:RetrieveTime",
                                                      "仓库:StorageName",
                                                      "垛位:PileName",
                                                      "备注:Remark"                                                      
                                                   };
            BoundField bfColumn;
            foreach (var kvp in ShowVerifyTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvExistMaterial.Columns.Add(bfColumn);
            }
        }

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                switch (_type)
                {
                    case "维修保养":
                        spgvMaterial.DataSource = from a in db.SrinDetails
                                                  where a.SrinReceiptID == _receiptid
                                                  && !(from b in db.SrinMaterialRepairDetails
                                                       where b.SrinRepairPlan.SrinReceiptID == _receiptid
                                                       select b.SrinDetailsID).Contains(a.SrinDetailsID)
                                                  select new
                                                  {
                                                      a.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                                      a.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                                      a.SrinStocktakingDetails.SrinSubDetails.TotleGentaojian,
                                                      a.Remark,
                                                      a.SrinDetailsID
                                                  };
                        spgvExistMaterial.DataSource = from a in db.SrinMaterialRepairDetails
                                                       where a.SrinRepairPlanID == _formid
                                                       select new
                                                       {
                                                           a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                                           a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                                           a.Manufacturer.ManufacturerName,
                                                           a.ArrivalTime,
                                                           a.Gentaojian,
                                                           a.RepairReason,
                                                           a.Remark
                                                       };
                        break;
                    case "回收检验":
                        spgvMaterial.DataSource = from a in db.SrinDetails
                                                  where a.SrinReceiptID == _receiptid
                                                  && !(from b in db.SrinMaterialVerifyDetails
                                                       where b.SrinVerifyTransfer.SrinReceiptID == _receiptid
                                                       select b.SrinDetailsID).Contains(a.SrinDetailsID)
                                                  select new
                                                  {
                                                      a.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                                      a.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                                      a.SrinStocktakingDetails.SrinSubDetails.TotleGentaojian,
                                                      a.SrinStocktakingDetails.StorageInfo.StorageName,
                                                      a.SrinStocktakingDetails.PileInfo.PileName,
                                                      a.SrinDetailsID
                                                  };
                        spgvExistMaterial.DataSource = from a in db.SrinMaterialVerifyDetails
                                                       where a.SrinVerifyTransferID == _formid
                                                       select new
                                                       {
                                                           a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                                           a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                                           a.Manufacturer.ManufacturerName,
                                                           a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.TotleGentaojian,
                                                           a.SrinDetails.SrinStocktakingDetails.StorageInfo.StorageName,
                                                           a.SrinDetails.SrinStocktakingDetails.PileInfo.PileName,
                                                           RetrieveTime = a.RetrieveTime.Date,
                                                           a.Remark,
                                                           a.SrinMaterialVerifyDetailsID
                                                       };

                        break;
                }

                spgvMaterial.DataBind();
                spgvExistMaterial.DataBind();
            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            switch (_type)
            {
                case "维修保养":
                    spgvMaterial.Columns[9].Visible = false;
                    break;
                case "回收检验":
                    spgvMaterial.Columns[9].Visible = false;
                    break;
            }        
           
            //还未加入回收物资的情况
            if (spgvExistMaterial.Rows.Count == 0)
            {
                GetControltByMaster("Label1").Visible = false ;
                GetControltByMaster("Panel2").Visible = false;
                GetControltByMaster("Label2").Visible = false;               
            }
            else
            {
                Panel p2 = (Panel)GetControltByMaster("Panel2");
                p2.Controls.Add(this.spgvExistMaterial);
            }

            if (this.spgvMaterial.Rows.Count == 0)//全部回收物资都已经加入的情况
            {
                GetControltByMaster("Panel1").Visible = false;
                chbQuickSet.Visible = false;
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
            Response.Redirect(strBackUrl, false);
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                switch (_type)
                {
                    case "维修保养":
                        this.SaveRepairDetails();
                        break;
                    case "回收检验":
                        this.SaveVerifyDetails();
                        break;
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", "<script>alert('请选择生产厂家！')</script>");
            }
        }

        void chbQuickSet_CheckedChanged(object sender, EventArgs e)
        {
            switch (_type)
            {
                case "维修保养":
                    this.QuickSetRepairDetails();
                    break;
                case "回收检验":
                    this.QuickSetVerifyDetails();
                    break;
            }
        }

        void spgvMaterial_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    //绑定仓库
                    DropDownList ddlManufacture = e.Row.Cells[3].Controls[0] as DropDownList;
                    ddlManufacture.Items.Clear();
                    ddlManufacture.DataSource = (from s in db.StorageStocks
                                                 where (from a in db.SrinDetails
                                                        where a.SrinDetailsID == Convert.ToInt32(e.Row.Cells[9].Text)
                                                        select a.SrinStocktakingDetails.SrinSubDetails.MaterialID).Contains(s.MaterialID)
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

        #endregion

        #region 辅助方法

        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }

        protected void RemoveControltFromMaster(string controlName)
        {
            Control ctr = this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
            this.Master.FindControl("PlaceHolderMain").Controls.Remove(ctr);
        }

        private void SaveRepairDetails()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                CheckBox chb;
                int iCount = 0;
                int iSrinDetailsID;
                SrinMaterialRepairDetails smrd;
                foreach (GridViewRow gvr in spgvMaterial.Rows)
                {
                    chb = (CheckBox)gvr.Cells[0].Controls[0];
                    if (!chb.Checked)
                        continue;

                    //将选中项保存到数据库
                    iSrinDetailsID = Convert.ToInt32(gvr.Cells[9].Text);

                    smrd = new SrinMaterialRepairDetails();
                    smrd.SrinRepairPlanID = _formid;
                    smrd.SrinDetailsID = iSrinDetailsID;
                    smrd.ManufactureID = Convert.ToInt32((gvr.Cells[3].Controls[0] as DropDownList).SelectedValue);
                    smrd.ArrivalTime = ((DateTimeControl)gvr.Cells[4].Controls[0]).SelectedDate;
                    smrd.Gentaojian = Convert.ToDecimal(((TextBox)(gvr.Cells[6].Controls[0])).Text);
                    smrd.RepairReason = ((TextBox)(gvr.Cells[7].Controls[0])).Text;
                    smrd.RealGentaojian = 0;
                    smrd.Remark = ((TextBox)(gvr.Cells[8].Controls[0])).Text;
                    smrd.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                    smrd.Creator = db.EmpInfo.SingleOrDefault(u => u.Account == SPContext.Current.Web.CurrentUser.LoginName).EmpID;

                    db.SrinMaterialRepairDetails.InsertOnSubmit(smrd);

                    iCount++;
                }
                db.SubmitChanges();

                //没有选中的情况
                if (iCount == 0)
                {
                    Response.Write("<script language='javaScript'>alert('没有选中要添加的物资！');</script>");
                    return;
                }
                Response.AddHeader("Refresh", "0");
            }
        }

        private void SaveVerifyDetails()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                CheckBox chb;
                int iCount = 0;
                int iSrinDetailsID;
                SrinMaterialVerifyDetails smvd;
                foreach (GridViewRow gvr in spgvMaterial.Rows)
                {
                    chb = (CheckBox)gvr.Cells[0].Controls[0];
                    if (!chb.Checked)
                        continue;

                    //将选中项保存到数据库
                    iSrinDetailsID = Convert.ToInt32(gvr.Cells[9].Text);

                    smvd = new SrinMaterialVerifyDetails();
                    smvd.SrinVerifyTransferID = _formid;
                    smvd.SrinDetailsID = iSrinDetailsID;
                    smvd.ManufactureID = Convert.ToInt32((gvr.Cells[3].Controls[0] as DropDownList).SelectedValue);
                    smvd.RetrieveTime = ((DateTimeControl)gvr.Cells[4].Controls[0]).SelectedDate;
                    smvd.Remark = ((TextBox)(gvr.Cells[8].Controls[0])).Text;
                    smvd.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                    smvd.Creator = db.EmpInfo.SingleOrDefault(u => u.Account == SPContext.Current.Web.CurrentUser.LoginName).EmpID;

                    db.SrinMaterialVerifyDetails.InsertOnSubmit(smvd);

                    iCount++;
                }
                db.SubmitChanges();

                //没有选中的情况
                if (iCount == 0)
                {
                    Response.Write("<script language='javaScript'>alert('没有选中要添加的物资！');</script>");
                    return;
                }
                Response.AddHeader("Refresh", "0");
            }
        }

        private void QuickSetRepairDetails()
        {
            int icount = 0;
            if (chbQuickSet.Checked)
            {                
                foreach (GridViewRow gvr in spgvMaterial.Rows)
                {
                    if ((gvr.Cells[0].Controls[0] as CheckBox).Checked)
                    {
                        //设置维修保养数量
                        (gvr.Cells[6].Controls[0] as TextBox).Text = gvr.Cells[5].Text;
                        //设置进库时间
                        (gvr.Cells[4].Controls[0] as DateTimeControl).SelectedDate = DateTime.Today;
                        icount++;
                    }
                }
            }
            else
            {
                foreach (GridViewRow gvr in spgvMaterial.Rows)
                {
                    //设置维修保养数量
                    (gvr.Cells[6].Controls[0] as TextBox).Text = "0";
                    //设置进库时间
                    (gvr.Cells[4].Controls[0] as DateTimeControl).ClearSelection();
                }
                icount = spgvMaterial.Rows.Count;
            }

            if (icount == 0)
            {
                Response.Write("<script language='javaScript'>alert('没有选中要设置的物资！');</script>");
                chbQuickSet.AutoPostBack = false;
                chbQuickSet.Checked = false;
                chbQuickSet.AutoPostBack = true;
            }

        }
        
        private void QuickSetVerifyDetails()
        {
            int icount = 0;
            if (chbQuickSet.Checked)
            {
                foreach (GridViewRow gvr in spgvMaterial.Rows)
                {
                    if ((gvr.Cells[0].Controls[0] as CheckBox).Checked)
                    {                       
                        //设置进库时间
                        (gvr.Cells[4].Controls[0] as DateTimeControl).SelectedDate = DateTime.Today;
                        icount++;
                    }
                }
            }
            else
            {
                foreach (GridViewRow gvr in spgvMaterial.Rows)
                {                   
                    //设置进库时间
                    (gvr.Cells[4].Controls[0] as DateTimeControl).ClearSelection();
                }
                icount = spgvMaterial.Rows.Count;
            }

            if (icount == 0)
            {
                Response.Write("<script language='javaScript'>alert('没有选中要设置的物资！');</script>");
                chbQuickSet.AutoPostBack = false;
                chbQuickSet.Checked = false;
                chbQuickSet.AutoPostBack = true;
            }

        }

        #endregion
    }
}
