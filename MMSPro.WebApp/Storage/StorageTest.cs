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
    public class StorageTest:System.Web.UI.Page
    {
        MMSProDBDataContext db;
        SPGridView gv;
        TextBox txtOpinion;
        Button btnOK;
        Button btnko;
        Button btnmodify;
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
                                     "根/套/件(实际):RealGentaojian",
                                     "米(实际):RealMetre",
                                     "吨(实际):RealTon",
                                     "预期使用项目:ProjectName",
                                     "预期到库时间:ExpectedTime",
                                     "所属批次:BatchIndex",
                                     "备注:Remark",
                                     "ID:MaterialsLeaderID"
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
                    dataLoad();
                }


                
               
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
            //发送资产组
            if (btnko.Enabled == false)
            {
                Response.Redirect("TaskCenter.aspx?StorageInID=" + _storageInID + "&&state=资产组员&&storageInType=正常入库&&QCBatch=" + _QCbatch + "&&TaskStorageID=" + _taskID + "");
            }
            else
            {
                if (ViewState["Temp"] != null)
                {
                    if (ViewState["Temp"].ToString() == "True")
                    {
                        Response.Redirect("TaskCenter.aspx?StorageInID=" + _storageInID + "&&state=资产组员&&storageInType=正常入库&&QCBatch=" + _QCbatch + "&&TaskStorageID=" + _taskID + "");
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
                    this.btnmodify.Enabled = false;
                    this._flag = false;
                }
            }
        }

        

        void ddlStorage_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlStorage = (DropDownList)sender;//获取现在的事件触发者
            GridViewRow gvr = (GridViewRow)ddlStorage.NamingContainer;//同属于在一个NamingContainer下
            DropDownList ddlPile = (DropDownList)gvr.Cells[19].Controls[0];//找到字段的DropDownList
            ddlPile.Items.Clear();
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    ddlPile.DataSource = from p in db.PileInfo
                                         where p.StorageID == Convert.ToInt32(ddlStorage.SelectedValue)
                                         select new
                                         {
                                             p.PileID,
                                             p.PileName
                                         };
                    ddlPile.DataTextField = "PileName";
                    ddlPile.DataValueField = "PileID";
                    ddlPile.DataBind();
                    ddlPile.Items.Insert(0, new ListItem("--请选择--", "0"));
                }
            }
            catch
            {
                ddlPile.Items.Insert(0, new ListItem("--请选择--", "0"));
                ddlPile.SelectedValue = "0";
            }
        }

        /// <summary>
        /// 绑定信息是否与线下信息一致
        /// </summary>
        /// <param name="e">e</param>
        /// <param name="name">DropDownList对象</param>
        /// <param name="cellIdx">单元格在GridView中对应行的索引</param>
        private void BindYesorNo(GridViewRowEventArgs e,DropDownList name, int cellIdx)
        {
            name = (DropDownList)e.Row.Cells[cellIdx].Controls[0];
            List<string> dataText = new List<string>();
            List<string> dataValue = new List<string>();
            dataText.Add("--请选择--");
            dataText.Add("是");
            dataText.Add("否");
            dataValue.Add("0");
            dataValue.Add("1");
            dataValue.Add("2");



            Dictionary<string, string> dic = new Dictionary<string, string>();
            for (int i = 0; i < dataText.Count && i < dataValue.Count; ++i)
            {
                dic.Add(dataText[i], dataValue[i]);
            }

            name.DataSource = dic;
            name.DataTextField = "Key";
            name.DataValueField = "Value";
            name.DataBind();



        }




        private void initControl(bool flag_)
        {

            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");

            //send
            ToolBarButton tbarbtnsend = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnsend.ID = "sendRow";
            tbarbtnsend.Text = "发送到资产组";
            tbarbtnsend.ImageUrl = "/_layouts/images/edit.gif";
            tbarbtnsend.Click += new EventHandler(tbarbtnsend_Click);
            tbarbtnsend.Visible = flag_;
            tbarTop.Buttons.Controls.Add(tbarbtnsend);

            ToolBarButton tbarbtnUploadinfo = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnUploadinfo.ID = "upload";
            tbarbtnUploadinfo.Text = "上传报告";
            tbarbtnUploadinfo.ImageUrl = "/_layouts/images/BACK.GIF";
            tbarbtnUploadinfo.Click += new EventHandler(tbarbtnUploadinfo_Click);
            tbarTop.Buttons.Controls.Add(tbarbtnUploadinfo);
            
           

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

        void tbarbtnUploadinfo_Click(object sender, EventArgs e)
        {
            if (this.btnko.Enabled == false)
            {
                Response.Redirect("StorageTestUpload.aspx?StorageInID=" + _storageInID + "&&state=质检&&storageInType=正常入库&&QCBatch=" + _QCbatch + "&&TaskStorageID=" + _taskID + "");
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请先提交数据后再上传!')</script>");
                return;
            }
        }


        private void control()
        {
            plinfo = (Panel)GetControltByMaster("plinfo");
            lblInfo = (Label)GetControltByMaster("lblInfo");
            txtOpinion = (TextBox)GetControltByMaster("txtOpinion");
            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Text = "完成审核";
            btnOK.Click += new EventHandler(btnOK_Click);

            btnko = (Button)GetControltByMaster("btnko");
            btnko.Click += new EventHandler(btnko_Click);

            btnmodify = (Button)GetControltByMaster("btnmodify");
            btnmodify.Click += new EventHandler(btnmodify_Click);
        }

        void btnmodify_Click(object sender, EventArgs e)
        {
            try
            {


                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {

                    //更新数据
                        var tep = from a in db.StorageInTest
                                  where a.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"].ToString()) && a.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.BatchIndex == _QCbatch
                                  orderby a.StorageInTestID ascending
                                  select new { a.StorageInTestID };
                        var li = tep.ToList();


                    for (int i = 0; i < this.gv.Rows.Count; i++)
                    {

                        StorageInTest st = new StorageInTest();
                        st = db.StorageInTest.SingleOrDefault(u => u.StorageInTestID == li[i].StorageInTestID);


                        st.MaterialsLeaderID = Convert.ToInt32(this.gv.Rows[i].Cells[this.gv.Columns.Count - 1].Text.ToString());
                        TextBox gentaojian_qualified = (TextBox)(this.gv.Rows[i].Cells[5].Controls[0]);
                        st.TestGentaojian = Convert.ToDecimal(gentaojian_qualified.Text.Trim());
                        TextBox gentaojian_unqualified = (TextBox)(this.gv.Rows[i].Cells[6].Controls[0]);
                        st.FailedGentaojian = Convert.ToDecimal(gentaojian_unqualified.Text.Trim());

                        TextBox metre_qualified = (TextBox)(this.gv.Rows[i].Cells[8].Controls[0]);
                        st.TestMetre = Convert.ToDecimal(metre_qualified.Text.Trim());
                        TextBox metre_unqualified = (TextBox)(this.gv.Rows[i].Cells[9].Controls[0]);
                        st.FailedMetre = Convert.ToDecimal(metre_unqualified.Text.Trim());


                        TextBox ton_qualified = (TextBox)(this.gv.Rows[i].Cells[11].Controls[0]);
                        st.TestTon = Convert.ToDecimal(ton_qualified.Text.Trim());
                        TextBox ton_unqualified = (TextBox)(this.gv.Rows[i].Cells[12].Controls[0]);
                        st.FailedTon = Convert.ToDecimal(ton_unqualified.Text.Trim());

                        TextBox reportNum = (TextBox)(this.gv.Rows[i].Cells[13].Controls[0]);
                        st.InspectionReportNum = reportNum.Text.Trim();

                        st.FileNameStr = "reportqualified";

                        var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                        st.CreateTime = SevTime.First();
                        st.Creator = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
                        db.SubmitChanges();
                       

                    }

                    //存库标识
                    ViewState["Temp"] = flag =true;

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

        private void dataLoad()
        {
            if (!string.IsNullOrEmpty(Request.QueryString["QCBatch"]))
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    var tp = from a in db.StorageInTest
                             where a.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.BatchIndex == Request.QueryString["QCBatch"].ToString()
                             select a;
                    if (tp.ToArray().Length > 0)
                    {
                        this.btnko.Enabled = false;
                        //如果任务已完成修改控件状态
                        if (this._flag == true)
                        {
                            this.btnmodify.Enabled = true;
                        }
                        else
                        {
                            this.btnmodify.Enabled = false;
                        }


                        //初始化质检合格表
                        var cg = from u in db.StorageInTest
                                 where u.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && u.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.BatchIndex == Request.QueryString["QCBatch"].ToString()
                                 orderby u.StorageInTestID ascending
                                 select new {
                                              u.StorageInTestID,
                                              u.TestGentaojian,
                                              u.TestMetre,
                                              u.TestTon,
                                              u.FailedGentaojian,
                                              u.FailedMetre,
                                              u.FailedTon,
                                              u.InspectionReportNum,
                                              u.FileNameStr,
                                            };


                        var li = cg.ToList();
                        for (int i = 0; i < this.gv.Rows.Count; i++)
                        {

                            TextBox gentaojian = (TextBox)(this.gv.Rows[i].Cells[5].Controls[0]);
                            gentaojian.Text = li[i].TestGentaojian.ToString();
                            TextBox f_gentaojian = (TextBox)(this.gv.Rows[i].Cells[6].Controls[0]);
                            f_gentaojian.Text = li[i].FailedGentaojian.ToString();

                            TextBox metre = (TextBox)(this.gv.Rows[i].Cells[8].Controls[0]);
                            metre.Text = li[i].TestMetre.ToString();
                            TextBox f_metre = (TextBox)(this.gv.Rows[i].Cells[9].Controls[0]);
                            f_metre.Text = li[i].FailedMetre.ToString();

                            TextBox ton = (TextBox)(this.gv.Rows[i].Cells[11].Controls[0]);
                            ton.Text = li[i].TestTon.ToString();
                            TextBox f_ton = (TextBox)(this.gv.Rows[i].Cells[12].Controls[0]);
                            f_ton.Text = li[i].FailedTon.ToString();

                            TextBox reportNum = (TextBox)(this.gv.Rows[i].Cells[13].Controls[0]);
                            reportNum.Text = li[i].InspectionReportNum.ToString(); 
                 
                        }



                    }
                    else
                    {
                        this.btnko.Enabled = true;
                        this.btnmodify.Enabled = false;
                    }


                }
            }
            else
            {
                this.btnko.Enabled = true;
                this.btnmodify.Enabled = false;
            }
        }



        void btnko_Click(object sender, EventArgs e)
        {
            try
            {
                

                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {

                    var tp = from a in db.StorageInTest
                             where a.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.StorageInMaterialsLeader.StorageInMaterials.StorageProduce.BatchIndex == Request.QueryString["QCBatch"].ToString()
                             select a;
                    if (tp.ToArray().Length > 0)
                    {
                        //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('不能重复插入记录! ')</script>");
                        this.btnko.Enabled = false;
                        this.btnmodify.Enabled = true;
                        return;
                    }



                    for (int i = 0; i < this.gv.Rows.Count; i++)
                    {

                        StorageInTest st = new StorageInTest();
                        st.MaterialsLeaderID =Convert.ToInt32( this.gv.Rows[i].Cells[this.gv.Columns.Count - 1].Text.ToString());
                        TextBox gentaojian_qualified = (TextBox)(this.gv.Rows[i].Cells[5].Controls[0]);
                        st.TestGentaojian = Convert.ToDecimal(gentaojian_qualified.Text.Trim());
                        TextBox gentaojian_unqualified = (TextBox)(this.gv.Rows[i].Cells[6].Controls[0]);
                        st.FailedGentaojian = Convert.ToDecimal(gentaojian_unqualified.Text.Trim());

                        TextBox metre_qualified = (TextBox)(this.gv.Rows[i].Cells[8].Controls[0]);
                        st.TestMetre = Convert.ToDecimal(metre_qualified.Text.Trim());
                        TextBox metre_unqualified = (TextBox)(this.gv.Rows[i].Cells[9].Controls[0]);
                        st.FailedMetre = Convert.ToDecimal(metre_unqualified.Text.Trim());


                        TextBox ton_qualified = (TextBox)(this.gv.Rows[i].Cells[11].Controls[0]);
                        st.TestTon = Convert.ToDecimal(ton_qualified.Text.Trim());
                        TextBox ton_unqualified = (TextBox)(this.gv.Rows[i].Cells[12].Controls[0]);
                        st.FailedTon = Convert.ToDecimal(ton_unqualified.Text.Trim());

                        TextBox reportNum = (TextBox)(this.gv.Rows[i].Cells[13].Controls[0]);
                        st.InspectionReportNum = reportNum.Text.Trim();

                        st.FileNameStr = "reportqualified";

                        var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                        st.CreateTime = SevTime.First();
                        st.Creator = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
                        db.StorageInTest.InsertOnSubmit(st);
                        db.SubmitChanges();

                        //存库标识
                        ViewState["Temp"] = flag = true;

                    }
                    this.btnko.Enabled = false;

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

        void btnOK_Click(object sender, EventArgs e)
        {
            //修改审核状态为初始值
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
               
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

        protected void tbarbtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
              



                if (!string.IsNullOrEmpty(Request.QueryString["TaskStorageID"]))
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "Messages", "<script>alert('回退任务不能新增批次.')</script>");
                    return;
                }
                else
                {
                    Response.Redirect("StorageDetailedCreate.aspx?StorageInID=" + _storageInID + "");
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));
            }

          

            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");

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
        

                TemplateField reportNum = new TemplateField();
                reportNum.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "MaterialsLeaderID", "txtgentaojianNum");
                reportNum.HeaderTemplate = new MulTextBoxTemplate("根/台/套/件(合格)", DataControlRowType.Header);
                reportNum.ItemStyle.Width = 150;
                this.gv.Columns.Insert(5, reportNum);


                TemplateField f_reportNum = new TemplateField();
                f_reportNum.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "MaterialsLeaderID", "txtgentaojianNums");
                f_reportNum.HeaderTemplate = new MulTextBoxTemplate("根/台/套/件(不合格)", DataControlRowType.Header);
                f_reportNum.ItemStyle.Width = 150;
                this.gv.Columns.Insert(6, f_reportNum);


                TemplateField metreNum = new TemplateField();
                metreNum.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "MaterialsLeaderID", "txtmetreNum");
                metreNum.HeaderTemplate = new MulTextBoxTemplate("米(合格)", DataControlRowType.Header);
                metreNum.ItemStyle.Width = 150;
                this.gv.Columns.Insert(8, metreNum);

                TemplateField f_metreNum = new TemplateField();
                f_metreNum.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "MaterialsLeaderID", "txtmetreNums");
                f_metreNum.HeaderTemplate = new MulTextBoxTemplate("米(不合格)", DataControlRowType.Header);
                f_metreNum.ItemStyle.Width = 150;
                this.gv.Columns.Insert(9, f_metreNum);



                TemplateField tonNum = new TemplateField();
                tonNum.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "MaterialsLeaderID", "txttonNum");
                tonNum.HeaderTemplate = new MulTextBoxTemplate("吨(合格)", DataControlRowType.Header);
                tonNum.ItemStyle.Width = 150;
                this.gv.Columns.Insert(11, tonNum);

                TemplateField f_tonNum = new TemplateField();
                f_tonNum.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "MaterialsLeaderID", "txttonNums");
                f_tonNum.HeaderTemplate = new MulTextBoxTemplate("吨(不合格)", DataControlRowType.Header);
                f_tonNum.ItemStyle.Width = 150;
                this.gv.Columns.Insert(12, f_tonNum);

                TemplateField testReport = new TemplateField();
                testReport.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "MaterialsLeaderID", "txtreportNum");
                testReport.HeaderTemplate = new MulTextBoxTemplate("质检报告号", DataControlRowType.Header);
                testReport.ItemStyle.Width = 150;
                this.gv.Columns.Insert(13, testReport);

                //HyperLinkField hlTask = new HyperLinkField();
                //hlTask.HeaderText = "上传质检报告";
                //this.gv.Columns.Insert(14, hlTask);


                this.gv.DataSource = from a in db.StorageInMaterialsLeader
                                     
                                     join b in db.StorageInMain on a.StorageInMaterials.StorageProduce.StorageInID equals b.StorageInID

                                     where a.StorageInMaterials.StorageProduce.StorageInID == _storageInID && a.StorageInMaterials.StorageProduce.BatchIndex == (string.IsNullOrEmpty(QCbatch) ? a.StorageInMaterials.StorageProduce.BatchIndex : QCbatch)
                                     select new
                                     {
                                        a.MaterialsLeaderID,
                                        a.StorageInMaterials.StorageProduce.MaterialInfo.MaterialName,
                                        a.StorageInMaterials.StorageProduce.MaterialInfo.SpecificationModel,
                                        a.StorageInMaterials.StorageProduce.MaterialInfo.FinanceCode,
                                        b.StorageInCode,
                                        a.StorageInMaterials.RealGentaojian,
                                        a.StorageInMaterials.RealMetre,
                                        a.StorageInMaterials.RealTon,
                                        a.StorageInMaterials.StorageProduce.ProjectInfo.ProjectName,
                                        a.StorageInMaterials.StorageProduce.ExpectedTime,
                                        a.StorageInMaterials.StorageProduce.BatchIndex,
                                        a.Remark
                                     };
                this.gv.RowDataBound += new GridViewRowEventHandler(gv_RowDataBound);
                this.gv.DataBind();
                this.gv.Columns[this.gv.Columns.Count - 1].Visible = false;
               
                Panel p1 = (Panel)GetControltByMaster("Panel1");
                p1.Controls.Add(this.gv);

            }

        }

        void gv_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //e.Row.Cells[14].Text = string.Format("<a href=\"javaScript:onClick=window.showModalDialog(encodeURI('../DocAndIndexManager/UploadFile.aspx?detailsID=" + e.Row.Cells[this.gv.Columns.Count - 1].Text.Trim() + "&&Type=正常入库&&ReportNum=" + e.Row.Cells[13].Text.Trim() + "'),'0','resizable:true;dialogWidth:800px;dialogHeight:600px')\">上传报告</a>", int.Parse(e.Row.Cells[19].Text));
                //e.Row.Cells[14].Text = string.Format("<a href=\"javaScript:onClick=window.open('../DocAndIndexManager/UploadFile.aspx?detailsID=" + e.Row.Cells[this.gv.Columns.Count - 1].Text.Trim() + "&&Type=正常入库&&ReportNum=" + (e.Row.Cells[13].Controls[0] as TextBox).Text.Trim() + "','newwindow','height=800, width=750, toolbar =no, menubar=no, scrollbars=yes, resizable=no, location=no, status=no');window.location.reload();\">上传报告</a>", int.Parse(e.Row.Cells[19].Text));
                //Response.AddHeader("Refresh", "0"); 

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
