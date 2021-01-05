/*------------------------------------------------------------------------------
 * Unit Name：ManageSrinSubDoc.cs
 * Description: 回收入库--物资管理员管理回收分单的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-08-19
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
    public class ManageSrinSubDoc:Page
    {
        private SPGridView spgvSubDoc;
        private int _userid;

        private static string[] ShowTlist =  {                                                                                                  
                                                 "回收项目:ProjectName",                                                 
                                                 "创建时间:CreateTime",
                                                 "包含物资:MaterialCount",
                                                 "备注:Remark",                                                                                      
                                                 "TakerID:Taker",
                                                 "ID:SrinSubDocID"
                                               };

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
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
        }

        #region 初始化和数据绑定方法

        private void InitializeCustomControls()
        {
            InitToolBar();

            this.spgvSubDoc = new SPGridView();
            this.spgvSubDoc.AutoGenerateColumns = false;
            this.spgvSubDoc.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");

           

            BoundField bfColumn;
            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvSubDoc.Columns.Add(bfColumn);
            }

            //添加选择列
            TemplateField tfieldCheckbox = new TemplateField();
            tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "SrinSubDocID");
            tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
            this.spgvSubDoc.Columns.Insert(0, tfieldCheckbox);           

            SPMenuField colMenu = new SPMenuField();
            colMenu.HeaderText = "清点负责人";
            colMenu.TextFields = "TakerName";
            colMenu.MenuTemplateId = "spmfTaker";

            colMenu.NavigateUrlFields = "SrinSubDocID,Taker"; //定义方式:"列名1,列名2..."
            colMenu.NavigateUrlFormat = "ManageSrinSubDetails.aspx?SubDocID={0}&IsValidate=true";
            colMenu.TokenNameAndValueFields = "curID=SrinSubDocID,takerID=Taker";//定义方式:"别名1=列名1,别名2=列名2...."

            MenuTemplate menuItemCollection = new MenuTemplate();
            menuItemCollection.ID = "spmfTaker";

            MenuItemTemplate mitMaterial = new MenuItemTemplate("提交清点", "/_layouts/images/newitem.gif");
            mitMaterial.ClientOnClickNavigateUrl = "SrinDispatchCenter.aspx?TaskType=物资组清点&FormID=%curID%&Executor=%takerID%";
            menuItemCollection.Controls.Add(mitMaterial);            

            this.Controls.Add(menuItemCollection);
            this.spgvSubDoc.Columns.Insert(1, colMenu);
            this.spgvSubDoc.RowDataBound += new GridViewRowEventHandler(spgvSubDoc_RowDataBound);

            //添加任务详情列
            HyperLinkField hlfTask = new HyperLinkField();
            hlfTask.HeaderText = "任务详情";
            this.spgvSubDoc.Columns.Insert(8, hlfTask);

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
                this.spgvSubDoc.DataSource = from a in db.SrinSubDoc
                                               where a.Creator == this._userid
                                               select new
                                               {
                                                   a.Remark,
                                                   a.CreateTime,
                                                   a.ProjectInfo.ProjectName,
                                                   MaterialCount = a.SrinSubDetails.Count,
                                                   TakerName = a.EmpInfo1.EmpName,
                                                   a.Taker,                                                   
                                                   a.SrinSubDocID
                                               };
                this.spgvSubDoc.DataBind();
            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvSubDoc);
            
            this.spgvSubDoc.Columns[6].Visible = false;
            this.spgvSubDoc.Columns[7].Visible = false;
        }

        #endregion

        #region 控件事件方法

        protected void tbarbtnAdd_Click(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("CreateSrinSubDoc.aspx?CurrentUserID={0}", this._userid), false);
        }

        protected void tbarbtnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    CheckBox chb;
                    int subdocid;
                    foreach (GridViewRow gvr in this.spgvSubDoc.Rows)
                    {
                        chb = (CheckBox)gvr.Cells[0].Controls[0];
                        if (chb.Checked)
                        {
                            subdocid = int.Parse(gvr.Cells[7].Text);
                            //分支流程--已经进入流程的情况
                            if (db.TaskStorageIn.Count(u => u.StorageInType.Equals("回收入库") && u.TaskType.Equals("物资组清点") && u.StorageInID.Equals(subdocid)) != 0)
                            {
                                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}处理的回收分单已进入回收入库流程，不能编辑！')</script>", (gvr.Cells[1].Controls[0] as Microsoft.SharePoint.WebControls.Menu).Text));
                                continue;
                            }

                            Response.Redirect(string.Format("CreateSrinSubDoc.aspx?SubDocID={0}", Convert.ToInt32(gvr.Cells[7].Text)), false);
                            return;
                        }
                    }
                    ClientScript.RegisterClientScriptBlock(typeof(string), "提示", "<script>alert('请选择需要修改的记录!')</script>");
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));
            }
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
                    SrinSubDoc ssd;
                    CheckBox chb;
                    int subdocid;
                    foreach (GridViewRow gvr in spgvSubDoc.Rows)
                    {
                        chb = (CheckBox)gvr.Cells[0].Controls[0];
                        if (!chb.Checked)
                            continue;
                        ichecked++;
                        subdocid = int.Parse(gvr.Cells[7].Text);

                        //分支流程--已经进入流程的情况
                        if(db.TaskStorageIn.Count(u => u.StorageInType.Equals("回收入库") && u.TaskType.Equals("物资组清点") && u.StorageInID.Equals(subdocid)) != 0)
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}处理的回收分单已进入回收入库流程，不能删除！')</script>", (gvr.Cells[1].Controls[0] as Microsoft.SharePoint.WebControls.Menu).Text));
                            continue;
                        }

                        ssd = db.SrinSubDoc.SingleOrDefault(a => a.SrinSubDocID == subdocid);
                        db.SrinSubDoc.DeleteOnSubmit(ssd);

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

        void spgvSubDoc_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
                e.Row.Cells[8].Text = string.Format("<a href=\"javaScript:onClick=window.showModalDialog(encodeURI('SrinTaskHistoryInfo.aspx?WorkID={0}&TaskType=物资组清点'),'0','resizable:yes;dialogWidth:968px;dialogHeight:545px')\">任务详情</a>", int.Parse(e.Row.Cells[7].Text));
        }

        #endregion

        #region 辅助函数
       
        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }

        #endregion
    }
}
