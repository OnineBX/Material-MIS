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
    public class TaskCenter:System.Web.UI.Page
    {
        MMSProDBDataContext db;
        TextBox txtStorageinNum;


        DropDownList ddlbatch;
        Label lblCreator;          
        TextBox txtRecipient;

        TextBox txtTaskTitle;
        TextBox txtRemark;
        Label lblre;

        Button btnSave;
        Button btnQuit;
        string batchStr;
        string currentFlow;
        Literal L1;
        string QCBatch;
        string errmsg;
        string DisposeUrl;
        string BackUrl;
        string _InspectState;
        string storageInType;
        int oldDor;//委托人
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                InitControl();
                QCBatch = Request.QueryString["QCBatch"];
                if (!string.IsNullOrEmpty(Request.QueryString["InspectState"]))
                {
                    _InspectState = Request.QueryString["InspectState"];
                }
                else
                {
                    _InspectState = "未审核";
                }
                
                checkSendPolicy();

                if (!IsPostBack)
                {
                    BindDefaultDate();
                    using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        var bi = from a in db.BatchOfIndex
                                 select a;
                        if (bi.ToArray().Length < 1)
                        {
                            batchCount();
                        }
                    }


                    BindBatch();
                   

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

                if (Request.QueryString["state"].ToString() == "物资组员")
                {
      
                    this.ddlbatch.Enabled = true;
                    lblre.Text = "物资组员";
                    batchStr = this.ddlbatch.SelectedItem.Text.Trim().ToString();
                    currentFlow = "物资组员";
                }

                if (Request.QueryString["state"].ToString() == "物资组长")
                {
                
                    this.ddlbatch.Enabled = false;
                    lblre.Text = "物资组长";
                    batchStr = this.ddlbatch.SelectedItem.Text.Trim().ToString();
                    currentFlow = "物资组长";
                }

                if (Request.QueryString["state"].ToString() == "质检")
                {
              
                    this.ddlbatch.Enabled = false;
                    lblre.Text = "质检人";
                    batchStr = this.ddlbatch.SelectedItem.Text.Trim().ToString();
                    currentFlow = "质检";
                }

                if (Request.QueryString["state"].ToString() == "资产组员")
                {
        
                    this.ddlbatch.Enabled = false;
                    lblre.Text = "资产组员";
                    batchStr = this.ddlbatch.SelectedItem.Text.Trim().ToString();
                    currentFlow = "资产组员";
                }

                if (Request.QueryString["state"].ToString() == "资产组长")
                {
  
                    this.ddlbatch.Enabled = false;
                    lblre.Text = "资产组长";
                    batchStr = this.ddlbatch.SelectedItem.Text.Trim().ToString();
                    currentFlow = "资产组长";
                }

                if (Request.QueryString["state"].ToString() == "主任审核")
                {

                    this.ddlbatch.Enabled = false;
                    lblre.Text = "主任审核";
                    batchStr = this.ddlbatch.SelectedItem.Text.Trim().ToString();
                    currentFlow = "主任审核";
                }



                if (Request.QueryString["state"].ToString() == "材料会计审核")
                {



                    batchStr = Request.QueryString["QCBatch"].ToString();
                    this.ddlbatch.Enabled = false;
                    lblre.Text = "材料会计";
                    currentFlow = "材料会计审核";
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
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
            if (!string.IsNullOrEmpty(Request.QueryString["StorageInID"]))
            {
                id = Convert.ToInt32(Request.QueryString["StorageInID"]);
                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    StorageInMain temp = db.StorageInMain.SingleOrDefault(u => u.StorageInID == id);
                    this.txtStorageinNum.Text = temp.StorageInCode.ToString();
                    this.txtStorageinNum.Enabled = false;

                }
            }
            else
            {
                Response.Redirect("StorageInManage.aspx");
            }


        }
        private void InitControl()
        {
            this.txtStorageinNum = (TextBox)GetControltByMaster("txtStorageinNum");

            this.ddlbatch = (DropDownList)GetControltByMaster("ddlbatch");
            this.lblre = (Label)GetControltByMaster("lblre");
            this.txtStorageinNum.Enabled = false;
            this.lblCreator = (Label)GetControltByMaster("lblCreator") ;
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

        private void batchCount()
        {
            using (MMSProDBDataContext dk = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                List<string> li = new List<string>();
                li.Add("第一批");
                li.Add("第二批");
                li.Add("第三批");
                li.Add("第四批");
                li.Add("第五批");
                li.Add("第六批");
                li.Add("第七批");
                li.Add("第八批");
                li.Add("第九批");
                li.Add("第十批");



                for (int i = 0; i < 10; i++)
                {
                    BatchOfIndex bi = new BatchOfIndex();
                    bi.BatchOfIndexName = li[i].ToString();
                    dk.BatchOfIndex.InsertOnSubmit(bi);
                    dk.SubmitChanges();
                }
            }
        }
        //绑定质检批次，已存在的质检批次不在绑定.
        private void BindBatch()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {

                Dictionary<string, string> dic = new Dictionary<string, string>();

                var list = from a in db.BatchOfIndex

                           select new { a.BatchOfIndexName, a.BatchOfIndexID };
                var templist = list.ToList();

                var tempBatch = from a in db.StorageInMaterials
                                where a.StorageProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"].Trim())
                                select new { a.StorageProduce.BatchIndex };
                var le = tempBatch.ToList();
              
                for (int i = 0; i < templist.Count; i++)
                {
                    dic.Add(templist[i].BatchOfIndexID.ToString(), templist[i].BatchOfIndexName.ToString());
                }




                //var temp = from a in db.BatchOfIndex
                //           select new
                //           {
                //               Key = a.BatchOfIndexName,
                //               Value = a.BatchOfIndexID
                //           };

                this.ddlbatch.DataSource = dic;
                this.ddlbatch.DataTextField = "Value";
                this.ddlbatch.DataValueField = "Key";
                this.ddlbatch.DataBind();
                this.ddlbatch.SelectedItem.Text = QCBatch;
            }
        }
        /// <summary>
        /// 检查是否有质检物资
        /// </summary>
        private void checkSendPolicy()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //判断所选物资批次是否在生产组信息表中
                var tmp = from a in db.StorageProduce
                          where a.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"])
                          select new { a.StorageInProduceID };
                if (tmp.ToArray().Length == 0)
                {
                    errmsg = "无待质检物资,请先新建质检物资.";
                    DisposeUrl = "../StorageIn/StorageInDetailedManage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "";
                    BackUrl = "../StorageIn/StorageInManage.aspx";
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
                    var tmp = from a in db.StorageProduce
                              where a.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.BatchIndex == batchStr
                              select new { a.StorageInProduceID };
                    if (tmp.ToArray().Length == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('无此待质检批次,请重新选择 ')</script>");
                        return;

                    }
                    //判断任务列表中是否已发过此条任务
                    TaskStorageIn ts = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == Convert.ToInt32(Request.QueryString["TaskStorageID"]));
                    var task = from a in db.TaskStorageIn
                               where a.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.QCBatch == batchStr && a.TaskType == currentFlow && a.StorageInType == storageInType
                               select new { a.TaskCreaterID, a.InspectState };
                    if (ts.InspectState != "驳回")
                    {
                        if (task.ToArray().Length != 0)
                        {
                            //如果是驳回的信息允许再次发送
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('不能重复发送相同任务 ')</script>");
                            return;
                        }
                    }

                    //修改完成状态
                    if (!string.IsNullOrEmpty(Request.QueryString["TaskStorageID"]))
                    {
                        
                        if (ts != null)
                        {
                            ts.TaskState = "已完成";
                            
                        }
                    }

                    //生成质检完成流水号
                    if (Request.QueryString["state"].ToString() == "材料会计审核")
                    {
                        StorageInMain si = db.StorageInMain.SingleOrDefault(u => u.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]));

                    }


                    //代理step1
                    int iDirector = reEmpId(this.txtRecipient.Text.Trim());//主任id
                    if (Request.QueryString["state"].ToString() == "主任审核")
                    {
                        //检查当前登录用户是否有代理任务
                        
                        oldDor = iDirector;//保存主任id
                        iDirector = Proxy.send(iDirector, 1);//转给代理人
                    }
                  


                    //发送新任务
                    TaskStorageIn TSI = new TaskStorageIn();

                    TSI.TaskCreaterID = reEmpId(this.lblCreator.Text.Trim());
                    TSI.TaskTargetID = iDirector;

                   


                    if (TSI.TaskTargetID == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('不存在质检用户，请同步AD账户 ')</script>");
                        return;
                    }

                    TSI.StorageInID = Convert.ToInt32(Request.QueryString["StorageInID"]);
                    TSI.StorageInType = storageInType;
                    TSI.TaskTitle = this.txtTaskTitle.Text.Trim();
                    TSI.TaskState = "未完成";
                    TSI.TaskDispose = "未废弃";
                    TSI.TaskType = Request.QueryString["state"].ToString();
                    TSI.InspectState = _InspectState;

                    //TSI.BatchOfIndex = this.ddlbatch.SelectedItem.Text.ToString();

                    TSI.QCBatch = batchStr;


                    TSI.Remark = this.txtRemark.Text.Trim();
                    var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                    TSI.CreateTime = SevTime.First();

                    db.TaskStorageIn.InsertOnSubmit(TSI);
                    db.SubmitChanges();

                    //代理step2
                    //保存代理任务信息
                    Proxy.saveTask(TSI.TaskStorageID, Proxy.getProxyTaskId(oldDor, 1));


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
            int valueEmp =0;
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

            if (Request.QueryString["state"].ToString() == "物资组长")
            {
                Response.Redirect("StorageMaterials.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&QCBatch=" + Request.QueryString["QCBatch"] + "");
            }

            if (Request.QueryString["state"].ToString() == "质检")
            {
                Response.Redirect("StorageMaterialsLeader.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&QCBatch=" + Request.QueryString["QCBatch"] + "");
            }

            if (Request.QueryString["state"].ToString() == "资产组员")
            {
                Response.Redirect("StorageTest.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&QCBatch=" + Request.QueryString["QCBatch"] + "");
            }


            if (Request.QueryString["state"].ToString() == "资产组长")
            {
                Response.Redirect("StorageAssets.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&QCBatch=" + Request.QueryString["QCBatch"] + "");
            }

            if (Request.QueryString["state"].ToString() == "主任审核")
            {
                Response.Redirect("StorageAssetsLeader.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&QCBatch=" + Request.QueryString["QCBatch"] + "");
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
