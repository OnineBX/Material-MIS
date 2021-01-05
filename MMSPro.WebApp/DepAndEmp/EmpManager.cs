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
    public class EmpManager : System.Web.UI.Page
    {
        SPGridView spgviewEmploy;        
        static string[] Tlist = new string[5];
        List<DirectoryOrganizationalUnit> ous;
        List<DirectoryUser> users;
        List<string> noSyncDeps;
        string _domainName;
        string _adminOfDC;
        string _pwdOfDC;
        string _nameOfRootOU;
        string _domainAbbreviate;
        string _mmsRightGroup;

        protected void Page_Load(object sender, EventArgs e)
        {
            this.spgviewEmploy = new SPGridView();
            this.spgviewEmploy.AutoGenerateColumns = false;
            Tlist[0] = "账号:Account";
            Tlist[1] = "姓名:EmpName";
            Tlist[2] = "所属部门:DepName";
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
            btnSync.Text = "同步AD账户";
            btnSync.ImageUrl = "/_layouts/images/ALLUSR.GIF";
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
            this._domainName = ConfigurationManager.AppSettings["mmsDomainName"].ToString();
            this._adminOfDC = ConfigurationManager.AppSettings["mmsAdminOfDC"].ToString();
            this._pwdOfDC = ConfigurationManager.AppSettings["mmsPwdOfDC"].ToString();
            this._nameOfRootOU = ConfigurationManager.AppSettings["mmsNameOfRootOU"].ToString();
            this._domainAbbreviate = ConfigurationManager.AppSettings["mmsDomainAbbreviate"].ToString();
            this._mmsRightGroup = ConfigurationManager.AppSettings["mmsRightGroup"].ToString();

            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                using (DirectoryContext dc = new DirectoryContext(this._domainName, this._adminOfDC, this._pwdOfDC))
                {
                    if (db.EmpInfo.SingleOrDefault(emp => emp.Account.ToLower() == SPContext.Current.Web.CurrentUser.LoginName.ToLower()) == null)
                    {
                        ous = new List<DirectoryOrganizationalUnit>();
                        users = new List<DirectoryUser>();
                        ous = dc.OrganizationalUnits;
                        users = dc.Users;
                        noSyncDeps = new List<string>();
                        var rootOU = ous.SingleOrDefault(ou => ou.Name == this._nameOfRootOU);
                        GetChildUser(rootOU);

                        var rootUsers = users.Where(u => u.ParentGuid == rootOU.Guid);
                        if (rootUsers != null && rootUsers.Count() > 0)
                        {
                            foreach (var curUser in rootUsers)
                            {
                                var eiTemp = db.EmpInfo.SingleOrDefault(u => u.Account == this._domainAbbreviate + "\\" + curUser.LogonName);
                                #region
                                if (eiTemp == null) //表示员工中不存在该员工才添加
                                {
                                    EmpInfo ei = new EmpInfo();
                                    ei.Account = this._domainAbbreviate + "\\" + curUser.LogonName;
                                    ei.EmpName = curUser.DisplayName;
                                    var diTemp = db.DepInfo.SingleOrDefault(u => u.DepName == rootOU.Name);
                                    if (diTemp != null) //表示部门表中存在该部门才添加
                                    {
                                        ei.DepID = diTemp.DepID;
                                        db.EmpInfo.InsertOnSubmit(ei);
                                        db.SubmitChanges();

                                        //WSSRightHelper rightHelper = new WSSRightHelper();
                                        //SPWeb rWeb = SPContext.Current.Web;
                                        //string addLoginName = this._domainAbbreviate + "\\" + curUser.LogonName;
                                        //if (rightHelper.IsExistGroup(rWeb, this._mmsRightGroup))
                                        //{
                                        //    if (!rightHelper.IsExistUser(rWeb, addLoginName, this._mmsRightGroup))
                                        //    {
                                        //        rightHelper.AddUserToGroup(rWeb, addLoginName, this._mmsRightGroup, curUser.Mail, curUser.DisplayName, curUser.Note);
                                        //    }
                                        //}

                                    }
                                    else
                                    {
                                        noSyncDeps.Add(rootOU.Name);//记录未同步到系统中的部门
                                    }
                                }
                                #endregion

                                /*---------------------Begin 为了避免员工表中存在该员工时不能通步AD账户到Sharepoint权限里面-------------*/
                                WSSRightHelper rightHelper = new WSSRightHelper();
                                SPWeb rWeb = SPContext.Current.Web;
                                string addLoginName = this._domainAbbreviate + "\\" + curUser.LogonName;
                                if (rightHelper.IsExistGroup(rWeb, this._mmsRightGroup))
                                {
                                    if (!rightHelper.IsExistUser(rWeb, addLoginName, this._mmsRightGroup))
                                    {
                                        rightHelper.AddUserToGroup(rWeb, addLoginName, this._mmsRightGroup, curUser.Mail, curUser.DisplayName, curUser.Note);
                                    }
                                }
                                /*---------------------End 为了避免员工表中存在该员工时不能通步AD账户到Sharepoint权限里面-------------*/
                            }
                        }
                    }
                    else
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('您没有权利同步AD账户!')</script>");
                        return;
                    }
                }
            }
            string arrNoSyncDeps = string.Empty;
            noSyncDeps = noSyncDeps.Distinct().ToList();
            foreach (var no in noSyncDeps)
            {
                arrNoSyncDeps += no + ";";
            }
            if (arrNoSyncDeps.Length != 0)
            {
                arrNoSyncDeps += "等部门";
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('" + arrNoSyncDeps + "没有从AD同步到系统，请先同步这些部门!');window.location.href='EmpManager.aspx';</script>");
                return;
            }
            Response.Redirect("EmpManager.aspx");
            
        }
        /// <summary>
        /// 递归获取任意员工
        /// </summary>
        /// <param name="parentOU">父OU</param>
        protected void GetChildUser(DirectoryOrganizationalUnit parentOU)
        {
            //string domainName = ConfigurationManager.AppSettings["mmsDomainName"].ToString();
            //string adminOfDC = ConfigurationManager.AppSettings["mmsAdminOfDC"].ToString();
            //string pwdOfDC = ConfigurationManager.AppSettings["mmsPwdOfDC"].ToString();
            //string nameOfRootOU = ConfigurationManager.AppSettings["mmsNameOfRootOU"].ToString();
            //string domainAbbreviate = ConfigurationManager.AppSettings["mmsDomainAbbreviate"].ToString();
            var childOUs = ous.Where(u => u.ParentGuid == parentOU.Guid);
            EmpInfo ei;
            if (childOUs != null && childOUs.Count() > 0)
            {
                foreach (var curChildOU in childOUs)//遍历当前OU下的所有OU
                {
                    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        var curChildUsers = users.Where(u => u.ParentGuid == curChildOU.Guid);
                        if (curChildUsers != null && curChildUsers.Count() > 0)
                        {
                            using (DirectoryContext dc = new DirectoryContext(this._domainName, this._adminOfDC, this._pwdOfDC))
                            {
                                foreach (var curUser in curChildUsers)//遍历curChildOU下的所有用户
                                {
                                    var eiTemp = db.EmpInfo.SingleOrDefault(u => u.Account == this._domainAbbreviate + "\\" + curUser.LogonName);
                                    if (eiTemp == null) //表示员工表中不存在该员工才添加
                                    {
                                        ei = new EmpInfo();
                                        ei.Account =this._domainAbbreviate + "\\" + curUser.LogonName;
                                        ei.EmpName = curUser.DisplayName;
                                        var diTemp = db.DepInfo.SingleOrDefault(u => u.DepName == curChildOU.Name);//根据组织单元的名称找到相应的部门
                                        if (diTemp != null) //表示部门表中存在该部门才添加
                                        {
                                            ei.DepID = diTemp.DepID; //把员工关联到相应部门
                                            db.EmpInfo.InsertOnSubmit(ei);
                                            db.SubmitChanges();

                                            //WSSRightHelper rightHelper = new WSSRightHelper();
                                            //SPWeb rWeb = SPContext.Current.Web;
                                            //string addLoginName = this._domainAbbreviate + "\\" + curUser.LogonName;
                                            //if (rightHelper.IsExistGroup(rWeb, this._mmsRightGroup))
                                            //{
                                            //    if (!rightHelper.IsExistUser(rWeb, addLoginName, this._mmsRightGroup))
                                            //    {
                                            //        rightHelper.AddUserToGroup(rWeb, addLoginName, this._mmsRightGroup, curUser.Mail, curUser.DisplayName, curUser.Note);
                                            //    }
                                            //}

                                        }
                                        else
                                        {
                                            noSyncDeps.Add(curChildOU.Name);//记录未同步到系统中的部门
                                        }
                                    }

                                    /*---------------------Begin 为了避免员工表中存在该员工时不能通步AD账户到Sharepoint权限里面-------------*/
                                    WSSRightHelper rightHelper = new WSSRightHelper();
                                    SPWeb rWeb = SPContext.Current.Web;
                                    string addLoginName = this._domainAbbreviate + "\\" + curUser.LogonName;
                                    if (rightHelper.IsExistGroup(rWeb, this._mmsRightGroup))
                                    {
                                        if (!rightHelper.IsExistUser(rWeb, addLoginName, this._mmsRightGroup))
                                        {
                                            rightHelper.AddUserToGroup(rWeb, addLoginName, this._mmsRightGroup, curUser.Mail, curUser.DisplayName, curUser.Note);
                                        }
                                    }
                                    /*---------------------End 为了避免员工表中存在该员工时不能通步AD账户到Sharepoint权限里面-------------*/
                                }
                            }
                        }
                        GetChildUser(curChildOU); //递归遍历curChildOU下的所有OU(部门)，从而递归遍历所有用户(员工)
                    }
                }
            }
        }
        void btnRefresh_Click(object sender, EventArgs e)
        {
            Response.Redirect("EmpManager.aspx");

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
                EmpInfo di;
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    foreach (var li in listString)
                    {
                        di = db.EmpInfo.SingleOrDefault(a => a.EmpID == int.Parse(li.ToolTip));
                        if (di != null)
                        {
                            db.EmpInfo.DeleteOnSubmit(di);

                        }
                    }
                    db.SubmitChanges();
                }
                Response.Redirect("EmpManager.aspx");
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
                Response.Redirect("EmpEditer.aspx?EmpID=" + listString[0].ToolTip);
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择一条且仅能有一条记录进行编辑!')</script>");
            }
        }

        protected void tbarbtnAdd_Click(object sender, EventArgs e)
        {
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");
            Response.Redirect("EmpCreater.aspx");
        }

        /// <summary>
        /// 绑定Gridview
        /// </summary>
        private void BindGridView()
        {
            this.spgviewEmploy.Columns.Clear();//清空列，避免重复添加行
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                BoundField bfColumn;
                //添加选择列
                TemplateField tfieldCheckbox = new TemplateField();
                tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "EmpID");
                tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
                this.spgviewEmploy.Columns.Add(tfieldCheckbox);
                foreach (var kvp in Tlist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.spgviewEmploy.Columns.Add(bfColumn);
                }
                this.spgviewEmploy.DataSource = from a in db.EmpInfo
                                        join b in db.DepInfo on a.DepID equals b.DepID
                                        select new
                                        {
                                            a.EmpID,
                                            a.Account,
                                            a.EmpName,
                                            b.DepName,
                                            a.Contact,
                                            a.Remark
                                        };
                this.spgviewEmploy.DataBind();
                Panel p1 = (Panel)GetControltByMaster("Panel1");
                p1.Controls.Add(this.spgviewEmploy);

            }
      
        }
        /// <summary>
        /// 返回选中的列表
        /// </summary>
        /// <returns>返回list构成的列表</returns>
        private List<CheckBox> GetCheckedID()
        {
            List<CheckBox> list = new List<CheckBox>();

            foreach (GridViewRow row in this.spgviewEmploy.Rows)
            {

                CheckBox ck = (CheckBox)row.Cells[0].FindControl("SMItem");
                if (ck.Checked)
                {
                    list.Add(ck);
                }
            }
            return list;
        }

    }
}
