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
    public class RelaProBusEditer : System.Web.UI.Page
    {
        TextBox txtProjectName;
        TextBox txtProjectCode;
        TextBox txtProjectProperty;
        TextBox txtRemark;
        Button btnSave;
        Button btnQuit;
        protected void Page_Load(object sender, EventArgs e)
        {
            //  ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");

            InvtControl();
            if (!string.IsNullOrEmpty(Request.QueryString["ProjectID"]))
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
            if (int.TryParse(Request.QueryString["ProjectID"], out intID))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    ProjectInfo di = db.ProjectInfo.SingleOrDefault(a => a.ProjectID == intID);
                    if (di != null)
                    {
                        this.txtProjectName.Text = di.ProjectName;
                        this.txtProjectCode.Text = di.ProjectCode;
                        this.txtProjectProperty.Text = di.ProjectProperty;                       
                       this.txtRemark.Text = di.Remark;                      
                    }
                }
            }
            else
            {
                Response.Redirect("ProjectManager.aspx");
            }

        }
     
        private void InvtControl()
        {
            this.txtProjectName = (TextBox)GetControltByMaster("txtProjectName");
            this.txtProjectCode = (TextBox)GetControltByMaster("txtProjectCode");
            this.txtProjectProperty = (TextBox)GetControltByMaster("txtProjectProperty");
            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
        }
        void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("ProjectManager.aspx");
        }
        void btnSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtProjectName.Text) && !string.IsNullOrEmpty(this.txtProjectCode.Text))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    int intID = 0;
                    if (int.TryParse(Request.QueryString["ProjectID"], out intID))
                    {
                        var temp = db.ProjectInfo.SingleOrDefault(a => a.ProjectCode == this.txtProjectCode.Text.Trim());
                        if (temp == null)
                        {
                            InsertRow();
                        }
                            
                        else 
                        {
                            if (temp.ProjectID == int.Parse(Request.QueryString["ProjectID"]))
                            {
                                InsertRow();
                                
                            }
                            
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('项目编码已存在请更改!')</script>");
                        }
                    }
                }
            }
        }
        void InsertRow()
        {
            
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                ProjectInfo ei = db.ProjectInfo.SingleOrDefault(a => a.ProjectID == int.Parse(Request.QueryString["ProjectID"]));
                if (ei != null)
                {
                    ei.ProjectName = this.txtProjectName.Text.Trim();                   
                    ei.ProjectCode = this.txtProjectCode.Text.Trim();
                    //ei.Remark = this.txtRemark.Text.Trim();
                    db.SubmitChanges();
                    Response.Redirect("ProjectManager.aspx");
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
