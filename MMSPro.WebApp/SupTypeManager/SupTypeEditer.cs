﻿using System;
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
    public class SupTypeEditer : System.Web.UI.Page
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
            if (!string.IsNullOrEmpty(Request.QueryString["SupplierTypeID"]))
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
            if (int.TryParse(Request.QueryString["SupplierTypeID"], out intID))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    SupplierType di = db.SupplierType.SingleOrDefault(a => a.SupplierTypeID == intID);
                    if (di != null)
                    {
                        this.txtSupTypeName.Text = di.SupplierTypeName;
                        this.txtSupTypeCode.Text = di.SupplierTypeCode;       
                       this.txtRemark.Text = di.Remark;                      
                    }
                }
            }
            else
            {
                Response.Redirect("SupTypeManager.aspx");
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
            Response.Redirect("SupTypeManager.aspx");
        }
        void btnSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtSupTypeName.Text) && !string.IsNullOrEmpty(this.txtSupTypeCode.Text))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    int intID = 0;
                    if (int.TryParse(Request.QueryString["SupplierTypeID"], out intID))
                    {
                        var temp = db.SupplierType.SingleOrDefault(a => a.SupplierTypeCode == this.txtSupTypeCode.Text.Trim());
                        if (temp == null)
                        {
                            InsertRow();
                        }
                            
                        else 
                        {
                            if (temp.SupplierTypeID == int.Parse(Request.QueryString["SupplierTypeID"]))
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
                SupplierType ei = db.SupplierType.SingleOrDefault(a => a.SupplierTypeID == int.Parse(Request.QueryString["SupplierTypeID"]));
                if (ei != null)
                {
                    ei.SupplierTypeName = this.txtSupTypeName.Text.Trim();                   
                    ei.SupplierTypeCode = this.txtSupTypeCode.Text.Trim();
                    ei.Remark = this.txtRemark.Text.Trim();
                    db.SubmitChanges();
                    Response.Redirect("SupTypeManager.aspx");
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
