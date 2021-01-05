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
    public class WarningEditer : System.Web.UI.Page
    {
        TextBox txtMaterialName;
        TextBox txtGentaojian;
        TextBox txtMetre;
        TextBox txtTon;
        Button btnSave;
        Button btnQuit;
        protected void Page_Load(object sender, EventArgs e)
        {
            //  ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");

            InvtControl();
            if (!string.IsNullOrEmpty(Request.QueryString["WarningID"]))
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
            if (int.TryParse(Request.QueryString["WarningID"], out intID))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    WarningList a = db.WarningList.SingleOrDefault(c => c.WarningID == intID);
                    if (a != null)
                    {

                        this.txtMaterialName.Text =  a.MaterialInfo.MaterialChildType.MaterialMainType.MaterialMainTypeCode + a.MaterialInfo.MaterialChildType.MaterialChildTypeCode + "|" + a.MaterialInfo.MaterialChildType.MaterialMainType.MaterialMainTypeName + "-" + a.MaterialInfo.MaterialChildType.MaterialChildTypeName;
                        this.txtGentaojian.Text = a.QuantityGentaojian.ToString();
                        this.txtMetre.Text = a.QuantityMetre.ToString();
                        this.txtTon.Text = a.QuantityTon.ToString();

                        //this.ddlMaterialType.SelectedValue = di.MaterialchildTypeID.ToString();
                    }
                }
            }
            else
            {
                Response.Redirect("WarningManager.aspx");
            }

        }
       
        private void InvtControl()
        {
            //this.txtSupplierID = (TextBox)GetControltByMaster("txtSupplierID");
            this.txtMaterialName = (TextBox)GetControltByMaster("txtMaterialName");
            this.txtGentaojian = (TextBox)GetControltByMaster("txtGentaojian");
            this.txtMetre = (TextBox)GetControltByMaster("txtMetre");
            this.txtTon = (TextBox)GetControltByMaster("txtTon");
           
            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
        }
        void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("WarningManager.aspx");
        }
        void btnSave_Click(object sender, EventArgs e)
        {
            //值为0不报警
            decimal a, b, c;
            if (!decimal.TryParse(this.txtGentaojian.Text.Trim(), out a))
            {
                a = 0;
            }
            if (!decimal.TryParse(this.txtMetre.Text.Trim(), out b))
            {
                b = 0;
            }
            if (!decimal.TryParse(this.txtTon.Text.Trim(), out c))
            {
                c = 0;
            }
            if (a == 0 && b == 0 && c == 0)
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('至少填写一项预警值!')</script>");
                return;
            }

            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                WarningList wl = db.WarningList.SingleOrDefault(t => t.WarningID == int.Parse(Request.QueryString["WarningID"]));
                wl.QuantityGentaojian = a;
                wl.QuantityMetre = b;
                wl.QuantityTon = c;
                db.SubmitChanges();
            }

            Response.Redirect("WarningManager.aspx");

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
