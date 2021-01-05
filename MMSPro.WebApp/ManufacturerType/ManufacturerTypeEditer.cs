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
    public class ManufacturerTypeEditer : System.Web.UI.Page
    {
        TextBox txtSupTypeName;
        TextBox txtSupTypeCode;      
        TextBox txtRemark;
        Button btnSave;
        Button btnQuit;
        protected void Page_Load(object sender, EventArgs e)
        {
            //  ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");

            InvtControl();
            if (!string.IsNullOrEmpty(Request.QueryString["ManufacturerTypeID"]))
            {
                if (!IsPostBack)
                {                    
                    BindData();
                }
            }
        }

        private void BindData()
        {
            int intID = 0;
            if (int.TryParse(Request.QueryString["ManufacturerTypeID"], out intID))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    ManufacturerType di = db.ManufacturerType.SingleOrDefault(a => a.ManufacturerTypeID == intID);
                    if (di != null)
                    {
                        this.txtSupTypeName.Text = di.ManufacturerTypeName;
                        this.txtSupTypeCode.Text = di.ManufacturerTypeCode;       
                       this.txtRemark.Text = di.Remark;                      
                    }
                }
            }
            else
            {
                Response.Redirect("ManufacturerTypeManager.aspx");
            }

        }
     
        private void InvtControl()
        {
            this.txtSupTypeName = (TextBox)GetControltByMaster("txtSupTypeName");
            this.txtSupTypeCode = (TextBox)GetControltByMaster("txtSupTypeCode");           
            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);      
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ClearTbox", "<script>");
        }
        void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("ManufacturerTypeManager.aspx");
        }
        void btnSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtSupTypeName.Text) && !string.IsNullOrEmpty(this.txtSupTypeCode.Text))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    int intID = 0;
                    if (int.TryParse(Request.QueryString["ManufacturerTypeID"], out intID))
                    {
                        var temp = db.ManufacturerType.SingleOrDefault(a => a.ManufacturerTypeCode == this.txtSupTypeCode.Text.Trim());
                        if (temp == null)
                        {
                            InsertRow();
                        }
                            
                        else 
                        {
                            if (temp.ManufacturerTypeID == int.Parse(Request.QueryString["ManufacturerTypeID"]))
                            {
                                InsertRow();
                                
                            }
                            
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('类别编码已存在请更改!')</script>");
                        }
                    }
                }
            }
        }
        void InsertRow()
        {
            
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                ManufacturerType ei = db.ManufacturerType.SingleOrDefault(a => a.ManufacturerTypeID == int.Parse(Request.QueryString["ManufacturerTypeID"]));
                if (ei != null)
                {
                    ei.ManufacturerTypeName = this.txtSupTypeName.Text.Trim();
                    ei.ManufacturerTypeCode = this.txtSupTypeCode.Text.Trim();
                    ei.Remark = this.txtRemark.Text.Trim();
                    db.SubmitChanges();
                    Response.Redirect("ManufacturerTypeManager.aspx");
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
