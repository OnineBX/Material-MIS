using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Configuration;

namespace MMSPro.WebApp
{
    public class StorageEdit : System.Web.UI.Page
    {
        TextBox txtStorageName;
        TextBox txtStorageCode;
        TextBox txtStorageEmp;
        TextBox txtRemark;
        Button btnEdit;
        Button btnCancel;
        Literal L1;

        protected void Page_Load(object sender, EventArgs e)
        {
            txtStorageEmp = (TextBox)GetControltByMaster("txtStorageEmp");
            L1 = (Literal)GetControltByMaster("L1");
            L1.Text = JSDialogAid.GetJSForDialog(txtStorageEmp.ClientID, "SelectUser.aspx");
            InitControls();
            if (!IsPostBack)
            {
                InitData();
            }


            btnEdit = (Button)GetControltByMaster("btnEdit");
            btnEdit.Click += new EventHandler(btnEdit_Click);
            btnCancel = (Button)GetControltByMaster("btnCancel");
            btnCancel.Click += new EventHandler(btnCancel_Click);
            btnCancel.CausesValidation = false;
        }

        protected void InitData()
        {
            int intID = 0;
            if (int.TryParse(Request.QueryString["StorageID"], out intID))
            {
                using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    StorageInfo si = dc.StorageInfo.SingleOrDefault(u => u.StorageID == intID);
                    if (si != null)
                    {
                        this.txtStorageName.Text = si.StorageName;
                        this.txtStorageCode.Text = si.StorageCode;
                        this.txtStorageEmp.Text = dc.EmpInfo.SingleOrDefault(u => u.EmpID == si.EmpID).EmpName;
                        this.txtRemark.Text = si.Remark;
                    }
                }
            }
        }
        void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("StorageManager.aspx");
        }

        void btnEdit_Click(object sender, EventArgs e)
        {
            int intID = 0;
            if (int.TryParse(Request.QueryString["StorageID"], out intID))
            {
                using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    StorageInfo si = dc.StorageInfo.SingleOrDefault(u => u.StorageID == intID);
                    if (si != null)
                    {
                        si.StorageName = this.txtStorageName.Text;
                        si.StorageCode= this.txtStorageCode.Text;
                        si.Remark = this.txtRemark.Text;

                        var tempEmp = dc.EmpInfo.SingleOrDefault(u => u.Account == this.txtStorageEmp.Text);
                        if (tempEmp == null)
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "Fail", "<script>alert('系统中不存在该员工，请添加该员工或者选择其他的员工')</script>");
                        }
                        else
                        {
                            si.EmpID = tempEmp.EmpID;
                            dc.SubmitChanges();
                            Response.Redirect("StorageManager.aspx");
                        }
                    }
                }
            }
        }

        protected void InitControls()
        {
            this.txtStorageName = (TextBox)GetControltByMaster("txtStorageName");
            this.txtStorageCode = (TextBox)GetControltByMaster("txtStorageCode");
            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
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
