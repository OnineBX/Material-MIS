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
    public class StorageInDetailedManage:System.Web.UI.Page
    {
        MMSProDBDataContext db;
        SPGridView gv;
        TextBox txtOpinion;
        Button btnOK;
        Label lblInfo;
        Panel plinfo;
        bool flag =true;
        int _storageInID;
        string QCbatch;
        static string[] Titlelist = {
                                     "交货通知单编号:StorageInCode",
                                     "物料名称:MaterialName",
                                     "物料规格:SpecificationModel",
                                     "物料编码:MaterialCode",
                                     "根/套/件:QuantityGentaojian",
                                     "米:QuantityMetre",
                                     "吨:QuantityTon",
                                     "所选数量:Quantity",
                                     "所选单位:CurUnit",
                                     "单价:UnitPrice",
                                     "金额:Amount",
                                     "所属仓库:StorageName",
                                     "所属垛位:PileCode",
                                     "所属批次:BatchIndex",
                                     "财务编号:financeCode",
                                     "到库时间:StorageTime",
                                     "供应商:SupplierName",
                                     "物资管理员:MaterialsManager",
                                     "资产管理员:WarehouseWorker",
                                     "备注:Remark"
                                    };



        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                initControl();
                //任务回退时
                if (!string.IsNullOrEmpty(Request.QueryString["TaskStorageID"]))
                {
                    this.plinfo.Visible = true;
                    this.txtOpinion.Visible = true;
                    this.btnOK.Visible = true;
                    this.lblInfo.Text = "质检信息:";

                    using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == Convert.ToInt32(Request.QueryString["TaskStorageID"]));
                        _storageInID = tsi.StorageInID;
                        this.txtOpinion.Text = tsi.Remark;
                        if (tsi.TaskState == "已完成")
                        {
                            this.btnOK.Enabled = false;
                            flag = false;

                        }
                    }
                }
                //新建任务时
                else
                {
                    this.plinfo.Visible = false;
                    //this.txtOpinion.Visible = false;
                    //this.btnOK.Visible = false;
                    //this.lblInfo.Visible=false;
                    _storageInID = Convert.ToInt32(Request.QueryString["StorageInID"]);
                }



                this.gv = new SPGridView();
                this.gv.AutoGenerateColumns = false;

                BindGridView();

                //添加按钮到toolbar
                ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");
                //新建
                ToolBarButton tbarbtnAdd = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
                tbarbtnAdd.ID = "AddNewRow";
                tbarbtnAdd.Text = "新建";
                tbarbtnAdd.ImageUrl = "/_layouts/images/newitem.gif";
                tbarbtnAdd.Click += new EventHandler(tbarbtnAdd_Click);
                tbarbtnAdd.Visible = flag;
                tbarTop.Buttons.Controls.Add(tbarbtnAdd);
                //修改
                ToolBarButton tbarbtnEdit = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
                tbarbtnEdit.ID = "EditRow";
                tbarbtnEdit.Text = "修改";
                tbarbtnEdit.ImageUrl = "/_layouts/images/edit.gif";
                tbarbtnEdit.Click += new EventHandler(tbarbtnEdit_Click);
                tbarbtnEdit.Visible = flag;
                tbarTop.Buttons.Controls.Add(tbarbtnEdit);
                //删除


                ToolBarButton tbarbtnDelte = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
                tbarbtnDelte.ID = "DeleteRow";
                tbarbtnDelte.Text = "删除";
                tbarbtnDelte.ImageUrl = "/_layouts/images/delete.gif";
                tbarbtnDelte.Click += new EventHandler(tbarbtnDelte_Click);
                tbarbtnDelte.Visible = flag;
                StringBuilder sbScript = new StringBuilder();
                sbScript.Append("var aa= window.confirm('确认删除所选项?');");
                sbScript.Append("if(aa == false){");
                sbScript.Append("return false;}");
                tbarbtnDelte.OnClientClick = sbScript.ToString();
                tbarTop.Buttons.Controls.Add(tbarbtnDelte);

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
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
        }

        private void initControl()
        {
            plinfo = (Panel)GetControltByMaster("plinfo");
            lblInfo = (Label)GetControltByMaster("lblInfo");
            txtOpinion = (TextBox)GetControltByMaster("txtOpinion");
            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Text = "完成审核";
            btnOK.Click += new EventHandler(btnOK_Click);


        }

        void btnOK_Click(object sender, EventArgs e)
        {
            //修改审核状态为初始值
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //旧任务
                TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == Convert.ToInt32(Request.QueryString["TaskStorageID"]));
                tsi.TaskState = "已完成";
                StorageIn si = db.StorageIn.SingleOrDefault(u => u.StorageInID == tsi.StorageInID);

                //新任务
                TaskStorageIn TSI = new TaskStorageIn();

                TSI.TaskCreaterID = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
                TSI.TaskTargetID = tsi.TaskCreaterID;
                if (TSI.TaskTargetID == 0)
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('不存在质检用户，请同步AD账户 ')</script>");
                    return;
                }

                TSI.StorageInID =_storageInID;
                TSI.StorageInType = "正常入库";
                TSI.TaskTitle = "交货通知单编号为:" + si.StorageInCode + "的" + tsi.QCBatch.ToString() + "物资已修改，请重新质检";
                TSI.TaskState = "未完成";
                TSI.TaskDispose = "未废弃";
                TSI.TaskType = "质检";
                TSI.InspectState = "未审核";
                TSI.QCBatch = tsi.QCBatch;
                TSI.Remark = "交货通知单编号为:" + si.StorageInCode + "的" + tsi.QCBatch.ToString() + "物资已修改";
                var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                TSI.CreateTime = SevTime.First();
                db.TaskStorageIn.InsertOnSubmit(TSI);
                db.SubmitChanges();
                Response.Redirect("../../default-old.aspx",false);
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
            Response.Redirect("StorageInManage.aspx");
        }

        protected void tbarbtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (checkInFlow() == false)
                {
                    if (!string.IsNullOrEmpty(Request.QueryString["TaskStorageID"]))
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "Messages", "<script>alert('任务已进入流程不能新建物资.')</script>");
                        return;
                    }
                }



                if (!string.IsNullOrEmpty(Request.QueryString["TaskStorageID"]))
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "Messages", "<script>alert('回退任务不能新增批次.')</script>");
                    return;
                }
                else
                {
                    Response.Redirect("StorageInDetailedCreate.aspx?StorageInID=" + _storageInID + "");
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


        /// <summary>
        /// 检查任务是否进入流程
        /// </summary>
        /// <returns></returns>
        private bool checkInFlow()
        {

            using (MMSProDBDataContext data = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {


                var temp = from a in data.TaskStorageIn
                           where a.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.StorageInType == "正常入库"
                           select a;


                if (temp.ToList().Count > 0)
                {
                    return false;
                }


            }

            return true;
        }

        private bool checkInFlow(string selID)
        {

            using (MMSProDBDataContext data = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                StorageInDetailed sid = data.StorageInDetailed.SingleOrDefault(u => u.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && u.StorageDetailedID == Convert.ToInt32(selID));
                string CurBatch = sid.BatchIndex;

                var temp = from a in data.TaskStorageIn
                           where a.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"]) && a.StorageInType == "正常入库" && a.QCBatch == CurBatch
                           select a;


                if (temp.ToList().Count > 0)
                {
                    return false;
                }


            }

            return true;
        }

        protected void tbarbtnEdit_Click(object sender, EventArgs e)
        {
            List<CheckBox> listString = GetCheckedID();
            if (listString.Count == 1)
            {
                if (checkInFlow(listString[0].ToolTip) == false)
                {
                    if (string.IsNullOrEmpty(Request.QueryString["TaskStorageID"]))
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "Messages", "<script>alert('任务已进入流程不能修改物资.')</script>");
                        return;
                    }
                }


                if (!string.IsNullOrEmpty(Request.QueryString["TaskStorageID"]))
                {
                    Response.Redirect("StorageInDetailedEdit.aspx?StorageDetailedID=" + listString[0].ToolTip + "&&StorageInID=" + _storageInID + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "");
                }
                else
                {
                    Response.Redirect("StorageInDetailedEdit.aspx?StorageDetailedID=" + listString[0].ToolTip + "&&StorageInID=" + _storageInID + "");
                }
                
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择一条且仅能有一条记录进行编辑!')</script>");
            }
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {

            Response.Redirect("StorageInDetailedManage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&QCBatch=" + Request.QueryString["QCBatch"] + "");

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
                        if (checkInFlow(li.ToolTip) == false)
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "Messages", "<script>alert('任务已进入流程不能删除物资.')</script>");
                            return;
                        }

                        SID = db.StorageInDetailed.SingleOrDefault(a => a.StorageDetailedID == int.Parse(li.ToolTip));
                        if (SID != null)
                        {
                            db.StorageInDetailed.DeleteOnSubmit(SID);

                        }
                    }
                    db.SubmitChanges();
                }



                if (!string.IsNullOrEmpty(Request.QueryString["TaskStorageID"]))
                {
                   
                    Response.Redirect("StorageInDetailedManage.aspx?StorageInID=" + _storageInID + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "");
                }
                else
                {
                    Response.Redirect("StorageInDetailedManage.aspx?StorageInID=" + _storageInID + "");
                }



                
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
                
                if (!string.IsNullOrEmpty(Request.QueryString["QCBatch"]))
                {
                    QCbatch = Request.QueryString["QCBatch"];
                }
                BoundField bfColumn;
                //添加选择列
                TemplateField tfieldCheckbox = new TemplateField();
                tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "StorageDetailedID");
                tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
                this.gv.Columns.Add(tfieldCheckbox);
                foreach (var kvp in Titlelist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.gv.Columns.Add(bfColumn);
                }
                this.gv.DataSource = from a in db.StorageInDetailed
                                     join b in db.StorageIn on a.StorageInID equals b.StorageInID
                                     join c in db.MaterialInfo on a.MaterialID equals c.MaterialID
                                     join d in db.PileInfo on a.PileID equals d.PileID
                                     join e in db.SupplierInfo on a.SupplierID equals e.SupplierID

                                     where a.StorageInID == _storageInID && a.BatchIndex == (string.IsNullOrEmpty(QCbatch)? a.BatchIndex : QCbatch)
                                     select new
                                     {
                                        a.StorageDetailedID,
                                        a.SpecificationModel,
                                        a.BatchIndex,
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
