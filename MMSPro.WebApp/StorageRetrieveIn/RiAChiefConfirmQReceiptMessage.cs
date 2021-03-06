﻿/*------------------------------------------------------------------------------
 * Unit Name：RiAChiefConfirmQReceiptMessage.cs
 * Description: 回收入库--显示资产组长确认回收入库单(合格)信息的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-09-19
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
    public class RiAChiefConfirmQReceiptMessage:Page
    {
        private int _taskid, _receiptid;
        private SPGridView spgvMaterial;
        private Button btnOK;

        private bool bfinished = false;//任务是否已经完成
        private bool bwriteoff;//是否需要执行冲销

        private static string[] ShowTlist = {                                                      
                                                "生产厂家:ManufacturerName",                                      
                                                "根/台/套/件:Gentaojian",
                                                "米:Metre",
                                                "吨:Ton",
                                                "出库单价(原):OutUnitPrice",
                                                "入库单价(新):InUnitPrice",
                                                "金额:Amount",
                                                "回收单号:RetrieveCode",
                                                "检验报告号:VerifyCode",
                                                "备注:Remark"
                                            };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    _receiptid = db.SrinQualifiedReceipt.SingleOrDefault(u => u.TaskID == GetPreviousTaskID(0, _taskid)).SrinQualifiedReceiptID;
                    if (db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID.Equals(_taskid)).TaskState.Equals("已完成"))//分支流程--任务已经完成的情况
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
            
            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);

            //初始化spgvMaterial
            this.spgvMaterial = new SPGridView();
            this.spgvMaterial.AutoGenerateColumns = false;
            this.spgvMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");
            this.spgvMaterial.AllowGrouping = true;
            this.spgvMaterial.AllowGroupCollapse = true;
            this.spgvMaterial.GroupDescriptionField = "Description";
            this.spgvMaterial.GroupField = "MaterialName";
            this.spgvMaterial.GroupFieldDisplayName = "回收检验合格物资"; 

            BoundField bfColumn;

            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvMaterial.Columns.Add(bfColumn);
            }

        }

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息
                SrinAChiefQReceiptConfirm smqc = db.SrinAChiefQReceiptConfirm.SingleOrDefault(u => u.TaskID == _taskid);

                ((Label)GetControltByMaster("lblProject")).Text = smqc.SrinQualifiedReceipt.SrinInspectorVerifyTransfer.SrinProduceVerifyTransfer.SrinVerifyTransfer.SrinReceipt.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.ProjectInfo.ProjectName;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(smqc.SrinQualifiedReceipt.CreateTime.ToLongDateString(), smqc.SrinQualifiedReceipt.CreateTime.ToLongTimeString());
                ((Label)GetControltByMaster("lblCode")).Text = smqc.SrinQualifiedReceipt.SrinQualifiedReceiptCode;

                //初始化调拨明细
                this.spgvMaterial.DataSource = from a in db.SrinAssetQualifiedDetails
                                               where a.SrinQualifiedReceiptID == _receiptid
                                               let mname = a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName
                                               let fcode = a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.FinanceCode
                                               let smodel = a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel
                                               select new
                                               {
                                                   MaterialName = string.Format("{0}--规格型号：{1}", mname, smodel),
                                                   a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.Manufacturer.ManufacturerName,
                                                   a.Gentaojian,
                                                   a.Metre,
                                                   a.Ton,
                                                   a.OutUnitPrice,
                                                   a.InUnitPrice,
                                                   a.Amount,
                                                   a.SrinInspectorVerifyDetails.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.RetrieveCode,
                                                   a.SrinInspectorVerifyDetails.VerifyCode,
                                                   a.Remark,
                                                   Description = string.Format("财务编码：{0}--回收合格数量(根/台/套/件)：{1}",fcode, a.SrinInspectorVerifyDetails.QualifiedGentaojian)
                                               };
                this.spgvMaterial.DataBind();

                //初始化表尾信息
                (GetControltByMaster("lblAsset") as Label).Text = smqc.SrinQualifiedReceipt.EmpInfo.EmpName;
                (GetControltByMaster("lblAChief") as Label).Text = smqc.EmpInfo.EmpName;
                (GetControltByMaster("lblAChiefDate") as Label).Text = string.Concat(smqc.ConfirmTime.ToLongDateString(), smqc.ConfirmTime.ToLongTimeString());
                (GetControltByMaster("lblWriteOff") as Label).Text = smqc.SrinQualifiedReceipt.NeedWriteOff ? "执行冲销" : "不执行冲销";
                (GetControltByMaster("lblRemark") as Label).Text = smqc.Remark;
                bwriteoff = smqc.SrinQualifiedReceipt.NeedWriteOff;

            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            if (bfinished)//分支流程--任务已经完成的情况
            {
                btnOK.Visible = false;
                (GetControltByMaster("ltrInfo") as Literal).Visible = true;
            }
            else
            {
                if (bwriteoff)
                    btnOK.Text = "通知冲销";                
            }
            
        }

        #endregion

        #region 控件事件方法

        void btnRefresh_Click(object sender, EventArgs e)
        {

        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            if (bfinished)//分支流程--任务已经完成的情况
                Response.Redirect("../../default-old.aspx", false);
            else
                Response.Redirect(string.Format("RiAChiefConfirmQReceipt.aspx?TaskID={0}", _taskid), false);
        }

        void btnOK_Click(object sender, EventArgs e)
        {            
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {                   
                    string strReceiver;                        
                    strReceiver = HttpUtility.UrlEncode(string.Concat(db.SrinQualifiedReceipt.SingleOrDefault(u => u.SrinQualifiedReceiptID.Equals(_receiptid)).EmpInfo.Account, ";"));                    
                    Response.Redirect(string.Format("../PublicPage/SendMessage.aspx?TaskID={0}&Receivers={1}", _taskid, strReceiver), false);                   
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
