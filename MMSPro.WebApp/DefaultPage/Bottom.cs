using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace MMSPro.WebApp
{
    public class Bottom : System.Web.UI.Page
    {
        Label lblCurUserName;

        protected void Page_Load(object sender, EventArgs e)
        {
            //lblCurUserName = (Label)Master.FindControl("PlaceHolderMain").FindControl("txtUserName");
            lblCurUserName = (Label)this.FindControl("lblCurUserName");
            lblCurUserName.Text = SPContext.Current.Web.CurrentUser.Name;
        }
    }
}
