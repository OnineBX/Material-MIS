/*------------------------------------------------------------------------------
 * Unit Name：SendMessage.cs
 * Description: 发送消息页面
 * Author: Xu Chun Lei
 * Created Date: 2010-07-30
 * ----------------------------------------------------------------------------*/
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
using System.Text.RegularExpressions;

namespace MMSPro.WebApp
{
    /// <summary>
    /// SendMessage公共页面用于发送消息：当处在任务中时，传递参数为TaskID:任务ID;Sort:任务所属的分类--出库、入库、移库报废
    /// 当不处在任务中时，传递参数为Process:所属流程;Step:流程中的节点（即TaskType）;MessageID:消息中要涉及的物资表单的ID
    /// </summary>
    public class SendMessage : Page
    {
        private string _process, _step,_receivers;
        private string strAttachUrl,strBackUrl,strFinishUrl;
        
        private int _taskid,_messageid;//_taskid:任务ID,系统也可能在没有任务时发送消息;_messageid:消息涉及的表单ID  

        private TextBox txtContent, txtTitle, txtReceivers;
        private Button btnOK;
        private CheckBox chbPublic,chbAttach;
        private Literal ltrJS;

        private bool bfinishtask = true;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                _taskid =Convert.ToInt32(Request.QueryString["TaskID"]);
                if (!string.IsNullOrEmpty(Request.QueryString["Receivers"]))//触发发送消息的页面是否传递接收者信息
                    _receivers = HttpUtility.UrlDecode(Request.QueryString["Receivers"]);

                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskid);
                    _process = tsi.StorageInType;
                    _step = tsi.TaskType;
                    strFinishUrl = "../../default-old.aspx";
                    SetPageConfig();                    
                }

                InitializeCustomControls();
                ShowCustomControls();
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
        }

        #region 初始化和数据绑定方法

        private void InitBar()
        {
            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");

            //返回
            ToolBarButton tbarbtnBack = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnBack.ID = "backRow";
            tbarbtnBack.Text = "返回";
            tbarbtnBack.ImageUrl = "/_layouts/images/BACK.GIF";
            tbarbtnBack.Click += new EventHandler(tbarbtnBack_Click);
            tbarTop.Buttons.Controls.Add(tbarbtnBack);

            ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnRefresh.ID = "btnRefresh";
            btnRefresh.Text = "刷新";
            btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
            btnRefresh.Padding = "0,5,0,0";
            btnRefresh.Click += new EventHandler(btnRefresh_Click);
            tbarTop.RightButtons.Controls.Add(btnRefresh);
        }      

        private void InitializeCustomControls()
        {
            InitBar();

            ((Label)GetControltByMaster("lblCreater")).Text = SPContext.Current.Web.CurrentUser.LoginName;
            txtContent = (TextBox)GetControltByMaster("txtContent");
            txtTitle = (TextBox)GetControltByMaster("txtTitle");
            txtReceivers = (TextBox)GetControltByMaster("txtReceivers");
            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);
            chbPublic = (CheckBox)GetControltByMaster("chbPublic");
            chbPublic.CheckedChanged += new EventHandler(chbPublic_CheckedChanged);
            ltrJS = (Literal)GetControltByMaster("ltrJS");
            chbAttach = (CheckBox)GetControltByMaster("chbAttach");
            chbAttach.CheckedChanged += new EventHandler(chbAttach_CheckedChanged);
            ltrJS.Text = JSDialogAid.GetUsersJS(txtReceivers.ClientID);   
        
            txtContent.Text = Request.QueryString["MessageInfo"];
                                     
        }

        private void ShowCustomControls()
        {
            if (string.IsNullOrEmpty(_receivers))
            {                
                ltrJS.Text = JSDialogAid.GetUsersJS(txtReceivers.ClientID);
            }
            else//已经选定接收者
            {
                chbPublic.Enabled = false;
                txtReceivers.ReadOnly = true;
                txtReceivers.Text = _receivers;
                ltrJS.Text = string.Empty;
            }
        }

        #endregion

        #region 控件事件方法

        void btnRefresh_Click(object sender, EventArgs e)
        {

        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect(strBackUrl, false);
        }     

        void btnOK_Click(object sender, EventArgs e)
        {
            try            
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    List<int> ReceiversID = new List<int>();
                    if (!chbPublic.Checked)
                    {
                        //私有信息判断接收用户是否存在
                        Regex regex = new Regex("\\s+");//去除字符串中的回车换行字符
                        string strReceivers = regex.Replace(txtReceivers.Text, string.Empty);
                        List<string> Receivers = strReceivers.Split(';').ToList();
                        if(Receivers.Count != 1)
                            Receivers.RemoveAt(Receivers.Count - 1);                        

                        int empid;
                        foreach (string receiver in Receivers)
                        {
                            empid = ReturnEmpIDByName(receiver);
                            if (empid != 0)
                                ReceiversID.Add(empid);
                        }
                        if (ReceiversID.Count != Receivers.Count)
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('接受者包含非法用户，请修改后重新提交！ ')</script>");
                            return;
                        }
                    }                   

                    //发送消息--记录消息内容
                    MessageInfo mi = new MessageInfo();
                    mi.Creater = ReturnEmpIDByName(SPContext.Current.Web.CurrentUser.LoginName);
                    mi.MessageTitle = txtTitle.Text.Trim();
                    mi.MessageContent = txtContent.Text.Trim();
                    mi.MessageSource = this._process;
                    mi.MessageStatus = "未读";
                    mi.MessageType = chbPublic.Checked ? "公共消息" : "私有消息";           
                    mi.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                    mi.TaskID = _taskid;
                    db.MessageInfo.InsertOnSubmit(mi);
                    db.SubmitChanges();

                    //发送消息--私有信息发给接收者
                    if (!chbPublic.Checked)
                    {
                        foreach (int recieveid in ReceiversID)
                        {
                            MessageReceiver mr = new MessageReceiver();
                            mr.MessageInfoID = mi.MessageInfoID;
                            mr.ReceiverID = recieveid;
                            db.MessageReceiver.InsertOnSubmit(mr);
                        }
                        db.SubmitChanges();
                    }                    

                    //结束当前任务
                    if(bfinishtask)
                        TerminateCurTask();
                    Response.Redirect(strFinishUrl, false);
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

        void chbPublic_CheckedChanged(object sender, EventArgs e)
        {
            if (chbPublic.Checked)
            {
                txtReceivers.Text = "全部人员";
                txtReceivers.Enabled = false;
                ltrJS.Text = string.Empty;
            }
            else
            {
                txtReceivers.Text = string.Empty;
                txtReceivers.Enabled = true;
                ltrJS.Text = JSDialogAid.GetUsersJS(txtReceivers.ClientID);
            }
        }

        void chbAttach_CheckedChanged(object sender, EventArgs e)
        {
            string strAttachContent = string.Format("<br><br>查看物资明细请点击：{0}", strAttachUrl);
            if (chbAttach.Checked)
                txtContent.Text = string.Format("{0}{1}", txtContent.Text, strAttachContent);
            else
                txtContent.Text = txtContent.Text.Replace(strAttachContent, string.Empty);
                
        }

        #endregion

        #region 辅助方法

        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }

        private void SelectUser(ListBox users)
        {

            JSDialogAid.GetJSForDialog(users.ClientID, "../PublicPage/SelectUsers.aspx");
        }

        private int ReturnEmpIDByName(string empName)
        {
            int empID = 0;

            try
            {
                using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    empID = dc.EmpInfo.SingleOrDefault(u => u.Account == empName).EmpID;
                }
                
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));
            }
            return empID;
        }

        /// <summary>
        /// 获得返回页面的URL
        /// </summary>
        private void SetPageConfig()
        {            
            switch (_process)
            {
                case "回收入库":
                    switch (_step)
                    {
                        case "资产组长确认合格物资":
                            strBackUrl = string.Format("RiAChiefConfirmQReceiptMessage.aspx?TaskID={0}", _taskid);
                            break;
                        case "检验员检验修复物资":
                            strFinishUrl = string.Format("../StorageRetrieveIn/RiInspectorVerifyRDetailsMessage.aspx?TaskID={0}", _taskid);//完成后的Url和返回的Url一样
                            strBackUrl = strFinishUrl;
                            bfinishtask = false;                            
                            break;
                        case "检验员质检":
                            strFinishUrl = string.Format("../StorageRetrieveIn/RiInspectorVerifyDetailsMessage.aspx?TaskID={0}", _taskid);
                            strBackUrl = strFinishUrl;
                            bfinishtask = false;
                            break;
                    }
                    break;
            }            
        }

        private void TerminateCurTask()
        {
            using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                if (_process.Contains("入库"))
                {
                    TaskStorageIn tsi = dc.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskid);
                    if (tsi.TaskState.Equals("未完成"))
                        tsi.TaskState = "已完成";
                    else
                    {
                        Response.Redirect("../../default-old.aspx", false);
                        return;
                    }
                }

                dc.SubmitChanges();
            }            
        }
        #endregion
    }
}