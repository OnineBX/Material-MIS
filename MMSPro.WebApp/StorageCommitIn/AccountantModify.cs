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
    public class AccountantModify : System.Web.UI.Page
    {
        private int _storageinid;              //交货通知单编号
        private int _taskstorageid;            //任务编号
        private string _taskstate;             //任务状态

        private string _errorpage = "123.aspx";

        SPGridView spgviewAuditControl;
        Button btnModify,btnCancel;
        CheckBox chbAuditAll;

        #region 初始化和数据绑定
        private void InitializeCustomControls()
        {
            this.spgviewAuditControl = new SPGridView();
            this.spgviewAuditControl.AutoGenerateColumns = false;
            this.spgviewAuditControl.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");
            string[] ShowTlist =  { 
                                     "交货通知单编号:CommitInCode",
                                     "入库单号:CommitInQualifiedNum",
                                     "物料名称:MaterialName",
                                     "物料编码:MaterialCode",
                                     "规格型号:SpecificationModel",
                                     "质检合格根/套/件数量:QuantityGentaojian",
                                     "质检合格米数量:QuantityMetre",
                                     "质检合格吨数量:QuantityTon",
                                     "所选单位数量:Quantity",
                                     "计量单位:CurUnit",
                                     "单价:UnitPrice",
                                     "金额:Amount",
                                     "质检时间:InspectionTime",
                                     "检验报告号:InspectionReportNum",
                                     "所属仓库:StorageName",
                                     "所属垛位:PileCode",
                                     "财务编号:financeCode",
                                     "到库时间:StorageTime",
                                     "供应商:SupplierName",
                                     "物资管理员:MaterialsManager",
                                     "仓库员:WarehouseWorker",                                     
                                     "备注:Remark",
                                     "状态:AuditStatus",
                                     "审核时间:AuditTime",
                                     //"CommitInAuditedID:CommitInAuditedID"
                                   };
                                
            BoundField bfColumn;

            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgviewAuditControl.Columns.Add(bfColumn);
            }

            //添加审核列
            CommandField cdf = new CommandField();
            cdf.HeaderText = "审核";            
            cdf.SelectText = @"通过/未通过";            
            cdf.ShowSelectButton = true;
            this.spgviewAuditControl.SelectedIndexChanging += new GridViewSelectEventHandler(spgviewAuditControl_SelectedIndexChanging);
            this.spgviewAuditControl.Columns.Add(cdf);



            bfColumn = new BoundField();
            bfColumn.HeaderText = "CommitInAuditedID";
            bfColumn.DataField = "CommitInAuditedID";
            this.spgviewAuditControl.Columns.Add(bfColumn);
            //添加控件到panel1            

            btnModify = new Button();
            btnModify.Text = "修改审核清单";
            btnModify.Click += new EventHandler(btnModify_Click);

            btnCancel = new Button();
            btnCancel.Text = "返回";            
            btnCancel.Click += new EventHandler(btnCancel_Click);

            btnCancel.Width = 100;
            btnModify.Width = 100;

            chbAuditAll = new CheckBox();
            chbAuditAll.Checked = false;
            chbAuditAll.AutoPostBack = true;
            chbAuditAll.Text = "<font size = 2pt>全部通过审核</font>";
            chbAuditAll.CheckedChanged += new EventHandler(chbAuditAll_CheckedChanged);


        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");  
            p1.Controls.Add(this.spgviewAuditControl);
            Panel p2 = (Panel)GetControltByMaster("Panel2");
            if (_taskstate.Equals("未完成"))
            {
                this.spgviewAuditControl.Columns[22].Visible = true;
                p2.Controls.Add(new LiteralControl("<BR/>"));
                p2.Controls.Add(chbAuditAll);
                p2.Controls.Add(new LiteralControl("<BR/><BR/>"));
                p2.Controls.Add(btnModify);
            }
            else
            {
                this.spgviewAuditControl.Columns[22].Visible = false;
                p2.Controls.Add(new LiteralControl("<BR/><font size = 2pt color = green>信息：该任务已完成,您正在查看审核清单...</font><BR/><BR/>"));                
            }
            p2.Controls.Add(btnCancel);
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("../../default-old.aspx",false);
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

                CommitIn siq = db.CommitIn.SingleOrDefault(s => s.CommitInID == _storageinid);
                if (siq == null)
                {
                    Response.Redirect(_errorpage);
                    return;
                }
                
                this.spgviewAuditControl.DataSource = from a in db.CommitInAudited
                                                      join b in db.CommitIn on a.CommitInID equals b.CommitInID
                                                      join c in db.MaterialInfo on a.MaterialID equals c.MaterialID
                                                      join d in db.PileInfo on a.PileID equals d.PileID
                                                      join e in db.SupplierInfo on a.SupplierID equals e.SupplierID
                                                      where a.CommitInID == _storageinid
                                                      select new
                                                      {
                                                          a.CommitInAuditedID,
                                                          b.CommitInCode,
                                                          c.MaterialName,
                                                          //c.MaterialCode,
                                                          b.CommitInQualifiedNum,
                                                          a.SpecificationModel,
                                                          a.Quantity,
                                                          a.UnitPrice,
                                                          a.Amount,
                                                          a.QuantityGentaojian,
                                                          a.QuantityMetre,
                                                          a.QuantityTon,
                                                          a.CurUnit,
                                                          a.NumberQualified,
                                                          a.InspectionReportNum,
                                                          a.InspectionTime,
                                                          d.StorageInfo.StorageName,
                                                          d.PileCode,
                                                          a.financeCode,
                                                          a.StorageTime,
                                                          e.SupplierName,
                                                          e.SupplierID,
                                                          MaterialsManager = db.EmpInfo.SingleOrDefault(u => u.EmpID == a.MaterialsManager).EmpName,
                                                          WarehouseWorker = db.EmpInfo.SingleOrDefault(u => u.EmpID == a.WarehouseWorker).EmpName,
                                                          MaterialAccounting = db.EmpInfo.SingleOrDefault(u => u.EmpID == a.MaterialAccounting).EmpName,
                                                          a.AuditStatus,
                                                          a.AuditTime,
                                                          a.Remark,
                                                          MaterialsManagerID = a.MaterialsManager,
                                                          WarehouseWorkerID = a.WarehouseWorker
                                                      };

                this.spgviewAuditControl.DataBind();
                this.spgviewAuditControl.Columns[this.spgviewAuditControl.Columns.Count - 1].Visible = false;
           
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
                        this.spgviewAuditControl.Rows[i].Cells[22].Text = "<font color = red>已通过</font>";
                        ViewState[i.ToString()] = "<font color = red>已通过</font>";
                    }                    
                    ViewState["AuditCount"] = this.spgviewAuditControl.Rows.Count;
                    break;
                case Audit.None:
                    for (int i = 0; i < this.spgviewAuditControl.Rows.Count; i++)
                    {
                        this.spgviewAuditControl.Rows[i].Cells[22].Text = "未通过";                        
                        ViewState[i.ToString()] = "未通过";
                    }
                    ViewState["AuditCount"] = 0;                  
                    break;
                case Audit.Normal:                   
                    int iAuditFailedCount = 0;
                    for (int i = 0; i < this.spgviewAuditControl.Rows.Count; i++)
                    {
                        string strAudit = ViewState[i.ToString()].ToString();
                        this.spgviewAuditControl.Rows[i].Cells[22].Text = strAudit;                        
                        if (strAudit == "未通过")
                            iAuditFailedCount++;
                    }
                    ViewState["AuditCount"] = spgviewAuditControl.Rows.Count - iAuditFailedCount;                   
                    break;                
            }
        }
        private enum Audit { All, None, Normal, Init };
        
        private void ChangeStatusStyle()
        {           
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                int iAuditCount = this.spgviewAuditControl.Rows.Count;
                string strAudit;
                for (int i = 0; i < this.spgviewAuditControl.Rows.Count; i++)
                {
                    strAudit = this.spgviewAuditControl.Rows[i].Cells[22].Text;
                    if (strAudit == "未通过")
                    {
                        this.spgviewAuditControl.Rows[i].Cells[22].Text = strAudit;
                        iAuditCount--;
                    }
                    else
                        this.spgviewAuditControl.Rows[i].Cells[22].Text = "<font color = red>已通过</font>";


                    ViewState[i.ToString()] = this.spgviewAuditControl.Rows[i].Cells[23].Text;
                }
                ViewState["AuditCount"] = iAuditCount;
                if (iAuditCount == this.spgviewAuditControl.Rows.Count)
                    SetAuditAllStatus(true);
                else
                    SetAuditAllStatus(false);
            }
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
                        this._taskstate = tsi.TaskState;

                    }

                    InitializeCustomControls();
                    BindGridView();
                    ShowCustomControls();
                    ChangeStatusStyle();
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
        }

        void btnModify_Click(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    CommitInAudited sia;
                    string status;
                    foreach (GridViewRow gvr in this.spgviewAuditControl.Rows)
                    {
                        status = gvr.Cells[22].Text == "未通过" ? "未通过" : "已通过";
                        sia = db.CommitInAudited.SingleOrDefault(u => u.CommitInAuditedID == Convert.ToInt32(gvr.Cells[21].Text));

                        if (!sia.AuditStatus.Equals(status))
                        {
                            sia.AuditStatus = status;
                            sia.AuditTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                        }
                        else
                            continue;

                        db.SubmitChanges();
                    }

                    Response.Redirect("AccountantManage.aspx?TaskStorageID=" + _taskstorageid + " ");
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_UPDATEERROR));
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
                string strStatus = gvr.Cells[22].Text;
                int iAuditCount = Convert.ToInt32(ViewState["AuditCount"]);

                switch (strStatus)
                {
                    case "未通过":
                        gvr.Cells[22].Text = "<font color = red>已通过</font>";
                        ViewState[e.NewSelectedIndex.ToString()] = "<font color = red>已通过</font>";

                        //所有项都是已通过时,将"全部通过审核"置为True
                        iAuditCount++;
                        if (iAuditCount == this.spgviewAuditControl.Rows.Count)
                            SetAuditAllStatus(true);
                        break;
                    case "<font color = red>已通过</font>":
                        gvr.Cells[22].Text = "未通过";
                        ViewState[e.NewSelectedIndex.ToString()] = "未通过";
                        iAuditCount--;
                        SetAuditAllStatus(false);
                        break;
                }

                ViewState["AuditCount"] = iAuditCount;
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
