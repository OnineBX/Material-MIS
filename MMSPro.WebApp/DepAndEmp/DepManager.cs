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
using MMSPro.ADHelper.DirectoryServices;

namespace MMSPro.WebApp
{
    public class DepManager : System.Web.UI.Page
    {
        SPGridView spgviewDepartment;
        //List<KeyValuePair<string, string>> Tlist = new List<KeyValuePair<string, string>>();
        static string[] Tlist = new string[5];
        List<DirectoryOrganizationalUnit> ous;
        string _domainName;
        string _adminOfDC;
        string _pwdOfDC;
        string _nameOfRootOU;
        string _domainAbbreviate;

        //List<CheckBoxAid> _chbSelectedItems;
        protected void Page_Load(object sender, EventArgs e)
        {
            //string[] str = new string[5];
            //  ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");
            this.spgviewDepartment = new SPGridView();
            this.spgviewDepartment.AutoGenerateColumns = false;
            //spgridview.RowCreated += new GridViewRowEventHandler(spgridview_RowCreated);
            Tlist[0] = "部门名称:DepName";
            Tlist[1] = "部门编码:DepCode";
            Tlist[2] = "负责人:Incharge";
            Tlist[3] = "联系方式:Contact";
            Tlist[4] = "备注:Remark";
            BindGridView();
            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarEmployee");
            //新建
            ToolBarButton tbarbtnAdd = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnAdd.ID = "AddNewRow";
            tbarbtnAdd.Text = "新建";
            tbarbtnAdd.ImageUrl = "/_layouts/images/newitem.gif";
            tbarbtnAdd.Click += new EventHandler(tbarbtnAdd_Click);
            tbarbtnAdd.Visible = false;
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

            ToolBarButton btnSync = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnSync.ID = "btnSync";
            btnSync.Text = "同步AD部门";
            btnSync.ImageUrl = "/_layouts/images/addressbook.gif";
            btnSync.Click += new EventHandler(btnSync_Click);
            tbarTop.Buttons.Controls.Add(btnSync);

            ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnRefresh.ID = "btnRefresh";
            btnRefresh.Text = "刷新";
            btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
            btnRefresh.Padding = "0,5,0,0";
            btnRefresh.Click += new EventHandler(btnRefresh_Click);
            tbarTop.RightButtons.Controls.Add(btnRefresh);
        }

        void btnSync_Click(object sender, EventArgs e)
        {
            #region 老版本 仅能获得第一层部门，不能获得递归部门
            //using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            //{
            //    string domainName = ConfigurationManager.AppSettings["mmsDomainName"].ToString();
            //    string adminOfDC = ConfigurationManager.AppSettings["mmsAdminOfDC"].ToString();
            //    string pwdOfDC = ConfigurationManager.AppSettings["mmsPwdOfDC"].ToString();
            //    string nameOfRootOU = ConfigurationManager.AppSettings["mmsNameOfRootOU"].ToString();
            //    using (DirectoryContext dc = new DirectoryContext(domainName, adminOfDC, pwdOfDC))
            //    {
            //        ous = dc.OrganizationalUnits;
            //        var myOU = ous.SingleOrDefault(ou => ou.Name == nameOfRootOU);
            //        var allMyOus = ous.Where(ou => ou.ParentGuid == myOU.Guid);
            //        List<DepInfo> listDeps = new List<DepInfo>();
            //        DepInfo di;
            //        foreach (var ou in allMyOus)
            //        {
            //            var tempDep = db.DepInfo.SingleOrDefault(u => u.DepName == ou.Name);
            //            if (tempDep == null)
            //            {
            //                di = new DepInfo();
            //                di.DepName = ou.Name;
            //                di.DepCode = ou.Guid.ToString();
            //                di.InCharge = "";
            //                di.Contact = "";
            //                di.Remark = "";
            //                listDeps.Add(di);
            //            }
            //        }
            //        if (listDeps != null)
            //        {
            //            db.DepInfo.InsertAllOnSubmit(listDeps);
            //            db.SubmitChanges();
            //            BindGridView();
            //        }
            //    }
            //}
            #endregion
            this._domainName = ConfigurationManager.AppSettings["mmsDomainName"].ToString();
            this._adminOfDC = ConfigurationManager.AppSettings["mmsAdminOfDC"].ToString();
            this._pwdOfDC = ConfigurationManager.AppSettings["mmsPwdOfDC"].ToString();
            this._nameOfRootOU = ConfigurationManager.AppSettings["mmsNameOfRootOU"].ToString();
            this._domainAbbreviate = ConfigurationManager.AppSettings["mmsDomainAbbreviate"].ToString();
            using (DirectoryContext dc = new DirectoryContext(this._domainName, this._adminOfDC, this._pwdOfDC))
            {
                ous = new List<DirectoryOrganizationalUnit>();
                ous = dc.OrganizationalUnits;
                var rootOU = ous.SingleOrDefault(ou => ou.Name == this._nameOfRootOU);
                GetChildOU(rootOU);
                Response.Redirect("DepManager.aspx");
            }
        }
        /// <summary>
        /// 递归获取任意部门
        /// </summary>
        /// <param name="parentOU">父OU</param>
        protected void GetChildOU(DirectoryOrganizationalUnit parentOU)
        {
            var childOUs = ous.Where(u => u.ParentGuid == parentOU.Guid);
            DepInfo di;
            if (childOUs != null && childOUs.Count() > 0)
            {
                foreach (var childOU in childOUs)
                {
                    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        using (DirectoryContext dc = new DirectoryContext(this._domainName, this._adminOfDC, this._pwdOfDC))
                        {
                            var tempDep = db.DepInfo.SingleOrDefault(u => u.DepName == childOU.Name);
                            if (tempDep == null)
                            {
                                di = new DepInfo();
                                di.DepName = childOU.Name;
                                di.DepCode = childOU.Guid.ToString();
                                di.InCharge = "";
                                di.Contact = "";
                                di.Remark = "";
                                db.DepInfo.InsertOnSubmit(di);
                                db.SubmitChanges();
                            }
                        }
                    }
                    GetChildOU(childOU);
                }
            }
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {
            Response.Redirect("DepManager.aspx");

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
        protected void tbarbtnDelte_Click(object sender, EventArgs e)
        {
            List<CheckBox> listString = GetCheckedID();
            if (listString.Count > 0)
            {
                DepInfo di;
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    foreach (var li in listString)
                    {
                        if (db.EmpInfo.FirstOrDefault(a => a.DepID == int.Parse(li.ToolTip)) != null)
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('所删除的记录包含有用户,请先移除所有用户后删除类别!')</script>");
                            return;
                        }
                        di = db.DepInfo.SingleOrDefault(a => a.DepID == int.Parse(li.ToolTip));
                        if (di != null)
                        {
                            db.DepInfo.DeleteOnSubmit(di);

                        }
                    }
                    db.SubmitChanges();
                }
                Response.Redirect("DepManager.aspx");
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择需要删除的记录!')</script>");
            }

        }

