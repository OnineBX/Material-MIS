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
    public class BusinessEdite:System.Web.UI.Page
    {
         MMSProDBDataContext db;
        TextBox txtName;
        TextBox txtCode;
        DropDownList DdlType;
        TextBox txtAdree;
        TextBox txtTel;
        TextBox txtCharge;
        TextBox txtRemark;
        Button btnSave;
        Button btnQuit;
        protected void Page_Load(object sender, EventArgs e)
        {
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello ')</script>");
            InitControl();
            if (!string.IsNullOrEmpty(Request.QueryString["BusinessUnitID"]))
            {
                
                if (!IsPostBack)
                {
                    BindDDL();
                    LoadData();
                }
            }
           
        }
        private void LoadData()
        {
            
            using(db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                int id = Convert.ToInt32(Request.QueryString["BusinessUnitID"]);
                BusinessUnitInfo bui = db.BusinessUnitInfo.SingleOrDefault(a => a.BusinessUnitID ==id);
                if (bui != null)
                {
                    this.txtName.Text = bui.BusinessUnitName.ToString();
                    this.txtCode.Text = bui.BusinessUnitCode.ToString();
                    this.DdlType.SelectedValue = bui.BusinessUnitTypeID.ToString();
                    this.txtAdree.Text = bui.BusinessUnitAddress1.ToString();
                    this.txtTel.Text = bui.BusinessUnitPhone.ToString();
                    this.txtCharge.Text = bui.InCharger.ToString();
                    this.txtRemark.Text = bui.Remark.ToString();
                }
                else
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('记录不存在! ');</script>");
                    Response.Redirect("BusinessManage.aspx",false);
                }
            }
        }
        private void InitControl()
        {
            this.txtName = (TextBox)GetControltByMaster("txtName");
            this.txtCode = (TextBox)GetControltByMaster("txtCode");
            this.DdlType = (DropDownList)GetControltByMaster("txtType");
            this.txtAdree = (TextBox)GetControltByMaster("txtAdree");
            this.txtTel = (TextBox)GetControltByMaster("txtTel");

            this.txtCharge = (TextBox)GetControltByMaster("txtCharge");
            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");



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


                    int id = Convert.ToInt32(Request.QueryString["BusinessUnitID"]);
                    BusinessUnitInfo bui = db.BusinessUnitInfo.SingleOrDefault(a => a.BusinessUnitID == id);
                    bui.BusinessUnitName = this.txtName.Text.Trim();
                    BusinessUnitInfo code = db.BusinessUnitInfo.SingleOrDefault(u=>u.BusinessUnitCode ==this.txtCode.Text.Trim());
                    if (code == null)
                    {
                        bui.BusinessUnitCode = this.txtCode.Text.Trim();
                    }
                    else
                    {
                        if (bui.BusinessUnitID == code.BusinessUnitID)
                        {
                            bui.BusinessUnitCode = this.txtCode.Text.Trim();
                        }
                        else
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('往来单位代码重复！')</script>");
                            return;
                        }
                    }
                   


                    if (this.DdlType.SelectedIndex == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择往来单位类别！')</script>");
                        return;
                    }
                    bui.BusinessUnitTypeID = Convert.ToInt32(this.DdlType.SelectedValue);
                    bui.BusinessUnitAddress1 = this.txtAdree.Text.Trim();
                    bui.BusinessUnitPhone = this.txtTel.Text.Trim();
                    bui.InCharger = this.txtCharge.Text.Trim();
                    bui.Remark = this.txtRemark.Text.Trim();
                    db.SubmitChanges();
                    Response.Redirect("BusinessManage.aspx",false);


                }
            }
        }
        public void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("BusinessManage.aspx");
        }

        private void BindDDL()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = from a in db.BusinessUnitType
                           select new
                           {
                               Key = a.BusinessUnitTypeName,
                               Value = a.BusinessUnitTypeID
                           };

                this.DdlType.DataSource = temp;
                this.DdlType.DataTextField = "Key";
                this.DdlType.DataValueField = "Value";
                this.DdlType.DataBind();
                this.DdlType.Items.Insert(0, "--请选择--");
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
