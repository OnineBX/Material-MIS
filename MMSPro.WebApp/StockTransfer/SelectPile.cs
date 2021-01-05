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
    public class SelectPile : System.Web.UI.Page
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
                    //string domainName = ConfigurationManager.AppSettings["mmsDomainName"].ToString();
                    //string adminOfDC = ConfigurationManager.AppSettings["mmsAdminOfDC"].ToString();
                    //string pwdOfDC = ConfigurationManager.AppSettings["mmsPwdOfDC"].ToString();
                    ////using (DirectoryContext dc = new DirectoryContext("LDAP://baibei.com", "administrator", "p@ssw0rd"))
                    //using (DirectoryContext dc = new DirectoryContext(domainName, adminOfDC, pwdOfDC))
                    //{
                    //    ous = new List<DirectoryOrganizationalUnit>();
                    //    users = new List<DirectoryUser>();
                    //    ous = dc.OrganizationalUnits;
                    //    users = dc.Users;
                        BindTree();
                    //}
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
                if (TreeView1.SelectedNode.ImageToolTip == "仓库")
                {
                    Page.RegisterStartupScript("error", "<script>alert('不能选择仓库,请选择垛位')</" + "script>");
                }
                else
                {
                    //Response.Write(TreeView1.SelectedNode.Value.Trim());
                    Page.RegisterStartupScript("success", "<script>window.returnValue='" +TreeView1.SelectedNode.Parent.Text+"|"+ TreeView1.SelectedNode.Text+"|"+ TreeView1.SelectedNode.Value.Trim() + "';window.close()</" + "script>");
                }
            }
            catch
            {
                Page.RegisterStartupScript("success", "<script>window.returnValue='" + "" + "';window.close()</" + "script>");
            }
        }

        public void BindTree()
        {
            TreeView TreeView1 = (TreeView)this.FindControl("TreeView1");
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var listStorageInfo = (from a in db.StorageInfo
                                      select a).ToList();
                TreeNode tnChild;
                foreach (var si in listStorageInfo)
                {
                    tnChild = new TreeNode(si.StorageName, si.StorageID.ToString(), "../../images/AD/kthememgr.png");                    
                    BindChildNodes(tnChild);
                    tnChild.ImageToolTip = "仓库";
                    TreeView1.Nodes.Add(tnChild);
                    
                }
            }

        }

        public void BindChildNodes(TreeNode parentNode)
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var listPile = db.PileInfo.Where(a => a.StorageID == Convert.ToInt32(parentNode.Value)).ToList();
                TreeNode tnChild;
                foreach (var pi in listPile)
                {
                    tnChild = new TreeNode(pi.PileName, pi.PileCode, "../../images/AD/kthememgr.png");
                    tnChild.ImageToolTip = "垛位";
                    parentNode.ChildNodes.Add(tnChild);
                }
                               
                               
            }
        }
    }
}
