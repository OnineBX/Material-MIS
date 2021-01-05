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
    public class ReceivingTypeEdit:System.Web.UI.Page
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
            if (!string.IsNullOrEmpty(Request.QueryString["ReceivingTypeID"]))
            {

                if (!IsPostBack)
                {
                    LoadData();
                }
            }

        }
        private void LoadData()
        {

            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                int id = Convert.ToInt32(Request.QueryString["ReceivingTypeID"]);
                ReceivingTypeInfo RTI = db.ReceivingTypeInfo.SingleOrDefault(a => a.ReceivingTypeID == id);
                if (RTI != null)
                {
                    this.txtName.Text = RTI.ReceivingTypeName.ToString();
                    this.txtCode.Text = RTI.ReceivingTypeCode.ToString();
                }
                else
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('记录不存在! ');</script>");
                    Response.Redirect("ReceivingTypeManage.aspx");
                }
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


                    int id = Convert.ToInt32(Request.QueryString["ReceivingTypeID"]);
                    ReceivingTypeInfo RTI = db.ReceivingTypeInfo.SingleOrDefault(a => a.ReceivingTypeID == id);
                    RTI.ReceivingTypeName = this.txtName.Text.Trim();
                    ReceivingTypeInfo code = db.ReceivingTypeInfo.SingleOrDefault(u => u.ReceivingTypeCode == this.txtCode.Text.Trim());
                    if (code == null)
                    {
                        RTI.ReceivingTypeCode = this.txtCode.Text.Trim();
                    }
                    else
                    {
                        if (RTI.ReceivingTypeID == code.ReceivingTypeID)
                        {
                            RTI.ReceivingTypeCode = this.txtCode.Text.Trim();
                        }
                        else
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('出库类型编码重复！')</script>");
                            return;
                        }
                    }

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
