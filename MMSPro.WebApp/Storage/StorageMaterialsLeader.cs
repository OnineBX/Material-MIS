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
    public class StorageMaterialsLeader:System.Web.UI.Page
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
                                     "预期使用项目:ProjectName",
                                     "根/套/件(实际):RealGentaojian",
                                     "米(实际):RealMetre",
                                     "吨(实际):RealTon",
                                     "供应商:SupplierName",
                                     "供应商信息是否一致:Supplier",
                                     "生产厂家:ManufacturerName",
                                     "生产厂家信息是否一致:IsManufacturer",
                                     "资料是否齐全:Data",
                                     "制造标准是否一致:Standard",
                                     "配件是否齐全:Parts",
                                     "外观是否完好:Appearance",
                                     "所属仓库:StorageName",
                                     "所属垛位:PileName",
                                     "所属批次:BatchIndex",
                                     "实际到库时间:StorageTime",
                                     "物资管理员:Creator",
                                     "ID:StorageInMaterialsID"
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
            //发送质检部
            if (btnko.Enabled == false)
            {
                Response.Redirect("TaskCenter.aspx?StorageInID=" + _storageInID + "&&state=质检&&storageInType=正常入库&&QCBatch=" + _QCbatch + "&&TaskStorageID=" + _taskID + "");
            }
            else
            {
                if (ViewState["Temp"] != null)
                {
                    if (ViewState["Temp"].ToString() == "True")
                    {
                        Response.Redirect("TaskCenter.aspx?StorageInID=" + _storageInID + "&&state=质检&&storageInType=正常入库&&QCBatch=" + _QCbatch + "&&TaskStorageID=" + _taskID + "");
                    }
                    else
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请先保存数据后再发送! ')</script>");
                        return;
                    }
                }
            }

            
        
        }

       


        private void initControl(bool flag_)
        {

            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");

            //send
            ToolBarButton tbarbtnsend = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnsend.ID = "sendRow";
            tbarbtnsend.Text = "发送质检部质检";
            tbarbtnsend.ImageUrl = "/_layouts/images/edit.gif";
            tbarbtnsend.Click += new EventHandler(tbarbtnsend_Click);
            tbarbtnsend.Visible = flag_;
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

                var tp = from a in db.StorageInMaterialsLeader
                         where a.StorageInMaterials.StorageProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.StorageInMaterials.StorageProduce.BatchIndex == Request.QueryString["QCBatch"].ToString()
                         select a;
                if (tp.ToArray().Length > 0)
                {
                    btnko.Enabled = false;
                    checkPass.Enabled = false;
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
                    this._flag = false;
                }
            }
        }

        void btnko_Click(object sender, EventArgs e)
        {
            try
            {
                

                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {

                    var tp = from a in db.StorageInMaterialsLeader
                             where a.StorageInMaterials.StorageProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.StorageInMaterials.StorageProduce.BatchIndex == Request.QueryString["QCBatch"].ToString()
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

                            StorageInMaterialsLeader sil = new StorageInMaterialsLeader();
                            sil.MaterialsID = Convert.ToInt32(this.gv.Rows[i].Cells[this.gv.Columns.Count - 1].Text.ToString());
                            sil.Auditing = "是";
                            var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                            sil.CreateTime = SevTime.First();
                            sil.Creator = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
                            db.StorageInMaterialsLeader.InsertOnSubmit(sil);
                            db.SubmitChanges();

                        }
                        btnko.Enabled = false;
                        checkPass.Enabled = false;
                        //存库标识
                        ViewState["Temp"] = flag = true;
                    }
                    else
                    { 
                        //不审核库，回退操作
                        //删除写入的数据
                        var temp = from a in db.StorageInMaterialsLeader
                                   where a.StorageInMaterials.StorageProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.StorageInMaterials.StorageProduce.BatchIndex == Request.QueryString["QCBatch"].ToString()
                                   select new { a.MaterialsLeaderID };
                        var li = temp.ToList();

                        for (int i = 0; i < li.Count; i++)
                        {
                            StorageInMaterialsLeader sd = db.StorageInMaterialsLeader.SingleOrDefault(u => u.MaterialsLeaderID == li[i].MaterialsLeaderID);
                            if (sd != null)
                            {
                                db.StorageInMaterialsLeader.DeleteOnSubmit(sd);
                                db.SubmitChanges();
                            }
                        }

                        //回发任务
                        //原任务
                        TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskID);
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
                        TSI.StorageInType = "正常入库";
                        TSI.TaskTitle = tsi.TaskTitle + "(物资组长审核未通过)";
                        TSI.TaskState = "未完成";
                        TSI.TaskDispose = "未废弃";
                        TSI.TaskType = "物资组员";
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

                //添加选择列
        

                //TemplateField reportNum = new TemplateField();
                //reportNum.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "StorageInProduceID", "txtgentaojianNum");
                //reportNum.HeaderTemplate = new MulTextBoxTemplate("根/台/套/件(实际)", DataControlRowType.Header);
                //reportNum.ItemStyle.Width = 150;
                //this.gv.Columns.Insert(5, reportNum);

                //TemplateField metreNum = new TemplateField();
                //metreNum.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "StorageInProduceID", "txtmetreNum");
                //metreNum.HeaderTemplate = new MulTextBoxTemplate("米(实际)", DataControlRowType.Header);
                //metreNum.ItemStyle.Width = 150;
                //this.gv.Columns.Insert(7, metreNum);

                //TemplateField tonNum = new TemplateField();
                //tonNum.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "StorageInProduceID", "txttonNum");
                //tonNum.HeaderTemplate = new MulTextBoxTemplate("吨(实际)", DataControlRowType.Header);
                //tonNum.ItemStyle.Width = 150;
                //this.gv.Columns.Insert(9, tonNum);

                //TemplateField Supplier = new TemplateField();
                //Supplier.HeaderText = "供应商";
                //Supplier.ItemTemplate = new MulDropDownListTemplate(DataControlRowType.DataRow, "ddlSupplier");
                //this.gv.Columns.Insert(10, Supplier);



                this.gv.DataSource = from a in db.StorageInMaterials

                                     join b in db.StorageInMain on a.StorageProduce.StorageInID equals b.StorageInID

                                     where a.StorageProduce.StorageInID == _storageInID && a.StorageProduce.BatchIndex == (string.IsNullOrEmpty(QCbatch) ? a.StorageProduce.BatchIndex : QCbatch)
                                     select new
                                     {
                                         a.StorageInMaterialsID,
                                         b.StorageInCode,
                                         a.StorageProduce.MaterialInfo.MaterialName,
                                         a.StorageProduce.MaterialInfo.SpecificationModel,
                                         a.StorageProduce.MaterialInfo.FinanceCode,
                                         a.StorageProduce.ProjectInfo.ProjectName,

                                         a.RealGentaojian,
                                         a.RealMetre,
                                         a.RealTon,
                                         a.SupplierInfo.SupplierName,
                                         a.Supplier,
                                         a.Manufacturer.ManufacturerName,
                                         a.IsManufacturer,
                                         a.Data,
                                         a.Standard,
                                         a.Parts,
                                         a.Appearance,
                                         a.PileInfo.StorageInfo.StorageName,
                                         a.PileInfo.PileName,
                                         a.StorageProduce.BatchIndex,
                                         a.StorageTime,
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
