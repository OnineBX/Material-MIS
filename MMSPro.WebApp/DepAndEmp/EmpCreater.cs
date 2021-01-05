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
    public class EmpCreater : System.Web.UI.Page
    {
        TextBox txtAccount;
      //  TextBox txtPassword;
        TextBox txtEmpName;
        DropDownList ddlDepartment;
        TextBox txtContact;
        TextBox txtRemark;
        Button btnSave;
        Button btnQuit;
        protected void Page_Load(object sender, EventArgs e)
        {
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello ')</script>");
            InvtControl();
            if (!IsPostBack)
            {
                BindDDL();
            }
        }

        private void InvtControl()
        {
            this.txtAccount = (TextBox)GetControltByMaster("txtAccount");
            //this.txtPassword = (TextBox)GetControltByMaster("txtPassword");
            this.txtEmpName = (TextBox)GetControltByMaster("txtEmpName");
            this.txtContact = (TextBox)GetControltByMaster("txtContact");
            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
            this.ddlDepartment = (DropDownList)GetControltByMaster("ddlDepartment");


            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
        }

        private void BindDDL()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = from a in db.DepInfo
                           select new
                           {
                               Key = a.DepName,
                               Value = a.DepID
                           };
        
                this.ddlDepartment.DataSource = temp;
                this.ddlDepartment.DataTextField = "Key";
                this.ddlDepartment.DataValueField = "Value";               
                this.ddlDepartment.DataBind();
                this.ddlDepartment.Items.Insert(0, "--请选择--");
            }
        }

        void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("EmpManager.aspx");
        }

        void btnSave_Click(object sender, EventArgs e)
        {

            if (!string.IsNullOrEmpty(this.txtAccount.Text) )
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))                
                {
                   //检查唯一性
                    var temp = db.EmpInfo.SingleOrDefault(a => a.Account == this.txtAccount.Text.Trim());
                    if (temp == null)
                    {
                        EmpInfo ei = new EmpInfo();
                        ei.Account = this.txtAccount.Text.Trim();
                        ei.Contact = this.txtContact.Text.Trim();
                        if (this.ddlDepartment.SelectedIndex  == 0)
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择所属部门！')</script>");
                            return;
                        }
                        ei.DepID = int.Parse(this.ddlDepartment.SelectedValue);                        
                        ei.EmpName = this.txtEmpName.Text.Trim();
                        //ei.Password = this.txtPassword.Text.Trim();
                        ei.Remark = this.txtRemark.Text.Trim();
                        db.EmpInfo.InsertOnSubmit(ei);
                        db.SubmitChanges();
                        Response.Redirect("EmpManager.aspx");
                    }
                    else
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('账号已存在')</script>");
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

