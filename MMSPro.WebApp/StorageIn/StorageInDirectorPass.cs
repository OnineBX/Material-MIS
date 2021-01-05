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
    public class StorageInDirectorPass: System.Web.UI.Page
    {
        MMSProDBDataContext db;
        SPGridView gv;
        Button btnPass;
        static string[] Titlelist = {
                                     "交货通知单编号:StorageInCode",
                                     "物料名称:MaterialName",
                                     "物料编码:MaterialCode",
                                     "物料规格:SpecificationModel",
                                     "已审核根/套/件数量:QuantityGentaojian",
                                     "已审核米数量:QuantityMetre",
                                     "已审核吨数量:QuantityTon",
                                     "所选单位数量:Quantity",
                                     "计量单位:CurUnit",
                                     "单价:UnitPrice",
                                     "金额:Amount",

                                     "合格数量:NumberQualified",
                                     "质检号:InspectionReportNum",
                                     "质检时间:InspectionTime",

                                     "所属仓库:StorageName",
                                     "所属垛位:PileCode",
                                     "财务编号:financeCode",
                                     "到库时间:StorageTime",
                                     "供应商:SupplierName",
                                     "物资管理员:MaterialsManager",
                                     "资产管理员:WarehouseWorker",

                                     "材料会计:MaterialAccounting",
                                     "会计审核状态:AuditStatus",
                                     "会计审核时间:AuditTime",
                                     "主任审批状态:AuditStatus",
                                     "主任审批时间:AuditTime",
                                     "所属批次:BatchIndex",  
                                     "备注:Remark",

                                     
                                    };



        protected void Page_Load(object sender, EventArgs e)
        {
            this.gv = new SPGridView();
            this.gv.AutoGenerateColumns = false;
            try
            {

                BindGridView();

                //添加按钮到toolbar
                ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");

                ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
                btnRefresh.ID = "btnRefresh";
                btnRefresh.Text = "刷新";
                btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
                btnRefresh.Padding = "0,5,0,0";
                btnRefresh.Click += new EventHandler(btnRefresh_Click);
                tbarTop.RightButtons.Controls.Add(btnRefresh);
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
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
            Response.Redirect("QualityControlManage.aspx?TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&StorageInID=" + Request.QueryString["StorageInID"] + "");
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

        protected void tbarbtnDelte_Click(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_DELETEERROR));
            }

        }

        /// <summary>
        /// 绑定Gridview
        /// </summary>
        private void BindGridView()
        {
            int sid = Convert.ToInt32(Request.QueryString["StorageInID"]);
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
                this.gv.DataSource = from a in db.StorageInDirector
                                     join b in db.StorageIn on a.StorageInID equals b.StorageInID
                                     join c in db.MaterialInfo on a.MaterialID equals c.MaterialID
                                     join d in db.PileInfo on a.PileID equals d.PileID
                                     join e in db.SupplierInfo on a.SupplierID equals e.SupplierID

                                     where a.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.BatchIndex == Request.QueryString["QCBatch"]
                                     select new
                                     {
                                         a.StorageInDirectorID,
                                         a.SpecificationModel,
                                         b.StorageInCode,
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
                                         MaterialAccounting = db.EmpInfo.SingleOrDefault(u => u.EmpID == a.MaterialAccounting).EmpName,
                                         a.AuditStatus,
                                         a.AuditTime,
                                         a.BatchIndex,
                                         a.Remark
                                     };
                this.gv.DataBind();
                btnPass = new Button();
                btnPass.Text = "完成 ";
                btnPass.Click += new EventHandler(btnSend_Click);

  


                Panel p1 = (Panel)GetControltByMaster("Panel1");

                p1.Controls.Add(this.gv);
                p1.Controls.Add(btnPass);



            }

        }

        void btnPassNagetive_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void btnCannel_Click(object sender, EventArgs e)
        {
            Response.Redirect("QualityControlManage.aspx?TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&StorageInID=" + Request.QueryString["StorageInID"] + "");
        }

        void btnSend_Click(object sender, EventArgs e)
        {

            Response.Redirect("../../default-old.aspx",false);

            //Response.Redirect("QualityControlMessage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&state=材料会计审核");
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