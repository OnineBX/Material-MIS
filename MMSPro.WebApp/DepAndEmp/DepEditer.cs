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
    public class DepEditer : System.Web.UI.Page
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
          //  ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");
                
            InvtControl();
            if (!string.IsNullOrEmpty(Request.QueryString["DepID"]))
            {
                if (!IsPostBack)
                {
                    BindData();
                }
            }
        }

        private void BindData()
        {
            int intID =0;
            if (int.TryParse(Request.QueryString["DepID"], out intID))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    DepInfo di = db.DepInfo.SingleOrDefault(a => a.DepID == intID);
                    if (di != null)
                    {
                        this.txtDepName.Text = di.DepName;
                        this.txtRemark.Text = di.Remark;
                        this.txtDepCode.Text = di.DepCode;
                        this.txtDepContact.Text = di.Contact;
                        this.txtDepInCharge.Text = di.InCharge;
                        
                    }
                }
            }
            else
            {
                Response.Redirect("DepManager.aspx");
            }
        
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
                {
                    int intID = 0;
                    if (int.TryParse(Request.QueryString["DepID"], out intID))
                    {
                          var temp = db.DepInfo.SingleOrDefault(a => a.DepCode == this.txtDepCode.Text.Trim());
                          if (temp == null)
                          {
                              InsertRow();
                          }
                          else
                          {
                              if (temp.DepID == intID)
                              {
                                  InsertRow();
                              }
                              ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('部门编码重复请更改')</script>");
                          }
                    }
                }
            }
        }
        void InsertRow()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                DepInfo di = db.DepInfo.SingleOrDefault(a => a.DepID == int.Parse(Request.QueryString["DepID"]));
                if (di != null)
                {
                    di.DepName = this.txtDepName.Text.Trim();
                    di.DepCode = this.txtDepCode.Text.Trim();
                    di.Contact = this.txtDepContact.Text.Trim();
                    di.InCharge = this.txtDepInCharge.Text.Trim();
                    di.Remark = this.txtRemark.Text.Trim();
                    db.SubmitChanges();
                    Response.Redirect("DepManager.aspx");
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
