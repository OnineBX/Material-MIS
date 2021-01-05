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
    public class DepCreater : System.Web.UI.Page
    {
        TextBox txtDepName;
        TextBox txtDepCode;
        TextBox txtDepInCharge;
        TextBox txtDepContact;
        TextBox txtRemark;
        Button btnSave;
        Button btnQuit;
        protected void Page_Load(object sender, EventArgs e)
        {
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello ')</script>");
            InvtControl();
        }

        private void InvtControl()
        {
            this.txtDepName = (TextBox)GetControltByMaster("txtDepName");
            this.txtDepCode = (TextBox)GetControltByMaster("txtDepCode");
            this.txtDepInCharge = (TextBox)GetControltByMaster("txtDepInCharge");
            this.txtDepContact = (TextBox)GetControltByMaster("txtDepContact");
            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ClearTbox", "<script>");
        }

        void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("DepManager.aspx");
        }

        void btnSave_Click(object sender, EventArgs e)
        {
            
            if (!string.IsNullOrEmpty(this.txtDepName.Text) && !string.IsNullOrEmpty(this.txtDepCode.Text))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
             //   using (MMSProDBDataContext db = new MMSProDBDataContext(@"Data Source=192.168.1.199;Initial Catalog=MMS;User ID=sa;Password=111qqq..." providerName="System.Data.SqlClient"))
                {
                    var temp = db.DepInfo.SingleOrDefault(a => a.DepCode == this.txtDepCode.Text.Trim());
                    if (temp == null)
                    {

                        DepInfo di = new DepInfo();
                        di.DepName = this.txtDepName.Text.Trim();
                        di.DepCode = this.txtDepCode.Text.Trim();
                        di.Contact = this.txtDepContact.Text.Trim();
                        di.InCharge = this.txtDepInCharge.Text.Trim();
                        di.Remark = this.txtRemark.Text.Trim();
                        db.DepInfo.InsertOnSubmit(di);
                        db.SubmitChanges();
                        Response.Redirect("DepManager.aspx");
                    }
                    else
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('部门编码重复请更改')</script>");
                    }
                }
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
