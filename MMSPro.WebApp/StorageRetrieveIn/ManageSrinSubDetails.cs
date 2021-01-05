/*------------------------------------------------------------------------------
 * Unit Name：ManageSrinSubDetails.cs
 * Description: 回收入库--回收分单物资管理页面
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

namespace MMSPro.WebApp
{
    public class ManageSrinSubDetails: System.Web.UI.Page
    {        
        private int _subdocid, _taskid;
        private SPGridView spgvMaterial;
        private bool isValidate;

        private static string[] ShowTlist = {   "物料名称:MaterialName",                                     
                                                "规格型号:SpecificationModel",                                                
                                                "财务编码:FinanceCode",                                                                                           
                                                "SrinSubDetailsID:SrinSubDetailsID"
                                             };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._subdocid = Convert.ToInt32(Request.QueryString["SubDocID"]);
                _taskid = string.IsNullOrEmpty(Request.QueryString["TaskID"]) ? 0 : Convert.ToInt32(Request.QueryString["TaskID"]);
                isValidate = Convert.ToBoolean(Request.QueryString["IsValidate"]);//是否验证该回收分单单已进入流程

                if (isValidate)
                {
                    if (SubDocIsInProcess())
                    {
                        Response.Redirect(string.Format("ViewSrinSubDetails.aspx?SubDocID={0}", _subdocid), false);
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

            //添加选择列
            TemplateField tfSelect = new TemplateField();
            tfSelect.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "SrinSubDetailsID");
            tfSelect.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
            this.spgvMaterial.Columns.Insert(0, tfSelect);
            
            //加入回收数量--根套件列
            TemplateField tfQuantityGtj = new TemplateField();
            tfQuantityGtj.HeaderText = "根/台/套/件";
            tfQuantityGtj.ItemTemplate = new TextBoxTemplate("TotleGentaojian", "TotleGentaojian", "^(-?\\d+)(\\.\\d+)?$", "0");
            this.spgvMaterial.Columns.Insert(4, tfQuantityGtj);

            //加入回收数量--米列
            TemplateField tfQuantityMetre = new TemplateField();
            tfQuantityMetre.HeaderText = "米";
            tfQuantityMetre.ItemTemplate = new TextBoxTemplate("TotleMetre", "TotleMetre", "^(-?\\d+)(\\.\\d+)?$", "0");
            this.spgvMaterial.Columns.Insert(5, tfQuantityMetre);

            //加入回收数量--吨列
            TemplateField tfQuantityTon = new TemplateField();
            tfQuantityTon.HeaderText = "吨";
            tfQuantityTon.ItemTemplate = new TextBoxTemplate("TotleTon", "TotleTon", "^(-?\\d+)(\\.\\d+)?$", "0");
            this.spgvMaterial.Columns.Insert(6, tfQuantityTon);

            //加入回收单号列
            TemplateField tfRetrieveCode = new TemplateField();
            tfRetrieveCode.HeaderText = "回收单号";
            tfRetrieveCode.ItemTemplate = new TextBoxTemplate("回收单号", DataControlRowType.DataRow, "RetrieveCode");
            this.spgvMaterial.Columns.Insert(7, tfRetrieveCode);

            //加入备注列            
            TemplateField tfRemark = new TemplateField();
            tfRemark.HeaderText = "备注";
            tfRemark.ItemTemplate = new TextBoxTemplate("备注", DataControlRowType.DataRow,"Remark");
            this.spgvMaterial.Columns.Insert(8, tfRemark);            
                  
        }        

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                this.spgvMaterial.DataSource = from a in db.SrinSubDetails
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
                                                   a.Remark,
                                                   a.SrinSubDetailsID
                                               };
                this.spgvMaterial.DataBind();
            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            this.spgvMaterial.Columns[9].Visible = false;
        }

        #endregion

        #region 控件方法事件

        void tbarbtnAdd_Click(object sender, EventArgs e)
        {            
            Response.Redirect(string.Format("SelectSrinSubDetails.aspx?SubDocID={0}", _subdocid), false);
        }  

        void btnRefresh_Click(object sender, EventArgs e)
        {
            
        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            this.ModifyDetails();
            Response.Redirect("ManageSrinSubDoc.aspx",false);
        }

        void tbarbtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    int ichecked = 0;
                    SrinSubDetails ssd;
                    CheckBox chb;
                    foreach (GridViewRow gvr in spgvMaterial.Rows)
                    {
                        chb = (CheckBox)gvr.Cells[0].Controls[0];
                        if (!chb.Checked)
                            continue;
                        ssd = db.SrinSubDetails.SingleOrDefault(a => a.SrinSubDetailsID == int.Parse(gvr.Cells[9].Text));
                        db.SrinSubDetails.DeleteOnSubmit(ssd);
                        ichecked++;

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
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_DELETEERROR));
            }        
        }       

        #endregion

        #region 辅助方法
        private bool SubDocIsInProcess()
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    //分支流程--已经进入流程的情况
                    if (db.TaskStorageIn.Count(u => u.StorageInType.Equals("回收入库") && u.TaskType.Equals("物资组清点") && u.StorageInID.Equals(_subdocid)) != 0)                                            
                        return true;                    
                }

            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));
            }

            return false;
        }

        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }

        private void ModifyDetails()
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {                    
                    foreach (GridViewRow gvr in this.spgvMaterial.Rows)
                    {
                        SrinSubDetails ssd = db.SrinSubDetails.SingleOrDefault(u => u.SrinSubDetailsID == Convert.ToInt32(gvr.Cells[9].Text));
                        ssd.TotleGentaojian = Convert.ToDecimal(((TextBox)gvr.Cells[4].Controls[0]).Text.Trim());
                        ssd.TotleMetre = Convert.ToDecimal(((TextBox)gvr.Cells[5].Controls[0]).Text.Trim());
                        ssd.TotleTon = Convert.ToDecimal(((TextBox)gvr.Cells[6].Controls[0]).Text.Trim());
                        ssd.RetrieveCode = ((TextBox)gvr.Cells[7].Controls[0]).Text.Trim();
                        ssd.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                        ssd.Remark = ((TextBox)gvr.Cells[8].Controls[0]).Text.Trim();
                        db.SubmitChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_UPDATEERROR));
            }

        }       

        #endregion
    }
}
