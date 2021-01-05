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

namespace MMSPro.WebApp
{
    public class QualifiedManage:System.Web.UI.Page
    {
        MMSProDBDataContext db;
        SPGridView gv;
        CheckBox chbAgree;
        TextBox txtOpinion;
        Button btnOK;
        string _taskID;
        static string[] Titlelist = {
                                     "交货通知单编号:StorageInCode",
                                     "物料名称:MaterialName",
                                     "物料编码:MaterialCode",
                                     "物料规格:SpecificationModel",
                                     "质检根/套/件数量:QuantityGentaojian",
                                     "质检米数量:QuantityMetre",
                                     "质检吨数量:QuantityTon",
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
                                     "供应商:SupplierName",
                                     "物资管理员:MaterialsManager",
                                     "仓库员:WarehouseWorker",
                                     "备注:Remark",
                                     "所属批次:BatchIndex"

                                    };



        protected void Page_Load(object sender, EventArgs e)
        {
            _taskID = Request.QueryString["TaskStorageID"];

            this.gv = new SPGridView();
            this.gv.AutoGenerateColumns = false;
            
            initControl();
            
            BindGridView();
            
            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");
            //新建
            //ToolBarButton tbarbtnAdd = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            //tbarbtnAdd.ID = "AddNewRow";
            //tbarbtnAdd.Text = "新建";
            //tbarbtnAdd.ImageUrl = "/_layouts/images/newitem.gif";
            //tbarbtnAdd.Click += new EventHandler(tbarbtnAdd_Click);
            //tbarTop.Buttons.Controls.Add(tbarbtnAdd);
            //修改
            ToolBarButton tbarbtnEdit = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnEdit.ID = "EditRow";
            tbarbtnEdit.Text = "修改";
            tbarbtnEdit.ImageUrl = "/_layouts/images/edit.gif";
            tbarbtnEdit.Click += new EventHandler(tbarbtnEdit_Click);
            tbarTop.Buttons.Controls.Add(tbarbtnEdit);

            //发送审核
            //ToolBarButton tbarbtncheck = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            //tbarbtncheck.ID = "sendRow";
            //tbarbtncheck.Text = "发送审核";
            //tbarbtncheck.ImageUrl = "/_layouts/images/32316.GIF";
            //tbarbtncheck.Click += new EventHandler(tbarbtncheck_Click);
            //tbarTop.Buttons.Controls.Add(tbarbtncheck);

            //删除

            //ToolBarButton tbarbtnDelte = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            //tbarbtnDelte.ID = "DeleteRow";
            //tbarbtnDelte.Text = "删除";
            //tbarbtnDelte.ImageUrl = "/_layouts/images/delete.gif";
            //tbarbtnDelte.Click += new EventHandler(tbarbtnDelte_Click);
            //StringBuilder sbScript = new StringBuilder();
            //sbScript.Append("var aa= window.confirm('确认删除所选项?');");
            //sbScript.Append("if(aa == false){");
            //sbScript.Append("return false;}");
            //tbarbtnDelte.OnClientClick = sbScript.ToString();
            //tbarTop.Buttons.Controls.Add(tbarbtnDelte);

            ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnRefresh.ID = "btnRefresh";
            btnRefresh.Text = "刷新";
            btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
            btnRefresh.Padding = "0,5,0,0";
            btnRefresh.Click += new EventHandler(btnRefresh_Click);
            tbarTop.RightButtons.Controls.Add(btnRefresh);
        }

        void tbarbtncheck_Click(object sender, EventArgs e)
        {
            
        }

