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
    public class IndexTop : System.Web.UI.Page
    {
        private static LogHelper _log = LogHelper.GetInstance();

        protected void Page_Load(object sender, EventArgs e)
        {
            Label lblLoginName = this.FindControl("lblLoginName") as Label;
            lblLoginName.Text += SPContext.Current.Web.CurrentUser.Name;
        }
    }
}
