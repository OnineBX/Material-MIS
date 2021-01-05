//***********************************************************
//--Description:主任代理--仅主任可进入此页面设定            *
//--Created By: adonis                                      *
//--Date:2010.8.24                                          *
//--*********************************************************
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
    public class ProxyTaskCreate:System.Web.UI.Page
    {
        MMSProDBDataContext db;
        Label lblPrincipal;
        TextBox txtFiduciary;
        DropDownList ddlTaskType;
        DateTimeControl StartTime;
        DateTimeControl EndTime;
        Literal L1;
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


                if (!IsPostBack)
                {
                    //插入初始数据
                    using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        var bi = from a in db.TaskProxyType
                                 select a;
                        if (bi.ToArray().Length < 1)
                        {
                            InsertProxyType();
                        }
                    }
                    BindProxyType();
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

                    TaskProxy TPY = db.TaskProxy.SingleOrDefault(u => u.TaskProxyType.TaskProxyTypeName == this.ddlTaskType.SelectedItem.Text && u.ProxyPrincipal == reEmpId(SPContext.Current.Web.CurrentUser.LoginName.ToString()) && u.TaskDispose != "已完成");
                    if (TPY != null)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert(' 同一委托类型不能重复创建委托人')</script>");
                        return;
                    }


                    TaskProxy TP = new TaskProxy();

                    TP.ProxyPrincipal = db.EmpInfo.SingleOrDefault(u => u.Account == SPContext.Current.Web.CurrentUser.LoginName.ToString()).EmpID;
                    TP.ProxyFiduciary = reEmpId(this.txtFiduciary.Text.Trim());
                    TP.ProxyTaskType = Convert.ToInt32(this.ddlTaskType.SelectedValue);
                    TP.StartTime = this.StartTime.SelectedDate;
                    TP.EndTime = this.EndTime.SelectedDate;
                    TP.TaskDispose = "待处理";
                    TP.Remark = this.txtRemark.Text.Trim();
                    var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                    TP.CreateTime = SevTime.First();
                    db.TaskProxy.InsertOnSubmit(TP);
                    db.SubmitChanges();




                }
                Response.Redirect("ProxyTaskManage.aspx",false);
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
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
        /// 插入委托任务类型
        /// </summary>
        private void InsertProxyType()
        {
            try
            {
                using (MMSProDBDataContext dk = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    List<string> li = new List<string>();
                    li.Add("正常入库");
                    li.Add("委外入库");
                    li.Add("回收入库");
                    li.Add("正常出库");
                    li.Add("委外出库");
                    li.Add("报废出库");
                    for (int i = 0; i < 6; i++)
                    {
                        TaskProxyType TPT = new TaskProxyType();
                        TPT.TaskProxyTypeName = li[i].ToString();
                        dk.TaskProxyType.InsertOnSubmit(TPT);
                        dk.SubmitChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
            }
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
