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
    public class BusinessTypeEdit:System.Web.UI.Page
    {
        MMSProDBDataContext db;
        TextBox txtTypeName;
        TextBox txtTypeCode;
        TextBox txtRemark;
        Button btnSave;
        Button btnQuit;
        protected void Page_Load(object sender, EventArgs e)
        {
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello ')</script>");
            InitControl();
            if (!string.IsNullOrEmpty(Request.QueryString["BusinessUnitTypeID"]))
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
                int id = Convert.ToInt32(Request.QueryString["BusinessUnitTypeID"]);
                BusinessUnitType bui = db.BusinessUnitType.SingleOrDefault(a => a.BusinessUnitTypeID == id);
                if (bui != null)
                {
                    this.txtTypeName.Text = bui.BusinessUnitTypeName.ToString();
                    this.txtTypeCode.Text = bui.BusinessUnitTypeCode.ToString();
                    this.txtRemark.Text = bui.Remark.ToString();
                }
                else
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('记录不存在! ');</script>");
                    Response.Redirect("BusinessTypeManage.aspx");
                }
            }
        }
        private void InitControl()
        {
            this.txtTypeName = (TextBox)GetControltByMaster("txtTypeName");
            this.txtTypeCode = (TextBox)GetControltByMaster("txtTypeCode");
            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
        }




        public void btnSave_Click(object sender, EventArgs e)
        {

            if (!string.IsNullOrEmpty(this.txtTypeName.Text) && !string.IsNullOrEmpty(this.txtTypeCode.Text))
            {
                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {


                    int id = Convert.ToInt32(Request.QueryString["BusinessUnitTypeID"]);
                    BusinessUnitType but = db.BusinessUnitType.SingleOrDefault(a => a.BusinessUnitTypeID == id);
                    but.BusinessUnitTypeName = this.txtTypeName.Text.Trim();

                    BusinessUnitType code = db.BusinessUnitType.SingleOrDefault(u => u.BusinessUnitTypeCode == this.txtTypeCode.Text.Trim());
                    if (code == null)
                    {
                        but.BusinessUnitTypeCode = this.txtTypeCode.Text.Trim();

                       
                    }
                    else
                    {
                        if (but.BusinessUnitTypeID == code.BusinessUnitTypeID)
                        {
                            but.BusinessUnitTypeCode = this.txtTypeCode.Text.Trim();

                        }
                        else
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('单位类别代码重复! ');</script>");
                            return;
                        }
                    }

                   
                    but.Remark = this.txtRemark.Text.Trim();
                    db.SubmitChanges();
                    Response.Redirect("BusinessTypeManage.aspx");


                }
            }
        }
        public void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("BusinessTypeManage.aspx");
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
