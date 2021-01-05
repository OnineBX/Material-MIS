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
    public class MatTypeEditer : System.Web.UI.Page
    {
        TextBox txtMatChildName;
        TextBox txtMatChildCode;
        //DropDownList ddlMatMain;
        Button btnSave;
        Button btnQuit;
        protected void Page_Load(object sender, EventArgs e)
        {
            //  ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");

            InvtControl();
            if (!string.IsNullOrEmpty(Request.QueryString["MaterialTypeID"]))
            {
                if (!IsPostBack)
                {
                    //BindDDL();
                    BindData();
                }
            }
        }

        private void BindData()
        {
            int intID = 0;
            if (int.TryParse(Request.QueryString["MaterialTypeID"], out intID))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    MaterialType di = db.MaterialType.SingleOrDefault(a => a.MaterialTypeID == intID);
                    if (di != null)
                    {
                     
                        this.txtMatChildCode.Text = di.MaterialTypeCode;
                        this.txtMatChildName.Text = di.MaterialTypeName;
                        //this.ddlMatMain.SelectedValue = di.MaterialMainTypeID.ToString();
                    }
                }
            }
            else
            {
                Response.Redirect("MetTypeManager.aspx");
            }

        }
        //private void BindDDL()
        //{
        //    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
        //    {
        //        var temp = from a in db.MaterialMainType
        //                   select new
        //                   {
        //                       Key = a.MaterialMainTypeCode + "|" + a.MaterialMainTypeName,
        //                       Value = a.MaterialMainTypeID
        //                   };

        //        this.ddlMatMain.DataSource = temp;
        //        this.ddlMatMain.DataTextField = "Key";
        //        this.ddlMatMain.DataValueField = "Value";
        //        this.ddlMatMain.DataBind();
        //        this.ddlMatMain.Items.Insert(0, "--请选择--");
        //    }
        //}
        private void InvtControl()
        {
            //this.txtSupplierID = (TextBox)GetControltByMaster("txtSupplierID");
            this.txtMatChildName = (TextBox)GetControltByMaster("txtMatChildName");
            this.txtMatChildCode = (TextBox)GetControltByMaster("txtMatChildCode");
            //this.ddlMatMain = (DropDownList)GetControltByMaster("ddlMatMain");
            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
        }

        void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("MatTypeManager.aspx");
        }
        void btnSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtMatChildCode.Text) && !string.IsNullOrEmpty(this.txtMatChildName.Text))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    int intID = 0;
                    if (int.TryParse(Request.QueryString["MaterialTypeID"], out intID))
                    {
                        var temp = db.MaterialType.SingleOrDefault(a => a.MaterialTypeCode == this.txtMatChildCode.Text.Trim());
                        if (temp == null)
                        {
                            InsertRow();
                        }
                            
                        else 
                        {
                            if (temp.MaterialTypeID == int.Parse(Request.QueryString["MaterialTypeID"]))
                            {
                                InsertRow();
                                
                            }
                            
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('物料主类编码已存在重复请更改')</script>");
                        }
                    }
                }
            }
        }
        void InsertRow()
        {
            
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                MaterialType ei = db.MaterialType.SingleOrDefault(a => a.MaterialTypeID == int.Parse(Request.QueryString["MaterialTypeID"]));
                if (ei != null)
                {
               
                    ei.MaterialTypeName = this.txtMatChildName.Text.Trim();
                    ei.MaterialTypeCode = this.txtMatChildCode.Text.Trim();
                    //if (this.ddlMatMain.SelectedIndex == 0)
                    //{
                    //    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择所属物料大类！')</script>");
                    //    return;
                    //}
                    //ei.MaterialMainTypeID = int.Parse(this.ddlMatMain.SelectedValue);                  
                    db.SubmitChanges();
                    Response.Redirect("MatTypeManager.aspx");
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
