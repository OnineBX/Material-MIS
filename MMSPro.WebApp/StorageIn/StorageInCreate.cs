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
    public class StorageInCreate:System.Web.UI.Page
    {
        MMSProDBDataContext db;
        TextBox txtBillCode;
        DateTimeControl DateTimeStorageIn;
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
            this.txtBillCode = (TextBox)GetControltByMaster("txtBillCode");
            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
            this.DateTimeStorageIn = (DateTimeControl)GetControltByMaster("DateTimeStorageIn");
            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
        }

        public void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(this.txtBillCode.Text))
                {
                    using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {



                        StorageIn SI = new StorageIn();
                        StorageIn scode = db.StorageIn.SingleOrDefault(u => u.StorageInCode == this.txtBillCode.Text.Trim());

                        if (scode != null)
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('单据编码重复！')</script>");
                            return;
                        }
                        SI.StorageInCode = this.txtBillCode.Text.Trim();

                        SI.Remark = this.txtRemark.Text.Trim();
                        //var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                        SI.Creator = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
                        SI.CreateTime = this.DateTimeStorageIn.SelectedDate;
                        db.StorageIn.InsertOnSubmit(SI);
                        db.SubmitChanges();
                        Response.Redirect("StorageInManage.aspx");


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
            Response.Redirect("StorageInManage.aspx");
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
