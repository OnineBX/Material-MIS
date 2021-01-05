/*------------------------------------------------------------------------------
 * Unit Name：ManageCommitOutDetails.cs
 * Description: 委外出库--调拨单物资明细管理页面
 * Author: Xu Chun Lei
 * Created Date: 2010-07-06
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
    public class ManageCommitOutDetails:System.Web.UI.Page
    {
        private string strBackUrl;
        private int _noticeid,_taskid;
        private SPGridView spgvMaterial;

        private static string[] ShowTlist = {
                                                "财务编码:FinanceCode",
                                                "物资名称:MaterialName",
                                                "规格型号:SpecificationModel",                                             
                                                "库存数量(根/台/套/件):StocksGentaojian",
                                                "库存数量(米):StocksMetre",
                                                "库存数量(吨):StocksTon",
                                                "StorageCommitOutDetailsID:StorageCommitOutDetailsID"
                                            };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._noticeid = Convert.ToInt32(Request.QueryString["NoticeID"]);
                _taskid = string.IsNullOrEmpty(Request.QueryString["TaskID"]) ? 0 : Convert.ToInt32(Request.QueryString["TaskID"]);                

                if (NoticeIsInProcess())
                {
                    Response.Redirect(string.Format("ViewCommitOutDetails.aspx?NoticeID={0}", _noticeid),false);
                    return;
                }

                strBackUrl = "ManageCommitOutNotice.aspx";//主流程--没有进入审批流程的情况

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
            tbarbtnDelete.Click += new EventHandler(tbarbtnDelte_Click);
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
            tfSelect.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "StorageCommitOutDetailsID");
            tfSelect.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
            this.spgvMaterial.Columns.Insert(0, tfSelect);
            
            //加入调拨数量(根/台/套/件)列
            TemplateField tfGentaojian = new TemplateField();
            tfGentaojian.HeaderText = "调拨数量(根/台/套/件)";
            tfGentaojian.ItemTemplate = new TextBoxTemplate("Gentaojian", "Gentaojian", "^(-?\\d+)(\\.\\d+)?$","0", 80);
            this.spgvMaterial.Columns.Insert(5, tfGentaojian);

            //加入调拨数量(米)列
            TemplateField tfMetre = new TemplateField();
            tfMetre.HeaderText = "调拨数量(米)";
            tfMetre.ItemTemplate = new TextBoxTemplate("Metre", "Metre", "^(-?\\d+)(\\.\\d+)?$","0", 80);
            this.spgvMaterial.Columns.Insert(7, tfMetre);

            //加入调拨数量(根/台/套/件)列
            TemplateField tfTon = new TemplateField();
            tfTon.HeaderText = "调拨数量(吨)";
            tfTon.ItemTemplate = new TextBoxTemplate("Ton", "Ton", "^(-?\\d+)(\\.\\d+)?$","0", 80);
            this.spgvMaterial.Columns.Insert(9, tfTon);

            //加入备注列
            TemplateField tfRemark = new TemplateField();
            tfRemark.HeaderText = "备注";
            tfRemark.ItemTemplate = new TextBoxTemplate("Remark", DataControlRowType.DataRow, "Remark");
            this.spgvMaterial.Columns.Insert(10, tfRemark);

        }

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {                
                //初始化调拨明细
                this.spgvMaterial.DataSource = (from a in db.StorageCommitOutDetails
                                                join b in db.StorageStocks on a.MaterialID equals b.MaterialID
                                                where a.StorageCommitOutNoticeID == _noticeid
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
                                                    a.Remark,
                                                    a.StorageCommitOutDetailsID
                                                }).Distinct();
                this.spgvMaterial.DataBind();
            }


        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            this.spgvMaterial.Columns[11].Visible = false;                 
        }        

        #endregion

        #region 控件事件        

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            if (this._taskid == 0) //主流程--没有进入审批流程的情况
                this.ModifyDetails();

            Response.Redirect(strBackUrl,false);
        }

        protected void tbarbtnAdd_Click(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("SelectCommitOutDetails.aspx?NoticeID={0}", _noticeid),false);
        }        

        void btnRefresh_Click(object sender, EventArgs e)
        {


        }       

        protected void tbarbtnDelte_Click(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    int ichecked = 0;
                    StorageCommitOutDetails scod;
                    CheckBox chb;
                    foreach (GridViewRow gvr in spgvMaterial.Rows)
                    {
                        chb = (CheckBox)gvr.Cells[0].Controls[0];
                        if (!chb.Checked)
                            continue;
                        scod = db.StorageCommitOutDetails.SingleOrDefault(a => a.StorageCommitOutDetailsID == int.Parse(gvr.Cells[11].Text));
                        db.StorageCommitOutDetails.DeleteOnSubmit(scod);
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

        private bool NoticeIsInProcess()
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    if(db.StorageOutTask.Count(u => u.NoticeID.Equals(_noticeid) && u.Process.Equals("委外出库")) !=0)
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


        /// <summary>
        /// 获取控件
        /// </summary>
        /// <param name="controlName">控件的ID名称</param>
        /// <returns>返回Control，需要强制类型转换为对应控件</returns>
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
                    StorageCommitOutDetails scod;
                    foreach (GridViewRow gvr in this.spgvMaterial.Rows)
                    {
                        scod = db.StorageCommitOutDetails.SingleOrDefault(u => u.StorageCommitOutDetailsID == Convert.ToInt32(gvr.Cells[11].Text));
                        scod.Gentaojian = Convert.ToDecimal((gvr.Cells[5].Controls[0] as TextBox).Text.Trim());
                        scod.Metre = Convert.ToDecimal((gvr.Cells[7].Controls[0] as TextBox).Text.Trim());
                        scod.Ton = Convert.ToDecimal((gvr.Cells[9].Controls[0] as TextBox).Text.Trim());
                        scod.Remark = (gvr.Cells[10].Controls[0] as TextBox).Text.Trim();
                        scod.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();

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
