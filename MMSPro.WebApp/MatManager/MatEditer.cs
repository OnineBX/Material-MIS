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
    public class MatEditer : System.Web.UI.Page
    {
        TextBox txtMaterialName;
        TextBox txtMaterialCode;
        DropDownList ddlMaterialType;
        TextBox txtMeasuringUnit;
        TextBox txtRemark;
        Button btnSave;
        Button btnQuit;
        protected void Page_Load(object sender, EventArgs e)
        {
            //  ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");

            InvtControl();
            if (!string.IsNullOrEmpty(Request.QueryString["MaterialID"]))
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
            if (int.TryParse(Request.QueryString["MaterialID"], out intID))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    MaterialInfo di = db.MaterialInfo.SingleOrDefault(a => a.MaterialID == intID);
                    if (di != null)
                    {
                  
                        this.txtRemark.Text = di.Remark;
                        this.txtMaterialCode.Text = di.FinanceCode;
                        this.txtMaterialName.Text = di.MaterialName;
                        this.txtMeasuringUnit.Text = di.SpecificationModel;

                        this.ddlMaterialType.SelectedValue = di.MaterialchildTypeID.ToString();
                    }
                }
            }
            else
            {
                Response.Redirect("MatManager.aspx");
            }

        }
        private void BindDDL()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = from a in db.MaterialChildType
                           select new
                           {
                               Key = a.MaterialMainType.MaterialMainTypeCode + a.MaterialChildTypeCode + "|" + a.MaterialMainType.MaterialMainTypeName + "-" + a.MaterialChildTypeName,
                               Value = a.MaterialChildTypeID
                           };

                this.ddlMaterialType.DataSource = temp;
                this.ddlMaterialType.DataTextField = "Key";
                this.ddlMaterialType.DataValueField = "Value";
                this.ddlMaterialType.DataBind();
                this.ddlMaterialType.Items.Insert(0, "--请选择--");
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
        void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("MatManager.aspx");
        }
        void btnSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtMaterialName.Text) && !string.IsNullOrEmpty(this.txtMaterialCode.Text))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    int intID = 0;
                    if (int.TryParse(Request.QueryString["MaterialID"], out intID))
                    {
                        var temp = db.MaterialInfo.SingleOrDefault(a => a.FinanceCode == this.txtMaterialCode.Text.Trim());
                        if (temp == null)
                        {
                            InsertRow();
                        }
                            
                        else 
                        {
                            if (temp.MaterialID == int.Parse(Request.QueryString["MaterialID"]))
                            {
                                InsertRow();
                                
                            }
                            
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('小类编码已存在重复请更改')</script>");
                        }
                    }
                }
            }
        }
        void InsertRow()
        {
            
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                MaterialInfo ei = db.MaterialInfo.SingleOrDefault(a => a.MaterialID == int.Parse(Request.QueryString["MaterialID"]));
                if (ei != null)
                {
                    ei.FinanceCode = this.txtMaterialCode.Text.Trim();
                    ei.MaterialName = this.txtMaterialName.Text.Trim();

                    if (this.ddlMaterialType.SelectedIndex == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择所属物料中类类别！')</script>");
                        return;
                    }
                    ei.MaterialchildTypeID = int.Parse(this.ddlMaterialType.SelectedValue);
                    ei.SpecificationModel = this.txtMeasuringUnit.Text.Trim();
                    ei.Remark = this.txtRemark.Text.Trim();
                    db.SubmitChanges();
                    Response.Redirect("MatManager.aspx");
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
