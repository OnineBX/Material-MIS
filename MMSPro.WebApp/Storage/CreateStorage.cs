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
using System.Reflection;
namespace MMSPro.WebApp
{
    public class CreateStorage: System.Web.UI.Page
    {
        MMSProDBDataContext db;
        TextBox txtBillCode;
        DateTimeControl DateTimeStorageIn;
        DropDownList ddlType;
        TextBox txtRemark;
        Button btnSave;
        Button btnQuit;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                InitControl();
                if (!IsPostBack)
                {
                    BindType();
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
        }
        private void InitControl()
        {
            this.ddlType = (DropDownList)GetControltByMaster("ddlType");
            this.txtBillCode = (TextBox)GetControltByMaster("txtBillCode");
            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
            this.DateTimeStorageIn = (DateTimeControl)GetControltByMaster("DateTimeStorageIn");
            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
        }
        /// <summary>
        /// 绑定入库类型
        /// </summary>
        private void BindType()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = from a in db.ReceivingTypeInfo
                           select new
                           {
                               Key = a.ReceivingTypeName,
                               Value = a.ReceivingTypeID
                           };

                this.ddlType.DataSource = temp;
                this.ddlType.DataTextField = "Key";
                this.ddlType.DataValueField = "Value";
                this.ddlType.DataBind();
                this.ddlType.Items.Insert(0, "--请选择--");
            }
        }
           

        public void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(this.txtBillCode.Text))
                {
                    using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {



                        StorageInMain SI = new StorageInMain();
                        StorageInMain scode = db.StorageInMain.SingleOrDefault(u => u.StorageInCode == this.txtBillCode.Text.Trim());

                        if (scode != null)
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('单据编码重复！')</script>");
                            return;
                        }
                        if (this.ddlType.SelectedIndex == 0)
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择入库类型！')</script>");
                            return;
                        }
                        SI.StorageInCode = this.txtBillCode.Text.Trim();
                        SI.ReceivingType = Convert.ToInt32(this.ddlType.SelectedValue.Trim());
                        SI.Remark = this.txtRemark.Text.Trim();
                        //var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                        SI.Creator = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
                        SI.CreateTime = this.DateTimeStorageIn.SelectedDate;
                        db.StorageInMain.InsertOnSubmit(SI);
                        db.SubmitChanges();
                        Response.Redirect("StorageManage.aspx");


                    }
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
            }
        }

        private int reEmpId(string Emptbox)
        {
            int reID = 0;
            using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {

                EmpInfo ei = dc.EmpInfo.SingleOrDefault(u => u.Account == Emptbox);
                if (ei == null)
                {
                    return 0;
                }
                reID = ei.EmpID;

            }
            return reID;
        }

        public void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("StorageManage.aspx");
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
