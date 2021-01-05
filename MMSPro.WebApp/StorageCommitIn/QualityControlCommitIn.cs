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
    public class QualityControlCommitIn : System.Web.UI.Page
    {
        SPGridView spgviewQualityControl;
        Button btnCancel;
        Button btnEdit;
        CheckBox chbQuickSet;
        string strUrl = "../../default-old.aspx";
        Button btnOk;
        Panel p1;
        TextBox tboxQualified;
        string batchidx = string.Empty;
         

        static string[] Tlist =  { 
                                     "序号:CommitDetailedID",
                                     "交货通知单编号:CommitInCode",
                                     "物料名称:MaterialName",
                                     "物料编码:MaterialCode",
                                     "物料规格:SpecificationModel",
                                     "质检前根/套/件数量:QuantityGentaojian",
                                     "质检前米:QuantityMetre",
                                     "质检前吨:QuantityTon",
                                     "质检前所选单位数量:Quantity",
                                     "计量单位:CurUnit",
                                     "单价:UnitPrice",
                                     "金额:Amount",
                                     "所属仓库:StorageName",
                                     "所属垛位:PileCode",
                                     "财务编号:financeCode",
                                     "到库时间:StorageTime",
                                     "供应商:SupplierName",
                                     "物资管理员:MaterialsManager",
                                     "仓库员:WarehouseWorker",
                                     "备注:Remark",
                                     "sid:SupplierID"
                                   };
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                if (Request.QueryString.Count >= 2)
                {
                    if (string.IsNullOrEmpty(Request.QueryString["StorageInID"]) || string.IsNullOrEmpty(Request.QueryString["TaskStorageID"]))
                    {
                        Response.Redirect(strUrl);
                    }
                }
                else
                {
                    Response.Redirect(strUrl);
                }


                initControl();
                LoadPageinfo();

                //添加按钮到toolbar
                ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarEmployee");
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }

         
        }

        void chbQuickSet_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.chbQuickSet.Checked == true)
                {
                    setDefault();
                }
                else
                {
                    setDefaultClear();
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        private void LoadPageinfo()
        {


            //判断质检单是否存在


            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var tp = from a in db.CommitInQualified
                         where a.CommitInID == Convert.ToInt32(Request.QueryString["StorageInID"])
                         select a;
                if (tp.ToArray().Length > 0)
                {
                    this.btnOk.Enabled = false;
                    this.btnEdit.Enabled = true;


                    //初始化质检合格表
                    var cg = from u in db.CommitInQualified
                             where u.CommitInID == Convert.ToInt32(Request.QueryString["StorageInID"])
                             orderby u.CommitInQualifiedID ascending
                             select new { u.QuantityGentaojian, u.QuantityMetre, u.QuantityTon, u.InspectionReportNum};


                    var li = cg.ToList();
                    for (int i = 0; i < this.spgviewQualityControl.Rows.Count; i++)
                    {
                        TextBox gentaojian = (TextBox)(this.spgviewQualityControl.Rows[i].Cells[10].Controls[0]);
                        gentaojian.Text = li[i].QuantityGentaojian.ToString();
                        TextBox metre = (TextBox)(this.spgviewQualityControl.Rows[i].Cells[11].Controls[0]);
                        metre.Text = li[i].QuantityMetre.ToString();
                        TextBox ton = (TextBox)(this.spgviewQualityControl.Rows[i].Cells[12].Controls[0]);
                        ton.Text = li[i].QuantityTon.ToString();
                        TextBox ti = (TextBox)(this.spgviewQualityControl.Rows[i].Cells[13].Controls[0]);
                        ti.Text = li[i].InspectionReportNum.ToString();
                    }



                }
                else
                {
                    this.btnOk.Enabled = true;
                    this.btnEdit.Enabled = false;
                }


            }











            //修改任务完成状态
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                TaskStorageIn tk = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == Convert.ToInt32(Request.QueryString["TaskStorageID"]));
                if (tk != null)
                {
                    if (tk.TaskState == "已完成")
                    {
                        this.btnOk.Enabled = false;
                        this.btnEdit.Enabled = false;
                    }
                }
            }
        }

        private void initControl()
        {
            this.spgviewQualityControl = new SPGridView();
            this.spgviewQualityControl.Columns.Clear();
            this.spgviewQualityControl.AutoGenerateColumns = false;
            this.spgviewQualityControl.RowCreated += new GridViewRowEventHandler(spgviewSupplierType_RowCreated);
            BindGridView();

            btnOk = new Button();
            btnCancel = new Button();
            btnEdit = new Button();
            btnOk.Text = "生成质检清单";
            btnEdit.Text = "修改质检清单";
            btnCancel.Text = "取消";
            btnCancel.Width = 100;
            btnOk.Click += new EventHandler(btnOk_Click);
            btnCancel.Click += new EventHandler(btnCancel_Click);
            btnEdit.Click += new EventHandler(btnEdit_Click);
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            Panel p2 = (Panel)GetControltByMaster("Panel2");
            chbQuickSet = (CheckBox)GetControltByMaster("chbQuickSet");
            this.chbQuickSet.CheckedChanged += new EventHandler(chbQuickSet_CheckedChanged);
            p2.Controls.Add(btnOk);
            p2.Controls.Add(btnEdit);
            p2.Controls.Add(btnCancel);
        }

        void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                //检查输入格式
                if (CheckStringEmpty() != true)
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请将数据填写完整,且合格数量只能是数字 ')</script>");
                    return;
                }

                //检查输入数据是否溢出
                if (CheckOverFlow() != true)
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('合格数量不能超过质检前数量! ')</script>");
                    return;
                }

                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {



                    var task = from a in db.TaskStorageIn
                               where a.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.TaskType == "质检" && a.InspectState == "驳回" && a.TaskState == "未完成" && a.StorageInType=="委外入库"
                               select new { a.TaskCreaterID, a.InspectState };
                    //如果是回退到质检的调拨单
                    if (task.ToList().Count > 0)
                    {

                        //更新
                        var tep = from a in db.CommitInQualified
                                  where a.CommitInID == Convert.ToInt32(Request.QueryString["StorageInID"].ToString())
                                  orderby a.CommitInQualifiedID ascending
                                  select new { a.CommitInQualifiedID };
                        var li = tep.ToList();

                        for (int i = 0; i < this.spgviewQualityControl.Rows.Count; i++)
                        {

                            CommitInQualified SIQ = new CommitInQualified();
                            SIQ = db.CommitInQualified.SingleOrDefault(u => u.CommitInQualifiedID == li[i].CommitInQualifiedID);

                            TextBox gentaojian = (TextBox)(this.spgviewQualityControl.Rows[i].Cells[10].Controls[0]);
                            SIQ.QuantityGentaojian = Convert.ToDecimal(gentaojian.Text.Trim());
                            TextBox metre = (TextBox)(this.spgviewQualityControl.Rows[i].Cells[11].Controls[0]);
                            SIQ.QuantityMetre = Convert.ToDecimal(metre.Text.Trim());
                            TextBox ton = (TextBox)(this.spgviewQualityControl.Rows[i].Cells[12].Controls[0]);
                            SIQ.QuantityTon = Convert.ToDecimal(ton.Text.Trim());
                            TextBox ti = (TextBox)(this.spgviewQualityControl.Rows[i].Cells[13].Controls[0]);
                            SIQ.InspectionReportNum = ti.Text.Trim();

                            db.SubmitChanges();

                        }




                        //旧任务
                        TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == Convert.ToInt32(Request.QueryString["TaskStorageID"]));
                        tsi.TaskState = "已完成";
                        tsi.InspectState = "已审核";
                        CommitIn si = db.CommitIn.SingleOrDefault(u => u.CommitInID == tsi.StorageInID);

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
                        TSI.StorageInType = tsi.StorageInType;
                        TSI.TaskTitle = "交货通知单编号为:" + si.CommitInCode + "的物资已质检，请重新审核";
                        TSI.TaskState = "未完成";
                        TSI.TaskDispose = "未废弃";
                        TSI.TaskType = "材料会计审核";
                        TSI.InspectState = "未审核";
                        TSI.Remark = "交货通知单编号为:" + si.CommitInCode + "的物资已重新质检";
                        var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                        TSI.CreateTime = SevTime.First();
                        db.TaskStorageIn.InsertOnSubmit(TSI);
                        db.SubmitChanges();
                        Response.Redirect("../../default-old.aspx",false);
                    }

                    else
                    {
                        //更新数据
                        var tep = from a in db.CommitInQualified
                                  where a.CommitInID == Convert.ToInt32(Request.QueryString["StorageInID"].ToString())
                                  orderby a.CommitInQualifiedID ascending
                                  select new { a.CommitInQualifiedID };
                        var li = tep.ToList();

                        for (int i = 0; i < this.spgviewQualityControl.Rows.Count; i++)
                        {

                            CommitInQualified SIQ = new CommitInQualified();
                            SIQ = db.CommitInQualified.SingleOrDefault(u => u.CommitInQualifiedID == li[i].CommitInQualifiedID);
                            TextBox gentaojian = (TextBox)(this.spgviewQualityControl.Rows[i].Cells[10].Controls[0]);
                            SIQ.QuantityGentaojian = Convert.ToDecimal(gentaojian.Text.Trim());
                            TextBox metre = (TextBox)(this.spgviewQualityControl.Rows[i].Cells[11].Controls[0]);
                            SIQ.QuantityMetre = Convert.ToDecimal(metre.Text.Trim());
                            TextBox ton = (TextBox)(this.spgviewQualityControl.Rows[i].Cells[12].Controls[0]);
                            SIQ.QuantityTon = Convert.ToDecimal(ton.Text.Trim());
                            TextBox ti = (TextBox)(this.spgviewQualityControl.Rows[i].Cells[13].Controls[0]);
                            SIQ.InspectionReportNum = ti.Text.Trim();
                            db.SubmitChanges();

                        }
                        Response.Redirect("QualifiedCommitIn.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] +"");


                    }


                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_UPDATEERROR));
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

        void btnCancel_Click(object sender, EventArgs e)
        {

            Response.Redirect("../../default-old.aspx",false);
        }

        void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                //检查输入格式
                if (CheckStringEmpty() != true)
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请将数据填写完整,且合格数量只能是数字! ')</script>");
                    return;
                }
                //检查输入数据是否溢出
                if (CheckOverFlow() != true)
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('合格数量不能超过质检前数量! ')</script>");
                    return;
                }

                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {

                    var tp = from a in db.CommitInQualified
                             where a.CommitInID == Convert.ToInt32(Request.QueryString["StorageInID"])
                             select a;
                    if (tp.ToArray().Length > 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('不能重复插入记录! ')</script>");
                        return;
                    }



                    for (int i = 0; i < this.spgviewQualityControl.Rows.Count; i++)
                    {

                        CommitInQualified SIQ = new CommitInQualified();
                        SIQ.CommitInID = Convert.ToInt32(Request.QueryString["StorageInID"]);

                        MaterialInfo mi = db.MaterialInfo.SingleOrDefault(u => u.MaterialCode == this.spgviewQualityControl.Rows[i].Cells[3].Text.ToString());
                        SIQ.MaterialID = mi.MaterialID;
                        SIQ.SpecificationModel = this.spgviewQualityControl.Rows[i].Cells[4].Text.ToString();

                        SIQ.Quantity = Convert.ToDecimal(this.spgviewQualityControl.Rows[i].Cells[8].Text.ToString());
                        SIQ.CurUnit = this.spgviewQualityControl.Rows[i].Cells[9].Text.ToString();
                        TextBox gentaojian = (TextBox)(this.spgviewQualityControl.Rows[i].Cells[10].Controls[0]);
                        SIQ.QuantityGentaojian = Convert.ToDecimal(gentaojian.Text.Trim());
                        TextBox metre = (TextBox)(this.spgviewQualityControl.Rows[i].Cells[11].Controls[0]);
                        SIQ.QuantityMetre = Convert.ToDecimal(metre.Text.Trim());
                        TextBox ton = (TextBox)(this.spgviewQualityControl.Rows[i].Cells[12].Controls[0]);
                        SIQ.QuantityTon = Convert.ToDecimal(ton.Text.Trim());
                        TextBox ti = (TextBox)(this.spgviewQualityControl.Rows[i].Cells[13].Controls[0]);
                        SIQ.InspectionReportNum = ti.Text.Trim();
                        SIQ.NumberQualified = 0;
                        SIQ.UnitPrice = Convert.ToDecimal(this.spgviewQualityControl.Rows[i].Cells[14].Text.ToString());
                        SIQ.Amount = Convert.ToDecimal(this.spgviewQualityControl.Rows[i].Cells[15].Text.ToString());

                        PileInfo pi = db.PileInfo.SingleOrDefault(u => u.PileCode == this.spgviewQualityControl.Rows[i].Cells[17].Text.ToString());
                        SIQ.PileID = pi.PileID;

                        SIQ.financeCode = this.spgviewQualityControl.Rows[i].Cells[18].Text.ToString();
                        SIQ.StorageTime = Convert.ToDateTime(this.spgviewQualityControl.Rows[i].Cells[19].Text.ToString());

                        //这里不唯一要出问题。

                        SIQ.SupplierID = Convert.ToInt32(this.spgviewQualityControl.Rows[i].Cells[24].Text.ToString());

                        var temp = from a in db.CommitInDetailed
                                   where a.CommitInID == Convert.ToInt32(Request.QueryString["StorageInID"])
                                   select new { a.MaterialsManager, a.WarehouseWorker };
                        var list = temp.ToList();
                        SIQ.MaterialsManager = list[i].MaterialsManager;
                        SIQ.WarehouseWorker = list[i].WarehouseWorker;
                        var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                        SIQ.InspectionTime = SevTime.First();
                        SIQ.Remark = this.spgviewQualityControl.Rows[i].Cells[23].Text.ToString();
                        db.CommitInQualified.InsertOnSubmit(SIQ);
                        db.SubmitChanges();



                    }

                }
                Response.Redirect("QualifiedCommitIn.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + " ");

            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
            }
        }

        void spgviewSupplierType_RowCreated(object sender, GridViewRowEventArgs e)
        {
            //  e.Row.Attributes.Add("onclick", "SmtGridSelectItem(this)");
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {
    

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


        protected void tbarbtnEdit_Click(object sender, EventArgs e)
        {
            List<CheckBox> listString = GetCheckedID();
            if (listString.Count == 1)
            {
                Response.Redirect("SupTypeEditer.aspx?SupplierTypeID=" + listString[0].ToolTip);
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择一条且仅能有一条记录进行编辑!')</script>");
            }
        }

        protected void tbarbtnAdd_Click(object sender, EventArgs e)
        {
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");
            Response.Redirect("SupTypeCreater.aspx");
        }

        /// <summary>
        /// 绑定Gridview
        /// </summary>
        private void BindGridView()
        {
            try
            {

                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    string strTaskID = this.Request.QueryString["TaskStorageID"];
                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(a => a.TaskStorageID == int.Parse(strTaskID));
                    if (tsi == null)
                    {
                        Response.Redirect(strUrl);
                    }
                    Label labTaskCreater = (Label)GetControltByMaster("labTaskCreater");
                    labTaskCreater.Text = tsi.EmpInfo.EmpName + "[" + tsi.EmpInfo.Account + "]";
                    ((Label)GetControltByMaster("labCreateTime")).Text = tsi.CreateTime.ToString();
                    ((Label)GetControltByMaster("labTaskTitle")).Text = tsi.TaskTitle;
                    ((Label)GetControltByMaster("labRemark")).Text = tsi.Remark;

                    string strID = this.Request.QueryString["StorageInID"];
                    CommitIn si = db.CommitIn.SingleOrDefault(a => a.CommitInID == int.Parse(strID));
                    if (si == null)
                    {
                        Response.Redirect(strUrl);
                    }
                    BoundField bf = new BoundField();
                    BoundField bfColumn;

                    //TemplateField tfieldTextBox = new TemplateField();
                    //tfieldTextBox.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "CommitDetailedID", "txtCount");
                    //tfieldTextBox.HeaderTemplate = new MulTextBoxTemplate("合格数量", DataControlRowType.Header);
                    //tfieldTextBox.ItemStyle.Width = 150;

                    TemplateField GentaojianTextBox = new TemplateField();
                    GentaojianTextBox.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "StorageDetailedID", "txtGentaojian");
                    GentaojianTextBox.HeaderTemplate = new MulTextBoxTemplate("根/套/件", DataControlRowType.Header);
                    GentaojianTextBox.ItemStyle.Width = 150;

                    TemplateField MetreTextBox = new TemplateField();
                    MetreTextBox.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "StorageDetailedID", "txtMetre");
                    MetreTextBox.HeaderTemplate = new MulTextBoxTemplate("米", DataControlRowType.Header);
                    MetreTextBox.ItemStyle.Width = 150;

                    TemplateField TonTextBox = new TemplateField();
                    TonTextBox.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "StorageDetailedID", "txtTon");
                    TonTextBox.HeaderTemplate = new MulTextBoxTemplate("吨", DataControlRowType.Header);
                    TonTextBox.ItemStyle.Width = 150;


                    TemplateField reportTextBox = new TemplateField();
                    reportTextBox.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "CommitDetailedID", "txtQualityNum");
                    reportTextBox.HeaderTemplate = new MulTextBoxTemplate("质检报告号", DataControlRowType.Header);
                    reportTextBox.ItemStyle.Width = 150;

                    foreach (var kvp in Tlist)
                    {
                        bfColumn = new BoundField();
                        bfColumn.HeaderText = kvp.Split(':')[0];
                        bfColumn.DataField = kvp.Split(':')[1];
                        this.spgviewQualityControl.Columns.Add(bfColumn);
                    }

                    this.spgviewQualityControl.Columns.Insert(10, GentaojianTextBox);
                    this.spgviewQualityControl.Columns.Insert(11, MetreTextBox);
                    this.spgviewQualityControl.Columns.Insert(12, TonTextBox);


                    //this.spgviewQualityControl.Columns.Insert(13, tfieldTextBox);
                    this.spgviewQualityControl.Columns.Insert(13, reportTextBox);
                    this.spgviewQualityControl.DataSource = from a in db.CommitInDetailed
                                                            join b in db.CommitIn on a.CommitInID equals b.CommitInID
                                                            join c in db.MaterialInfo on a.MaterialID equals c.MaterialID
                                                            join d in db.PileInfo on a.PileID equals d.PileID
                                                            join e in db.SupplierInfo on a.SupplierID equals e.SupplierID



                                                            where a.CommitInID == Convert.ToInt32(Request.QueryString["StorageInID"])


                                                            select new
                                                            {
                                                                a.CommitDetailedID,
                                                                b.CommitInCode,
                                                                a.SpecificationModel,
                                                                c.MaterialName,
                                                                c.MaterialCode,
                                                                a.Quantity,
                                                                a.UnitPrice,
                                                                a.Amount,
                                                                d.StorageInfo.StorageName,
                                                                d.PileCode,
                                                                a.financeCode,
                                                                a.StorageTime,
                                                                e.SupplierName,
                                                                a.QuantityGentaojian,
                                                                a.QuantityMetre,
                                                                a.QuantityTon,
                                                                a.CurUnit,
                                                                MaterialsManager = db.EmpInfo.SingleOrDefault(u => u.EmpID == a.MaterialsManager).EmpName,
                                                                WarehouseWorker = db.EmpInfo.SingleOrDefault(u => u.EmpID == a.WarehouseWorker).EmpName,
                                                                a.Remark,
                                                                e.SupplierID
                                                            };
                    this.spgviewQualityControl.DataBind();
                    this.spgviewQualityControl.Columns[0].Visible = false;
                    this.spgviewQualityControl.Columns[this.spgviewQualityControl.Columns.Count - 1].Visible = false;
                    p1 = (Panel)GetControltByMaster("Panel1");
                    p1.Controls.Add(this.spgviewQualityControl);

                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));
            }

        }
        /// <summary>
        /// 检查输入的合格数量和质检号码是否为空
        /// </summary>
        /// <returns></returns>
        private bool CheckStringEmpty()
        {

            for (int i = 0; i < this.spgviewQualityControl.Rows.Count; i++)
            {
                for (int k = 10; k < 14; k++)
                {
                    if (this.spgviewQualityControl.Rows[i].Cells[k].Controls[0] is TextBox)
                    {
                        tboxQualified = (TextBox)this.spgviewQualityControl.Rows[i].Cells[k].Controls[0];
                        if (tboxQualified.Text == string.Empty)
                        {
                            return false;
                        }
                        if (k == 10 || k == 11 || k == 12)
                        {
                            if (!PageValidate.IsNumberTwoDecimal(tboxQualified.Text.Trim()))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
        /// <summary>
        /// 检查合格适量是否超出质检前数量
        /// </summary>
        /// <returns></returns>
        private bool CheckOverFlow()
        {

            decimal beforeGenTaoJianText;
            decimal beforeMetreText;
            decimal beforeTonText;
            TextBox nextGenTaoJianText;
            TextBox nextMetreText;
            TextBox nextTonText;
            for (int i = 0; i < this.spgviewQualityControl.Rows.Count; i++)
            {
                beforeGenTaoJianText = Convert.ToDecimal(this.spgviewQualityControl.Rows[i].Cells[5].Text);
                beforeMetreText = Convert.ToDecimal(this.spgviewQualityControl.Rows[i].Cells[6].Text);
                beforeTonText = Convert.ToDecimal(this.spgviewQualityControl.Rows[i].Cells[7].Text);

                if (this.spgviewQualityControl.Rows[i].Cells[10].Controls[0] is TextBox
                    && this.spgviewQualityControl.Rows[i].Cells[11].Controls[0] is TextBox
                    && this.spgviewQualityControl.Rows[i].Cells[12].Controls[0] is TextBox
                    )
                {
                    nextGenTaoJianText = (TextBox)this.spgviewQualityControl.Rows[i].Cells[10].Controls[0];
                    nextMetreText = (TextBox)this.spgviewQualityControl.Rows[i].Cells[11].Controls[0];
                    nextTonText = (TextBox)this.spgviewQualityControl.Rows[i].Cells[12].Controls[0];
                    if (nextGenTaoJianText.Text != string.Empty
                        && nextMetreText.Text != string.Empty
                        && nextTonText.Text != string.Empty)
                    {

                        if (Convert.ToDecimal(nextGenTaoJianText.Text.Trim()) > beforeGenTaoJianText
                            && Convert.ToDecimal(nextMetreText.Text.Trim()) > beforeMetreText
                            && Convert.ToDecimal(nextTonText.Text.Trim()) > beforeTonText
                            )
                        {
                            return false;
                        }
                    }

                }

            }

            return true;
        }
        /// <summary>
        /// 快速设置合格数量
        /// </summary>
        private void setDefault()
        {
            string beforeGenTaoJianText;
            string beforeMetreText;
            string beforeTonText;
            TextBox nextGenTaoJianText;
            TextBox nextMetreText;
            TextBox nextTonText;
            for (int i = 0; i < this.spgviewQualityControl.Rows.Count; i++)
            {
                beforeGenTaoJianText = this.spgviewQualityControl.Rows[i].Cells[5].Text;
                beforeMetreText = this.spgviewQualityControl.Rows[i].Cells[6].Text;
                beforeTonText = this.spgviewQualityControl.Rows[i].Cells[7].Text;

                if (this.spgviewQualityControl.Rows[i].Cells[10].Controls[0] is TextBox
                    && this.spgviewQualityControl.Rows[i].Cells[11].Controls[0] is TextBox
                    && this.spgviewQualityControl.Rows[i].Cells[12].Controls[0] is TextBox
                    )
                {
                    nextGenTaoJianText = (TextBox)this.spgviewQualityControl.Rows[i].Cells[10].Controls[0];
                    nextMetreText = (TextBox)this.spgviewQualityControl.Rows[i].Cells[11].Controls[0];
                    nextTonText = (TextBox)this.spgviewQualityControl.Rows[i].Cells[12].Controls[0];

                    nextGenTaoJianText.Text = beforeGenTaoJianText;
                    nextMetreText.Text = beforeMetreText;
                    nextTonText.Text = beforeTonText;


                }
            }
        }
        /// <summary>
        /// 清空默认数据
        /// </summary>
        private void setDefaultClear()
        {
            TextBox nextGenTaoJianText;
            TextBox nextMetreText;
            TextBox nextTonText;
            for (int i = 0; i < this.spgviewQualityControl.Rows.Count; i++)
            {


                if (this.spgviewQualityControl.Rows[i].Cells[10].Controls[0] is TextBox
                   && this.spgviewQualityControl.Rows[i].Cells[11].Controls[0] is TextBox
                   && this.spgviewQualityControl.Rows[i].Cells[12].Controls[0] is TextBox
                   )
                {
                    nextGenTaoJianText = (TextBox)this.spgviewQualityControl.Rows[i].Cells[10].Controls[0];
                    nextMetreText = (TextBox)this.spgviewQualityControl.Rows[i].Cells[11].Controls[0];
                    nextTonText = (TextBox)this.spgviewQualityControl.Rows[i].Cells[12].Controls[0];
                    nextGenTaoJianText.Text = nextMetreText.Text = nextTonText.Text = string.Empty;

                }
            }
        }


        /// <summary>
        /// 返回选中的列表
        /// </summary>
        /// <returns>返回list构成的列表</returns>
        private List<CheckBox> GetCheckedID()
        {
            List<CheckBox> list = new List<CheckBox>();

            foreach (GridViewRow row in this.spgviewQualityControl.Rows)
            {

                CheckBox ck = (CheckBox)row.Cells[0].FindControl("SMItem");
                if (ck.Checked)
                {
                    list.Add(ck);
                }
            }
            return list;
        }

    }
}
