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
    public class CommitEditStorage : System.Web.UI.Page
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
                if (!string.IsNullOrEmpty(Request.QueryString["StorageInID"]))
                {

                    if (!IsPostBack)
                    {

                        LoadData();
                        
                    }
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }

        }
        private void LoadData()
        {

            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                int id = Convert.ToInt32(Request.QueryString["StorageInID"]);
                CommitInMain SI = db.CommitInMain.SingleOrDefault(a => a.StorageInID == id);
                if (SI != null)
                {
                    this.txtBillCode.Text = SI.StorageInCode.ToString();
                    this.DateTimeStorageIn.SelectedDate = SI.CreateTime;
                    this.txtRemark.Text = SI.Remark.ToString();
                }
                else
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('记录不存在! ');</script>");
                    Response.Redirect("StorageManage.aspx");
                }
            }
        }
        private void InitControl()
        {

            this.txtBillCode = (TextBox)GetControltByMaster("txtBillCode");
            this.DateTimeStorageIn = (DateTimeControl)GetControltByMaster("DateTimeStorageIn");
            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");

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


                        int id = Convert.ToInt32(Request.QueryString["StorageInID"]);
                        CommitInMain SI = db.CommitInMain.SingleOrDefault(a => a.StorageInID == id);
                        //单据编号唯一
                        CommitInMain scode = db.CommitInMain.SingleOrDefault(u => u.StorageInCode == this.txtBillCode.Text.Trim());

                        if (scode == null)
                        {
                            SI.StorageInCode = this.txtBillCode.Text.Trim();
                        }
                        else
                        {
                            if (SI.StorageInID == scode.StorageInID)
                            {
                                SI.StorageInCode = this.txtBillCode.Text.Trim();
                            }
                            else
                            {
                                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('单据号重复！')</script>");
                                return;
                            }
                        }
                        
                        SI.StorageInCode = this.txtBillCode.Text.Trim();
                        SI.ReceivingType = "委外入库";
                        SI.Remark = this.txtRemark.Text.Trim();
                        var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                        SI.CreateTime = SevTime.First();
                        db.SubmitChanges();
                        Response.Redirect("StorageManage.aspx");


                    }
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_UPDATEERROR));
            }
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
