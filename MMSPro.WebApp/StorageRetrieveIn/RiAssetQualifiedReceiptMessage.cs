/*------------------------------------------------------------------------------
 * Unit Name：RiAssetQualifiedReceiptMessage.cs
 * Description: 回收入库--物资管理员创建回收入库单(合格)后显示信息的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-09-17
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
    public class RiAssetQualifiedReceiptMessage:Page
    {
        private int _taskid, _receiptid;
        private SPGridView spgvMaterial;
        private Button btnOK;
        private Literal ltrInfo;
        private Label lblWriteOff;        
        private string strBackUrl;
        private CommandType ctAsset = CommandType.SendTask;//btnOK执行的命令类型，默认为发送任务

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

                    _receiptid = db.SrinQualifiedReceipt.SingleOrDefault(u => u.TaskID == _taskid).SrinQualifiedReceiptID;  
                 
                    //分支流程--冲销完成的情况
                    if (db.SrinWriteOffDetails.Count(u => u.SrinQualifiedReceiptID.Equals(_receiptid)) != 0)
                    {
                        Response.Redirect(string.Format("RiAssetWriteOffDetails.aspx?TaskID={0}",_taskid),false);
                        return;
                    }
                }
                strBackUrl = string.Format("RiAssetModifyQualifiedReceipt.aspx?TaskID={0}", _taskid);

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

            ltrInfo = (Literal)GetControltByMaster("ltrInfo");
            lblWriteOff = GetControltByMaster("lblWriteOff") as Label;

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
                SrinQualifiedReceipt sqrp = db.SrinQualifiedReceipt.SingleOrDefault(u => u.SrinQualifiedReceiptID == _receiptid);

                ((Label)GetControltByMaster("lblProject")).Text = sqrp.SrinInspectorVerifyTransfer.SrinProduceVerifyTransfer.SrinVerifyTransfer.SrinReceipt.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.ProjectInfo.ProjectName;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(sqrp.CreateTime.ToLongDateString(), sqrp.CreateTime.ToLongTimeString());
                ((Label)GetControltByMaster("lblCode")).Text = sqrp.SrinQualifiedReceiptCode;

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
                                                   Description = string.Format("财务编码：{0}--回收合格数量(根/台/套/件)：{1}", fcode, a.SrinInspectorVerifyDetails.QualifiedGentaojian)
                                               };
                this.spgvMaterial.DataBind();

                //初始化表尾信息
                lblWriteOff.Text = sqrp.NeedWriteOff ? "执行冲销" : "不执行冲销";
                ((Label)GetControltByMaster("lblRemark")).Text = sqrp.Remark;                

            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);                                   

            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskType.Equals("资产组长确认合格物资") && u.StorageInID.Equals(_receiptid));
                if (tsi != null)//分支流程--已经发送任务的情况
                {
                    strBackUrl = "../../default-old.aspx";
                    btnOK.Text = lblWriteOff.Text.Equals("执行冲销") ? "冲销" : "入库";                    
                    ctAsset = lblWriteOff.Text.Equals("执行冲销") ? CommandType.WriteOff : CommandType.Storing;

                    //获取显示信息
                    ltrInfo.Visible = true;
                    string strAChief = string.Format("<font style=\"font-size:x-small;font-weight:bold;color:red\">{0}</font>", tsi.EmpInfo1.EmpName);
                    
                    SrinAChiefQReceiptConfirm smqc = db.SrinAChiefQReceiptConfirm.SingleOrDefault(u => u.SrinQualifiedReceiptID == _receiptid);
                    if (smqc != null)//分支流程--资产组长已经确认的情况   
                    {
                        if (lblWriteOff.Text.Equals("执行冲销"))
                            ltrInfo.Text = string.Format("回收入库单(合格)已经通过资产组长{0}确认，请执行冲销. . .<br/><br/>", strAChief);
                        else
                            ltrInfo.Text = string.Format("回收入库单(合格)已经通过资产组长{0}确认，请执行入库. . .<br/><br/>", strAChief);
                        btnOK.Enabled = true;
                    }
                    else
                    {
                        ltrInfo.Text = string.Format("回收入库单(合格)已经发送资产组长{0}确认，请等待. . .<br/><br/>", strAChief);
                        btnOK.Enabled = false;
                    }                                       
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

        void btnOK_Click(object sender, EventArgs e)
        {
            switch (ctAsset)
            {
                case CommandType.SendTask:
                    Response.Redirect(string.Format("CreateSrinTask.aspx?TaskID={0}&WorkID={1}&TaskType=资产组长确认合格物资", _taskid, _receiptid), false);
                    break;
                case CommandType.Storing:                    
                    Response.Redirect(string.Format("RiAssetStorageDetails.aspx?TaskID={0}", _taskid), false);
                    break;
                case CommandType.WriteOff:
                    Response.Redirect(string.Format("RiAssetWriteoffDetails.aspx?TaskID={0}",_taskid), false);
                    break;
            }            
        }

        #endregion

        #region 辅助方法

        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }        

        private enum CommandType {SendTask,Storing,WriteOff };
        #endregion

    }
}
