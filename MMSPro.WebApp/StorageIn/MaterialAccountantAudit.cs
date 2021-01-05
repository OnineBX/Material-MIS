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
    public class MaterialAccountantAudit : System.Web.UI.Page
    {
        private int _storageinid;              //交货通知单编号
        private int _taskstorageid;            //任务编号
        private string _batchname;               //批次

        private string _errorpage ="123.aspx";
        
        
        SPGridView spgviewAuditControl;
        Button  btnSend,btnCancel;
        CheckBox chbAuditAll;

        #region 初始化和数据绑定
        private void InitializeCustomControls()
        {
            this.spgviewAuditControl = new SPGridView();
            this.spgviewAuditControl.AutoGenerateColumns = false;
            this.spgviewAuditControl.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");

            string[] ShowTlist =  { 
                                     "交货通知单编号:StorageInCode",
                                     "入库单号:StorageInQualifiedNum",
                                     "物料名称:MaterialName",
                                     "物料编码:MaterialCode",
                                     "规格型号:SpecificationModel",
                                     "质检合格根/套/件数量:QuantityGentaojian",
                                     "质检合格米数量:QuantityMetre",
                                     "质检合格吨数量:QuantityTon",
                                     "所选单位数量:Quantity",
                                     "计量单位:CurUnit",
                                     "质检时间:InspectionTime",
                                     "检验报告号:InspectionReportNum",
                                     "单价:UnitPrice",
                                     "金额:Amount",
                                     "所属仓库:StorageName",
                                     "所属垛位:PileCode",
                                     "财务编号:financeCode",
                                     "到库时间:StorageTime",
                                     "批次信息:BatchIndex",
                                     "供应商:SupplierName",
                                     "物资管理员:MaterialsManager",
                                     "仓库员:WarehouseWorker",                                     
                                     "备注:Remark",
                                     "MID:MaterialID"
                                   };

            string[] HideTlist =  {                                       
                                     "SupplierIDCol:SupplierID",                                     
                                     "MaterialsManagerIDCol:MaterialsManagerID",
                                     "WarehouseWorkerIDCol:WarehouseWorkerID",                   
                                   };
            BoundField bfColumn;

            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgviewAuditControl.Columns.Add(bfColumn);
            }

            //为SPGridView添加状态列

            TemplateField tlfAudit = new TemplateField();
            tlfAudit.HeaderText = "状态";
            tlfAudit.ItemTemplate = new LabelTemplate("状态", DataControlRowType.DataRow);
            this.spgviewAuditControl.Columns.Insert(23,tlfAudit);


            CommandField cdf = new CommandField();
            cdf.HeaderText = "审核";
            cdf.SelectText = "通过/未通过";
            cdf.ShowSelectButton = true;
            this.spgviewAuditControl.SelectedIndexChanging += new GridViewSelectEventHandler(spgviewAuditControl_SelectedIndexChanging);
            this.spgviewAuditControl.Columns.Insert(24,cdf);

            //添加隐藏列
            foreach (var kvp in HideTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgviewAuditControl.Columns.Add(bfColumn);
            }

            //添加控件到panel1
            btnSend = new Button();
            btnSend.Text = "生成审核清单";
            btnSend.Enabled = false;
            btnSend.Width = 100;
            btnSend.Click += new EventHandler(btnSend_Click);

            btnCancel = new Button();
            btnCancel.Text = "返回";
            btnCancel.Width = 100;
            btnCancel.Click += new EventHandler(btnCancel_Click);

            chbAuditAll = new CheckBox();
            chbAuditAll.Checked = false;
            chbAuditAll.AutoPostBack = true;
            chbAuditAll.Text = "<font size = 2pt>全部通过审核</font>";
            chbAuditAll.CheckedChanged += new EventHandler(chbAuditAll_CheckedChanged);

            //验证任务是否已经完成
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == Convert.ToInt32(Request.QueryString["TaskStorageID"]));
                if (tsi.TaskState == "已完成")
                {
                    this.chbAuditAll.Enabled = false;
                    cdf.Visible = false;
                }
            }

        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("../../default-old.aspx",false);
        }        

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            Panel p2 = (Panel)GetControltByMaster("Panel2");
            p1.Controls.Add(this.spgviewAuditControl);
            p2.Controls.Add(new LiteralControl("<BR/>"));
            p2.Controls.Add(chbAuditAll);
            p2.Controls.Add(new LiteralControl("<BR/><BR/>"));            
            p2.Controls.Add(btnSend);
            p2.Controls.Add(new LiteralControl("<BR/><BR/><font size = 2pt color = red>注意：列表中存在\"待审核\"项时,不能生成审核清单...</font>"));
        }        

        private void BindGridView()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化TaskStoreIn信息
                TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(t => t.TaskStorageID == _taskstorageid);
                if (tsi == null)
                {
                    Response.Redirect(_errorpage);
                    return;
                }
                ((Label)GetControltByMaster("labTaskCreater")).Text = tsi.EmpInfo.EmpName + "[" + tsi.EmpInfo.Account + "]";
                ((Label)GetControltByMaster("labCreateTime")).Text = tsi.CreateTime.ToString();
                ((Label)GetControltByMaster("labTaskTitle")).Text = tsi.TaskTitle;
                ((Label)GetControltByMaster("labRemark")).Text = tsi.Remark;

                //初始化StoreInQualified信息

                StorageIn siq = db.StorageIn.SingleOrDefault(s => s.StorageInID == _storageinid);
                if (siq == null)
                {
                    Response.Redirect(_errorpage);
                    return;
                }                
                this.spgviewAuditControl.DataSource = from a in db.StorageInQualified
                                                      join b in db.StorageIn on a.StorageInID equals b.StorageInID
                                                      join c in db.MaterialInfo on a.MaterialID equals c.MaterialID
                                                      join d in db.PileInfo on a.PileID equals d.PileID
                                                      join e in db.SupplierInfo on a.SupplierID equals e.SupplierID
                                                      where a.StorageInID == this._storageinid && a.BatchIndex == _batchname
                                                      select new
                                                      {
                                                          a.StorageInQualifiedID,
                                                          b.StorageInQualifiedNum,
                                                          b.StorageInCode,
                                                          c.MaterialName,
                                                          c.MaterialCode,
                                                          c.MaterialID,
                                                          a.SpecificationModel,
                                                          a.Quantity,
                                                          a.QuantityGentaojian,
                                                          a.QuantityMetre,
                                                          a.QuantityTon,
                                                          a.CurUnit,
                                                          a.InspectionTime,
                                                          a.InspectionReportNum,
                                                          a.UnitPrice,
                                                          a.Amount,
                                                          d.StorageInfo.StorageName,
                                                          d.PileCode,
                                                          a.financeCode,
                                                          a.StorageTime,
                                                          a.BatchIndex,
                                                          e.SupplierName,
                                                          MaterialsManager = db.EmpInfo.SingleOrDefault(u => u.EmpID == a.MaterialsManager).EmpName,
                                                          WarehouseWorker = db.EmpInfo.SingleOrDefault(u => u.EmpID == a.WarehouseWorker).EmpName,
                                                          a.Remark,
                                                          e.SupplierID,
                                                          MaterialsManagerID = a.MaterialsManager,
                                                          WarehouseWorkerID = a.WarehouseWorker,                                                          
                                                      };                               

                this.spgviewAuditControl.DataBind();
                this.spgviewAuditControl.Columns[this.spgviewAuditControl.Columns.Count-1].Visible = false;
                this.spgviewAuditControl.Columns[this.spgviewAuditControl.Columns.Count - 2].Visible = false;
                this.spgviewAuditControl.Columns[this.spgviewAuditControl.Columns.Count - 3].Visible = false;               
            }

        }
                
        #endregion 
       
        #region 辅助方法
        //更改"全部审批"的状态
        private void SetAuditAllStatus(bool status)
        {
            this.chbAuditAll.AutoPostBack = false;
            this.chbAuditAll.Checked = status;
            this.chbAuditAll.AutoPostBack = true;
        }

        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }

        //设置审核状态        
        private void SetAuditStatus(Audit status)
        {
            switch (status)
            {
                case Audit.All:
                    for (int i = 0; i < this.spgviewAuditControl.Rows.Count; i++)
                    {
                        ((Label)this.spgviewAuditControl.Rows[i].FindControl("LBItem")).Text = "<font color = red>已通过</font>";
                        ViewState[i.ToString()] = "<font color = red>已通过</font>";                                           
                    }
                    btnSend.Enabled = true;
                    ViewState["AuditCount"] = this.spgviewAuditControl.Rows.Count;     
                    ViewState["AuditFailedCount"] = 0;
                    break;
                case Audit.None:
                    for (int i = 0; i < this.spgviewAuditControl.Rows.Count; i++)
                    {
                        ((Label)this.spgviewAuditControl.Rows[i].FindControl("LBItem")).Text = "未通过";
                        ViewState[i.ToString()] = "未通过";                                               
                    }
                    ViewState["AuditCount"] = 0;
                    ViewState["AuditFailedCount"] = 0;
                    btnSend.Enabled = true;
                    break;
                case Audit.Normal:
                    int iWaitforAuditCount = 0;
                    int iAuditFailedCount = 0;
                    for (int i = 0; i < this.spgviewAuditControl.Rows.Count; i++)
                    {
                        string strAudit = ViewState[i.ToString()].ToString();
                        ((Label)this.spgviewAuditControl.Rows[i].FindControl("LBItem")).Text = strAudit;
                        if (strAudit == "待审核")
                            iWaitforAuditCount++;
                        if (strAudit == "未通过")
                            iAuditFailedCount++;
                    }
                    ViewState["AuditCount"] = spgviewAuditControl.Rows.Count - iWaitforAuditCount - iAuditFailedCount;
                    ViewState["AuditFailedCount"] = iAuditFailedCount;
                    if (iWaitforAuditCount != 0)
                        btnSend.Enabled = false;                                         
                    break;
                case Audit.Init:                    
                    for (int i = 0; i < this.spgviewAuditControl.Rows.Count; i++)
                    {
                        ((Label)this.spgviewAuditControl.Rows[i].FindControl("LBItem")).Text = "待审核";
                        ViewState[i.ToString()] = "待审核";                                                
                    }
                    ViewState["AuditCount"] = 0;
                    ViewState["AuditFailedCount"] = 0;
                    break;
            }
        }
        private enum Audit { All, None, Normal, Init };

        //判断是否被审核过
        private bool IsAudited()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var tmpResult = from a in db.StorageInAudited
                                where a.StorageInID == _storageinid && a.BatchIndex == _batchname
                                select a;
                if (tmpResult.ToList<StorageInAudited>().Count == 0)
                    return false;
            }

            return true;
        }        
        
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string strTaskstorageid = Request.QueryString["TaskStorageID"];

                if (string.IsNullOrEmpty(strTaskstorageid))
                {
                    Response.Redirect(_errorpage);
                    return;
                }
                else
                {
                    this._taskstorageid = Convert.ToInt32(strTaskstorageid);
                    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskstorageid);
                        this._storageinid = tsi.StorageInID;
                        this._batchname = tsi.QCBatch;

                    }
                    InitializeCustomControls();
                    BindGridView();
                    ShowCustomControls();

                    if (this.IsAudited())
                        Response.Redirect("AuditedModify.aspx?TaskStorageID=" + _taskstorageid + " ");
                    else
                    {
                        if (!Page.IsPostBack)
                        {
                            //初始化审核状态为"待审核"                        
                            SetAuditStatus(Audit.Init);
                        }
                        else
                        {
                            //审核后更改审核状态
                            SetAuditStatus(Audit.Normal);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }            
        }        

        void btnSend_Click(object sender, EventArgs e)
        {                       
                foreach (GridViewRow gvr in this.spgviewAuditControl.Rows)
                {
      //              Label lb = (Label)gvr.Cells[20].Controls[0];
                    InsertAudited(gvr);                 

                }
             
                Response.Redirect("AuditedManage.aspx?StorageInID=" + _storageinid + "&&TaskStorageID=" + _taskstorageid + " ");
 
        }

        private void InsertAudited(GridViewRow gvr)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    Label lb = (Label)gvr.Cells[23].Controls[0];



                    //新增

                    StorageInAudited sia = new StorageInAudited();
                    sia.StorageInID = _storageinid;
                    sia.MaterialID = (db.MaterialInfo.SingleOrDefault(u => u.MaterialID == Convert.ToInt32( gvr.Cells[25].Text))).MaterialID;
                    sia.SpecificationModel = gvr.Cells[4].Text;
                   

                    sia.QuantityGentaojian = Convert.ToDecimal(gvr.Cells[5].Text);
                    sia.QuantityMetre = Convert.ToDecimal(gvr.Cells[6].Text);
                    sia.QuantityTon = Convert.ToDecimal(gvr.Cells[7].Text);
                    sia.Quantity = Convert.ToDecimal(gvr.Cells[8].Text);
                    sia.CurUnit = gvr.Cells[9].Text;

                    sia.NumberQualified = 0;
                    sia.InspectionTime = Convert.ToDateTime(gvr.Cells[10].Text);
                    sia.InspectionReportNum = gvr.Cells[11].Text;
                    sia.UnitPrice = Convert.ToDecimal(gvr.Cells[12].Text);
                    sia.Amount = Convert.ToDecimal(gvr.Cells[13].Text);
                    sia.PileID = (db.PileInfo.SingleOrDefault(u => u.PileCode == gvr.Cells[15].Text)).PileID;
                    sia.financeCode = gvr.Cells[16].Text;
                    sia.StorageTime = Convert.ToDateTime(gvr.Cells[17].Text);
                    sia.BatchIndex = gvr.Cells[18].Text;

                    var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                    sia.AuditTime = SevTime.First();
                    sia.Remark = gvr.Cells[22].Text;

                    sia.SupplierID = Convert.ToInt32(gvr.Cells[26].Text);
                    sia.MaterialsManager = Convert.ToInt32(gvr.Cells[27].Text);
                    sia.WarehouseWorker = Convert.ToInt32(gvr.Cells[28].Text);

                    sia.AuditStatus = lb.Text == "未通过" ? "未通过" : "已通过";
                    sia.MaterialAccounting = (db.EmpInfo.SingleOrDefault(u => u.EmpName == SPContext.Current.Web.CurrentUser.Name)).EmpID;

                    db.StorageInAudited.InsertOnSubmit(sia);




                    db.SubmitChanges();

                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
            }
        }

        void chbAuditAll_CheckedChanged(object sender, EventArgs e)
        {
            if (chbAuditAll.Checked)            
                SetAuditStatus(Audit.All);                            
            else            
                SetAuditStatus(Audit.None);                            

        }

        

        void spgviewAuditControl_SelectedIndexChanging(object sender, GridViewSelectEventArgs e)
        {
            try
            {
                GridViewRow gvr = this.spgviewAuditControl.Rows[e.NewSelectedIndex];
                Label lb = (Label)gvr.FindControl("LBItem");
                int iAuditCount = Convert.ToInt32(ViewState["AuditCount"]);
                int iAuditFailedCount = Convert.ToInt32(ViewState["AuditFailedCount"]);

                switch (lb.Text)
                {
                    case "未通过":
                        lb.Text = "<font color = red>已通过</font>";
                        ViewState[e.NewSelectedIndex.ToString()] = "<font color = red>已通过</font>";

                        //所有项都是已通过时,将"全部通过审核"置为True
                        iAuditCount++;
                        iAuditFailedCount--;
                        if (iAuditCount == this.spgviewAuditControl.Rows.Count)
                            SetAuditAllStatus(true);
                        break;
                    case "<font color = red>已通过</font>":
                        lb.Text = "未通过";
                        ViewState[e.NewSelectedIndex.ToString()] = "未通过";
                        iAuditCount--;
                        iAuditFailedCount++;
                        SetAuditAllStatus(false);
                        break;
                    case "待审核":
                        lb.Text = "<font color = red>已通过</font>";
                        ViewState[e.NewSelectedIndex.ToString()] = "<font color = red>已通过</font>";
                        //所有项都是已通过时,将"全部通过审核"置为True
                        iAuditCount++;
                        if (iAuditCount == this.spgviewAuditControl.Rows.Count)
                            SetAuditAllStatus(true);
                        break;
                }

                ViewState["AuditCount"] = iAuditCount;
                ViewState["AuditFailedCount"] = iAuditFailedCount;

                int icount = iAuditCount + iAuditFailedCount;
                if (icount == this.spgviewAuditControl.Rows.Count)
                    btnSend.Enabled = true;
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
        }                                      

    }    
    
}
