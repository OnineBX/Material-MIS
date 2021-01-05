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
    public class MatMainCreater : System.Web.UI.Page
    {
        TextBox txtMatMainName;
        TextBox txtMatMainCode;
        DropDownList ddlMatMain;
        //TextBox txtRemark;
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
            this.txtMatMainName = (TextBox)GetControltByMaster("txtMatMainName");
            this.txtMatMainCode = (TextBox)GetControltByMaster("txtMatMainCode");        
           // this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
            this.ddlMatMain = (DropDownList)GetControltByMaster("ddlMatMain");
            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
        }
    

        void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("MatMainManager.aspx");
        }
        private void BindDDL()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = from a in db.MaterialType
                           select new
                           {
                               Key = a.MaterialTypeCode + "|" + a.MaterialTypeName,
                               Value = a.MaterialTypeID
                           };

                this.ddlMatMain.DataSource = temp;
                this.ddlMatMain.DataTextField = "Key";
                this.ddlMatMain.DataValueField = "Value";
                this.ddlMatMain.DataBind();
                this.ddlMatMain.Items.Insert(0, "--请选择--");
            }
        }

        void btnSave_Click(object sender, EventArgs e)
        {

            if (!string.IsNullOrEmpty(this.txtMatMainName.Text) && !string.IsNullOrEmpty(this.txtMatMainCode.Text))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))                
                {
                   //检查唯一性
                    var temp = db.MaterialMainType.SingleOrDefault(a => a.MaterialMainTypeCode == this.txtMatMainCode.Text.Trim());
                    if (temp == null)
                    {
                        MaterialMainType ei = new MaterialMainType();
                        ei.MaterialMainTypeName = this.txtMatMainName.Text.Trim();    
                        ei.MaterialMainTypeCode = this.txtMatMainCode.Text.Trim();
                        if (this.ddlMatMain.SelectedIndex == 0)
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择所属主类！')</script>");
                            return;
                        }
                        ei.MaterialTypeID = int.Parse(this.ddlMatMain.SelectedValue); 
                      //  ei.Remark = this.txtRemark.Text.Trim();
                        db.MaterialMainType.InsertOnSubmit(ei);
                        db.SubmitChanges();
                        Response.Redirect("MatMainManager.aspx");
                    }
                    else
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('大类编码已存在')</script>");
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

