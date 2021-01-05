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
    public class SelectUsers:Page
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
                StringBuilder sbUsers = new StringBuilder();
                TreeView tvUsers = (TreeView)this.FindControl("tvUsers");                
                foreach (TreeNode tn in tvUsers.Nodes)
                {
                    GetSelectedUsers(tn, sbUsers);
                }

                ClientScript.RegisterStartupScript(typeof(string), "OK", string.Format("<script>window.returnValue='{0}';window.close()</script>", sbUsers.ToString()));
                
            }
            catch
            {
                ClientScript.RegisterStartupScript(typeof(string), "error","<script>window.returnValue='';window.close()</script>");
            }
        }

        private void GetSelectedUsers(TreeNode tnparent,StringBuilder sbUsers)
        {
            string domainName = ConfigurationManager.AppSettings["mmsDomainAbbreviate"].ToString();
            if (tnparent.ChildNodes.Count == 0)
            {
                if(tnparent.Checked)
                    sbUsers.Append(string.Format("{0}\\\\{1};\\r\\n", domainName, tnparent.Value));
            }
            else
            {
                foreach (TreeNode tnchild in tnparent.ChildNodes)
                {
                    GetSelectedUsers(tnchild, sbUsers);
                }
            }
                
        }

        public void BindTree()
        {
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
            TreeView tvUsers = (TreeView)this.FindControl("tvUsers");
            tvUsers.Nodes.Add(RootNode);
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
