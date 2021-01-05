using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
namespace MMSPro.WebApp
{
    public class MatCreater : System.Web.UI.Page
    {
        //TextBox txtSupplierID;
        TextBox txtMaterialName;
        TextBox txtMaterialCode;
        DropDownList ddlMaterialType;
        TextBox txtMeasuringUnit;
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
            //this.txtSupplierID = (TextBox)GetControltByMaster("txtSupplierID");
            this.txtMaterialName = (TextBox)GetControltByMaster("txtMaterialName");
            this.txtMaterialCode = (TextBox)GetControltByMaster("txtMaterialCode");
            this.txtMeasuringUnit = (TextBox)GetControltByMaster("txtMeasuringUnit");        
            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
            this.ddlMaterialType = (DropDownList)GetControltByMaster("ddlMaterialType");

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
                var temp = from a in db.MaterialChildType
                           select new
                           {
                               Key = a.MaterialMainType.MaterialMainTypeCode+a.MaterialChildTypeCode+"|"+a.MaterialMainType.MaterialMainTypeName+"-"+a.MaterialChildTypeName,
                               Value = a.MaterialChildTypeID
                           };
        
                this.ddlMaterialType.DataSource = temp;
                this.ddlMaterialType.DataTextField = "Key";
                this.ddlMaterialType.DataValueField = "Value";               
                this.ddlMaterialType.DataBind();
                this.ddlMaterialType.Items.Insert(0, "--请选择--");
            }
        }

        void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("MatManager.aspx");
        }

        void btnSave_Click(object sender, EventArgs e)
        {

            if (!string.IsNullOrEmpty(this.txtMaterialCode.Text) && !string.IsNullOrEmpty(this.txtMaterialName.Text))
           {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))                
                {
                   //检查唯一性
                    var temp = db.MaterialInfo.SingleOrDefault(a => a.FinanceCode == this.txtMaterialCode.Text.Trim());
                    if (temp == null)
                    {
                        MaterialInfo ei = new MaterialInfo();
                     
                        ei.FinanceCode = this.txtMaterialCode.Text.Trim();
                        ei.MaterialName = this.txtMaterialName.Text.Trim();
                
                        if (this.ddlMaterialType.SelectedIndex  == 0)
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择所属物料中类类别！')</script>");
                            return;
                        }
                        ei.MaterialchildTypeID = int.Parse(this.ddlMaterialType.SelectedValue);                                                
                        ei.Remark = this.txtRemark.Text.Trim();
                        ei.SpecificationModel = this.txtMeasuringUnit.Text.Trim();
                        db.MaterialInfo.InsertOnSubmit(ei);
                        db.SubmitChanges();
                        Response.Redirect("MatManager.aspx");
                    }
                    else
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('小类编码已存在')</script>");
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

