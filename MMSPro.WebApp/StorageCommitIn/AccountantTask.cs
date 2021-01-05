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

    public class AccountantTask : System.Web.UI.Page
    {
        MMSProDBDataContext db;
        private int _storageinid;
        private int _taskstorageid;
        private DropDownList ddlReceivingType;
        private TextBox txtDirector;   

        Button btnSave;
        Button btnQuit;

        Literal L1;
        int oldDor;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this.InitControl();
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
            _taskstorageid = Convert.ToInt32(Request.QueryString["TaskStorageID"]);

            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskstorageid);
                this._storageinid = tsi.StorageInID;

            }



            ((Label)this.GetControltByMaster("lblCreator")).Text = SPContext.Current.Web.CurrentUser.LoginName;
            txtDirector = (TextBox)this.GetControltByMaster("txtDirector");
            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click +=new EventHandler(btnSave_Click);       
            this.btnQuit.Click += new EventHandler(btnQuit_Click);

            selUser(txtDirector);
            
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {

                //初始化交货通知单号
               
                ((Label)this.GetControltByMaster("lblStorageInCode")).Text = (db.CommitIn.SingleOrDefault(u=>u.CommitInID ==_storageinid)).CommitInCode;

                //初始化入库单号
                ((Label)this.GetControltByMaster("lblMessageCode")).Text = (db.CommitIn.SingleOrDefault(u => u.CommitInID == _storageinid)).CommitInQualifiedNum;

                //初始化规格类型信息
                this.ddlReceivingType = (DropDownList)this.GetControltByMaster("ddlReceivingType");

                this.ddlReceivingType.DataSource = from r in db.ReceivingTypeInfo
                                                   select new
                                                   {
                                                       r.ReceivingTypeName,
                                                       r.ReceivingTypeID
                                                   };
                this.ddlReceivingType.DataTextField = "ReceivingTypeName";
                this.ddlReceivingType.DataValueField = "ReceivingTypeID";
                this.ddlReceivingType.DataBind();
                this.ddlReceivingType.SelectedValue = (db.ReceivingTypeInfo.SingleOrDefault(u => u.ReceivingTypeName == "线下入库")).ReceivingTypeID.ToString();
                this.ddlReceivingType.Enabled = false;

                //若已经发送审批，则提示信息
                TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.StorageInID == _storageinid&& u.TaskType == "主任审核" && u.InspectState == "未审核" && u.StorageInType=="委外入库");
                if (tsi != null)
                {
                    txtDirector.Text = (db.EmpInfo.SingleOrDefault(u => u.EmpID == tsi.TaskTargetID)).Account.Trim();
                    TextBox txtTaskTitle = (TextBox)this.GetControltByMaster("txtTaskTitle");
                    TextBox txtRemark = (TextBox)this.GetControltByMaster("txtRemark");
                    txtTaskTitle.Text = tsi.TaskTitle.Trim();
                    txtRemark.Text = tsi.Remark.Trim();
                    txtDirector.Enabled = false;
                    txtRemark.Enabled = false;
                    txtTaskTitle.Enabled = false;
                    this.btnSave.Enabled = false;                    

                }


            }

            
            

        }

        private void selUser(TextBox tbox_W)
        {            
            L1 = (Literal)GetControltByMaster("L1");
            L1.Text = JSDialogAid.GetJSForDialog(tbox_W.ClientID, "../StorageAndPile/SelectUser.aspx");
        }       



        public void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {

                    //用户IE回退操作检验
                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.StorageInID == _storageinid && u.TaskType == "主任审核" && u.InspectState == "未审核" && u.StorageInType=="委外入库");
                    if (tsi != null)
                    {
                        Response.Redirect("../../default-old.aspx",false);
                        return;
                    }

                    //修改完成状态
                    if (!string.IsNullOrEmpty(Request.QueryString["TaskStorageID"]))
                    {
                        TaskStorageIn ts = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskstorageid);
                        ts.TaskState = "已完成";
                        ts.InspectState = "已审核";
                    }

                    //审核完在主表StorageIn中填入入库类型            
                    CommitIn si = db.CommitIn.SingleOrDefault(u => u.CommitInID == _storageinid);
                    si.ReceivingType = Convert.ToInt32(ddlReceivingType.SelectedValue);


                    int iDirector = reEmpId(txtDirector.Text.Trim());
                    if (iDirector == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('不存在质检用户，请同步AD账户 ')</script>");
                        return;
                    }

                    //在StorageInAudited表中填入审批主任
                    var Auditeds = from a in db.CommitInAudited
                                   where a.CommitInID == _storageinid
                                   select a;
                    foreach (CommitInAudited audited in Auditeds)
                    {
                        audited.Director = iDirector;
                    }

                    oldDor = iDirector;
                    iDirector = Proxy.send(iDirector, 2);


                    //发送新任务
                    TaskStorageIn TSI = new TaskStorageIn();

                    TSI.TaskCreaterID = reEmpId(((Label)this.GetControltByMaster("lblCreator")).Text.Trim());
                    TSI.TaskTargetID = iDirector;




                    TSI.StorageInID = _storageinid;
                    TSI.StorageInType = "委外入库";
                    TSI.TaskTitle = ((TextBox)this.GetControltByMaster("txtTaskTitle")).Text.Trim();
                    TSI.TaskState = "未完成";
                    TSI.TaskDispose = "未废弃";
                    TSI.TaskType = "主任审核";

                    //ProxyDirector pd = new ProxyDirector();
                    //TaskProxy TP = db.TaskProxy.SingleOrDefault(u => u.TaskProxyType.TaskProxyTypeName == "委外入库" && u.ProxyPrincipal == TSI.TaskTargetID);
                    //if (TSI.TaskType == "主任审核")
                    //{



                    //    if (TP != null)
                    //    {
                    //        pd.TaskProxy.ProxyPrincipal = TP.ProxyPrincipal;
                    //        pd.TaskID = TSI.TaskStorageID;
                    //        pd.TaskProxyID = TP.TaskProxyID;
                    //        pd.TaskProxy.ProxyFiduciary = TP.ProxyFiduciary;
                    //        TSI.TaskTargetID = TP.ProxyFiduciary;
                    //        TSI.TaskTitle = TP.EmpInfo.EmpName + "主任委托委外入库审批任务：" + TSI.TaskTitle;
                    //        db.ProxyDirector.InsertOnSubmit(pd);
                    //    }
                    //}


                    TSI.InspectState = "未审核";
                    TSI.Remark = ((TextBox)this.GetControltByMaster("txtRemark")).Text.Trim();
                    var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                    TSI.CreateTime = SevTime.First();
                    TSI.QCBatch = "第一批";

                    db.TaskStorageIn.InsertOnSubmit(TSI);

                    db.SubmitChanges();
                    //pd.TaskID = TSI.TaskStorageID;
                    //db.SubmitChanges();

                    //保存代理任务信息
                    Proxy.saveTask(TSI.TaskStorageID, Proxy.getProxyTaskId(oldDor, 2));
                    Response.Redirect("../../default-old.aspx",false);
                    
                   

                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
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
           if(this.btnSave.Enabled == false)
               Response.Redirect("../../default-old.aspx",false);
            else
               Response.Redirect("AccountantManage.aspx?TaskStorageID=" + _taskstorageid + " ");
        }
        
        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }
    }
}
