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

    public class TaskCommitIn : System.Web.UI.Page
    {
        MMSProDBDataContext db;
        TextBox txtStorageinNum;



        Label lblCreator;
        TextBox txtRecipient;
        TextBox txtMessageCode;
        TextBox txtTaskTitle;
        TextBox txtRemark;
        Label lblre;

        Button btnSave;
        Button btnQuit;
        string currentFlow;
        Literal L1;

        string errmsg;
        string DisposeUrl;
        string BackUrl;
        string storageInType;


        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                InitControl();

                checkSendPolicy();

                if (!IsPostBack)
                {
                    BindDefaultDate();
                }
                selUser(this.txtRecipient);
                if (!string.IsNullOrEmpty(Request.QueryString["storageInType"]))
                {
                    switch (Request.QueryString["storageInType"])
                    {
                        case "正常入库":
                            storageInType = "正常入库";
                            break;
                        case "委外入库":
                            storageInType = "委外入库";
                            break;
                        case "回收入库":
                            storageInType = "回收入库";
                            break;
                    }
 
                }

                if (Request.QueryString["state"].ToString() == "质检")
                {
                    this.txtMessageCode.Enabled = false;

                    lblre.Text = "质检人";

                    currentFlow = "质检";
                }
                if (Request.QueryString["state"].ToString() == "材料会计审核")
                {
                    this.txtMessageCode.Enabled = true;

                    lblre.Text = "材料会计";
                    currentFlow = "材料会计审核";
                }
                
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance; lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }

        }
        private void selUser(TextBox tbox_W)
        {
            L1 = (Literal)GetControltByMaster("L1");
            L1.Text = JSDialogAid.GetJSForDialog(tbox_W.ClientID, "../StorageAndPile/SelectUser.aspx");
        }
        private void BindDefaultDate()
        {
            int id = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["CommitInID"]))
            {
                id = Convert.ToInt32(Request.QueryString["CommitInID"]);
                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    CommitIn temp = db.CommitIn.SingleOrDefault(u => u.CommitInID == id);
                    this.txtStorageinNum.Text = temp.CommitInCode.ToString();
                    this.txtStorageinNum.Enabled = false;

                }
            }
            else
            {
                Response.Redirect("CommitInManage.aspx");
            }


        }
        private void InitControl()
        {
            this.txtStorageinNum = (TextBox)GetControltByMaster("txtStorageinNum");
            this.txtMessageCode = (TextBox)GetControltByMaster("txtMessageCode");

            this.lblre = (Label)GetControltByMaster("lblre");
            this.txtStorageinNum.Enabled = false;
            this.lblCreator = (Label)GetControltByMaster("lblCreator");
            this.lblCreator.Text = SPContext.Current.Web.CurrentUser.LoginName;
            this.txtRecipient = (TextBox)GetControltByMaster("txtRecipient");
            this.txtTaskTitle = (TextBox)GetControltByMaster("txtTaskTitle");
            //this.txtTaskState = (TextBox)GetControltByMaster("txtTaskState");
            //this.txtTaskDispose = (TextBox)GetControltByMaster("txtTaskDispose");
            //this.txtTaskType = (TextBox)GetControltByMaster("txtTaskType");
            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
        }


        /// <summary>
        /// 检查是否有质检物资
        /// </summary>
        private void checkSendPolicy()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //判断所选质检批次是否在待质检表中
                var tmp = from a in db.CommitInDetailed
                          where a.CommitInID == Convert.ToInt32(Request.QueryString["CommitInID"])
                          select new { a.CommitDetailedID };
                if (tmp.ToArray().Length == 0)
                {
                    errmsg = "无待质检物资,请先新建质检物资.";
                    DisposeUrl = "../StorageCommitIn/CommitInDetailedManage.aspx?CommitInID=" + Request.QueryString["CommitInID"] + "";
                    BackUrl = "../StorageCommitIn/CommitInManage.aspx";
                    Response.Redirect("../PublicPage/ErrorInfo.aspx?ErrorInfo=" + errmsg + "&&BackUrl=" + BackUrl + "&&DisposeUrl=" + DisposeUrl + "");

                }
            }
        }


        public void btnSave_Click(object sender, EventArgs e)
        {


            try
            {


                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    //判断所选质检批次是否在待质检表中
                    var tmp = from a in db.CommitInDetailed
                              where a.CommitInID == Convert.ToInt32(Request.QueryString["CommitInID"])
                              select new { a.CommitDetailedID };
                    if (tmp.ToArray().Length == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('无此待质检批次,请重新选择 ')</script>");
                        return;

                    }
                    //判断任务列表中是否已发过此条任务
                    var task = from a in db.TaskStorageIn
                               where a.StorageInID == Convert.ToInt32(Request.QueryString["CommitInID"]) && a.TaskType == currentFlow && a.StorageInType == storageInType
                               select new { a.TaskCreaterID, a.InspectState };
                    if (task.ToArray().Length != 0)
                    {
                        //如果是驳回的信息允许再次发送

                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('不能重复发送相同任务 ')</script>");
                        return;


                    }

                    //修改完成状态
                    if (!string.IsNullOrEmpty(Request.QueryString["TaskStorageID"]))
                    {
                        TaskStorageIn ts = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == Convert.ToInt32(Request.QueryString["TaskStorageID"]));
                        if (ts != null)
                        {
                            ts.TaskState = "已完成";
                        }
                    }

                    //生成质检完成流水号
                    if (Request.QueryString["state"].ToString() == "材料会计审核")
                    {
                        CommitIn si = db.CommitIn.SingleOrDefault(u => u.CommitInID == Convert.ToInt32(Request.QueryString["CommitInID"]));
                        si.CommitInQualifiedNum = this.txtMessageCode.Text.Trim();
                    }
    


                    //发送新任务
                    TaskStorageIn TSI = new TaskStorageIn();

                    TSI.TaskCreaterID = reEmpId(this.lblCreator.Text.Trim());
                    TSI.TaskTargetID = reEmpId(this.txtRecipient.Text.Trim());
                    if (TSI.TaskTargetID == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('不存在质检用户，请同步AD账户 ')</script>");
                        return;
                    }

                    TSI.StorageInID = Convert.ToInt32(Request.QueryString["CommitInID"]);
                    TSI.StorageInType = storageInType;
                    TSI.TaskTitle = this.txtTaskTitle.Text.Trim();
                    TSI.TaskState = "未完成";
                    TSI.TaskDispose = "未废弃";
                    TSI.TaskType = Request.QueryString["state"].ToString();
                    TSI.InspectState = "未审核";
                    TSI.QCBatch = "第一批";//无批次默认为第一批

                    TSI.Remark = this.txtRemark.Text.Trim();
                    var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                    TSI.CreateTime = SevTime.First();

                    db.TaskStorageIn.InsertOnSubmit(TSI);
                    db.SubmitChanges();

                   
                    Response.Redirect("../../default-old.aspx",false);
                  


                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance; lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
            }

        }
        private int reEmpId(string Emp)
        {
            int valueEmp = 0;
            using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                EmpInfo EI = dc.EmpInfo.SingleOrDefault(u => u.Account == Emp);
                if (EI != null)
                {
                    valueEmp = EI.EmpID;
                }

            }

            return valueEmp;
        }
        public void btnQuit_Click(object sender, EventArgs e)
        {
            if (Request.QueryString["state"].ToString() == "质检")
            {
                Response.Redirect("CommitInManage.aspx?CommitInID=" + Request.QueryString["CommitInID"] + "");
            }
            if (Request.QueryString["state"].ToString() == "材料会计审核")
            {
                Response.Redirect("QualifiedCommitIn.aspx?StorageInID=" + Request.QueryString["CommitInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "");
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
