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
    public class CommitStorageMaterials : System.Web.UI.Page
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
                                     "根/套/件(预期):QuantityGentaojian",
                                     "米(预期):QuantityMetre",
                                     "吨(预期):QuantityTon",
                                     "预期使用项目:ProjectName",
                                     "预期到库时间:ExpectedTime",
                                     "所属批次:BatchIndex",
                                     "备注:Remark",
                                     "ID:StorageInProduceID"
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
                this.gv.RowDataBound += new GridViewRowEventHandler(gv_RowDataBound);

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
            //发送物资组长
            if (btnko.Enabled == false)
            {
                Response.Redirect("TaskCenter.aspx?StorageInID=" + _storageInID + "&&state=物资组长&&storageInType=委外入库&&QCBatch=" + _QCbatch + "&&TaskStorageID=" + _taskID + "");
            }
            else
            {
                if (ViewState["Temp"] != null)
                {
                    if (ViewState["Temp"].ToString() == "True")
                    {
                        Response.Redirect("TaskCenter.aspx?StorageInID=" + _storageInID + "&&state=物资组长&&storageInType=委外入库&&QCBatch=" + _QCbatch + "&&TaskStorageID=" + _taskID + "");
                    }
                    else
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请先保存数据后再发送! ')</script>");
                        return;
                    }
                }
            }

            
        
        }

       

        void gv_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    //绑定供应商
                    DropDownList ddlSupplier = (DropDownList)e.Row.Cells[12].Controls[0];
                    ddlSupplier.Items.Clear();
                    ddlSupplier.DataSource = from s in db.SupplierInfo
                                            select new
                                            {
                                                s.SupplierID,
                                                s.SupplierName,
                                            };
                    ddlSupplier.DataTextField = "SupplierName";
                    ddlSupplier.DataValueField = "SupplierID";
                    ddlSupplier.DataBind();
                    ddlSupplier.AutoPostBack = false;
                    ddlSupplier.Items.Insert(0, new ListItem("--请选择--", "0"));

                    //绑定判断类型
                    BindYesorNo(e,ddlyn,13);

                    //绑定生产厂商

                    DropDownList ddlManufacturer = (DropDownList)e.Row.Cells[14].Controls[0];
                    ddlManufacturer.Items.Clear();
                    ddlManufacturer.DataSource = from s in db.Manufacturer
                                            select new
                                            {
                                                s.ManufacturerID,
                                                s.ManufacturerName,
                                            };
                    ddlManufacturer.DataTextField = "ManufacturerName";
                    ddlManufacturer.DataValueField = "ManufacturerID";
                    ddlManufacturer.AutoPostBack = false;
                    ddlManufacturer.DataBind();
                    ddlManufacturer.Items.Insert(0, new ListItem("--请选择--", "0"));
                    //绑定判断类型
                    BindYesorNo(e,ddlmf,15);
                    //资料是否一致
                    
                    BindYesorNo(e,ddldata,16);
                    //制造标准是否与采购合同一致
                    BindYesorNo(e, ddlstandar, 17);
                    //配件是否齐全
                    BindYesorNo(e, ddlparts, 18);
                    //外观是否完好
                    BindYesorNo(e, ddlappearance, 19);


                    //绑定仓库
                    DropDownList ddlStorage = (DropDownList)e.Row.Cells[10].Controls[0];
                    ddlStorage.SelectedIndexChanged += new EventHandler(ddlStorage_SelectedIndexChanged);
                    ddlStorage.Items.Clear();
                    ddlStorage.DataSource = from s in db.StorageInfo
                                            select new
                                            {
                                                s.StorageID,
                                                s.StorageName
                                            };
                    ddlStorage.DataTextField = "StorageName";
                    ddlStorage.DataValueField = "StorageID";
                    ddlStorage.DataBind();
                    ddlStorage.Items.Insert(0, new ListItem("--请选择--", "0"));

                    
                  


                }

            }
        }

        void ddlStorage_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlStorage = (DropDownList)sender;//获取现在的事件触发者
            GridViewRow gvr = (GridViewRow)ddlStorage.NamingContainer;//同属于在一个NamingContainer下
            DropDownList ddlPile = (DropDownList)gvr.Cells[11].Controls[0];//找到字段的DropDownList
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
                    ddlPile.AutoPostBack = false;
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
            name.AutoPostBack = false;
            name.DataBind();



        }




        private void initControl(bool flag_)
        {

            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");

            //send
            ToolBarButton tbarbtnsend = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnsend.ID = "sendRow";
            tbarbtnsend.Text = "发送物资组长审核";
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
                        var tep = from a in db.CommitInMaterials
                                  where a.CommitProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"].ToString()) && a.CommitProduce.BatchIndex == _QCbatch
                                  orderby a.StorageInMaterialsID ascending
                                  select new { a.StorageInMaterialsID };
                        var li = tep.ToList();


                    for (int i = 0; i < this.gv.Rows.Count; i++)
                    {

                        CommitInMaterials si = new CommitInMaterials();
                        si = db.CommitInMaterials.SingleOrDefault(u => u.StorageInMaterialsID == li[i].StorageInMaterialsID);
                        si.ProduceID = Convert.ToInt32(this.gv.Rows[i].Cells[this.gv.Columns.Count - 1].Text.ToString());
                        TextBox gentaojian = (TextBox)(this.gv.Rows[i].Cells[5].Controls[0]);
                        si.RealGentaojian = Convert.ToDecimal(gentaojian.Text.Trim());
                        TextBox metre = (TextBox)(this.gv.Rows[i].Cells[7].Controls[0]);
                        si.RealMetre = Convert.ToDecimal(metre.Text.Trim());
                        TextBox ton = (TextBox)(this.gv.Rows[i].Cells[9].Controls[0]);
                        si.RealTon = Convert.ToDecimal(ton.Text.Trim());
                        DropDownList ddlsupplier = (DropDownList)(this.gv.Rows[i].Cells[12].Controls[0]);
                        si.SupplierID = Convert.ToInt32(ddlsupplier.SelectedValue);
                        DropDownList supplieryn = (DropDownList)(this.gv.Rows[i].Cells[13].Controls[0]);
                        si.Supplier = supplieryn.SelectedItem.Text;
                        DropDownList manufacturer = (DropDownList)(this.gv.Rows[i].Cells[14].Controls[0]);
                        si.ManufacturerID = Convert.ToInt32(manufacturer.SelectedValue);
                        DropDownList manufactureryn = (DropDownList)(this.gv.Rows[i].Cells[15].Controls[0]);
                        si.IsManufacturer = manufactureryn.SelectedItem.Text;
                        DropDownList datayn = (DropDownList)(this.gv.Rows[i].Cells[16].Controls[0]);
                        si.Data = datayn.SelectedItem.Text;
                        DropDownList standardyn = (DropDownList)(this.gv.Rows[i].Cells[17].Controls[0]);
                        si.Standard = standardyn.SelectedItem.Text;
                        DropDownList partsyn = (DropDownList)(this.gv.Rows[i].Cells[18].Controls[0]);
                        si.Parts = partsyn.SelectedItem.Text;
                        DropDownList apperanceyn = (DropDownList)(this.gv.Rows[i].Cells[19].Controls[0]);
                        si.Appearance = apperanceyn.SelectedItem.Text;
                        DropDownList pileyn = (DropDownList)(this.gv.Rows[i].Cells[11].Controls[0]);
                        si.PileID = Convert.ToInt32(pileyn.SelectedValue);
                        DateTimeControl dtctime = (DateTimeControl)(this.gv.Rows[i].Cells[20].Controls[0]);
                        si.StorageTime = dtctime.SelectedDate;
                        var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                        si.CreateTime = SevTime.First();
                        si.Creator = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
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
                    var tp = from a in db.CommitInMaterials
                             where a.CommitProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.CommitProduce.BatchIndex == Request.QueryString["QCBatch"].ToString()
                             select a;
                    if (tp.ToArray().Length > 0)
                    {
                        this.btnko.Enabled = false;
                        if (this._flag == true)
                        {
                            this.btnmodify.Enabled = true;
                        }
                        else
                        {
                            this.btnmodify.Enabled = false;
                        }


                        //初始化质检合格表
                        var cg = from u in db.CommitInMaterials
                                 where u.CommitProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && u.CommitProduce.BatchIndex == Request.QueryString["QCBatch"].ToString()
                                 orderby u.StorageInMaterialsID ascending
                                 select new {
                                              u.StorageInMaterialsID,
                                              u.RealGentaojian,
                                              u.RealMetre,
                                              u.RealTon,
                                              u.SupplierID,
                                              u.Supplier,
                                              u.ManufacturerID,
                                              u.IsManufacturer,
                                              u.Data,
                                              u.Standard,
                                              u.Parts,
                                              u.Appearance,
                                              u.PileInfo.StorageInfo.StorageID,
                                              u.PileInfo.PileID,
                                              u.StorageTime,
                                            };


                        var li = cg.ToList();
                        for (int i = 0; i < this.gv.Rows.Count; i++)
                        {

                            TextBox gentaojian = (TextBox)(this.gv.Rows[i].Cells[5].Controls[0]);
                            gentaojian.Text = li[i].RealGentaojian.ToString(); 
                            TextBox metre = (TextBox)(this.gv.Rows[i].Cells[7].Controls[0]);
                            metre.Text = li[i].RealMetre.ToString(); 
                            TextBox ton = (TextBox)(this.gv.Rows[i].Cells[9].Controls[0]);
                            ton.Text = li[i].RealTon.ToString(); 
                            DropDownList ddlsupplier = (DropDownList)(this.gv.Rows[i].Cells[12].Controls[0]);
                            ddlsupplier.SelectedValue = li[i].SupplierID.ToString(); 
                            DropDownList supplieryn = (DropDownList)(this.gv.Rows[i].Cells[13].Controls[0]);
                            supplieryn.SelectedItem.Text = li[i].Supplier.ToString();
                            DropDownList manufacturer = (DropDownList)(this.gv.Rows[i].Cells[14].Controls[0]);
                            manufacturer.SelectedValue = li[i].ManufacturerID.ToString(); 
                            DropDownList manufactureryn = (DropDownList)(this.gv.Rows[i].Cells[15].Controls[0]);
                            manufactureryn.SelectedItem.Text = li[i].IsManufacturer.ToString();
                            DropDownList datayn = (DropDownList)(this.gv.Rows[i].Cells[16].Controls[0]);
                            datayn.SelectedItem.Text = li[i].Data.ToString();
                            DropDownList standardyn = (DropDownList)(this.gv.Rows[i].Cells[17].Controls[0]);
                            standardyn.SelectedItem.Text = li[i].Standard.ToString();
                            DropDownList partsyn = (DropDownList)(this.gv.Rows[i].Cells[18].Controls[0]);
                            partsyn.SelectedItem.Text = li[i].Parts.ToString();
                            DropDownList apperanceyn = (DropDownList)(this.gv.Rows[i].Cells[19].Controls[0]);
                            apperanceyn.SelectedItem.Text = li[i].Appearance.ToString();
                            DropDownList storageyn = (DropDownList)(this.gv.Rows[i].Cells[10].Controls[0]);
                            storageyn.SelectedValue = li[i].StorageID.ToString();
                           
                            DropDownList pileyn = (DropDownList)(this.gv.Rows[i].Cells[11].Controls[0]);
                            //绑定垛位                         
                            pileyn.Items.Clear();
                            pileyn.DataSource = from p in db.PileInfo
                                                 where p.StorageID == Convert.ToInt32(li[i].StorageID.ToString())
                                                 select new
                                                 {
                                                     p.PileID,
                                                     p.PileName
                                                 };
                            pileyn.DataTextField = "PileName";
                            pileyn.DataValueField = "PileID";
                            pileyn.DataBind();
                            pileyn.Items.Insert(0, new ListItem("--请选择--", "0"));
                            pileyn.SelectedValue = li[i].PileID.ToString();
                            DateTimeControl dtctime = (DateTimeControl)(this.gv.Rows[i].Cells[20].Controls[0]);
                            dtctime.SelectedDate = li[i].StorageTime;

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

        void btnko_Click(object sender, EventArgs e)
        {
            try
            {
                

                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {

                    var tp = from a in db.CommitInMaterials
                             where a.CommitProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.CommitProduce.BatchIndex == Request.QueryString["QCBatch"].ToString()
                             select a;
                    if (tp.ToArray().Length > 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('不能重复插入记录! ')</script>");
                        this.btnko.Enabled = false;
                        this.btnmodify.Enabled = true;
                        return;
                    }



                    for (int i = 0; i < this.gv.Rows.Count; i++)
                    {

                        CommitInMaterials si = new CommitInMaterials();
                        si.ProduceID =Convert.ToInt32( this.gv.Rows[i].Cells[this.gv.Columns.Count - 1].Text.ToString());
                        TextBox gentaojian = (TextBox)(this.gv.Rows[i].Cells[5].Controls[0]);
                        si.RealGentaojian = Convert.ToDecimal(gentaojian.Text.Trim());
                        TextBox metre = (TextBox)(this.gv.Rows[i].Cells[7].Controls[0]);
                        si.RealMetre = Convert.ToDecimal(metre.Text.Trim());
                        TextBox ton = (TextBox)(this.gv.Rows[i].Cells[9].Controls[0]);
                        si.RealTon = Convert.ToDecimal(ton.Text.Trim());
                        DropDownList ddlsupplier = (DropDownList)(this.gv.Rows[i].Cells[12].Controls[0]);
                        si.SupplierID = Convert.ToInt32(ddlsupplier.SelectedValue);
                        DropDownList supplieryn = (DropDownList)(this.gv.Rows[i].Cells[13].Controls[0]);
                        si.Supplier = supplieryn.SelectedItem.Text;
                        DropDownList manufacturer = (DropDownList)(this.gv.Rows[i].Cells[14].Controls[0]);
                        si.ManufacturerID = Convert.ToInt32(manufacturer.SelectedValue);
                        DropDownList manufactureryn = (DropDownList)(this.gv.Rows[i].Cells[15].Controls[0]);
                        si.IsManufacturer = manufactureryn.SelectedItem.Text;
                        DropDownList datayn = (DropDownList)(this.gv.Rows[i].Cells[16].Controls[0]);
                        si.Data = datayn.SelectedItem.Text;
                        DropDownList standardyn = (DropDownList)(this.gv.Rows[i].Cells[17].Controls[0]);
                        si.Standard = standardyn.SelectedItem.Text;
                        DropDownList partsyn = (DropDownList)(this.gv.Rows[i].Cells[18].Controls[0]);
                        si.Parts = partsyn.SelectedItem.Text;
                        DropDownList apperanceyn = (DropDownList)(this.gv.Rows[i].Cells[19].Controls[0]);
                        si.Appearance = apperanceyn.SelectedItem.Text;
                        DropDownList pileyn = (DropDownList)(this.gv.Rows[i].Cells[11].Controls[0]);
                        si.PileID = Convert.ToInt32(pileyn.SelectedValue);
                        DateTimeControl dtctime = (DateTimeControl)(this.gv.Rows[i].Cells[20].Controls[0]);
                        si.StorageTime = dtctime.SelectedDate;
                        var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                        si.CreateTime = SevTime.First();
                        si.Creator = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
                        db.CommitInMaterials.InsertOnSubmit(si);
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
                reportNum.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "StorageInProduceID", "txtgentaojianNum");
                reportNum.HeaderTemplate = new MulTextBoxTemplate("根/台/套/件(实际)", DataControlRowType.Header);
                reportNum.ItemStyle.Width = 150;
                this.gv.Columns.Insert(5, reportNum);

                TemplateField metreNum = new TemplateField();
                metreNum.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "StorageInProduceID", "txtmetreNum");
                metreNum.HeaderTemplate = new MulTextBoxTemplate("米(实际)", DataControlRowType.Header);
                metreNum.ItemStyle.Width = 150;
                this.gv.Columns.Insert(7, metreNum);

                TemplateField tonNum = new TemplateField();
                tonNum.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "StorageInProduceID", "txttonNum");
                tonNum.HeaderTemplate = new MulTextBoxTemplate("吨(实际)", DataControlRowType.Header);
                tonNum.ItemStyle.Width = 150;
                this.gv.Columns.Insert(9, tonNum);


                //加入仓库列
                TemplateField tfStorage = new TemplateField();
                tfStorage.HeaderText = "仓库";
                tfStorage.ItemTemplate = new MulDropDownListTemplate(DataControlRowType.DataRow, "DDLStorage");
                this.gv.Columns.Insert(10, tfStorage);

                //加入垛位列
                TemplateField tfPile = new TemplateField();
                tfPile.HeaderText = "垛位";
                tfPile.ItemTemplate = new MulDropDownListTemplate(DataControlRowType.DataRow, "DDLPile");
                this.gv.Columns.Insert(11, tfPile);



                TemplateField Supplier = new TemplateField();
                Supplier.HeaderText = "供应商";
                Supplier.ItemTemplate = new MulDropDownListTemplate(DataControlRowType.DataRow, "ddlSupplier");
                this.gv.Columns.Insert(12, Supplier);

                TemplateField SupplierYON = new TemplateField();
                SupplierYON.HeaderText = "供应商信息是否一致";
                SupplierYON.ItemTemplate = new MulDropDownListTemplate(DataControlRowType.DataRow, "ddlyn");
                this.gv.Columns.Insert(13, SupplierYON);


                TemplateField manufacturer = new TemplateField();
                manufacturer.HeaderText = "生产厂家";
                manufacturer.ItemTemplate = new MulDropDownListTemplate(DataControlRowType.DataRow, "ddlManufacturer");
                this.gv.Columns.Insert(14, manufacturer);

                TemplateField manufacturerYON = new TemplateField();
                manufacturerYON.HeaderText = "厂家信息是否一致";
                manufacturerYON.ItemTemplate = new MulDropDownListTemplate(DataControlRowType.DataRow, "ddlmf");
                this.gv.Columns.Insert(15, manufacturerYON);



                TemplateField dataYON = new TemplateField();
                dataYON.HeaderText = "资料是否齐全";
                dataYON.ItemTemplate = new MulDropDownListTemplate(DataControlRowType.DataRow, "ddldata");
                this.gv.Columns.Insert(16, dataYON);


                TemplateField standardYON = new TemplateField();
                standardYON.HeaderText = "制造标准是否一致";
                standardYON.ItemTemplate = new MulDropDownListTemplate(DataControlRowType.DataRow, "ddlstandar");
                this.gv.Columns.Insert(17, standardYON);

                TemplateField partsYON = new TemplateField();
                partsYON.HeaderText = "配件是否一致";
                partsYON.ItemTemplate = new MulDropDownListTemplate(DataControlRowType.DataRow, "ddlparts");
                this.gv.Columns.Insert(18, partsYON);

                TemplateField appearanceYON = new TemplateField();
                appearanceYON.HeaderText = "外观是否完好";
                appearanceYON.ItemTemplate = new MulDropDownListTemplate(DataControlRowType.DataRow, "ddlappearance");
                this.gv.Columns.Insert(19, appearanceYON);

                

                //实际到库时间
                TemplateField time = new TemplateField();
                time.HeaderText = "实际到库时间";
                time.ItemTemplate = new DateTimeTemplate(DataControlRowType.DataRow);
                this.gv.Columns.Insert(20, time);

                //TemplateField remark = new TemplateField();
                //remark.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "StorageInProduceID", "txtRemark");
                //remark.HeaderTemplate = new MulTextBoxTemplate("备注", DataControlRowType.Header);
                //remark.ItemStyle.Width = 150;
                //this.gv.Columns.Insert(21, remark);


                this.gv.DataSource = from a in db.CommitProduce
                                     
                                     join b in db.CommitInMain on a.StorageInID equals b.StorageInID
                                    
                                     where a.StorageInID == _storageInID && a.BatchIndex == (string.IsNullOrEmpty(QCbatch)? a.BatchIndex : QCbatch)
                                     select new
                                     {
                                        a.StorageInProduceID,
                                        a.MaterialInfo.MaterialName,
                                        a.MaterialInfo.SpecificationModel,
                                        a.MaterialInfo.FinanceCode,
                                        b.StorageInCode,
                                        a.QuantityGentaojian,
                                        a.QuantityMetre,
                                        a.QuantityTon,
                                        a.ProjectInfo.ProjectName,
                             
                                        a.ExpectedTime,
                                        a.BatchIndex,
                                        a.Remark
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
