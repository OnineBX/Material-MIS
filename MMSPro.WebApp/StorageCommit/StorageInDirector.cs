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
    public class CommitStorageInDirector : System.Web.UI.Page
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
                                     "质检报告号:InspectionReportNum",
                                     "入库单据号:BillCode",
                                     "物资属性:MaterialsAttribute",
                                     "预期使用项目:ProjectName",
                                     "实际到库时间:StorageTime",
                                     "生产厂家:ManufacturerName",
                                     "供应商:SupplierName",
                                     "仓库:StorageName",
                                     "垛位:PileCode",

                                     "资产组长审核:Auditing",
                                     "所属批次:BatchIndex",
                                     "备注:Remark",

                                     "入库类型:ReceivingType",
                                     "入库通知单:StorageInCode",
                                     "入库单号:BillCode",



                                     "stoId:StorageID",
                                     "AssetsID:Assets",
                                     "MaterialsID:Materials",
                                     "Pid:PileID",
                                     "EPid:ExpectedProject",
                                     "Mid:MaterialID",
                                     "manu:ManufacturerID",
                                     "sid:SupplierID",
                                     "ID:StorageInHeadID"

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
                initControl();
                

               


                this.gv = new SPGridView();
                this.gv.AutoGenerateColumns = false;

                BindGridView();

                if (!IsPostBack)
                {
                    ViewState["Temp"] = false;

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
            //发送质检部
            if (btnko.Enabled == false)
            {
                Response.Redirect("TaskCenter.aspx?StorageInID=" + _storageInID + "&&state=主任审核&&storageInType=委外入库&&QCBatch=" + _QCbatch + "&&TaskStorageID=" + _taskID + "");
            }
            else
            {
                if (ViewState["Temp"] != null)
                {
                    if (ViewState["Temp"].ToString() == "True")
                    {
                        Response.Redirect("TaskCenter.aspx?StorageInID=" + _storageInID + "&&state=主任审核&&storageInType=委外入库&&QCBatch=" + _QCbatch + "&&TaskStorageID=" + _taskID + "");
                    }
                    else
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请先保存数据后再发送! ')</script>");
                        return;
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
                }
            }
        }


        private void initControl()
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

                var tp = from a in db.CommitDirector
                         where a.CommitInHead.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.CommitInHead.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.BatchIndex == Request.QueryString["QCBatch"].ToString()
                         select a;
                if (tp.ToArray().Length > 0)
                {
                    btnko.Enabled = false;
                    checkPass.Enabled = false;
                }
            }
        }

        void btnko_Click(object sender, EventArgs e)
        {
            try
            {
                

                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {

                    var tp = from a in db.CommitDirector
                             where a.CommitInHead.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.CommitInHead.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.BatchIndex == Request.QueryString["QCBatch"].ToString()
                             select a;
                    if (tp.ToArray().Length > 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('不能重复插入记录! ')</script>");
                        return;
                    }


                    if (this.checkPass.Checked)
                    {
                        for (int i = 0; i < this.gv.Rows.Count; i++)
                        {

                            CommitDirector sih = new CommitDirector();
                            sih.HeadID = Convert.ToInt32(this.gv.Rows[i].Cells[this.gv.Columns.Count - 1].Text.ToString());
                            sih.Approve = "是";
                            var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                            sih.CreateTime = SevTime.First();
                            sih.Creator = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
                            db.CommitDirector.InsertOnSubmit(sih);
                            db.SubmitChanges();


                            //写入线下数据库
                            TableOfStocks tos = new TableOfStocks();
                            tos.StorageInID = Convert.ToInt32(Request.QueryString["StorageInID"]);
                            tos.StorageInType = "委外入库";
                            tos.MaterialID = Convert.ToInt32(this.gv.Rows[i].Cells[this.gv.Columns.Count - 4].Text.ToString());
                            tos.MaterialCode = "N/A";
                            tos.QuantityGentaojian = Convert.ToDecimal(this.gv.Rows[i].Cells[4].Text.ToString());
                            tos.QuantityMetre = Convert.ToDecimal(this.gv.Rows[i].Cells[5].Text.ToString());
                            tos.QuantityTon = Convert.ToDecimal(this.gv.Rows[i].Cells[6].Text.ToString());
                            tos.CurUnit = this.gv.Rows[i].Cells[7].Text.ToString();
                            tos.UnitPrice =  Convert.ToDecimal(this.gv.Rows[i].Cells[8].Text.ToString());
                            tos.Amount = Convert.ToDecimal(this.gv.Rows[i].Cells[9].Text.ToString());
                            tos.ExpectedProject = Convert.ToInt32(this.gv.Rows[i].Cells[this.gv.Columns.Count - 5].Text.ToString());
                            tos.Remark = this.gv.Rows[i].Cells[12].Text.ToString();
                            tos.BatchIndex = _QCbatch;
                            tos.ManufacturerID = Convert.ToInt32(this.gv.Rows[i].Cells[this.gv.Columns.Count - 3].Text.ToString());
                            tos.SupplierID = Convert.ToInt32(this.gv.Rows[i].Cells[this.gv.Columns.Count - 2].Text.ToString());
                            tos.PileID = Convert.ToInt32(this.gv.Rows[i].Cells[this.gv.Columns.Count - 6].Text.ToString());
                            tos.StorageID = Convert.ToInt32(this.gv.Rows[i].Cells[this.gv.Columns.Count - 9].Text.ToString());
                            tos.MaterialsManager = Convert.ToInt32(this.gv.Rows[i].Cells[this.gv.Columns.Count - 7].Text.ToString());
                            tos.AssetsManager = Convert.ToInt32(this.gv.Rows[i].Cells[this.gv.Columns.Count - 8].Text.ToString());
                            

                            tos.ReceivingTypeName =this.gv.Rows[i].Cells[this.gv.Columns.Count - 12].Text.ToString();
                            tos.StorageInCode=this.gv.Rows[i].Cells[this.gv.Columns.Count - 11].Text.ToString();
                            tos.BillCode = this.gv.Rows[i].Cells[this.gv.Columns.Count - 10].Text.ToString();

                            tos.StorageTime = Convert.ToDateTime(this.gv.Rows[i].Cells[14].Text.ToString());
                            var Time = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                            tos.CreateTime = Time.First();
                            tos.Creator = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);

                            db.TableOfStocks.InsertOnSubmit(tos);
                            db.SubmitChanges();

                            //修改人物完成状态
                            TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskID);
                            tsi.TaskState = "已完成";
                            db.SubmitChanges();

                        }
                        btnko.Enabled = false;
                        checkPass.Enabled = false;


                       

                        
                    }
                    else
                    { 
                        //审核不通过，回退操作
                        
                        //删除写入的数据
                        var temp = from a in db.CommitDirector
                                   where a.CommitInHead.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.CommitInHead.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.BatchIndex == Request.QueryString["QCBatch"].ToString()
                                   select new { a.StorageInDirectorID };
                        var li = temp.ToList();

                        for (int i = 0; i < li.Count; i++)
                        {
                            CommitDirector sd = db.CommitDirector.SingleOrDefault(u => u.StorageInDirectorID == li[i].StorageInDirectorID);
                            if (sd != null)
                            {
                                db.CommitDirector.DeleteOnSubmit(sd);
                                db.SubmitChanges();
                            }
                        }

                        //回发任务
                        //原任务
                        TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID ==_taskID);
                        tsi.TaskState = "已完成";
                        tsi.InspectState = "已审核";


                        //新任务
                        TaskStorageIn TSI = new TaskStorageIn();

                        TSI.TaskCreaterID = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
                        TSI.TaskTargetID = tsi.TaskCreaterID;
                        if (TSI.TaskTargetID == 0)
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('不存在质检用户，请同步AD账户 ')</script>");
                            return;
                        }

                        TSI.StorageInID = Convert.ToInt32(Request.QueryString["StorageInID"]);
                        TSI.StorageInType = "委外入库";
                        TSI.TaskTitle = tsi.TaskTitle + "(主任审批未通过)";
                        TSI.TaskState = "未完成";
                        TSI.TaskDispose = "未废弃";
                        TSI.TaskType = "资产组长";
                        TSI.InspectState = "驳回";

                        //TSI.BatchOfIndex = this.ddlbatch.SelectedItem.Text.ToString();

                        TSI.QCBatch =_QCbatch;


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

                this.gv.DataSource = from a in db.CommitInHead

                                     join b in db.CommitInMain on a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.StorageInID equals b.StorageInID

                                     where a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.StorageInID == _storageInID && a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.BatchIndex == (string.IsNullOrEmpty(QCbatch) ? a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.BatchIndex : QCbatch)
                                     select new
                                     {
                                         a.StorageInHeadID,
                                         b.StorageInCode,
                                         b.ReceivingType,
                                         a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.MaterialInfo.MaterialName,
                                         a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.MaterialInfo.SpecificationModel,
                                         a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.MaterialInfo.FinanceCode,
                                         a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.ProjectInfo.ProjectName,
                                         a.CommitInAssets.CommitInTest.TestGentaojian,
                                         a.CommitInAssets.CommitInTest.TestMetre,
                                         a.CommitInAssets.CommitInTest.TestTon,
                                         a.CommitInAssets.CurUnit,
                                         a.CommitInAssets.UnitPrice,
                                         a.CommitInAssets.Amount,
                                         a.CommitInAssets.CommitInTest.InspectionReportNum,
                                         a.CommitInAssets.BillCode,
                                         a.CommitInAssets.MaterialsAttribute,
                                         a.Auditing,
                                         a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.Manufacturer.ManufacturerName,
                                         a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.SupplierInfo.SupplierName,
                                         a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.SupplierID,
                                         a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.ManufacturerID,
                                         a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.MaterialID,
                                         a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.ExpectedProject,
                                         a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.PileInfo.PileCode,
                                         a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.PileInfo.StorageInfo.StorageName,
                                         a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.PileID,
                                         a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.PileInfo.StorageInfo.StorageID,
                            
                                        
                                         Assets=a.CommitInAssets.Creator,
                                         Materials = a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.Creator,
                                         a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.BatchIndex,
                                         a.CommitInAssets.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.StorageTime,
                                         Creator = db.EmpInfo.SingleOrDefault(u => u.EmpID == a.Creator).EmpName,
                                         a.Remark,
                                     };
                this.gv.DataBind();
                this.gv.Columns[this.gv.Columns.Count - 1].Visible = false;
                this.gv.Columns[this.gv.Columns.Count - 2].Visible = false;
                this.gv.Columns[this.gv.Columns.Count - 3].Visible = false;
                this.gv.Columns[this.gv.Columns.Count - 4].Visible = false;
                this.gv.Columns[this.gv.Columns.Count - 5].Visible = false;
                this.gv.Columns[this.gv.Columns.Count - 6].Visible = false;
                this.gv.Columns[this.gv.Columns.Count - 7].Visible = false;
                this.gv.Columns[this.gv.Columns.Count - 8].Visible = false;
                this.gv.Columns[this.gv.Columns.Count - 9].Visible = false;
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
