/*------------------------------------------------------------------------------
 * Unit Name：SelectStorageOutDetails.cs
 * Description: 正常出库--生产管理员选择和添加调拨物资明细页面
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
using System.Data.Linq.SqlClient;

namespace MMSPro.WebApp
{
    public class SelectStorageOutDetails:System.Web.UI.Page
    {
        private int _noticeid;
        
        private SPGridView spgvMaterial,spgvExistMaterial;
        private TextBox txtMaterialName, txtFinanceCode, txtSpecificationModel;        
        private Button btnSearch, btnOK;
        private CheckBox chbShowAll;
        private string strBackUrl;

        private static string[] Titlelist = {        
                                                "财务编码:FinanceCode",
                                                "物料名称:MaterialName",                                                
                                                "规格型号:SpecificationModel",                                     
                                                "库存数量(根/台/套/件):StocksGentaojian",
                                                "库存数量(米):StocksMetre",
                                                "库存数量(吨):StocksTon",                                                 
                                                "MaterialID:MaterialID",
                                            };
        private static string[] ExistTitlelist = {      
                                                     "财务编码:FinanceCode",
                                                     "物料名称:MaterialName",                                                     
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
                _noticeid = Convert.ToInt32(Request.QueryString["NoticeID"]);

                strBackUrl = Request.QueryString["BackUrl"];
                if (string.IsNullOrEmpty(strBackUrl))
                    strBackUrl = string.Format("ManageStorageOutDetails.aspx?NoticeID={0}", _noticeid);
                else
                    strBackUrl = HttpUtility.UrlDecode(strBackUrl);

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

        #region 初始化和绑定方法

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

            //***初始化新建物资列表***//
            this.spgvMaterial = new SPGridView();
            this.spgvMaterial.AutoGenerateColumns = false;
            this.spgvMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");

            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);//应用分页模版，必须先将spgvMaterial加入到页面中

            //分页
            this.spgvMaterial.AllowPaging = true;
            this.spgvMaterial.PageSize = 10;
            this.spgvMaterial.PageIndexChanging += new GridViewPageEventHandler(spgvMaterial_PageIndexChanging);
            this.spgvMaterial.PagerTemplate = new PagerTemplate("{0} - {1}", spgvMaterial);
            this.spgvMaterial.Columns.Clear();//应用分页模版，必须清除所有列

            //添加选择列
            TemplateField tfieldCheckbox = new TemplateField();
            tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("选择", DataControlRowType.DataRow);
            tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("选择", DataControlRowType.Header);
            this.spgvMaterial.Columns.Add(tfieldCheckbox);

            BoundField bfColumn;
            foreach (var kvp in Titlelist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvMaterial.Columns.Add(bfColumn);
            }

            //加入调拨数量(根/台/套/件)列
            TemplateField tfGentaojian = new TemplateField();
            tfGentaojian.HeaderText = "调拨数量(根/台/套/件)";
            tfGentaojian.ItemTemplate = new TextBoxTemplate("Gentaojian", string.Empty, "^(-?\\d+)(\\.\\d+)?$", "0",80);
            this.spgvMaterial.Columns.Insert(5, tfGentaojian);

            //加入调拨数量(米)列
            TemplateField tfMetre = new TemplateField();
            tfMetre.HeaderText = "调拨数量(米)";
            tfMetre.ItemTemplate = new TextBoxTemplate("Metre", string.Empty, "^(-?\\d+)(\\.\\d+)?$", "0",80);
            this.spgvMaterial.Columns.Insert(7, tfMetre);

            //加入调拨数量(吨)列
            TemplateField tfTon = new TemplateField();
            tfTon.HeaderText = "调拨数量(吨)";
            tfTon.ItemTemplate = new TextBoxTemplate("Ton",string.Empty,"^(-?\\d+)(\\.\\d+)?$","0",80);
            this.spgvMaterial.Columns.Insert(9, tfTon);

            //加入备注
            TemplateField tfRemark = new TemplateField();
            tfRemark.HeaderText = "备注";
            tfRemark.ItemTemplate = new TextBoxTemplate("Remark", DataControlRowType.DataRow);
            this.spgvMaterial.Columns.Insert(10, tfRemark);

            //***初始化已加入物资列表***//
            this.spgvExistMaterial = new SPGridView();
            this.spgvExistMaterial.AutoGenerateColumns = false;
            this.spgvExistMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");            

            foreach (var kvp in ExistTitlelist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvExistMaterial.Columns.Add(bfColumn);
            }


            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);

            btnSearch = (Button)GetControltByMaster("btnSearch");
            btnSearch.Click += new EventHandler(btnSearch_Click);

            chbShowAll = (CheckBox)GetControltByMaster("chbShowAll");
            chbShowAll.CheckedChanged += new EventHandler(chbShowAll_CheckedChanged);
            
            txtMaterialName = GetControltByMaster("txtMaterialName") as TextBox;
            txtFinanceCode = GetControltByMaster("txtFinanceCode") as TextBox;
            txtSpecificationModel = GetControltByMaster("txtSpecificationModel") as TextBox;
           
        }                         

        private void BindDataToCustomControls()
        {          

            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                this.spgvMaterial.DataSource = (from a in db.StorageStocks
                                               where a.MaterialName.Contains(txtMaterialName.Text.Trim())
                                               && a.SpecificationModel.Contains(txtSpecificationModel.Text.Trim())
                                               && a.FinanceCode.Contains(txtFinanceCode.Text.Trim())
                                               && !(from b in db.StorageOutDetails
                                                    where b.StorageOutNoticeID == _noticeid
                                                    select b.MaterialID).Contains(a.MaterialID)
                                               select new
                                               {
                                                   a.MaterialName,                                                   
                                                   a.SpecificationModel,
                                                   a.FinanceCode,
                                                   StocksGenTaojian = (from c in db.StorageStocks
                                                                      where c.MaterialID == a.MaterialID
                                                                      select c).Sum(u => u.StocksGenTaojian),
                                                   StocksMetre = (from c in db.StorageStocks
                                                                  where c.MaterialID == a.MaterialID
                                                                  select c).Sum(u => u.StocksMetre),
                                                   StocksTon = (from c in db.StorageStocks
                                                                where c.MaterialID == a.MaterialID
                                                                select c).Sum(u => u.StocksTon),
                                                   a.MaterialID
                                               }).Distinct();                
                this.spgvMaterial.DataBind();

                this.spgvExistMaterial.DataSource = (from a in db.StorageOutDetails
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
                this.spgvExistMaterial.DataBind();
                                                   
            }
        }

        private void ShowCustomControls()
        {            
            this.spgvMaterial.Columns[11].Visible = false;

            Panel p2 = GetControltByMaster("Panel2") as Panel;
            p2.Controls.Add(this.spgvExistMaterial);

            //还未加入回收物资的情况
            if (spgvExistMaterial.Rows.Count == 0)
                GetControltByMaster("tblExist").Visible = false;
            else
                GetControltByMaster("tblExist").Visible = true;
            //没有可加入物资的情况
            if (spgvMaterial.Rows.Count == 0)
            {
                btnOK.Visible = false;
                GetControltByMaster("tblUnAdd").Visible = false;
            }
            else
            {
                btnOK.Visible = true;
                GetControltByMaster("tblUnAdd").Visible = true;
            }

           
        }

       

        #endregion

        #region 控件事件

        void spgvMaterial_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            spgvMaterial.PageIndex = e.NewPageIndex;
            spgvMaterial.DataBind();
        }           

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect(strBackUrl,false);
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {


        }

        void chbShowAll_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (!chbShowAll.Checked)
                    chbShowAll.Enabled = true;
                else
                {
                    txtFinanceCode.Text = string.Empty;
                    txtMaterialName.Text = string.Empty;
                    txtSpecificationModel.Text = string.Empty;
                    chbShowAll.Enabled = false;
                }
                Response.AddHeader("Refresh", "0");
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));  
            }            
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    CheckBox chb;
                    int iCount = 0;
                    int iMaterialID;
                    StorageOutDetails sod;
                    foreach (GridViewRow gvr in spgvMaterial.Rows)
                    {
                        chb = (CheckBox)gvr.Cells[0].Controls[0];
                        if (!chb.Checked)
                            continue;

                        //将选中项保存到数据库
                        iMaterialID = Convert.ToInt32(gvr.Cells[11].Text);
                        sod = new StorageOutDetails();
                        sod.StorageOutNoticeID = _noticeid;
                        sod.MaterialID = iMaterialID;
                        sod.Gentaojian = Convert.ToDecimal((gvr.Cells[5].Controls[0] as TextBox).Text.Trim());
                        sod.Metre = Convert.ToDecimal((gvr.Cells[7].Controls[0] as TextBox).Text.Trim());
                        sod.Ton = Convert.ToDecimal((gvr.Cells[9].Controls[0] as TextBox).Text.Trim());
                        sod.Creator = db.EmpInfo.SingleOrDefault(u => u.Account == SPContext.Current.Web.CurrentUser.LoginName).EmpID;
                        sod.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                        sod.Remark = (gvr.Cells[10].Controls[0] as TextBox).Text.Trim();
                        db.StorageOutDetails.InsertOnSubmit(sod);                        

                        iCount++;
                    }
                    db.SubmitChanges();

                    //没有选中的情况
                    if (iCount == 0)
                    {
                        Response.Write("<script language='javaScript'>alert('没有选中要添加的物资明细！');</script>");
                        return;
                    }
                }

                Response.AddHeader("Refresh", "0");
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
            }   
            
        }

        void btnSearch_Click(object sender, EventArgs e)
        {       
            chbShowAll.AutoPostBack = false;
            chbShowAll.Checked = false;
            chbShowAll.AutoPostBack = true;
            chbShowAll.Enabled = true;

        }

        #endregion                              
        
        #region 辅助方法
        /// <summary>
        /// 获取控件
        /// </summary>
        /// <param name="controlName">控件的ID名称</param>
        /// <returns>返回Control，需要强制类型转换为对应控件</returns>
        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }                

        #endregion
    }
}
