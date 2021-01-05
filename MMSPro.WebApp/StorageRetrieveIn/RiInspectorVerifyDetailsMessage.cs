/*------------------------------------------------------------------------------
 * Unit Name：RiInspectorVerifyDetailsMessage.cs
 * Description: 回收入库--显示检验人员质检信息的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-09-15
 * ----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using System.Data.Linq.SqlClient;

namespace MMSPro.WebApp
{
    public class RiInspectorVerifyDetailsMessage:Page
    {
        private int _taskid, _transferid;
        private SPGridView spgvQualifiedMaterial,spgvRepairMaterial,spgvRejectMaterial;
        private Button btnQualified,btnRepair,btnReject,btnOK;
        private string strBackUrl;

        private static string[] ShowTlist = {                                                                                                                        
                                                          "物资名称:MaterialName",
                                                          "规格型号:SpecificationModel",                                     
                                                          "生产厂家:ManufacturerName",                                                          
                                                          "合格数量:Gentaojian",
                                                          "质检报告号:VerifyCode",
                                                          "质检时间:RealVerifyTime",
                                                          "回收日期:RetrieveTime",
                                                          "仓库:StorageName",
                                                          "垛位:PileName", 
                                                          "备份:Remark",
                                                          "SrinInspectorVerifyDetailsID:SrinInspectorVerifyDetailsID",
                                                          "StorageID:StorageID",
                                                          "PileID:PileID",
                                                          "MaterialID:MaterialID",
                                                          "ManufactureID:ManufactureID"                                                          
                                                     };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
                strBackUrl = string.Format("RiInspectorVerifyDetails.aspx?TaskID={0}", _taskid);
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {                    
                    _transferid = db.SrinInspectorVerifyTransfer.SingleOrDefault(u => u.TaskID == _taskid).SrinInspectorVerifyTransferID;
                }

                InitializeCustomControls();
                BindDataToCustomControls();
                ShowCustomControls();
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }

        }

        #region 初始化和数据绑定方法

        private void InitToolBar()
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

        private void InitializeCustomControls()
        {
            InitToolBar();

            btnQualified = GetControltByMaster("btnQualified") as Button;
            btnQualified.Click += new EventHandler(btnQualified_Click);

            btnRepair = GetControltByMaster("btnRepair") as Button;
            btnRepair.Click += new EventHandler(btnRepair_Click);

            btnReject = GetControltByMaster("btnReject") as Button;
            btnReject.Click += new EventHandler(btnReject_Click);

            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);
            btnOK.OnClientClick = "return VerifyBtn()";
            (GetControltByMaster("ltrJS") as Literal).Text = JSDialogAid.GetVerifyBtnJS();

            BoundField bfColumn;

            //初始化spgvQualifiedMaterial
            this.spgvQualifiedMaterial = new SPGridView();
            this.spgvQualifiedMaterial.AutoGenerateColumns = false;
            this.spgvQualifiedMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");
            this.spgvQualifiedMaterial.RowDataBound += new GridViewRowEventHandler(spgvMaterial_RowDataBound);

            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                if (bfColumn.HeaderText.Equals("StorageID"))
                    break;
                this.spgvQualifiedMaterial.Columns.Add(bfColumn);
            }           

            (this.spgvQualifiedMaterial.Columns[3] as BoundField).DataField = "QualifiedGentaojian";

            //初始化spgvRepairMaterial
            this.spgvRepairMaterial = new SPGridView();
            this.spgvRepairMaterial.AutoGenerateColumns = false;
            this.spgvRepairMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");
            this.spgvRepairMaterial.RowDataBound += new GridViewRowEventHandler(spgvMaterial_RowDataBound);

            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                if (bfColumn.HeaderText.Equals("StorageID"))
                    break;
                this.spgvRepairMaterial.Columns.Add(bfColumn);
            }
            spgvRepairMaterial.Columns[3].HeaderText = "待修复数量";
            (this.spgvRepairMaterial.Columns[3] as BoundField).DataField = "RepairGentaojian";

            //初始化spgvRejectMaterial
            this.spgvRejectMaterial = new SPGridView();
            this.spgvRejectMaterial.AutoGenerateColumns = false;
            this.spgvRejectMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");
            this.spgvRejectMaterial.RowDataBound += new GridViewRowEventHandler(spgvMaterial_RowDataBound);

            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];                
                this.spgvRejectMaterial.Columns.Add(bfColumn);
            }
            spgvRejectMaterial.Columns[3].HeaderText = "待报废数量";
            (this.spgvRejectMaterial.Columns[3] as BoundField).DataField = "RejectGentaojian";

            //加入上传质检报告号列
            HyperLinkField hlfReport = new HyperLinkField();
            hlfReport.HeaderText = "上传质检报告";

            this.spgvQualifiedMaterial.Columns.Insert(5, hlfReport);
            this.spgvRepairMaterial.Columns.Insert(5, hlfReport);
            this.spgvRejectMaterial.Columns.Insert(5, hlfReport);

        }        

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息
                SrinInspectorVerifyTransfer sivt = db.SrinInspectorVerifyTransfer.SingleOrDefault(u => u.TaskID == _taskid);

                ((Label)GetControltByMaster("lblProject")).Text = sivt.SrinProduceVerifyTransfer.SrinVerifyTransfer.SrinReceipt.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.ProjectInfo.ProjectName;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(sivt.SrinProduceVerifyTransfer.SrinVerifyTransfer.CreateTime.ToLongDateString(), sivt.SrinProduceVerifyTransfer.SrinVerifyTransfer.CreateTime.ToLongTimeString());
                ((Label)GetControltByMaster("lblCode")).Text = sivt.SrinProduceVerifyTransfer.SrinVerifyTransfer.SrinVerifyTransferCode;
                ((Label)GetControltByMaster("lblMaterial")).Text = sivt.SrinProduceVerifyTransfer.SrinVerifyTransfer.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblReadyWork")).Text = sivt.SrinProduceVerifyTransfer.SrinVerifyTransfer.ReadyWorkIsFinished ? "是" : "否";
                ((Label)GetControltByMaster("lblProduce")).Text = sivt.SrinProduceVerifyTransfer.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblVerifyTime")).Text = sivt.SrinProduceVerifyTransfer.VerifyTime.ToLongDateString();

                //初始化质检合格物资
                spgvQualifiedMaterial.DataSource = from a in db.SrinInspectorVerifyDetails
                                          where a.SrinInspectorVerifyTransferID == _transferid
                                             && a.QualifiedGentaojian != 0
                                          select new
                                          {
                                              a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                              a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                              a.SrinMaterialVerifyDetails.Manufacturer.ManufacturerName,
                                              a.QualifiedGentaojian,
                                              a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.StorageInfo.StorageName,
                                              a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.PileInfo.PileName,
                                              a.SrinMaterialVerifyDetails.RetrieveTime,
                                              a.RealVerifyTime,
                                              a.VerifyCode,
                                              a.Remark,
                                              a.SrinInspectorVerifyDetailsID
                                          };
                this.spgvQualifiedMaterial.DataBind();

                //初始化质检需维修的物资
                this.spgvRepairMaterial.DataSource = from a in db.SrinInspectorVerifyDetails
                                                     where a.SrinInspectorVerifyTransferID == _transferid
                                                        && a.RepairGentaojian != 0
                                                     select new 
                                                     {
                                                         a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                                         a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                                         a.SrinMaterialVerifyDetails.Manufacturer.ManufacturerName,
                                                         a.RepairGentaojian,
                                                         a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.StorageInfo.StorageName,
                                                         a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.PileInfo.PileName,
                                                         a.SrinMaterialVerifyDetails.RetrieveTime,
                                                         a.RealVerifyTime,
                                                         a.VerifyCode,
                                                         a.Remark,
                                                         a.SrinInspectorVerifyDetailsID
                                                     };
                this.spgvRepairMaterial.DataBind();

                //初始化质检待报废的物资
                this.spgvRejectMaterial.DataSource = from a in db.SrinInspectorVerifyDetails
                                                     where a.SrinInspectorVerifyTransferID == _transferid
                                                        && a.RejectGentaojian != 0
                                                     select new
                                                     {
                                                         a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                                         a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                                         a.SrinMaterialVerifyDetails.Manufacturer.ManufacturerName,
                                                         a.RejectGentaojian,
                                                         a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.StorageInfo.StorageName,
                                                         a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.PileInfo.PileName,
                                                         a.SrinMaterialVerifyDetails.RetrieveTime,
                                                         a.RealVerifyTime,
                                                         a.VerifyCode,
                                                         a.Remark,
                                                         a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialID,
                                                         a.SrinMaterialVerifyDetails.ManufactureID,
                                                         a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.StorageID,
                                                         a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.PileID,
                                                         a.SrinInspectorVerifyDetailsID
                                                     };
                this.spgvRejectMaterial.DataBind();

                //初始化表尾        
                ((Label)GetControltByMaster("lblInspector")).Text = sivt.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblRemark")).Text = sivt.Remark;

            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvQualifiedMaterial);

            Panel p2 = (Panel)GetControltByMaster("Panel2");
            p2.Controls.Add(this.spgvRepairMaterial);

            Panel p3 = (Panel)GetControltByMaster("Panel3");
            p3.Controls.Add(this.spgvRejectMaterial);

            if (this.spgvQualifiedMaterial.Rows.Count == 0)
                GetControltByMaster("tblQualified").Visible = false;
            if(this.spgvRepairMaterial.Rows.Count == 0)
                GetControltByMaster("tblRepair").Visible = false;
            if (this.spgvRejectMaterial.Rows.Count == 0)
                GetControltByMaster("tblReject").Visible = false;

            this.spgvQualifiedMaterial.Columns[11].Visible = false;
            this.spgvRepairMaterial.Columns[11].Visible = false;
            
            this.spgvRejectMaterial.Columns[11].Visible = false;
            this.spgvRejectMaterial.Columns[12].Visible = false;
            this.spgvRejectMaterial.Columns[13].Visible = false;
            this.spgvRejectMaterial.Columns[14].Visible = false;
            this.spgvRejectMaterial.Columns[15].Visible = false;

            //分支流程--质检结果已经处理的情况
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                TaskStorageIn tsi;
                tsi = db.TaskStorageIn.SingleOrDefault(u => u.PreviousTaskID.Equals(_taskid) && u.StorageInID.Equals(_transferid) && u.TaskType.Equals("资产组处理合格物资") && u.StorageInType.Equals("回收入库"));
                if (tsi != null)//合格物资已经处理的情况
                {
                    btnQualified.Visible = false;
                    string strMaterial = string.Format("<font style=\"font-size:x-small;font-weight:bold;color:red\">{0}</font>", tsi.EmpInfo1.EmpName);
                    (GetControltByMaster("ltrQualified") as Literal).Text = string.Format("<font style=\"font-size:x-small;font-weight:bold;color:green\">合格物资已经发送资产管理员{0}处理. . .</font>",strMaterial);
                }

                tsi = db.TaskStorageIn.SingleOrDefault(u => u.PreviousTaskID.Equals(_taskid) && u.StorageInID.Equals(_transferid) && u.TaskType.Equals("生产组申请维修") && u.StorageInType.Equals("回收入库"));
                if (tsi != null)//待维修物资已经处理的情况
                {
                    btnRepair.Visible = false;
                    string strProduce = string.Format("<font style=\"font-size:x-small;font-weight:bold;color:red\">{0}</font>", tsi.EmpInfo1.EmpName);
                    (GetControltByMaster("ltrRepair") as Literal).Text = string.Format("<font style=\"font-size:x-small;font-weight:bold;color:green\">待修复物资已经发送物资管理员{0}处理. . .</font>", strProduce);
                }

                MessageInfo mi = db.MessageInfo.SingleOrDefault(u => u.TaskID.Equals(_taskid));
                if (mi != null)//已经发送报废通知的情况
                {
                    btnReject.Visible = false;
                    var receivers = from r in db.MessageReceiver
                                    where r.MessageInfoID.Equals(mi.MessageInfoID)
                                    select r;
                    string strMaterial = string.Empty;
                    foreach (MessageReceiver receiver in receivers)
                    {
                        strMaterial = string.Format("{0}、{1}", strMaterial, receiver.EmpInfo.EmpName);
                    }
                    strMaterial = strMaterial.Substring(1, strMaterial.Length - 1);
                    strMaterial = string.Format("<font style=\"font-size:x-small;font-weight:bold;color:red\">{0}</font>", strMaterial);
                    (GetControltByMaster("ltrReject") as Literal).Text = string.Format("<font style=\"font-size:x-small;font-weight:bold;color:green\">已经通知{0}处理待报废物资. . .</font>", strMaterial);
                }

                if (btnQualified.Visible == false || btnRepair.Visible == false || btnReject.Visible == false)//已经存在处理质检后物资的情况
                    strBackUrl = "../../default-old.aspx";

                //分支流程--任务已经完成的情况
                if (db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID.Equals(_taskid)).TaskState.Equals("已完成"))
                {
                    btnOK.Visible = false;
                    (GetControltByMaster("lblInfo") as Label).Text = "该任务已完成，您正在查看质检处理结果. . .";
                }
            }

        }

        #endregion

        #region 控件事件方法

        void btnRefresh_Click(object sender, EventArgs e)
        {

        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect(strBackUrl, false);
        }

        void btnReject_Click(object sender, EventArgs e)
        {                       
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    if (db.AwaitScrap.Count(u => u.TransferID.Equals(_transferid) && u.TransferType.Equals("正常检验")) == 0)//没有写入报废库则写入，考虑用户IE回退等问题
                    {
                        AwaitScrap asrp;
                        foreach (GridViewRow gvr in this.spgvRejectMaterial.Rows)
                        {
                            asrp = new AwaitScrap();
                            asrp.State = "待报废";
                            asrp.ScrapReportNum = "未填写";
                            asrp.TransferType = "正常检验";
                            asrp.StorageID = Convert.ToInt32(gvr.Cells[12].Text);
                            asrp.PileID = Convert.ToInt32(gvr.Cells[13].Text);
                            asrp.MaterialID = Convert.ToInt32(gvr.Cells[14].Text);
                            asrp.ManufactureID = Convert.ToInt32(gvr.Cells[15].Text);
                            asrp.Gentaojian = Convert.ToDecimal(gvr.Cells[3].Text);
                            asrp.TransferID = _transferid;
                            asrp.ProjectID = db.SrinInspectorVerifyTransfer.SingleOrDefault(u => u.SrinInspectorVerifyTransferID.Equals(_transferid)).SrinProduceVerifyTransfer.SrinVerifyTransfer.SrinReceipt.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.Project;
                            asrp.Creator = (db.EmpInfo.SingleOrDefault(u => u.EmpName == SPContext.Current.Web.CurrentUser.Name)).EmpID;
                            asrp.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                            db.AwaitScrap.InsertOnSubmit(asrp);
                        }
                        db.SubmitChanges();
                    }
                }

                Response.Redirect(string.Format("../PublicPage/SendMessage.aspx?TaskID={0}", _taskid), false);
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
            }
        }

        void btnRepair_Click(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("CreateSrinTask.aspx?TaskID={0}&WorkID={1}&TaskType=生产组申请维修", _taskid, _transferid), false);
        }

        void btnQualified_Click(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("CreateSrinTask.aspx?TaskID={0}&WorkID={1}&TaskType=资产组处理合格物资", _taskid, _transferid), false);
        }

        void spgvMaterial_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int iDetailsID = Convert.ToInt32(e.Row.Cells[11].Text);
                string strVCode = e.Row.Cells[4].Text;
                e.Row.Cells[5].Text = string.Format("<a href=\"javaScript:onClick=window.showModalDialog(encodeURI('../DocAndIndexManager/UploadFile.aspx?DetailsID={0}&Type=回收入库质检&ReportNum={1}'),'0','resizable:true;dialogWidth:800px;dialogHeight:600px')\">上传报告</a>", iDetailsID, strVCode);
            }
        }    

        void btnOK_Click(object sender, EventArgs e)
        {
            try
            {                
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                     //完成当前任务
                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskid);
                    
                    if (tsi.TaskState.Equals("未完成"))//分支流程--处理IE回退
                        tsi.TaskState = "已完成";
                    db.SubmitChanges();
                    Response.Redirect("../../default-old.aspx", false);                                              
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

        #endregion

        #region 辅助方法

        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }
       
        #endregion
    }
}
