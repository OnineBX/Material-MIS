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
    public class ProxyTaskEdit:System.Web.UI.Page
    {
        MMSProDBDataContext db;
        Label lblPrincipal;
        TextBox txtFiduciary;
        DropDownList ddlTaskType;
        DateTimeControl StartTime;
        DateTimeControl EndTime;
        Literal L1;
        string PageValue;
        TextBox txtRemark;
        Button btnSave;
        Button btnQuit;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                InitControl();
                selUser(this.txtFiduciary);
                this.lblPrincipal.Text = SPContext.Current.Web.CurrentUser.LoginName.ToString();
                PageValue = Request.QueryString["TaskProxyID"];
                if (!IsPostBack)
                {
                    BindProxyType();
                    InitData();
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
        }
        /// <summary>
        /// 初始化页面控件
        /// </summary>
        private void InitControl()
        {
            this.txtFiduciary = (TextBox)GetControltByMaster("txtFiduciary");
            this.lblPrincipal = (Label)GetControltByMaster("lblPrincipal");
            this.ddlTaskType = (DropDownList)GetControltByMaster("ddlTaskType");
            this.StartTime = (DateTimeControl)GetControltByMaster("StartTime");
            this.EndTime = (DateTimeControl)GetControltByMaster("EndTime");

            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
            this.btnSave = (Button)GetControltByMaster("btnSave");
            btnSave.Attributes.Add("onclick", "return confirm('受托人将具有委托人对此任务类型的所有审批权力,确定这样做么?');");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
        }
        private void InitData()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {

                TaskProxy TP = db.TaskProxy.SingleOrDefault(a => a.TaskProxyID == Convert.ToInt32(PageValue));
                if (TP.TaskDispose == "完成")
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('不能对已完成的委托任务进行修改! ');</script>");
                    Response.Redirect("ProxyTaskManage.aspx");
                }
                if (TP != null)
                {
                    this.StartTime.SelectedDate = TP.StartTime;
                    this.EndTime.SelectedDate = TP.EndTime;
                    this.txtFiduciary.Text = db.EmpInfo.SingleOrDefault(u => u.EmpID == TP.ProxyFiduciary).Account;
                    this.ddlTaskType.SelectedValue = TP.ProxyTaskType.ToString();
                    this.txtRemark.Text = TP.Remark.ToString();
                }
                else
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('记录不存在! ');</script>");
                    Response.Redirect("ProxyTaskManage.aspx");
                }
            }
        }

        /// <summary>
        /// 返回选定用户名称
        /// </summary>
        /// <param name="tbox_M"></param>
        private void selUser(TextBox tbox_M)
        {

            L1 = (Literal)GetControltByMaster("L1");
            L1.Text = JSDialogAid.GetJSForDialog(tbox_M.ClientID, "../StorageAndPile/SelectUser.aspx");
        }

        public void btnSave_Click(object sender, EventArgs e)
        {
            try
            {

                if (this.ddlTaskType.SelectedItem.Text == "--请选择--")
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择委托任务类型')</script>");
                    return;
                }

                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {

                    TaskProxy TP = db.TaskProxy.SingleOrDefault(a => a.TaskProxyID == Convert.ToInt32(PageValue));

                    TP.ProxyPrincipal = db.EmpInfo.SingleOrDefault(u => u.Account == SPContext.Current.Web.CurrentUser.LoginName.ToString()).EmpID;
                    TP.ProxyFiduciary = reEmpId(this.txtFiduciary.Text.Trim());
                    TP.ProxyTaskType = Convert.ToInt32(this.ddlTaskType.SelectedValue);
                    TP.StartTime = this.StartTime.SelectedDate;
                    TP.EndTime = this.EndTime.SelectedDate;
                    TP.Remark = this.txtRemark.Text.Trim();
                    var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                    TP.CreateTime = SevTime.First();
                    //如果当前时间在代理任务结束之前则修改状态信息
                    if (TP.EndTime > TP.CreateTime)
                    {
                        TP.TaskDispose = "待处理";
                    }
                    db.SubmitChanges();



                }
                Response.Redirect("ProxyTaskManage.aspx");
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }

        }
        //根据登录用户名返回用户ID
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

        /// <summary>
        /// 绑定代理类型
        /// </summary>
        private void BindProxyType()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = from a in db.TaskProxyType
                           select new
                           {
                               Key = a.TaskProxyTypeName,
                               Value = a.TaskProxyTypeID
                           };

                this.ddlTaskType.DataSource = temp;
                this.ddlTaskType.DataTextField = "Key";
                this.ddlTaskType.DataValueField = "Value";
                this.ddlTaskType.DataBind();
                this.ddlTaskType.Items.Insert(0, "--请选择--");
            }
        }

        public void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("ProxyTaskManage.aspx");
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
