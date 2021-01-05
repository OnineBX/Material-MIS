using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.WebControls;
using System.Configuration;
using MMSPro.ADHelper.DirectoryServices;

namespace MMSPro.WebApp
{
    public class ResetADUserPwd : System.Web.UI.Page
    {
        Label lblADUserName;
        TextBox txtOldPwd;
        TextBox txtNewPwd;
        TextBox txtSecondPwd;
        Button btnResetPwd;

        List<DirectoryOrganizationalUnit> ous;
        List<DirectoryUser> users;
        string _domainName;
        string _adminOfDC;
        string _pwdOfDC;
        string _nameOfRootOU;
        string _domainAbbreviate;

        protected void Page_Load(object sender, EventArgs e)
        {
            this.lblADUserName = (Label)GetControltByMaster("lblADUserName");
            this.txtOldPwd = (TextBox)GetControltByMaster("txtOldPwd");
            this.txtNewPwd = (TextBox)GetControltByMaster("txtNewPwd");
            this.txtSecondPwd = (TextBox)GetControltByMaster("txtSecondPwd");
            this.btnResetPwd = (Button)GetControltByMaster("btnResetPwd");
            this.btnResetPwd.Click += new EventHandler(btnResetPwd_Click);

            this.lblADUserName.Text = SPContext.Current.Web.CurrentUser.LoginName;
        }

        void btnResetPwd_Click(object sender, EventArgs e)
        {
            try
            {
                this._domainName = ConfigurationManager.AppSettings["mmsDomainName"].ToString();
                this._adminOfDC = ConfigurationManager.AppSettings["mmsAdminOfDC"].ToString();
                this._pwdOfDC = ConfigurationManager.AppSettings["mmsPwdOfDC"].ToString();
                this._nameOfRootOU = ConfigurationManager.AppSettings["mmsNameOfRootOU"].ToString();
                this._domainAbbreviate = ConfigurationManager.AppSettings["mmsDomainAbbreviate"].ToString();

                using (DirectoryContext dc = new DirectoryContext(this._domainName, this._adminOfDC, this._pwdOfDC))
                {
                    this.users = dc.Users;
                    DirectoryUser curUser = users.SingleOrDefault(u => (this._domainAbbreviate + "\\" + u.LogonName).ToLower() == SPContext.Current.Web.CurrentUser.LoginName.ToLower());
                    curUser.ChangePassword(this.txtOldPwd.Text.Trim(), this.txtNewPwd.Text.Trim());
                    dc.SubmitChanges();
                }
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('修改密码成功!')</script>");
            }
            catch (Exception ex)
            {
                System.Reflection.MethodBase mb = System.Reflection.MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
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
    }
}