        protected void tbarbtnEdit_Click(object sender, EventArgs e)
        {
            List<CheckBox> listString = GetCheckedID();
            if (listString.Count == 1)
            {
                Response.Redirect("DepEditer.aspx?DepID=" + listString[0].ToolTip);
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择一条且仅能有一条记录进行编辑!')</script>");
            }
        }

        protected void tbarbtnAdd_Click(object sender, EventArgs e)
        {
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");
            Response.Redirect("DepCreater.aspx");
        }

        /// <summary>
        /// 绑定Gridview
        /// </summary>
        private void BindGridView()
        {
            this.spgviewDepartment.Columns.Clear();
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                BoundField bfColumn;
                //添加选择列
                TemplateField tfieldCheckbox = new TemplateField();
                tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "DepID");
                tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
                this.spgviewDepartment.Columns.Add(tfieldCheckbox);
                foreach (var kvp in Tlist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.spgviewDepartment.Columns.Add(bfColumn);
                }
                this.spgviewDepartment.DataSource = from a in db.DepInfo
                                                    select a;
                this.spgviewDepartment.DataBind();
                Panel p1 = (Panel)GetControltByMaster("Panel1");
                p1.Controls.Add(this.spgviewDepartment);

            }
            #region
            //using (SPWeb spweb = SPContext.Current.Web)
            //{

            //    SPList splist = spweb.Lists["用户信息"];
            //    //生成绑定列
            //    //生成数据源
            //    BoundField bfColumn;
            //    StringCollection strcollection = new StringCollection();
            //    //添加选择列
            //    TemplateField tfieldCheckbox = new TemplateField();
            //    tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow);
            //    tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
            //    spgridview.Columns.Add(tfieldCheckbox);


            //    foreach (var kvp in Tlist)
            //    {
            //        bfColumn = new BoundField();
            //        bfColumn.HeaderText = kvp.Key;
            //        bfColumn.DataField = kvp.Value;
            //        spgridview.Columns.Add(bfColumn);

            //        strcollection.Add(kvp.Value);
            //    }

            //    SPQuery spquery = new SPQuery();

            //    spquery.ViewFields =  DataCom.getVileFieldXML(strcollection);
            //    spgridview.DataSource = splist.GetItems(spquery).GetDataTable();
            //    spgridview.DataBind();
            //    Panel p1 = (Panel)GetControltByMaster("Panel1");
            //    p1.Controls.Add(spgridview);
            //}
            #endregion
        }
        /// <summary>
        /// 返回选中的列表
        /// </summary>
        /// <returns>返回list构成的列表</returns>
        private List<CheckBox> GetCheckedID()
        {
            List<CheckBox> list = new List<CheckBox>();

            foreach (GridViewRow row in this.spgviewDepartment.Rows)
            {

                CheckBox ck = (CheckBox)row.Cells[0].FindControl("SMItem");
                if (ck.Checked)
                {
                    list.Add(ck);
                }
            }
            return list;
        }
        private void done()
        {

        }
    }
}
