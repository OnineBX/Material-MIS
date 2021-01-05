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
    public class ProjectCreater : System.Web.UI.Page
    {
        MMSProDBDataContext db;
        TextBox txtProjectName;
        TextBox txtProjectCode;
        TextBox txtProjectProperty;
        DropDownList ddlOwner;
        TextBox txtRemark;
        Button btnSave;
        Button btnQuit;
        protected void Page_Load(object sender, EventArgs e)
        {
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello ')</script>");
            InvtControl();
            if (!IsPostBack)
            {
                BindOwner();
            }
        }

        private void InvtControl()
        {
            this.txtProjectName = (TextBox)GetControltByMaster("txtProjectName");
            this.txtProjectCode = (TextBox)GetControltByMaster("txtProjectCode");
            this.txtProjectProperty = (TextBox)GetControltByMaster("txtProjectProperty");
            this.ddlOwner = (DropDownList)GetControltByMaster("ddlOwner"); 
            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
        }
        /// <summary>
        /// 绑定业主单位下面的所有业主
        /// </summary>
        private void BindOwner()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = from a in db.BusinessUnitInfo
                           where a.BusinessUnitType.BusinessUnitTypeName =="业主单位"
                           select new
                           {
                               Key = a.BusinessUnitName,
                               Value = a.BusinessUnitID
                           };

                this.ddlOwner.DataSource = temp;
                this.ddlOwner.DataTextField = "Key";
                this.ddlOwner.DataValueField = "Value";
                this.ddlOwner.DataBind();
                this.ddlOwner.Items.Insert(0, "--请选择--");
            }
        }

        void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("ProjectManager.aspx");
        }

        void btnSave_Click(object sender, EventArgs e)
        {

            if (!string.IsNullOrEmpty(this.txtProjectName.Text) && !string.IsNullOrEmpty(this.txtProjectCode.Text))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))                
                {
                   //检查唯一性
                    var temp = db.ProjectInfo.SingleOrDefault(a => a.ProjectCode == this.txtProjectCode.Text.Trim());
                    if (temp == null)
                    {
                        ProjectInfo ei = new ProjectInfo();
                        ei.ProjectName = this.txtProjectName.Text.Trim();    
                        ei.ProjectCode = this.txtProjectCode.Text.Trim();
                        ei.ProjectProperty = this.txtProjectProperty.Text.Trim();
                        ei.Owner =Convert.ToInt32( this.ddlOwner.SelectedValue);
                        ei.Remark = this.txtRemark.Text.Trim();
                        db.ProjectInfo.InsertOnSubmit(ei);
                        db.SubmitChanges();
                        Response.Redirect("ProjectManager.aspx");
                    }
                    else
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('项目编码已存在')</script>");
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

