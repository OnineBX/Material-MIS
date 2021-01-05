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
    public class CommitStorageTestUpload: System.Web.UI.Page
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

        static string[] Titlelist = {
                                     "交货通知单编号:StorageInCode",
                                     "物料名称:MaterialName",
                                     "物料规格:SpecificationModel",
                                     "财务编码:FinanceCode",
                                     "根/套/件(合格):TestGentaojian",
                                     "米(合格):TestMetre",
                                     "吨(合格):TestTon",
                                      "根/套/件(不合格):FailedGentaojian",
                                     "米(不合格):FailedMetre",
                                     "吨(不合格):FailedTon",
                                     "预期使用项目:ProjectName",
                                     "预期到库时间:ExpectedTime",
                                     "所属批次:BatchIndex",
                                     "质检报告报号:InspectionReportNum",
                                     "ID:StorageInTestID"
                                    };



        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                _storageInID = Convert.ToInt32(Request.QueryString["StorageInID"]);
                _taskID = Convert.ToInt32(Request.QueryString["TaskStorageID"]);
                _QCbatch = Request.QueryString["QCBatch"];
                initControl(_flag);
                

               


                this.gv = new SPGridView();
                this.gv.AutoGenerateColumns = false;
                

                BindGridView();

                if (!IsPostBack)
                {
                    ViewState["Temp"] = false;
           
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
            //发送资产组
            if (btnko.Enabled == false)
            {
                Response.Redirect("TaskCenter.aspx?StorageInID=" + _storageInID + "&&state=资产组员&&storageInType=正常入库&&QCBatch=" + _QCbatch + "&&TaskStorageID=" + _taskID + "");
            }
            else
            {
                if (ViewState["Temp"] != null)
                {
                    if (ViewState["Temp"].ToString() == "True")
                    {
                        Response.Redirect("TaskCenter.aspx?StorageInID=" + _storageInID + "&&state=资产组员&&storageInType=正常入库&&QCBatch=" + _QCbatch + "&&TaskStorageID=" + _taskID + "");
                    }
                    else
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请先保存数据后再发送! ')</script>");
                        return;
                    }
                }
            }

            
        
        }

        /// <summary>
        /// 根据任务状态显示控件状态
        /// </summary>
        private void taskState()
        {
            //using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            //{
            //    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskID);
            //    if (tsi.TaskState == "已完成")
            //    {
            //        this.btnko.Enabled = false;
            //        this.btnmodify.Enabled = false;
            //        this._flag = false;
            //    }
            //}
        }

        


        private void initControl(bool flag_)
        {

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
            
        }

        private void dataLoad()
        {
            
        }



        void btnko_Click(object sender, EventArgs e)
        {
           
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            //修改审核状态为初始值
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
               
            }

           
        }


        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("StorageTest.aspx?StorageInID=" + _storageInID + "&&state=质检&&storageInType=委外入库&&QCBatch=" + _QCbatch + "&&TaskStorageID=" + _taskID + "");
        }

        protected void tbarbtnAdd_Click(object sender, EventArgs e)
        {


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

                HyperLinkField hlTask = new HyperLinkField();
                hlTask.HeaderText = "上传质检报告";
                this.gv.Columns.Insert(4, hlTask);


                this.gv.DataSource = from a in db.CommitInTest
                                     
                                     join b in db.StorageInMain on a.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.StorageInID equals b.StorageInID

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
                                        a.FailedGentaojian,
                                        a.FailedMetre,
                                        a.FailedTon,

                                        a.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.ProjectInfo.ProjectName,
                                        a.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.ExpectedTime,
                                        a.CommitInMaterialsLeader.CommitInMaterials.CommitProduce.BatchIndex,
                                        a.InspectionReportNum,
                                        a.Remark
                                     };
                this.gv.RowDataBound += new GridViewRowEventHandler(gv_RowDataBound);
                this.gv.DataBind();
                this.gv.Columns[this.gv.Columns.Count - 1].Visible = false;
               
                Panel p1 = (Panel)GetControltByMaster("Panel1");
                p1.Controls.Add(this.gv);

            }

        }

        void gv_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //e.Row.Cells[14].Text = string.Format("<a href=\"javaScript:onClick=window.showModalDialog(encodeURI('../DocAndIndexManager/UploadFile.aspx?detailsID=" + e.Row.Cells[this.gv.Columns.Count - 1].Text.Trim() + "&&Type=正常入库&&ReportNum=" + e.Row.Cells[13].Text.Trim() + "'),'0','resizable:true;dialogWidth:800px;dialogHeight:600px')\">上传报告</a>", int.Parse(e.Row.Cells[19].Text));
                e.Row.Cells[4].Text = string.Format("<a href=\"javaScript:onClick=window.open('../DocAndIndexManager/UploadFile.aspx?detailsID=" + e.Row.Cells[this.gv.Columns.Count - 1].Text.Trim() + "&&Type=正常入库&&ReportNum=" + e.Row.Cells[this.gv.Columns.Count - 2].Text.Trim() + "','newwindow','height=800, width=750, toolbar =no, menubar=no, scrollbars=yes, resizable=no, location=no, status=no');window.location.reload();\">上传报告</a>", int.Parse(e.Row.Cells[this.gv.Columns.Count - 1].Text));
                
                //Response.AddHeader("Refresh", "0"); 

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
