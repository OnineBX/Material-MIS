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
    public class ReceivingTypeCreate:System.Web.UI.Page
    {
        MMSProDBDataContext db;
        TextBox txtName;
        TextBox txtCode;
        Button btnSave;
        Button btnQuit;
        protected void Page_Load(object sender, EventArgs e)
        {
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello ')</script>");
            InitControl();
            if (!IsPostBack)
            {

            }
        }
        private void InitControl()
        {
            this.txtName = (TextBox)GetControltByMaster("txtName");
            this.txtCode = (TextBox)GetControltByMaster("txtCode");

            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
        }

        public void btnSave_Click(object sender, EventArgs e)
        {

            if (!string.IsNullOrEmpty(this.txtName.Text) && !string.IsNullOrEmpty(this.txtCode.Text))
            {
                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {



                    ReceivingTypeInfo RTI = new ReceivingTypeInfo();
                    RTI.ReceivingTypeName = this.txtName.Text.Trim();
                    ReceivingTypeInfo code = db.ReceivingTypeInfo.SingleOrDefault(u => u.ReceivingTypeCode == this.txtCode.Text.Trim());
                    if (code != null)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('入库类型编码重复！')</script>");
                        return;
                    }
                    RTI.ReceivingTypeCode = this.txtCode.Text.Trim();

                    db.ReceivingTypeInfo.InsertOnSubmit(RTI);
                    db.SubmitChanges();
                    Response.Redirect("ReceivingTypeManage.aspx");


                }
            }
        }
        public void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("ReceivingTypeManage.aspx");
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
