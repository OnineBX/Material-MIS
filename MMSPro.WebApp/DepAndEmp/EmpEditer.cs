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
    public class EmpEditer : System.Web.UI.Page
    {
        TextBox txtAccount;
        //TextBox txtPassword;
        //TextBox txtRePassword;
        TextBox txtEmpName;
        DropDownList ddlDepartment;
        TextBox txtContact;
        TextBox txtRemark;
        Button btnSave;
        Button btnQuit;
        protected void Page_Load(object sender, EventArgs e)
        {
            //  ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");

            InvtControl();
            if (!string.IsNullOrEmpty(Request.QueryString["EmpID"]))
            {
                if (!IsPostBack)
                {
                    BindDDL();
                    BindData();
                }
            }
        }

        private void BindData()
        {
            int intID = 0;
            if (int.TryParse(Request.QueryString["EmpID"], out intID))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    EmpInfo di = db.EmpInfo.SingleOrDefault(a => a.EmpID == intID);
                    if (di != null)
                    {
                        this.txtAccount.Text = di.Account;
                       //this. txtPassword.Attributes.Add("Value", di.Password);
                       //this.txtRePassword.Attributes.Add("Value", di.Password);
                       this.txtEmpName.Text = di.EmpName;
                       this.txtContact.Text = di.Contact;
                       this.txtRemark.Text = di.Remark;
                       this.ddlDepartment.SelectedValue = di.DepID.ToString();

                    }
                }
            }
            else
            {
                Response.Redirect("DepManager.aspx");
            }

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
                if (temp == null)
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请先建立所属部门！')</script>");
                    Response.Redirect("DepManager.aspx");
                }
                this.ddlDepartment.DataSource = temp;
                this.ddlDepartment.DataTextField = "Key";
                this.ddlDepartment.DataValueField = "Value";
                this.ddlDepartment.DataBind();
                this.ddlDepartment.Items.Insert(0, "--请选择--");
            }
        }
        private void InvtControl()
        {
            this.txtAccount = (TextBox)GetControltByMaster("txtAccount");
            //this.txtPassword = (TextBox)GetControltByMaster("txtPassword");
            //this.txtRePassword = (TextBox)GetControltByMaster("txtRePassword");
            this.txtEmpName = (TextBox)GetControltByMaster("txtEmpName");
            this.txtContact = (TextBox)GetControltByMaster("txtContact");
            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
            this.ddlDepartment = (DropDownList)GetControltByMaster("ddlDepartment");


            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);      
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ClearTbox", "<script>");
        }
        void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("EmpManager.aspx");
        }
        void btnSave_Click(object sender, EventArgs e)
        {
            //if (!string.IsNullOrEmpty(this.txtAccount.Text) && !string.IsNullOrEmpty(this.txtPassword.Text))
                if (!string.IsNullOrEmpty(this.txtAccount.Text))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    int intID = 0;
                    if (int.TryParse(Request.QueryString["EmpID"], out intID))
                    {
                        var temp = db.EmpInfo.SingleOrDefault(a => a.Account == this.txtAccount.Text.Trim());
                        if (temp == null)
                        {
                            InsertRow();
                        }
                            
                        else 
                        {
                            if (temp.EmpID == int.Parse(Request.QueryString["EmpID"]))
                            {
                                InsertRow();
                                
                            }
                            
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('用户帐号已存在重复请更改')</script>");
                        }
                    }
                }
            }
        }
        void InsertRow()
        {
            
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                EmpInfo ei = db.EmpInfo.SingleOrDefault(a => a.EmpID ==int.Parse(Request.QueryString["EmpID"]));
                if (ei != null)
                {
                    ei.Account = this.txtAccount.Text.Trim();
                    ei.Contact = this.txtContact.Text.Trim();
                    if (this.ddlDepartment.SelectedIndex == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择所属部门！')</script>");
                        return;
                    }
                    ei.DepID = int.Parse(this.ddlDepartment.SelectedValue);
                    ei.EmpName = this.txtEmpName.Text.Trim();
                    //ei.Password = this.txtPassword.Text.Trim();
                    ei.Remark = this.txtRemark.Text.Trim();
                    db.SubmitChanges();
                    Response.Redirect("EmpManager.aspx");
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
