using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Configuration;
using MMSPro.ADHelper.DirectoryServices;

namespace MMSPro.WebApp
{
    public class SelectUser : System.Web.UI.Page
    {
        #region Fields
        public List<DirectoryOrganizationalUnit> ous;
        public List<DirectoryUser> users;
        public StringBuilder LoginNameCollection;
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    string domainName = ConfigurationManager.AppSettings["mmsDomainName"].ToString();
                    string adminOfDC = ConfigurationManager.AppSettings["mmsAdminOfDC"].ToString();
                    string pwdOfDC = ConfigurationManager.AppSettings["mmsPwdOfDC"].ToString();
                    //using (DirectoryContext dc = new DirectoryContext("LDAP://baibei.com", "administrator", "p@ssw0rd"))
                    using (DirectoryContext dc = new DirectoryContext(domainName, adminOfDC, pwdOfDC))
                    {
                        ous = new List<DirectoryOrganizationalUnit>();
                        users = new List<DirectoryUser>();
                        ous = dc.OrganizationalUnits;
                        users = dc.Users;
                        BindTree();
                    }
                }
                catch (Exception ex)
                {
                    Response.Write(ex.ToString());
                }
            }
            Button btnReturnUser = (Button)this.FindControl("btnReturnUser");
            btnReturnUser.Click += new EventHandler(btnReturnUser_Click);
        }

        protected void btnReturnUser_Click(object sender, EventArgs e)
        {
            try
            {
                TreeView TreeView1 = (TreeView)this.FindControl("TreeView1");
                if (TreeView1.SelectedNode.Value.Trim() == "-1")
                {
                    Page.RegisterStartupScript("error", "<script>alert('不能选择组')</" + "script>");
                }
                else
                {
                    //Response.Write(TreeView1.SelectedNode.Value.Trim());
                    Page.RegisterStartupScript("success", "<script>window.returnValue='" + TreeView1.SelectedNode.Value.Trim() + "';window.close()</" + "script>");
                }
            }
            catch
            {
                Page.RegisterStartupScript("success", "<script>window.returnValue='" + "" + "';window.close()</" + "script>");
            }
        }

        public void BindTree()
        {
            //var topnode = ous.SingleOrDefault(u => u.Name == "百倍");
            string nameOfRootOU = ConfigurationManager.AppSettings["mmsNameOfRootOU"].ToString(); //读取配置文件
            var topnode = ous.SingleOrDefault(u => u.Name == nameOfRootOU);
            TreeNode RootNode = new TreeNode(topnode.Name, "-1");
            RootNode.ImageUrl = "../../images/AD/top.png";

            BindChildNodes(topnode.Guid, RootNode);

            var topusers = users.Where(uu => uu.ParentGuid == topnode.Guid);
            if (topusers != null && topusers.Count() > 0)
            {
                foreach (var tuser in topusers)
                {
                    RootNode.ChildNodes.Add(new TreeNode(tuser.Name, tuser.LogonName, "../../images/AD/user.png"));
                }
            }
            TreeView TreeView1 = (TreeView)this.FindControl("TreeView1");
            TreeView1.Nodes.Add(RootNode);
            //TreeView1.Attributes.Add("onClick", "OnCheckEvent()");
        }

        public void BindChildNodes(Guid parentNodeGuid, TreeNode parentNode)
        {
            var childnodes = ous.Where(u => u.ParentGuid == parentNodeGuid);
            if (childnodes != null && childnodes.Count() > 0)
            {
                foreach (var node in childnodes)
                {
                    TreeNode tn = new TreeNode(node.Name, "-1", "../../images/AD/ou.png");
                    var chidusers = users.Where(uu => uu.ParentGuid == node.Guid);
                    if (chidusers != null && chidusers.Count() > 0)
                    {
                        foreach (var cuser in chidusers)
                        {
                            tn.ChildNodes.Add(new TreeNode(cuser.Name, cuser.LogonName, "../../images/AD/user.png"));
                        }
                    }
                    parentNode.ChildNodes.Add(tn);
                    BindChildNodes(node.Guid, tn);
                }
            }
        }
    }
}
