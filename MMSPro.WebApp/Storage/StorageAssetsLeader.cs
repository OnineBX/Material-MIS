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
    public class StorageAssetsLeader:System.Web.UI.Page
    {
        MMSProDBDataContext db;
        SPGridView gv;
        TextBox txtOpinion;
        Button btnOK;
        Button btnko;
        CheckBox checkPass;
        Label lblInfo;
        Panel plinfo;
        bool flag=false;
        bool _flag = true;
        int _storageInID;
        int _taskID;

        string QCbatch;
        string _QCbatch;//任务批次
        DropDownList ddlyn = new DropDownList();
        DropDownList ddlmf = new DropDownList();
        DropDownList ddldata = new DropDownList();
        DropDownList ddlstandar = new DropDownList();
        DropDownList ddlparts = new DropDownList();
        DropDownList ddlappearance = new DropDownList();
        static string[] Titlelist = {
                                     "交货通知单编号:StorageInCode",
                                     "物料名称:MaterialName",
                                     "物料规格:SpecificationModel",
                                     "财务编码:FinanceCode",
                                     "根/套/件(合格):TestGentaojian",
                                     "米(合格):TestMetre",
                                     "吨(合格):TestTon",
                                     "当前单位:CurUnit",
                                     "单价:UnitPrice",
                                     "金额:Amount",
                                     "入库单据号:BillCode",
                                     "物资属性:MaterialsAttribute",
                                     "预期使用项目:ProjectName",
                                     "实际到库时间:StorageTime",
                                     "所属批次:BatchIndex",
                                     "备注:Remark",
                                     "ID:StorageInAssetsID"

                                    };



        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                _storageInID = Convert.ToInt32(Request.QueryString["StorageInID"]);
                _taskID = Convert.ToInt32(Request.QueryString["TaskStorageID"]);
                _QCbatch = Request.QueryString["QCBatch"];
                control();
                taskState();
                initControl(_flag);
                

               


                this.gv = new SPGridView();
                this.gv.AutoGenerateColumns = false;

                BindGridView();

                if (!IsPostBack)
                {
                    ViewState["Temp"] = false;
                    //dataLoad();
                }

                saveState();
                
               
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
        }

        void tbarbtnsend_Click(object sender, EventArgs e)
        {
            //发送主任审批
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                if (btnko.Enabled == false)
                {
                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskID);
                    Response.Redirect("TaskCenter.aspx?StorageInID=" + _storageInID + "&&state=主任审核&&storageInType=正常入库&&QCBatch=" + _QCbatch + "&&TaskStorageID=" + _taskID + "&&InspectState=" + tsi.InspectState + "");
                }
                else
                {
                    if (ViewState["Temp"] != null)
                    {
                        if (ViewState["Temp"].ToString() == "True")
                        {
                            Response.Redirect("TaskCenter.aspx?StorageInID=" + _storageInID + "&&state=主任审核&&storageInType=正常入库&&QCBatch=" + _QCbatch + "&&TaskStorageID=" + _taskID + "");
                        }
                        else
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请先保存数据后再发送! ')</script>");
                            return;
                        }
                    }
                }
            }

            
        
        }

       


        private void initControl(bool flag)
        {

            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");

            //send
            ToolBarButton tbarbtnsend = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnsend.ID = "sendRow";
            tbarbtnsend.Text = "发送主任审批";
            tbarbtnsend.ImageUrl = "/_layouts/images/edit.gif";
            tbarbtnsend.Click += new EventHandler(tbarbtnsend_Click);
            tbarbtnsend.Visible = flag;
            tbarTop.Buttons.Controls.Add(tbarbtnsend);

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

        private void control()
        {
            plinfo = (Panel)GetControltByMaster("plinfo");
            lblInfo = (Label)GetControltByMaster("lblInfo");
            txtOpinion = (TextBox)GetControltByMaster("txtOpinion");

            btnko = (Button)GetControltByMaster("btnko");
            btnko.Attributes.Add("onclick", "return confirm('确认执行这个操作吗？');");
            btnko.Click += new EventHandler(btnko_Click);

            checkPass = (CheckBox)GetControltByMaster("checkPass");
        }

        private void saveState()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskID);

                var tp = from a in db.StorageInHead
                         where a.StorageInAssets.StorageInTest.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.StorageInAssets.StorageInTest.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.BatchIndex == Request.QueryString["QCBatch"].ToString()
                         select a;
                if (tsi.InspectState == "驳回")
                {
                    if (tsi.TaskState != "已完成")
                    {
                        btnko.Enabled = true;
                        checkPass.Enabled = true;
                    }
                    if (tp.ToArray().Length > 0)
                    {
                        btnko.Enabled = false;
                        checkPass.Enabled = false;
                    }
                }
                else
                {
                    if (tp.ToArray().Length > 0)
                    {
                        btnko.Enabled = false;
                        checkPass.Enabled = false;
                    }
                }
            }
        }


        /// <summary>
        /// 根据任务状态显示控件状态
        /// </summary>
        private void taskState()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskID);
                if (tsi.TaskState == "已完成")
                {
                    this.btnko.Enabled = false;
                    this.checkPass.Enabled = false;
                    this._flag = false;
                }

                var tp = from a in db.StorageInHead
                         where a.StorageInAssets.StorageInTest.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.StorageInAssets.StorageInTest.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.BatchIndex == Request.QueryString["QCBatch"].ToString()
                         select a;
                if (tp.ToArray().Length > 0)
                {
                    this.btnko.Enabled = false;
                    this.checkPass.Enabled = false;
                }
            }
        }


        void btnko_Click(object sender, EventArgs e)
        {
            try
            {
                

                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {

                    


                    if (this.checkPass.Checked)
                    {

                        var tp = from a in db.StorageInHead
                                 where a.StorageInAssets.StorageInTest.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.StorageInAssets.StorageInTest.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.BatchIndex == Request.QueryString["QCBatch"].ToString()
                                 select a;
                        if (tp.ToArray().Length > 0)
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('不能重复插入记录! ')</script>");
                            return;
                        }


                        for (int i = 0; i < this.gv.Rows.Count; i++)
                        {

                            StorageInHead sih = new StorageInHead();
                            sih.AssetsID = Convert.ToInt32(this.gv.Rows[i].Cells[this.gv.Columns.Count - 1].Text.ToString());
                            sih.Auditing = "是";
                            var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                            sih.CreateTime = SevTime.First();
                            sih.Creator = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
                            db.StorageInHead.InsertOnSubmit(sih);
                            db.SubmitChanges();

                        }
                        btnko.Enabled = false;
                        checkPass.Enabled = false;
                        
                    }
                    else
                    { 
                        //不审核库，回退操作
                        //删除写入的数据
                        var temp = from a in db.StorageInHead
                                   where a.StorageInAssets.StorageInTest.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.StorageInAssets.StorageInTest.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.BatchIndex == Request.QueryString["QCBatch"].ToString()
                                   select new { a.StorageInHeadID };
                        var li = temp.ToList();

                        for (int i = 0; i < li.Count; i++)
                        {
                            StorageInHead sd = db.StorageInHead.SingleOrDefault(u => u.StorageInHeadID == li[i].StorageInHeadID);
                            if (sd != null)
                            {
                                db.StorageInHead.DeleteOnSubmit(sd);
                                db.SubmitChanges();
                            }
                        }

                        //回发任务
                        //原任务
                        var query = db.TaskStorageIn.Where(u => u.StorageInID == _storageInID && u.StorageInType == "正常入库" && u.QCBatch == _QCbatch).OrderBy(a=>a.TaskStorageID);//edit by roro
                        var item = db.TaskStorageIn.OrderBy(a=>a.TaskStorageID).Where(u => u.StorageInID == _storageInID && u.TaskType == "质检" && u.StorageInType == "正常入库" && u.QCBatch == _QCbatch);
                        
                        var li2 = query.ToList();
                        var n =  li2[li2.LastIndexOf(item.ToList().LastOrDefault()) + 2];
                        //var qery = db.TaskStorageIn.Where(u => u.StorageInID == _storageInID && u.TaskType == "质检" && u.StorageInType == "正常入库" && u.QCBatch == _QCbatch);



                        //TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == n.TaskStorageID);

                        TaskStorageIn ts = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskID);
                        ts.TaskState = "已完成";
                        ts.InspectState = "已审核";


                        //新任务
                        TaskStorageIn TSI = new TaskStorageIn();

                        TSI.TaskCreaterID = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
                        TSI.TaskTargetID = n.TaskCreaterID;
                        if (TSI.TaskTargetID == 0)
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('不存在质检用户，请同步AD账户 ')</script>");
                            return;
                        }

                        TSI.StorageInID = Convert.ToInt32(Request.QueryString["StorageInID"]);
                        TSI.StorageInType = "正常入库";
                        TSI.TaskTitle = n.TaskTitle + "(资产组长审核未通过)";
                        TSI.TaskState = "未完成";
                        TSI.TaskDispose = "未废弃";
                        TSI.TaskType = "资产组员";
                        TSI.InspectState = "驳回";

                        //TSI.BatchOfIndex = this.ddlbatch.SelectedItem.Text.ToString();

                        TSI.QCBatch = _QCbatch;


                        TSI.Remark = "审核未通过";
                        var Sev = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                        TSI.CreateTime = Sev.First();

                        db.TaskStorageIn.InsertOnSubmit(TSI);
                        db.SubmitChanges();
                        Response.Redirect("../../default-old.aspx", false);





                    }
                    //存库标识
                    ViewState["Temp"] = flag = true;

                }
                //Response.Redirect("QualifiedManage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&QCBatch=" + Request.QueryString["QCBatch"] + " ");

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

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("../../default-old.aspx", false);
        }


        void btnRefresh_Click(object sender, EventArgs e)
        {

            

        }


        /// <summary>
        /// 返回选中的列表
        /// </summary>
        /// <returns>返回list构成的列表</returns>
        private List<CheckBox> GetCheckedID()
        {
            List<CheckBox> list = new List<CheckBox>();

            foreach (GridViewRow row in this.gv.Rows)
            {

                CheckBox ck = (CheckBox)row.Cells[0].FindControl("SMItem");
                if (ck.Checked)
                {
                    list.Add(ck);
                }
            }
            return list;
        }

        

        /// <summary>
        /// 绑定Gridview
        /// </summary>
        private void BindGridView()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                
                if (!string.IsNullOrEmpty(Request.QueryString["QCBatch"]))
                {
                    QCbatch = Request.QueryString["QCBatch"];
                }
                BoundField bfColumn;
                foreach (var kvp in Titlelist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.gv.Columns.Add(bfColumn);
                }

                this.gv.DataSource = from a in db.StorageInAssets

                                     join b in db.StorageInMain on a.StorageInTest.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.StorageInID equals b.StorageInID

                                     where a.StorageInTest.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.StorageInID == _storageInID && a.StorageInTest.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.BatchIndex == (string.IsNullOrEmpty(QCbatch) ? a.StorageInTest.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.BatchIndex : QCbatch)
                                     select new
                                     {
                                         a.StorageInAssetsID,
                                         b.StorageInCode,
                                         a.StorageInTest.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.MaterialInfo.MaterialName,
                                         a.StorageInTest.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.MaterialInfo.SpecificationModel,
                                         a.StorageInTest.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.MaterialInfo.FinanceCode,
                                         a.StorageInTest.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.ProjectInfo.ProjectName,
                                         a.StorageInTest.TestGentaojian,
                                         a.StorageInTest.TestMetre,
                                         a.StorageInTest.TestTon,
                                         a.CurUnit,
                                         a.UnitPrice,
                                         a.Amount,
                                         a.BillCode,
                                         a.MaterialsAttribute,
                                         a.StorageInTest.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.BatchIndex,
                                         a.StorageInTest.StorageInMaterialsLeader.StorageInMaterials.StorageTime,
                                         Creator = db.EmpInfo.SingleOrDefault(u => u.EmpID == a.Creator).EmpName,
                                         a.Remark,
                                     };
                this.gv.DataBind();
                this.gv.Columns[this.gv.Columns.Count - 1].Visible = false;
                Panel p1 = (Panel)GetControltByMaster("Panel1");
                p1.Controls.Add(this.gv);

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
