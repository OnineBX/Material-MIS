﻿using System;
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
    public class StockTransferMessage : System.Web.UI.Page
    {
        MMSProDBDataContext db;
        TextBox txtStorageinNum;


        DropDownList ddlbatch;
        Label lblCreator;          
        TextBox txtRecipient;
        TextBox txtMessageCode;
        TextBox txtTaskTitle;
        TextBox txtRemark;
        Label lblre;

        Button btnSave;
        Button btnQuit;
        string batchStr;
        string currentFlow;
        Literal L1;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                InitControl();
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
                if (Request.QueryString["state"].ToString() == "质检")
                {
                    this.txtMessageCode.Enabled = false;
                    this.ddlbatch.Enabled = true;
                    lblre.Text = "质检人";
                    batchStr = this.ddlbatch.SelectedItem.Text.Trim().ToString();
                    currentFlow = "质检";
                }
                if (Request.QueryString["state"].ToString() == "材料会计审核")
                {
                    this.txtMessageCode.Enabled = true;


                    batchStr = Request.QueryString["QCBatch"].ToString();
                    this.ddlbatch.Enabled = false;
                    lblre.Text = "材料会计";
                    currentFlow = "材料会计审核";
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));
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
                    StorageIn temp = db.StorageIn.SingleOrDefault(u => u.StorageInID == id);
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
            this.txtMessageCode = (TextBox)GetControltByMaster("txtMessageCode");
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

                var tempBatch = from a in db.StorageInQualified
                                where a.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"].Trim())
                                select new { a.BatchIndex };
                var le = tempBatch.ToList();
                for (int i = 0; i < le.Count; i++)
                {
                    for (int x = 0; x < templist.Count; x++)
                    {
                        if (templist[x].BatchOfIndexName.ToString() == le[i].BatchIndex.ToString())
                        {
                            templist.Remove(templist[x]);
                        }
                    }
                }


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
                this.ddlbatch.Items.Insert(0, "--请选择--");
            }
        }

       


        public void btnSave_Click(object sender, EventArgs e)
        {


            try
            {


                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    //判断所选质检批次是否在待质检表中
                    var tmp = from a in db.StorageInDetailed
                              where a.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.BatchIndex == batchStr
                              select new { a.StorageDetailedID };
                    if (tmp.ToArray().Length == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('无此待质检批次,请重新选择 ')</script>");
                        return;

                    }
                    //判断任务列表中是否已发过此条任务
                    var task = from a in db.TaskStorageIn
                               where a.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.QCBatch == batchStr && a.TaskType == currentFlow
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
                        StorageIn si = db.StorageIn.SingleOrDefault(u => u.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]));
                        si.StorageInQualifiedNum = this.txtMessageCode.Text.Trim();
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

                    TSI.StorageInID = Convert.ToInt32(Request.QueryString["StorageInID"]);
                    TSI.StorageInType = "正常入库";
                    TSI.TaskTitle = this.txtTaskTitle.Text.Trim();
                    TSI.TaskState = "未完成";
                    TSI.TaskDispose = "未废弃";
                    TSI.TaskType = Request.QueryString["state"].ToString();
                    TSI.InspectState = "未审核";

                    //TSI.BatchOfIndex = this.ddlbatch.SelectedItem.Text.ToString();

                    TSI.QCBatch = batchStr;


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
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
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
            if (Request.QueryString["state"].ToString() == "质检")
            {
                Response.Redirect("StorageInManage.aspx");
            }
            if (Request.QueryString["state"].ToString() == "材料会计审核")
            {
                Response.Redirect("QualifiedManage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&QCBatch=" + Request.QueryString["QCBatch"] + " ");
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
