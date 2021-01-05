/*------------------------------------------------------------------------------
 * Unit Name：RiInspectorVerifyRDetailsMessage.cs
 * Description: 回收入库--显示检验员检验修复物资信息的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-09-27
 * ----------------------------------------------------------------------------*/
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
    public class RiInspectorVerifyRDetailsMessage:Page
    {
        private int _taskid, _reportid,_transferid;
        private SPGridView spgvQualifiedMaterial,spgvRejectMaterial;
        private Button btnQualified,btnReject, btnOK;
        private bool bfinished = false;        

        private static string[] ShowTlist = {                                                                                                                                  
                                                          "物资名称:MaterialName",
                                                          "规格型号:SpecificationModel",                                     
                                                          "生产厂家:ManufacturerName",                                                          
                                                          "合格数量:Gentaojian",
                                                          "质检报告号:VerifyCode",
                                                          "质检时间:VerifyTime",                                                        
                                                          "备注:Remark",
                                                          "SrinInspectorVerifyRDetailsID:SrinInspectorVerifyRDetailsID",
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
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID.Equals(_taskid));
                    _reportid = tsi.StorageInID;
                    if (tsi.TaskState.Equals("已完成"))
                        bfinished = true;
                    
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

            btnReject = GetControltByMaster("btnReject") as Button;
            btnReject.Click += new EventHandler(btnReject_Click);

            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);
            btnOK.OnClientClick = "return VerifyBtn()";
            (GetControltByMaster("ltrJS") as Literal).Text = JSDialogAid.GetVerifyBtnJS() ;

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
            this.spgvRejectMaterial.Columns.Insert(5, hlfReport);

        }

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息
                SrinRepairReport srrp = db.SrinRepairReport.SingleOrDefault(u => u.SrinRepairReportID.Equals(_reportid));

                ((Label)GetControltByMaster("lblCode")).Text = srrp.SrinRepairReportCode.Trim();
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(srrp.CreateTime.ToLongDateString(), srrp.CreateTime.ToLongTimeString());
                ((Label)GetControltByMaster("lblProduce")).Text = srrp.EmpInfo.EmpName;
                (GetControltByMaster("lblRemark") as Label).Text = srrp.Remark.Trim();
                _transferid = srrp.SrinInspectorVerifyTransferID;

                //初始化质检合格物资
                spgvQualifiedMaterial.DataSource = from a in db.SrinInspectorVerifyRDetails
                                                   where a.SrinInspectorVerifyDetails.SrinInspectorVerifyTransferID == _transferid
                                                      && a.QualifiedGentaojian != 0
                                                   select new
                                                   {                                                       
                                                       a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                                       a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                                       a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.Manufacturer.ManufacturerName,
                                                       a.QualifiedGentaojian,                                                       
                                                       a.VerifyCode,
                                                       a.VerifyTime,
                                                       a.Remark,
                                                       a.SrinInspectorVerifyRDetailsID
                                                   };
                this.spgvQualifiedMaterial.DataBind();                

                //初始化质检待报废的物资
                this.spgvRejectMaterial.DataSource = from a in db.SrinInspectorVerifyRDetails
                                                     where a.SrinInspectorVerifyDetails.SrinInspectorVerifyTransferID == srrp.SrinInspectorVerifyTransferID
                                                        && a.RejectGentaojian != 0
                                                     select new
                                                     {                                                         
                                                         a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                                         a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                                         a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.Manufacturer.ManufacturerName,
                                                         a.RejectGentaojian,
                                                         a.VerifyCode,
                                                         a.VerifyTime,
                                                         a.Remark,
                                                         a.SrinInspectorVerifyRDetailsID,
                                                         a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialID,
                                                         a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.ManufactureID,
                                                         a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.StorageID,
                                                         a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.PileID
                                                     };
                this.spgvRejectMaterial.DataBind();
                
            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvQualifiedMaterial);
            

            Panel p2 = (Panel)GetControltByMaster("Panel2");
            p2.Controls.Add(this.spgvRejectMaterial);

            if (this.spgvQualifiedMaterial.Rows.Count == 0)
                GetControltByMaster("tblQualified").Visible = false;            
            if (this.spgvRejectMaterial.Rows.Count == 0)
                GetControltByMaster("tblReject").Visible = false;

            this.spgvQualifiedMaterial.Columns[8].Visible = false;

            this.spgvRejectMaterial.Columns[8].Visible = false;
            this.spgvRejectMaterial.Columns[9].Visible = false;
            this.spgvRejectMaterial.Columns[10].Visible = false;
            this.spgvRejectMaterial.Columns[11].Visible = false;
            this.spgvRejectMaterial.Columns[12].Visible = false;

            //分支流程--质检结果已经处理的情况
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                TaskStorageIn tsi;
                tsi = db.TaskStorageIn.SingleOrDefault(u => u.PreviousTaskID.Equals(_taskid) && u.StorageInID.Equals(_transferid) && u.TaskType.Equals("资产组处理修复合格物资") && u.StorageInType.Equals("回收入库"));
                if (tsi != null)//合格物资已经处理的情况
                {
                    btnQualified.Visible = false;
                    string strMaterial = string.Format("<font style=\"font-size:x-small;font-weight:bold;color:red\">{0}</font>", tsi.EmpInfo1.EmpName);
                    (GetControltByMaster("ltrQualified") as Literal).Text = string.Format("<font style=\"font-size:x-small;font-weight:bold;color:green\">合格物资已经发送资产管理员{0}处理. . .</font>", strMaterial);
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

                //分支流程--任务已经完成的情况
                if (bfinished)
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
            if (bfinished)
                Response.Redirect("../../default-old.aspx", false);
            else
                Response.Redirect(string.Format("RiInspectorVerifyRDetails.aspx?TaskID={0}", _taskid), false);
        }

        void btnReject_Click(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    if (db.AwaitScrap.Count(u => u.TransferID.Equals(_transferid) && u.TransferType.Equals("修复检验")) == 0)//没有写入报废库则写入，考虑用户IE回退等问题
                    {
                        AwaitScrap asrp;
                        foreach (GridViewRow gvr in this.spgvRejectMaterial.Rows)
                        {
                            asrp = new AwaitScrap();
                            asrp.State = "待报废";
                            asrp.ScrapReportNum = "未填写";
                            asrp.TransferType = "修复检验";
                            asrp.StorageID = Convert.ToInt32(gvr.Cells[9].Text);
                            asrp.PileID = Convert.ToInt32(gvr.Cells[10].Text);
                            asrp.MaterialID = Convert.ToInt32(gvr.Cells[11].Text);
                            asrp.ManufactureID = Convert.ToInt32(gvr.Cells[12].Text);
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

        void btnQualified_Click(object sender, EventArgs e)
        {
            Response.Redirect(string.Format("CreateSrinTask.aspx?TaskID={0}&WorkID={1}&TaskType=资产组处理修复合格物资", _taskid, _transferid), false);
        }

        void spgvMaterial_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int iDetailsID = Convert.ToInt32(e.Row.Cells[8].Text);
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
                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID.Equals(_taskid));
                    if (tsi.TaskState.Equals("未完成"))
                        tsi.TaskState = "已完成";
                    db.SubmitChanges();
                }
                Response.Redirect("../../default-old.aspx", false);
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

        private int GetPreviousTaskID(int step, int taskid)
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                int tid = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == taskid).PreviousTaskID.Value;
                if (step == 0)
                    return tid;
                return GetPreviousTaskID(--step, tid);
            }
        }

        #endregion
    }
}
