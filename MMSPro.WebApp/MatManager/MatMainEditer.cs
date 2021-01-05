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
    public class MatMainEditer : System.Web.UI.Page
    {
        TextBox txtMatMainName;
        TextBox txtMatMainCode;
        DropDownList ddlMatMain;
        Button btnSave;
        Button btnQuit;
        protected void Page_Load(object sender, EventArgs e)
        {
            //  ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");

            InvtControl();
            if (!string.IsNullOrEmpty(Request.QueryString["MaterialMainTypeID"]))
            {
                if (!IsPostBack)
                {                    
                    BindData();
                    BindDDL();
                }
            }
        }

        private void BindData()
        {
            int intID = 0;
            if (int.TryParse(Request.QueryString["MaterialMainTypeID"], out intID))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    MaterialMainType di = db.MaterialMainType.SingleOrDefault(a => a.MaterialMainTypeID == intID);
                    if (di != null)
                    {
                        this.txtMatMainName.Text = di.MaterialMainTypeName;
                        this.txtMatMainCode.Text = di.MaterialMainTypeCode;
                        this.ddlMatMain.SelectedValue = di.MaterialTypeID.ToString();
                      // this.txtRemark.Text = di.Remark;                      
                    }
                }
            }
            else
            {
                Response.Redirect("MatMainManager.aspx");
            }

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
        private void InvtControl()
        {
            this.txtMatMainName = (TextBox)GetControltByMaster("txtMatMainName");
            this.txtMatMainCode = (TextBox)GetControltByMaster("txtMatMainCode");
            this.ddlMatMain = (DropDownList)GetControltByMaster("ddlMatMain");
           // this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);      
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ClearTbox", "<script>");
        }
        void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("MatMainManager.aspx");
        }
        void btnSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtMatMainName.Text) && !string.IsNullOrEmpty(this.txtMatMainCode.Text))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    int intID = 0;
                    if (int.TryParse(Request.QueryString["MaterialMainTypeID"], out intID))
                    {
                        var temp = db.MaterialMainType.SingleOrDefault(a => a.MaterialMainTypeCode == this.txtMatMainCode.Text.Trim());
                        if (temp == null)
                        {
                            InsertRow();
                        }
                            
                        else 
                        {
                            if (temp.MaterialMainTypeID == int.Parse(Request.QueryString["MaterialMainTypeID"]))
                            {
                                InsertRow();
                                
                            }
                            
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('大类编码已存在请更改!')</script>");
                        }
                    }
                }
            }
        }
        void InsertRow()
        {
            
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                MaterialMainType ei = db.MaterialMainType.SingleOrDefault(a => a.MaterialMainTypeID == int.Parse(Request.QueryString["MaterialMainTypeID"]));
                if (ei != null)
                {
                    ei.MaterialMainTypeName = this.txtMatMainName.Text.Trim();                   
                    ei.MaterialMainTypeCode = this.txtMatMainCode.Text.Trim();
                    if (this.ddlMatMain.SelectedIndex == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择所属物料主类！')</script>");
                        return;
                    }
                    ei.MaterialTypeID = int.Parse(this.ddlMatMain.SelectedValue);         
                    //ei.Remark = this.txtRemark.Text.Trim();
                    db.SubmitChanges();
                    Response.Redirect("MatMainManager.aspx");
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
