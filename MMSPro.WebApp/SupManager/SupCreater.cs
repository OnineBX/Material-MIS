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
    public class SupCreater : System.Web.UI.Page
    {
        //TextBox txtSupplierID;
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

        private void BindDDL()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = from a in db.SupplierType
                           select new
                           {
                               Key = a.SupplierTypeName,
                               Value = a.SupplierTypeID
                           };
        
                this.ddlSupplierType.DataSource = temp;
                this.ddlSupplierType.DataTextField = "Key";
                this.ddlSupplierType.DataValueField = "Value";               
                this.ddlSupplierType.DataBind();
                this.ddlSupplierType.Items.Insert(0, "--请选择--");
            }
        }

        void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("SupManager.aspx");
        }

        void btnSave_Click(object sender, EventArgs e)
        {

            if (!string.IsNullOrEmpty(this.txtSupplierCode.Text) && !string.IsNullOrEmpty(this.txtSupplierName.Text))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))                
                {
                   //检查唯一性
                    var temp = db.SupplierInfo.SingleOrDefault(a => a.SupplierCode == this.txtSupplierCode.Text.Trim());
                    if (temp == null)
                    {
                        SupplierInfo ei = new SupplierInfo();
                        ei.InCharge = this.txtInCharge.Text.Trim();
                        ei.SupplierAddress1 = this.txtSupplierAddress1.Text.Trim();
                        ei.SupplierAddress2 = this.txtSupplierAddress2.Text.Trim();
                        ei.SupplierCode = this.txtSupplierCode.Text.Trim();
                        ei.SupplierName = this.txtSupplierName.Text.Trim();
                        ei.SupplierPhone = this.txtSupplierPhone.Text.Trim();                        
                        if (this.ddlSupplierType.SelectedIndex  == 0)
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择所属供货所类别！')</script>");
                            return;
                        }
                        ei.SupplierTypeID = int.Parse(this.ddlSupplierType.SelectedValue);                                                
                        ei.Remark = this.txtRemark.Text.Trim();
                        db.SupplierInfo.InsertOnSubmit(ei);
                        db.SubmitChanges();
                        Response.Redirect("SupManager.aspx");
                    }
                    else
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('供货商编码已存在')</script>");
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

