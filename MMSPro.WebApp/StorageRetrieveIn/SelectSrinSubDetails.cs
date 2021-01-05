/*------------------------------------------------------------------------------
 * Unit Name：SelectSrinSubDetails.cs
 * Description: 回收入库--选择和添加回收分单物资的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-07-27
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
    public class SelectSrinSubDetails : System.Web.UI.Page
    {
        private int _subdocid,_projectid;
        private string strBackUrl;
        private SPGridView spgvMaterial, spgvExistMaterial;
        private TextBox txtMaterialName, txtFinanceCode, txtSpecificationModel;        
        private Button btnSearch, btnOK;
        private CheckBox chbShowAll;

        private static string[] Titlelist = {                                     
                                                 "物料名称:MaterialName",                                     
                                                 "规格型号:SpecificationModel",                                                 
                                                 "财务编码:FinanceCode",                                                                                                                    
                                                 "MaterialID:MaterialID",
                                            };

        private static string[] ExistTitlelist = {                                                            
                                                      "物料名称:MaterialName",                                     
                                                      "规格型号:SpecificationModel",                                                      
                                                      "财务编码:FinanceCode",                                                      
                                                      "根/台/套/件:TotleGentaojian",
                                                      "米:TotleMetre",
                                                      "吨:TotleTon",
                                                      "回收单号:RetrieveCode",
                                                      "备注:Remark"                                                                                                                                                                                       
                                                };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                _subdocid = Convert.ToInt32(Request.QueryString["SubDocID"]);

                strBackUrl = Request.QueryString["BackUrl"];
                if (string.IsNullOrEmpty(strBackUrl))
                    strBackUrl = string.Format("ManageSrinSubDetails.aspx?SubDocID={0}&IsValidate=false", _subdocid);
                else
                    strBackUrl = HttpUtility.UrlDecode(strBackUrl);

                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    _projectid = db.SrinSubDoc.SingleOrDefault(u => u.SrinSubDocID == _subdocid).Project;
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

            //***初始化新建物资列表***//
            this.spgvMaterial = new SPGridView();
            this.spgvMaterial.AutoGenerateColumns = false;
            this.spgvMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");            

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

            //加入回收数量--根套件列
            TemplateField tfQuantityGtj = new TemplateField();
            tfQuantityGtj.HeaderText = "根/台/套/件";
            tfQuantityGtj.ItemTemplate = new TextBoxTemplate("TotleGentaojian", string.Empty, "^(-?\\d+)(\\.\\d+)?$", "0");
            this.spgvMaterial.Columns.Insert(4, tfQuantityGtj);

            //加入回收数量--米列
            TemplateField tfQuantityMetre = new TemplateField();
            tfQuantityMetre.HeaderText = "米";
            tfQuantityMetre.ItemTemplate = new TextBoxTemplate("TotleMetre", string.Empty, "^(-?\\d+)(\\.\\d+)?$", "0");
            this.spgvMaterial.Columns.Insert(5, tfQuantityMetre);

            //加入回收数量--吨列
            TemplateField tfQuantityTon = new TemplateField();
            tfQuantityTon.HeaderText = "吨";
            tfQuantityTon.ItemTemplate = new TextBoxTemplate("TotleTon", string.Empty, "^(-?\\d+)(\\.\\d+)?$", "0");
            this.spgvMaterial.Columns.Insert(6, tfQuantityTon);

            //加入回收单号列
            TemplateField tfRetrieveCode = new TemplateField();
            tfRetrieveCode.HeaderText = "回收单号";
            tfRetrieveCode.ItemTemplate = new TextBoxTemplate("回收单号", DataControlRowType.DataRow);
            this.spgvMaterial.Columns.Insert(7, tfRetrieveCode);

            //加入备注列            
            TemplateField tfRemark = new TemplateField();
            tfRemark.HeaderText = "备注";
            tfRemark.ItemTemplate = new TextBoxTemplate("备注", DataControlRowType.DataRow);
            this.spgvMaterial.Columns.Insert(8, tfRemark);

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
                //初始化回收项目
                (GetControltByMaster("lblProject") as Label).Text = db.ProjectInfo.SingleOrDefault(u => u.ProjectID == _projectid).ProjectName;               

                this.spgvMaterial.DataSource = (from a in db.StorageOutRealDetails
                                                join b in db.StorageStocks on new {a.StorageOutDetails.MaterialID,a.StocksID,Status = a.MaterialStatus} equals new {b.MaterialID,b.StocksID,b.Status}
                                                where a.StorageOutDetails.MaterialInfo.MaterialName.Contains(txtMaterialName.Text.Trim())
                                                && a.StorageOutDetails.MaterialInfo.FinanceCode.Contains(txtFinanceCode.Text.Trim())
                                                && a.StorageOutDetails.MaterialInfo.SpecificationModel.Contains(txtSpecificationModel.Text.Trim())
                                                && a.StorageOutNotice.ProjectID == _projectid
                                                && !(from c in db.SrinSubDetails
                                                     where c.SrinSubDocID == _subdocid
                                                     select c.MaterialID).Contains(a.StorageOutDetails.MaterialID)
                                                select new
                                                {
                                                    a.StorageOutDetails.MaterialInfo.MaterialName,
                                                    a.StorageOutDetails.MaterialInfo.SpecificationModel,
                                                    a.StorageOutDetails.MaterialInfo.FinanceCode,                                                    
                                                    b.MaterialID
                                                }).Distinct();                
                this.spgvMaterial.DataBind();

                this.spgvExistMaterial.DataSource = from a in db.SrinSubDetails
                                                    where a.SrinSubDocID == _subdocid
                                                    select new 
                                                    {
                                                        a.MaterialInfo.MaterialName,
                                                        a.MaterialInfo.SpecificationModel,
                                                        a.MaterialInfo.FinanceCode,
                                                        a.TotleGentaojian,
                                                        a.TotleMetre,
                                                        a.TotleTon,
                                                        a.RetrieveCode,
                                                        a.Remark
                                                    };
                this.spgvExistMaterial.DataBind();

            }
        }

        private void ShowCustomControls()
        {            
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            this.spgvMaterial.Columns[9].Visible = false;

            Panel p2 = (Panel)GetControltByMaster("Panel2");
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

        #region 控件事件方法
       
        void btnRefresh_Click(object sender, EventArgs e)
        {
            
        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect(strBackUrl, false);
        }       

        void chbShowAll_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (!chbShowAll.Checked)
                    chbShowAll.Enabled = true;
                else
                {                    
                    txtMaterialName.Text = string.Empty;
                    txtFinanceCode.Text = string.Empty;
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

        void btnSearch_Click(object sender, EventArgs e)
        {
            chbShowAll.AutoPostBack = false;
            chbShowAll.Checked = false;
            chbShowAll.AutoPostBack = true;
            chbShowAll.Enabled = true;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    CheckBox chb;
                    int iCount = 0;                    
                    SrinSubDetails ssd;
                    foreach (GridViewRow gvr in spgvMaterial.Rows)
                    {
                        chb = (CheckBox)gvr.Cells[0].Controls[0];
                        if (!chb.Checked)
                            continue;

                        //将选中项保存到数据库                        

                        ssd = new SrinSubDetails();
                        ssd.SrinSubDocID = _subdocid;
                        ssd.TotleGentaojian = Convert.ToDecimal(((TextBox)gvr.Cells[4].Controls[0]).Text.Trim());
                        ssd.TotleMetre = Convert.ToDecimal(((TextBox)gvr.Cells[5].Controls[0]).Text.Trim());
                        ssd.TotleTon = Convert.ToDecimal(((TextBox)gvr.Cells[6].Controls[0]).Text.Trim());
                        ssd.RetrieveCode = ((TextBox)gvr.Cells[7].Controls[0]).Text.Trim();
                        ssd.Remark = ((TextBox)gvr.Cells[8].Controls[0]).Text.Trim();
                        ssd.MaterialID = Convert.ToInt32(gvr.Cells[9].Text);                        
                        ssd.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                        ssd.Creator = db.EmpInfo.SingleOrDefault(u => u.Account == SPContext.Current.Web.CurrentUser.LoginName).EmpID;

                        db.SrinSubDetails.InsertOnSubmit(ssd);               

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

        #endregion

        #region 辅助方法

        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }            

        #endregion
    }
}
