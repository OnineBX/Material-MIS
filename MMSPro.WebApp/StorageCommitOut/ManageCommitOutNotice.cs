/*------------------------------------------------------------------------------
 * Unit Name：ManageCommitOutNotice.cs
 * Description: 委外出库--调拨单管理界面
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
    public class ManageCommitOutNotice : System.Web.UI.Page
    {        
        private SPGridView spgvCommitNotice;
        private int _userid;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    EmpInfo einfo = db.EmpInfo.SingleOrDefault(u => u.Account == SPContext.Current.Web.CurrentUser.LoginName);
                    _userid = einfo == null ? -1 : einfo.EmpID;
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
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>",LogToDBHelper.LOG_MSG_LOADERROR));                
            }
        }

        #region 初始化和数据绑定方法
      
        private void InitializeCustomControls()
        {
            InitToolBar();

            this.spgvCommitNotice = new SPGridView();
            this.spgvCommitNotice.AutoGenerateColumns = false;
            this.spgvCommitNotice.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");

            string[] ShowTlist =  {                                                                                                    
                                     "领料单位:BusinessUnitName",                                     
                                     "创建时间:CreateTime",
                                     "备注:Remark",                                     
                                     "ID:StorageCommitOutNoticeID"
                                   };

            BoundField bfColumn;
            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvCommitNotice.Columns.Add(bfColumn);
            }
            
            //添加选择列
            TemplateField tfieldCheckbox = new TemplateField();
            tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "StorageCommitOutNoticeID");
            tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
            this.spgvCommitNotice.Columns.Insert(0,tfieldCheckbox);
            
            //添加任务详情列
            HyperLinkField hlfTask = new HyperLinkField();
            hlfTask.HeaderText = "任务详情";  
            this.spgvCommitNotice.Columns.Insert(5, hlfTask);

            SPMenuField colMenu = new SPMenuField();
            colMenu.HeaderText = "调拨通知单编号";
            colMenu.TextFields = "StorageCommitOutNoticeCode";
            colMenu.MenuTemplateId = "spmfNoticeCode";

            colMenu.NavigateUrlFields = "StorageCommitOutNoticeID"; //定义方式:"列名1,列名2..."
            colMenu.NavigateUrlFormat = "ManageCommitOutDetails.aspx?NoticeID={0}";
            colMenu.TokenNameAndValueFields = "curNoticeID=StorageCommitOutNoticeID";//定义方式:"别名1=列名1,别名2=列名2...."

            MenuTemplate menuItemCollection = new MenuTemplate();
            menuItemCollection.ID = "spmfNoticeCode";



            MenuItemTemplate createMenuItem = new MenuItemTemplate("生产组长审核", "/_layouts/images/newitem.gif");
            createMenuItem.ClientOnClickNavigateUrl = "../PublicPage/AuditDispatchCenter.aspx?Process=委外出库&NoticeID=%curNoticeID%";            

            menuItemCollection.Controls.Add(createMenuItem);

            this.Controls.Add(menuItemCollection);
            this.spgvCommitNotice.Columns.Insert(1, colMenu);
            this.spgvCommitNotice.RowDataBound += new GridViewRowEventHandler(spgvCommitNotice_RowDataBound);

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
            tbarbtnAdd.Click +=new EventHandler(tbarbtnAdd_Click);
            tbarTop.Buttons.Controls.Add(tbarbtnAdd);
            //修改
            ToolBarButton tbarbtnEdit = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnEdit.ID = "EditRow";
            tbarbtnEdit.Text = "修改";
            tbarbtnEdit.ImageUrl = "/_layouts/images/edit.gif";
            tbarbtnEdit.Click += new EventHandler(tbarbtnEdit_Click);
            tbarTop.Buttons.Controls.Add(tbarbtnEdit);
            //删除

            ToolBarButton tbarbtnDelte = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnDelte.ID = "DeleteRow";
            tbarbtnDelte.Text = "删除";
            tbarbtnDelte.ImageUrl = "/_layouts/images/delete.gif";
            tbarbtnDelte.Click += new EventHandler(tbarbtnDelte_Click);
            StringBuilder sbScript = new StringBuilder();
            sbScript.Append("var aa= window.confirm('确认删除所选项?');");
            sbScript.Append("if(aa == false){");
            sbScript.Append("return false;}");
            tbarbtnDelte.OnClientClick = sbScript.ToString();
            tbarTop.Buttons.Controls.Add(tbarbtnDelte);


            ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnRefresh.ID = "btnRefresh";
            btnRefresh.Text = "刷新";
            btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
            btnRefresh.Padding = "0,5,0,0";
            btnRefresh.Click += new EventHandler(btnRefresh_Click);
            tbarTop.RightButtons.Controls.Add(btnRefresh);
        }

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                this.spgvCommitNotice.DataSource = from a in db.StorageCommitOutNotice
                                                   where a.Creator == this._userid
                                                   select new
                                                   {
                                                       a.Remark,
                                                       a.CreateTime,
                                                       a.BusinessUnitInfo.BusinessUnitName,
                                                       a.StorageCommitOutNoticeCode,
                                                       a.StorageCommitOutNoticeID
                                                   };
                this.spgvCommitNotice.DataBind();
            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvCommitNotice);

            //不显示调拨单ID
            this.spgvCommitNotice.Columns[5].Visible = false;         
        }


        #endregion

        #region 控件事件方法

        protected void tbarbtnAdd_Click(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("CreateCommitOutNotice.aspx?CurrentUserID={0}",this._userid),false);
        }

        protected void tbarbtnEdit_Click(object sender, EventArgs e)
        {
            CheckBox chb;
            foreach (GridViewRow gvr in this.spgvCommitNotice.Rows)
            {
                chb = (CheckBox)gvr.Cells[0].Controls[0];
                if (chb.Checked)
                {
                    Response.Redirect(string.Format("CreateCommitOutNotice.aspx?NoticeID={0}", Convert.ToInt32(gvr.Cells[5].Text)),false);
                    return;
                }                
            }
            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择需要修改的记录!')</script>");          
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {
            Response.AddHeader("Refresh", "0");
        }

        protected void tbarbtnDelte_Click(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    int ichecked = 0;
                    StorageCommitOutNotice scon;
                    CheckBox chb;
                    int noticeid;
                    foreach (GridViewRow gvr in spgvCommitNotice.Rows)
                    {
                        chb = (CheckBox)gvr.Cells[0].Controls[0];
                        if (!chb.Checked)
                            continue;
                        ichecked++;
                        noticeid = int.Parse(gvr.Cells[5].Text);

                        //分支流程--已经进入委外流程的情况
                        
                        if (db.StorageOutTask.Count(u => u.Process.Equals("委外出库") && u.NoticeID.Equals(noticeid)) != 0)
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('编号为{0}调拨单已进入委外出库流程，不能删除！')</script>",gvr.Cells[1].Text));
                            continue;
                        }

                        scon = db.StorageCommitOutNotice.SingleOrDefault(a => a.StorageCommitOutNoticeID == noticeid);
                        db.StorageCommitOutNotice.DeleteOnSubmit(scon);                        

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

        void spgvCommitNotice_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
                e.Row.Cells[6].Text = string.Format("<a href=\"javaScript:onClick=window.showModalDialog(encodeURI('../PublicPage/TaskHistoryInfo.aspx?NoticeID={0}&&TaskType=委外出库'),'0','resizable:yes;dialogWidth:968px;dialogHeight:545px')\">任务详情</a>", int.Parse(e.Row.Cells[5].Text));
        }  

        #endregion

        #region 辅助函数

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
