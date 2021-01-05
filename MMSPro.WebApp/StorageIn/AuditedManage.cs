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
    public class AuditedManage : System.Web.UI.Page
    {
        private int _storageinid;
        private int _taskstorageid;
        private string _batchname;        
        
        MMSProDBDataContext db;
        SPGridView gv;
        //Button btnSend;
        //Button btnCancel;
        ToolBar tbarTop;


        Button btnReCheck;
        Label lblTitle;
        TextBox txtOpinion;
        ToolBarButton tbarbtnApprove;

        static string[] Titlelist = {
                                     "交货通知单编号:StorageInCode",
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
                                     "质检号:InspectionReportNum",
                                     "质检时间:InspectionTime",
                                     "所属仓库:StorageName",
                                     "所属垛位:PileCode",
                                     "财务编号:financeCode",
                                     "到库时间:StorageTime",
                                     "批次信息:BatchIndex",
                                     "供应商:SupplierName",
                                     "物资管理员:MaterialsManager",
                                     "仓库员:WarehouseWorker",
                                     "材料会计:MaterialAccounting",
                                     "审核状态:AuditStatus",
                                     "审核时间:AuditTime",
                                     "备注:Remark"
                                    };



        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                _taskstorageid = Convert.ToInt32(Request.QueryString["TaskStorageID"]);

                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskstorageid);
                    this._storageinid = tsi.StorageInID;
                    this._batchname = tsi.QCBatch;

                }

                init();

                BindGridView();
                InitButton();
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
        }
        private void init()
        {
            this.gv = new SPGridView();
            this.gv.AutoGenerateColumns = false;
            this.gv.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");

            //添加按钮到toolbar
            tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");

            ToolBarButton tbarbtnEdit = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnEdit.ID = "EditRow";
            tbarbtnEdit.Text = "修改";
            tbarbtnEdit.ImageUrl = "/_layouts/images/edit.gif";
            tbarbtnEdit.Click += new EventHandler(tbarbtnEdit_Click);
            tbarTop.Buttons.Controls.Add(tbarbtnEdit);

            tbarbtnApprove = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnApprove.ID = "ApproveRow";
            tbarbtnApprove.Text = "发送审批";
            tbarbtnApprove.ImageUrl = "/_layouts/images/edit.gif";
            tbarbtnApprove.Click += new EventHandler(tbarbtnApprove_Click);
            tbarTop.Buttons.Controls.Add(tbarbtnApprove);

            ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnRefresh.ID = "btnRefresh";
            btnRefresh.Text = "刷新";
            btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
            btnRefresh.Padding = "0,5,0,0";
            btnRefresh.Click += new EventHandler(btnRefresh_Click);
            tbarTop.RightButtons.Controls.Add(btnRefresh);

            ToolBarButton btnBack = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnBack.ID = "btnBack";
            btnBack.Text = "返回";
            btnBack.ImageUrl = "/_layouts/images/refresh.GIF";
            btnBack.Padding = "0,5,0,0";
            btnBack.Click += new EventHandler(btnBack_Click);
            tbarTop.RightButtons.Controls.Add(btnBack);
        }

        void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("AuditedModify.aspx?TaskStorageID=" + Request.QueryString["TaskStorageID"] + "");
        }

        void tbarbtnApprove_Click(object sender, EventArgs e)
        {
            Response.Redirect("MaterialAccountantMessage.aspx?StorageInID=" + _storageinid + "&&TaskStorageID=" + _taskstorageid + "&&state=主任审批");
        }        

        protected void tbarbtnEdit_Click(object sender, EventArgs e)
        {
            Response.Redirect("AuditedModify.aspx?TaskStorageID=" + _taskstorageid + " ");
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {
            Response.Redirect("AuditedManage.aspx?TaskStorageID=" + Request.QueryString["TaskStorageID"] + " ");

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
                BoundField bfColumn;
                //添加选择列

                foreach (var kvp in Titlelist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.gv.Columns.Add(bfColumn);
                }
                this.gv.DataSource = from a in db.StorageInAudited
                                     join b in db.StorageIn on a.StorageInID equals b.StorageInID
                                     join c in db.MaterialInfo on a.MaterialID equals c.MaterialID
                                     join d in db.PileInfo on a.PileID equals d.PileID
                                     join e in db.SupplierInfo on a.SupplierID equals e.SupplierID
                                     where a.StorageInID == _storageinid && a.BatchIndex == this._batchname
                                     //&& a.AuditStatus == "已通过"
                                     select new
                                     {
                                         a.StorageInAuditedID,
                                         b.StorageInCode,
                                         c.MaterialName,
                                         c.MaterialCode,
                                         a.BatchIndex,
                                         a.SpecificationModel,
                                         a.Quantity,
                                         a.QuantityGentaojian,
                                         a.QuantityMetre,
                                         a.QuantityTon,
                                         a.CurUnit,
                                         a.UnitPrice,
                                         a.Amount,
                                         a.NumberQualified,
                                         a.InspectionReportNum,
                                         a.InspectionTime,
                                         d.StorageInfo.StorageName,
                                         d.PileCode,
                                         a.financeCode,
                                         a.StorageTime,
                                         e.SupplierName,
                                         MaterialsManager = db.EmpInfo.SingleOrDefault(u => u.EmpID == a.MaterialsManager).EmpName,
                                         WarehouseWorker = db.EmpInfo.SingleOrDefault(u => u.EmpID == a.WarehouseWorker).EmpName,
                                         MaterialAccounting = db.EmpInfo.SingleOrDefault(u =>u.EmpID == a.MaterialAccounting).EmpName,
                                         a.AuditStatus,
                                         a.AuditTime,
                                         a.Remark
                                     };
                this.gv.DataBind();
                
                Panel p1 = (Panel)GetControltByMaster("Panel1");
                p1.Controls.Add(this.gv);                

            }

        }

        private void InitButton()
        {           

            Panel p1 = (Panel)GetControltByMaster("Panel1");
            Panel p2 = (Panel)GetControltByMaster("Panel2");
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //同一批次的物资都通过审核才可以发送审批
                var qualified = from a in db.StorageInQualified
                                where a.StorageInID == _storageinid && a.BatchIndex == _batchname
                                select a;
                var audited = from b in db.StorageInAudited
                              where b.StorageInID == _storageinid && b.BatchIndex == _batchname && b.AuditStatus == "已通过"
                              select b;
                if (qualified.Count() == audited.Count())
                {
                    tbarTop.Buttons.Controls[2].Visible = true;
                    p2.Controls.Add(new LiteralControl("<BR/><BR/><font size = 2pt color = green>提示：该批次物资已经全部通过审核,请发送审批...</font>"));
                }
                else
                {
                    tbarTop.Buttons.Controls[2].Visible = false;
                    btnReCheck = new Button();
                    btnReCheck.Text = "重新质检";
                    btnReCheck.Click += new EventHandler(btnReCheck_Click);

                    lblTitle = new Label();
                    lblTitle.Text = "审核意见：";
                    lblTitle.Font.Bold = true;


                    txtOpinion = new TextBox();
                    txtOpinion.TextMode = TextBoxMode.MultiLine;
                    txtOpinion.Width = 279;
                    txtOpinion.Height = 179;
                    
                    p2.Controls.Add(new LiteralControl("<BR/><BR/><font size = 2pt color = red>注意：该批次物资未能全部通过审核,请返回重新质检...</font><br><br>"));
                    p2.Controls.Add(lblTitle);
                    p2.Controls.Add(new LiteralControl("<BR/><BR/>"));
                    p2.Controls.Add(txtOpinion);
                    p2.Controls.Add(new LiteralControl("<BR/><BR/>"));
                    p2.Controls.Add(btnReCheck);
                   
                    
                }
            }                                           
        }
        /// <summary>
        /// 发回重新质检
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnReCheck_Click(object sender, EventArgs e)
        {
            try
            {
                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {

                    //从审核表中删除此批次记录,之前写过库
                    var siaTemp = from a in db.StorageInAudited
                                  where a.StorageInID == _storageinid && a.BatchIndex == _batchname
                                  select new { a.StorageInAuditedID };
                    for (int i = 0; i < siaTemp.ToList().Count; i++)
                    {
                        StorageInAudited sia = db.StorageInAudited.SingleOrDefault(a => a.StorageInAuditedID == siaTemp.ToList()[i].StorageInAuditedID);
                        if (sia != null)
                        {
                            db.StorageInAudited.DeleteOnSubmit(sia);
                        }

                    }
                    db.SubmitChanges();


                    //任务信息
                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskstorageid);
                    tsi.TaskState = "已完成";
                    tsi.InspectState = "已审核";
                    StorageIn si = db.StorageIn.SingleOrDefault(u => u.StorageInID == tsi.StorageInID);
                    //发送新任务(回退任务)
                    TaskStorageIn TSI = new TaskStorageIn();

                    TSI.TaskCreaterID = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
                    TSI.TaskTargetID = tsi.TaskCreaterID;
                    if (TSI.TaskTargetID == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('不存在质检用户，请同步AD账户 ')</script>");
                        return;
                    }

                    TSI.StorageInID = _storageinid;
                    TSI.StorageInType = "正常入库";
                    TSI.TaskTitle = "材料会计审核任务：" + tsi.TaskTitle.ToString() + "(未通过)";
                    TSI.TaskState = "未完成";
                    TSI.TaskDispose = "未废弃";
                    TSI.TaskType = "质检";
                    TSI.InspectState = "驳回";

                    //TSI.BatchOfIndex = this.ddlbatch.SelectedItem.Text.ToString();

                    TSI.QCBatch = tsi.QCBatch;


                    TSI.Remark = "交货通知单编号为:" + si.StorageInCode + "的" + tsi.QCBatch.ToString() + "物资审核不合格原因：" + this.txtOpinion.Text.Trim();
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
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
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

        void btnCannel_Click(object sender, EventArgs e)
        {
            Response.Redirect("AuditedModify.aspx?TaskStorageID=" + Request.QueryString["TaskStorageID"] + "");
        }

        void btnSend_Click(object sender, EventArgs e)
        {
            Response.Redirect("MaterialAccountantMessage.aspx?StorageInID=" + _storageinid + "&&TaskStorageID=" + _taskstorageid + "&&state=主任审批");
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
