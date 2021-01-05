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
    public class CommitStorageAssets : System.Web.UI.Page
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
        static string[] Titlelist = {
                                     "交货通知单编号:StorageInCode",
                                     "物料名称:MaterialName",
                                     "物料规格:SpecificationModel",
                                     "财务编码:FinanceCode",
                                     "根/套/件(合格):TestGentaojian",
                                     "米(合格):TestMetre",
                                     "吨(合格):TestTon",
                                     "预期使用项目:ProjectName",
                                     "实际到库时间:StorageTime",
                                     "所属批次:BatchIndex",
                                     "备注:Remark",
                                     "ID:StorageInTestID"
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

        void gv_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                BindYesorNo(e, ddlyn, 7);
            }
        }

        void tbarbtnsend_Click(object sender, EventArgs e)
        {
            //发送资产组
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                if (btnko.Enabled == false)
                {

                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskID);
                    Response.Redirect("TaskCenter.aspx?StorageInID=" + _storageInID + "&&state=资产组长&&storageInType=委外入库&&QCBatch=" + _QCbatch + "&&TaskStorageID=" + _taskID + "&&InspectState=" + tsi.InspectState + "");
                }
                else
                {
                    if (ViewState["Temp"] != null)
                    {
                        if (ViewState["Temp"].ToString() == "True")
                        {
                            Response.Redirect("TaskCenter.aspx?StorageInID=" + _storageInID + "&&state=资产组长&&storageInType=委外入库&&QCBatch=" + _QCbatch + "&&TaskStorageID=" + _taskID + "");
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
            dataText.Add("根/台/套/件");
            dataText.Add("米");
            dataText.Add("吨");
            dataValue.Add("0");
            dataValue.Add("1");
            dataValue.Add("2");
            dataValue.Add("3");


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
            tbarbtnsend.Text = "发送资产组长审核";
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
                    var tep = from a in db.CommitInAssets
                              where a.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"].ToString()) && a.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.BatchIndex == _QCbatch
                                  orderby a.StorageInAssetsID ascending
                                  select new { a.StorageInAssetsID };
                        var li = tep.ToList();


                    for (int i = 0; i < this.gv.Rows.Count; i++)
                    {

                        CommitInAssets sia = db.CommitInAssets.SingleOrDefault(u => u.StorageInAssetsID == li[i].StorageInAssetsID);

                        DropDownList ddlunit = (DropDownList)(this.gv.Rows[i].Cells[7].Controls[0]);
                        sia.CurUnit = ddlunit.SelectedItem.Text;

                        TextBox unitprice = (TextBox)(this.gv.Rows[i].Cells[8].Controls[0]);
                        sia.UnitPrice = Convert.ToDecimal(unitprice.Text.Trim());

                        TextBox amount = (TextBox)(this.gv.Rows[i].Cells[9].Controls[0]);
                        sia.Amount = Convert.ToDecimal(amount.Text.Trim());

                        TextBox reportNum = (TextBox)(this.gv.Rows[i].Cells[10].Controls[0]);
                        sia.BillCode = reportNum.Text.Trim();

                        TextBox material = (TextBox)(this.gv.Rows[i].Cells[11].Controls[0]);
                        sia.MaterialsAttribute = material.Text.Trim();

                        sia.financeCode = "0";

                        var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                        sia.CreateTime = SevTime.First();
                        sia.Creator = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
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
                    var tp = from a in db.CommitInAssets
                             where a.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.BatchIndex == Request.QueryString["QCBatch"].ToString()
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
                        var cg = from u in db.CommitInAssets
                                 where u.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && u.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.BatchIndex == Request.QueryString["QCBatch"].ToString()
                                 orderby u.StorageInAssetsID ascending
                                 select new {
                                              u.StorageInAssetsID,
                                              u.BillCode,
                                              u.CurUnit,
                                              u.UnitPrice,
                                              u.Amount,
                                              u.MaterialsAttribute,
        
                                            };


                        var li = cg.ToList();
                        for (int i = 0; i < this.gv.Rows.Count; i++)
                        {

                            DropDownList ddlunit = (DropDownList)(this.gv.Rows[i].Cells[7].Controls[0]);
                            ddlunit.SelectedItem.Text = li[i].CurUnit.ToString();

                            TextBox unitprice = (TextBox)(this.gv.Rows[i].Cells[8].Controls[0]);
                            unitprice.Text = li[i].UnitPrice.ToString();

                            TextBox amount = (TextBox)(this.gv.Rows[i].Cells[9].Controls[0]);
                            amount.Text = li[i].Amount.ToString();

                            TextBox reportNum = (TextBox)(this.gv.Rows[i].Cells[10].Controls[0]);
                            reportNum.Text = li[i].BillCode.ToString();

                            TextBox material = (TextBox)(this.gv.Rows[i].Cells[11].Controls[0]);
                            material.Text = li[i].MaterialsAttribute.ToString();
                 
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

                    var tp = from a in db.CommitInAssets
                             where a.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.CommitInTest.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.BatchIndex == Request.QueryString["QCBatch"].ToString()
                             select a;
                    if (tp.ToArray().Length > 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('不能重复插入记录! ')</script>");
                        this.btnko.Enabled = false;
                        return;
                    }



                    for (int i = 0; i < this.gv.Rows.Count; i++)
                    {

                        CommitInAssets sia = new CommitInAssets();

                        sia.TestID = Convert.ToInt32(this.gv.Rows[i].Cells[this.gv.Columns.Count - 1].Text.ToString());

                        DropDownList ddlunit = (DropDownList)(this.gv.Rows[i].Cells[7].Controls[0]);
                        sia.CurUnit = ddlunit.SelectedItem.Text;

                        TextBox unitprice = (TextBox)(this.gv.Rows[i].Cells[8].Controls[0]);
                        sia.UnitPrice = Convert.ToDecimal(unitprice.Text.Trim());

                        TextBox amount = (TextBox)(this.gv.Rows[i].Cells[9].Controls[0]);
                        sia.Amount = Convert.ToDecimal(amount.Text.Trim());

                        TextBox reportNum = (TextBox)(this.gv.Rows[i].Cells[10].Controls[0]);
                        sia.BillCode = reportNum.Text.Trim();

                        TextBox material = (TextBox)(this.gv.Rows[i].Cells[11].Controls[0]);
                        sia.MaterialsAttribute = material.Text.Trim();

                        sia.financeCode = "0";

                        var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                        sia.CreateTime = SevTime.First();
                        sia.Creator = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
                        db.CommitInAssets.InsertOnSubmit(sia);
                        db.SubmitChanges();

                       

                    }
                    //存库标识
                    ViewState["Temp"] = flag = true;
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

                TemplateField standardYON = new TemplateField();
                standardYON.HeaderText = "计量单位";
                standardYON.ItemTemplate = new MulDropDownListTemplate(DataControlRowType.DataRow, "ddlstandar");
                this.gv.Columns.Insert(7, standardYON);

                TemplateField unitprice = new TemplateField();
                unitprice.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "StorageInTestID", "txtunitprice");
                unitprice.HeaderTemplate = new MulTextBoxTemplate("单价", DataControlRowType.Header);
                unitprice.ItemStyle.Width = 150;
                this.gv.Columns.Insert(8, unitprice);


                TemplateField amount = new TemplateField();
                amount.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "StorageInTestID", "txtamount");
                amount.HeaderTemplate = new MulTextBoxTemplate("金额", DataControlRowType.Header);
                amount.ItemStyle.Width = 150;
                this.gv.Columns.Insert(9, amount);

                TemplateField reportNum = new TemplateField();
                reportNum.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "StorageInTestID", "txtreportNum");
                reportNum.HeaderTemplate = new MulTextBoxTemplate("入库单据号", DataControlRowType.Header);
                reportNum.ItemStyle.Width = 150;
                this.gv.Columns.Insert(10, reportNum);

                TemplateField materials = new TemplateField();
                materials.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "StorageInTestID", "txtmaterials");
                materials.HeaderTemplate = new MulTextBoxTemplate("物资属性", DataControlRowType.Header);
                materials.ItemStyle.Width = 150;
                this.gv.Columns.Insert(11, materials);

                

      


                this.gv.DataSource = from a in db.CommitInTest
                                     
                                     join b in db.CommitInMain on a.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.StorageInID equals b.StorageInID

                                     where a.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.StorageInID == _storageInID && a.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.BatchIndex == (string.IsNullOrEmpty(QCbatch) ? a.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.BatchIndex : QCbatch)
                                     select new
                                     {
                                        a.StorageInTestID,
                                        a.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.MaterialInfo.MaterialName,
                                        a.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.MaterialInfo.SpecificationModel,
                                        a.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.MaterialInfo.FinanceCode,
                                        b.StorageInCode,
                                        a.TestGentaojian,
                                        a.TestMetre,
                                        a.TestTon,

                                        a.CommitInMaterialsLeader.CommitInMaterials.StorageTime,
                                        a.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.ProjectInfo.ProjectName,
                                        a.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.BatchIndex,
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
