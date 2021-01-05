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
    public class BusinessTypeCreate:System.Web.UI.Page
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



                    BusinessUnitType but = new BusinessUnitType();
                    but.BusinessUnitTypeName = this.txtTypeName.Text.Trim();
                    BusinessUnitType code = db.BusinessUnitType.SingleOrDefault(u => u.BusinessUnitTypeCode == this.txtTypeCode.Text.Trim());
                    if (code != null)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('单位类别代码重复! ');</script>");
                        return;
                    }
                    but.BusinessUnitTypeCode = this.txtTypeCode.Text.Trim();
                    but.Remark = this.txtRemark.Text.Trim();
                    db.BusinessUnitType.InsertOnSubmit(but);
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