        protected void tbarbtnAdd_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request.QueryString["StorageInID"]))
            {
                Response.Redirect("StorageInDetailedCreate.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "");
            }
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");

        }

        protected void tbarbtnEdit_Click(object sender, EventArgs e)
        {
            Response.Redirect("QualityControlManage.aspx?TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&StorageInID=" + Request.QueryString["StorageInID"] + "&&QualifiedBatch=" + Request.QueryString["QCBatch"] + "");
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {
            //Response.Redirect("QualifiedManage.aspx?TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&StorageInID=" + Request.QueryString["StorageInID"] + "");

        }
        
        private void initControl()
        {
            chbAgree = (CheckBox)GetControltByMaster("chbAgree");
            chbAgree.CheckedChanged += new EventHandler(chbAgree_CheckedChanged);
            txtOpinion = (TextBox)GetControltByMaster("txtOpinion");
            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Text = "完成审核";
            btnOK.Click += new EventHandler(btnOK_Click);
           
            
        }

        void chbAgree_CheckedChanged(object sender, EventArgs e)
        {
            if (chbAgree.Checked == true)
            {
                this.txtOpinion.Text = "质检合格";
            }
            else
            {
                this.txtOpinion.Text = "请写下不合格原因...";
                this.txtOpinion.Enabled = true;
            }

        }

        void btnOK_Click(object sender, EventArgs e)
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                if (chbAgree.Checked == true)
                {
                    Response.Redirect("QualityControlMessage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&QCBatch=" + Request.QueryString["QCBatch"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&state=材料会计审核&&storageInType=正常入库");
                }
                else
                {

                    //任务信息
                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID ==Convert.ToInt32( _taskID));
                    tsi.TaskState = "已完成";
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

                    TSI.StorageInID = Convert.ToInt32(Request.QueryString["StorageInID"]);
                    TSI.StorageInType = "正常入库";
                    TSI.TaskTitle = "质检任务：" + tsi.TaskTitle.ToString() + "(质检未通过)";
                    TSI.TaskState = "未完成";
                    TSI.TaskDispose = "未废弃";
                    TSI.TaskType = "质检前清单";
                    TSI.InspectState = "驳回";

                    //TSI.BatchOfIndex = this.ddlbatch.SelectedItem.Text.ToString();

                    TSI.QCBatch = tsi.QCBatch;


                    TSI.Remark = "交货通知单编号为:" + si.StorageInCode + "的" + tsi.QCBatch.ToString() + "物资质检不合格原因：" + this.txtOpinion.Text.Trim();
                    var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                    TSI.CreateTime = SevTime.First();
                    db.TaskStorageIn.InsertOnSubmit(TSI);
                    db.SubmitChanges();
                    Response.Redirect("../../default-old.aspx",false);
                }
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

        protected void tbarbtnDelte_Click(object sender, EventArgs e)
        {
            List<CheckBox> listString = GetCheckedID();
            if (listString.Count > 0)
            {
                StorageInDetailed SID;
                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    foreach (var li in listString)
                    {
                        SID = db.StorageInDetailed.SingleOrDefault(a => a.StorageDetailedID == int.Parse(li.ToolTip));
                        if (SID != null)
                        {
                            db.StorageInDetailed.DeleteOnSubmit(SID);

                        }
                    }
                    db.SubmitChanges();
                }
                Response.Redirect("StorageInDetailedManage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "");
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择需要删除的记录!')</script>");
            }

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
                this.gv.DataSource = from a in db.StorageInQualified
                                     join b in db.StorageIn on a.StorageInID equals b.StorageInID
                                     join c in db.MaterialInfo on a.MaterialID equals c.MaterialID
                                     join d in db.PileInfo on a.PileID equals d.PileID
                                     join e in db.SupplierInfo on a.SupplierID equals e.SupplierID

                                     where a.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.BatchIndex == Request.QueryString["QCBatch"].ToString()
                                     select new
                                     {
                                         a.StorageInQualifiedID,
                                         b.StorageInCode,
                                         a.SpecificationModel,
                                         a.BatchIndex,
                                         c.MaterialName,
                                         c.MaterialCode,
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

                                         a.Remark
                                     };
                this.gv.DataBind();
                //btnSend = new Button();
                //btnSend.Text ="发送审核";
                //btnSend.Click +=new EventHandler(btnSend_Click);
                //btnCannel = new Button();
                //btnCannel.Text = "取消";
                //btnCannel.Click += new EventHandler(btnCannel_Click);
                Panel p1 = (Panel)GetControltByMaster("Panel1");
               
                p1.Controls.Add(this.gv);
                //p1.Controls.Add(btnSend);
                //p1.Controls.Add(btnCannel);

            }

        }

        void btnCannel_Click(object sender, EventArgs e)
        {
            Response.Redirect("QualityControlManage.aspx?TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&StorageInID=" + Request.QueryString["StorageInID"] + "&&QualifiedBatch=" + Request.QueryString["QCBatch"] + "");
        }

        //void btnSend_Click(object sender, EventArgs e)
        //{
        //    Response.Redirect("QualityControlMessage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&QCBatch=" + Request.QueryString["QCBatch"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&state=材料会计审核");
        //}

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
