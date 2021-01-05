/*------------------------------------------------------------------------------
 * Unit Name：ErrorInfo.cs
 * Description: 处理错误的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-07-08
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

namespace MMSPro.WebApp
{  
    public class ErrorInfo:System.Web.UI.Page
    {
        private Button btnOK, btnClose;
        private string strBackUrl, strDisposeUrl;

        protected void Page_Load(object sender, EventArgs e)
        {
            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);
            btnClose = (Button)GetControltByMaster("btnClose");
            btnClose.Click += new EventHandler(btnClose_Click);

            strBackUrl = Request.QueryString["BackUrl"];
            strDisposeUrl = HttpUtility.UrlDecode(Request.QueryString["DisposeUrl"]);

            if (string.Equals(strBackUrl, strDisposeUrl))
                ((Panel)GetControltByMaster("Panel1")).Controls.Remove(btnOK);

            ((Label)GetControltByMaster("lblErrorInfo")).Text = Request.QueryString["ErrorInfo"];
        }

        void btnClose_Click(object sender, EventArgs e)
        {
            Response.Redirect(strBackUrl);
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            Response.Redirect(strDisposeUrl);
        }



        #region 辅助方法
        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }
        #endregion
    }
}
