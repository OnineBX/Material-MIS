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
    public class DirectorManage : System.Web.UI.Page
    {
        MMSProDBDataContext db;
        SPGridView gv;
        Button btnPass;
        CheckBox chbAgree;
        TextBox txtOpinion;
        static string[] Titlelist = {
                                     "交货通知单编号:CommitInCode",
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
                                     "备注:Remark",
                                     "sid:SupplierID"
                                     
                                    };



        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this.gv = new SPGridView();
                this.gv.AutoGenerateColumns = false;

                BindGridView();

                //添加按钮到toolbar
                ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");

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


                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    TaskStorageIn tk = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == Convert.ToInt32(Request.QueryString["TaskStorageID"]));
                    if (tk != null)
                    {
                        if (tk.TaskState == "已完成")
                        {
                            this.btnPass.Enabled = false;
                            this.chbAgree.Enabled = false;
                            this.txtOpinion.Enabled = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }


        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("../../default-old.aspx",false);
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



        /// <summary>
        /// 绑定Gridview
        /// </summary>
        private void BindGridView()
        {
            int sid =Convert.ToInt32(Request.QueryString["StorageInID"]);
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
                this.gv.DataSource = from a in db.CommitInAudited
                                     join b in db.CommitIn on a.CommitInID equals b.CommitInID
                                     join c in db.MaterialInfo on a.MaterialID equals c.MaterialID
                                     join d in db.PileInfo on a.PileID equals d.PileID
                                     join e in db.SupplierInfo on a.SupplierID equals e.SupplierID

                                     where a.CommitInID == Convert.ToInt32(Request.QueryString["StorageInID"])
                                     select new
                                     {
                                         a.CommitInAuditedID,
                                         a.SpecificationModel,
                                         b.CommitInCode,
                                         c.MaterialName,
                                         //c.MaterialCode,
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
                                         a.Remark,
                                         e.SupplierID
                                         
                                     };
                this.gv.DataBind();
                this.gv.Columns[this.gv.Columns.Count - 1].Visible = false;

                chbAgree = (CheckBox)GetControltByMaster("chbAgree");
                chbAgree.CheckedChanged += new EventHandler(chbAgree_CheckedChanged);
                txtOpinion = (TextBox)GetControltByMaster("txtOpinion");

                btnPass = new Button();
                btnPass.Text = "审批";
                btnPass.Width = 100;
                btnPass.Attributes.Add("onclick", "return confirm('你要执行这个操作吗？');");
                btnPass.Click += new EventHandler(btnSend_Click);




 

          
                Panel p1 = (Panel)GetControltByMaster("Panel1");
                Panel p2 = (Panel)GetControltByMaster("Panel2");
                p1.Controls.Add(this.gv);
                p2.Controls.Add(btnPass);




            }

        }

        void chbAgree_CheckedChanged(object sender, EventArgs e)
        {
            if (chbAgree.Checked == true)
            {
                this.txtOpinion.Text = "通过审批";
            }
            else
            {
                this.txtOpinion.Text = "未通过审批";
            }
        }

 

        void btnPassNagetive_Click(object sender, EventArgs e)
        {
            Response.Redirect("../../default-old.aspx",false);
        }

      

        void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    var tp = from a in db.CommitInDirector
                             where a.CommitInID == Convert.ToInt32(Request.QueryString["StorageInID"])
                             select a;
                    if (tp.ToArray().Length > 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('不能重复插入记录!')</script>");
                        return;
                    }
                }


                if (chbAgree.Checked == true)
                {
                    using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        for (int i = 0; i < this.gv.Rows.Count; i++)
                        {
                            //写入流程中的主任审核表
                            CommitInDirector sid = new CommitInDirector();

                            sid.CommitInID = Convert.ToInt32(Request.QueryString["StorageInID"]);

                            MaterialInfo mi = db.MaterialInfo.SingleOrDefault(u => u.MaterialCode == this.gv.Rows[i].Cells[2].Text.ToString());
                            sid.MaterialID = mi.MaterialID;
                            sid.SpecificationModel = this.gv.Rows[i].Cells[3].Text.ToString();

                            sid.QuantityGentaojian = Convert.ToDecimal(this.gv.Rows[i].Cells[4].Text.ToString());
                            sid.QuantityMetre = Convert.ToDecimal(this.gv.Rows[i].Cells[5].Text.ToString());
                            sid.QuantityTon = Convert.ToDecimal(this.gv.Rows[i].Cells[6].Text.ToString());
                            sid.Quantity = Convert.ToDecimal(this.gv.Rows[i].Cells[7].Text.ToString());
                            sid.CurUnit = this.gv.Rows[i].Cells[8].Text.ToString();
                            sid.NumberQualified = 0;

                            sid.InspectionReportNum = this.gv.Rows[i].Cells[12].Text.ToString();
                            sid.InspectionTime = Convert.ToDateTime(this.gv.Rows[i].Cells[13].Text.ToString());
                            sid.UnitPrice = Convert.ToDecimal(this.gv.Rows[i].Cells[9].Text.ToString());
                            sid.Amount = Convert.ToDecimal(this.gv.Rows[i].Cells[10].Text.ToString());

                            PileInfo pi = db.PileInfo.SingleOrDefault(u => u.PileCode == this.gv.Rows[i].Cells[15].Text.ToString());
                            sid.PileID = pi.PileID;

                            sid.financeCode = this.gv.Rows[i].Cells[16].Text.ToString();
                            sid.StorageTime = Convert.ToDateTime(this.gv.Rows[i].Cells[17].Text.ToString());

                            //这里不唯一要出问题。

                            sid.SupplierID = Convert.ToInt32(this.gv.Rows[i].Cells[25].Text.ToString());

                            var temp = from a in db.CommitInAudited
                                       where a.CommitInID == Convert.ToInt32(Request.QueryString["StorageInID"])
                                       select new { a.MaterialsManager, a.WarehouseWorker, a.MaterialAccounting };
                            var list = temp.ToList();
                            sid.MaterialsManager = list[i].MaterialsManager;
                            sid.WarehouseWorker = list[i].WarehouseWorker;
                            sid.MaterialAccounting = list[i].MaterialAccounting;
                            sid.AuditStatus = this.gv.Rows[i].Cells[22].Text.ToString();
                            sid.AuditTime = Convert.ToDateTime(this.gv.Rows[i].Cells[23].Text.ToString());
                            sid.DirectorStatus = "已审批";
                            var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                            sid.DirectorTime = SevTime.First();
                            sid.Remark = this.gv.Rows[i].Cells[24].Text.ToString();
                            //sid.BatchIndex = this.gv.Rows[i].Cells[20].Text.ToString();
                            sid.Director = reEmpId(SPContext.Current.Web.CurrentUser.LoginName.ToString());
                            db.CommitInDirector.InsertOnSubmit(sid);

                            //写入库存表
                            TableOfStocks tos = new TableOfStocks();
                            tos.StorageInID = Convert.ToInt32(Request.QueryString["StorageInID"]);
                            tos.StorageInType = "委外入库";
                            tos.MaterialID = mi.MaterialID;
                            tos.SpecificationModel = this.gv.Rows[i].Cells[3].Text.ToString();

                            tos.QuantityGentaojian = Convert.ToDecimal(this.gv.Rows[i].Cells[4].Text.ToString());
                            tos.QuantityMetre = Convert.ToDecimal(this.gv.Rows[i].Cells[5].Text.ToString());
                            tos.QuantityTon = Convert.ToDecimal(this.gv.Rows[i].Cells[6].Text.ToString());
                            tos.Quantity = Convert.ToDecimal(this.gv.Rows[i].Cells[7].Text.ToString());
                            tos.CurUnit = this.gv.Rows[i].Cells[8].Text.ToString();

                            tos.UnitPrice = sid.UnitPrice;
                            tos.NumberQualified = 0;
                            tos.PileID = pi.PileID;
                            tos.financeCode = sid.financeCode;
                            tos.StorageTime =sid.StorageTime;
                            tos.SupplierID =sid.SupplierID;
                            tos.MaterialsManager = list[i].MaterialsManager;
                            tos.WarehouseWorker = list[i].WarehouseWorker;
                            sid.Remark = sid.Remark;
                            db.TableOfStocks.InsertOnSubmit(tos);


                            db.SubmitChanges();
                        }

                        //修改完成状态
                        if (!string.IsNullOrEmpty(Request.QueryString["TaskStorageID"]))
                        {
                            TaskStorageIn ts = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == Convert.ToInt32(Request.QueryString["TaskStorageID"]));
                            if (ts != null)
                            {
                                ts.TaskState = "已完成";
                                ts.InspectState = "已审核";




                                ////修改代理任务完成状态

                                //ProxyDirector pd = db.ProxyDirector.SingleOrDefault(u => u.TaskID == Convert.ToInt32(Request.QueryString["TaskStorageID"]) && u.TaskProxy.TaskProxyType.TaskProxyTypeName == "正常入库");
                                //if (pd != null)
                                //{
                                //    TaskProxy tp = db.TaskProxy.SingleOrDefault(u => u.TaskProxyType.TaskProxyTypeName == "正常入库" && u.ProxyPrincipal == pd.TaskProxy.ProxyPrincipal && u.TaskDispose == "未完成");
                                //    tp.TaskDispose = "完成";
                                //}
                            }
                            db.SubmitChanges();
                        }

                    }

                    Response.Redirect("DirectorPass.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "");

                }
                else
                {
                    using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        //任务信息
                        TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == Convert.ToInt32(Request.QueryString["TaskStorageID"]));
                        tsi.TaskState = "已完成";
                        tsi.InspectState = "已审核";
                        CommitIn si = db.CommitIn.SingleOrDefault(u => u.CommitInID == tsi.StorageInID);

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
                        TSI.StorageInType = "委外入库";
                        TSI.TaskTitle = "主任审批任务：" + tsi.TaskTitle.ToString() + "(未通过)";
                        TSI.TaskState = "未完成";
                        TSI.TaskDispose = "未废弃";
                        TSI.TaskType = "材料会计审核";
                        TSI.InspectState = "驳回";

                        TSI.QCBatch = tsi.QCBatch;


                        TSI.Remark = "交货通知单编号为:" + si.CommitInCode + "的物资审批不合格原因：" + this.txtOpinion.Text.Trim();
                        var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                        TSI.CreateTime = SevTime.First();
                        db.TaskStorageIn.InsertOnSubmit(TSI);
                        db.SubmitChanges();
                    }
                    Response.Redirect("../../default-old.aspx",false);

                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
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
