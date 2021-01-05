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
    public class ManufacturerEditer : System.Web.UI.Page
    {
        TextBox txtSupplierName;
        TextBox txtSupplierCode;
        DropDownList ddlSupplierType;
        TextBox txtSupplierAddress1;
        TextBox txtSupplierAddress2;
        TextBox txtSupplierPhone;
        TextBox txtInCharge;
        TextBox txtRemark;
        Button btnSave;
        Button btnQuit;
        protected void Page_Load(object sender, EventArgs e)
        {
            //  ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");

            InvtControl();
            if (!string.IsNullOrEmpty(Request.QueryString["ManufacturerID"]))
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
            if (int.TryParse(Request.QueryString["ManufacturerID"], out intID))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    Manufacturer di = db.Manufacturer.SingleOrDefault(a => a.ManufacturerID == intID);
                    if (di != null)
                    {
                        this.txtInCharge.Text = di.principal;
                        this.txtRemark.Text = di.Remark;
                        this.txtSupplierAddress1.Text = di.ManufacturerAddress1;
                        this.txtSupplierAddress2.Text = di.ManufacturerAddress2;
                        this.txtSupplierCode.Text = di.ManufacturerCode;
                        this.txtSupplierName.Text = di.ManufacturerName;
                        this.txtSupplierPhone.Text = di.ManufacturerPhone;
                        this.ddlSupplierType.SelectedValue = di.ManufacturerTypeID.ToString();
                    }
                }
            }
            else
            {
                Response.Redirect("ManufacturerManager.aspx");
            }

        }
        private void BindDDL()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = from a in db.ManufacturerType
                           select new
                           {
                               Key = a.ManufacturerTypeName,
                               Value = a.ManufacturerTypeID
                           };

                this.ddlSupplierType.DataSource = temp;
                this.ddlSupplierType.DataTextField = "Key";
                this.ddlSupplierType.DataValueField = "Value";
                this.ddlSupplierType.DataBind();
                this.ddlSupplierType.Items.Insert(0, "--请选择--");
            }
        }
        private void InvtControl()
        {
            this.txtSupplierName = (TextBox)GetControltByMaster("txtSupplierName");
            this.txtSupplierCode = (TextBox)GetControltByMaster("txtSupplierCode");
            this.txtSupplierAddress1 = (TextBox)GetControltByMaster("txtSupplierAddress1");
            this.txtSupplierAddress2 = (TextBox)GetControltByMaster("txtSupplierAddress2");
            this.txtSupplierPhone = (TextBox)GetControltByMaster("txtSupplierPhone");
            this.txtInCharge = (TextBox)GetControltByMaster("txtInCharge");
            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
            this.ddlSupplierType = (DropDownList)GetControltByMaster("ddlSupplierType");


            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
        }
        void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("ManufacturerManager.aspx");
        }
        void btnSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtSupplierCode.Text) && !string.IsNullOrEmpty(this.txtSupplierName.Text))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    int intID = 0;
                    if (int.TryParse(Request.QueryString["ManufacturerID"], out intID))
                    {
                        var temp = db.Manufacturer.SingleOrDefault(a => a.ManufacturerCode == this.txtSupplierCode.Text.Trim());
                        if (temp == null)
                        {
                            InsertRow();
                        }
                            
                        else 
                        {
                            if (temp.ManufacturerID == int.Parse(Request.QueryString["ManufacturerID"]))
                            {
                                InsertRow();
                                
                            }
                            
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('供应商编码已存在重复请更改')</script>");
                        }
                    }
                }
            }
        }
        void InsertRow()
        {
            
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                Manufacturer ei = db.Manufacturer.SingleOrDefault(a => a.ManufacturerID == int.Parse(Request.QueryString["ManufacturerID"]));
                if (ei != null)
                {
                    ei.principal = this.txtInCharge.Text.Trim();
                    ei.ManufacturerAddress1 = this.txtSupplierAddress1.Text.Trim();
                    ei.ManufacturerAddress2 = this.txtSupplierAddress2.Text.Trim();
                    ei.ManufacturerCode = this.txtSupplierCode.Text.Trim();
                    ei.ManufacturerName = this.txtSupplierName.Text.Trim();
                    ei.ManufacturerPhone = this.txtSupplierPhone.Text.Trim();
                    if (this.ddlSupplierType.SelectedIndex == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择所属供货所类别！')</script>");
                        return;
                    }
                    ei.ManufacturerTypeID = int.Parse(this.ddlSupplierType.SelectedValue);
                    ei.Remark = this.txtRemark.Text.Trim();
                    db.SubmitChanges();
                    Response.Redirect("ManufacturerManager.aspx");
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
