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
    public class StorageCreate : System.Web.UI.Page
    {
        TextBox txtStorageName;
        TextBox txtStorageCode;
        TextBox txtStorageEmp;
        TextBox txtRemark;
        Button btnCreate;
        Button btnCancel;
        Literal L1;

        protected void Page_Load(object sender, EventArgs e)
        {
            txtStorageEmp = (TextBox)GetControltByMaster("txtStorageEmp");
            //StringBuilder sb = new StringBuilder();
            //sb.Append("<script type=\"text/javascript\">");
            //sb.Append("function OpenDialogSelectUser()");
            //sb.Append("{");
            //sb.Append("var uuu=window.showModalDialog('SelectUser.aspx','0','dialogWidth:300px;dialogHeight:450px');");
            //sb.Append("document.getElementById('" +txtStorageEmp.ClientID + "').value=\"baibei\\\\\"+uuu;");
            //sb.Append("}");
            //sb.Append("</script>");
            L1 = (Literal)GetControltByMaster("L1");
            L1.Text = JSDialogAid.GetJSForDialog(txtStorageEmp.ClientID,"SelectUser.aspx");

            btnCreate = (Button)GetControltByMaster("btnCreate");
            btnCreate.Click += new EventHandler(btnCreate_Click);
            btnCancel = (Button)GetControltByMaster("btnCancel");
            btnCancel.Click += new EventHandler(btnCancel_Click);
            btnCancel.CausesValidation = false;
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("StorageManager.aspx");
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            InitControls();
            using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //var tempSi = dc.StorageInfo.SingleOrDefault(u => u.StorageCode == this.txtStorageCode.Text.Trim());
                //if (tempSi == null)
                //{
                    StorageInfo si = new StorageInfo();
                    si.StorageName = this.txtStorageName.Text;
                    si.StorageCode = this.txtStorageCode.Text;
                    si.Remark = this.txtRemark.Text;
                    var tempEmp = dc.EmpInfo.SingleOrDefault(u => u.Account == this.txtStorageEmp.Text);
                    if (tempEmp == null)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "Fail", "<script>alert('系统中不存在该员工，请添加该员工或者选择其他的员工')</script>");
                    }
                    else
                    {
                        si.EmpID = tempEmp.EmpID;
                        dc.StorageInfo.InsertOnSubmit(si);
                        dc.SubmitChanges();
                        Response.Redirect("StorageManager.aspx");
                    }

                    //SPUser aa = SPContext.Current.Web.AllUsers["baibei\litao"];
                //}
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
