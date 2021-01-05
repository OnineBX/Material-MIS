/*------------------------------------------------------------------------------
 * Unit Name：RiMaterialStocktakingMessage.cs
 * Description: 回收入库--物资管理员清点回收物资后，显示清点信息的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-08-11
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
    public class RiMaterialStocktakingMessage:Page
    {
        private int _taskid, _stocktakingid,_subdocid;
        private SPGridView spgvMaterial;
        private Button btnOK;
        private Label lblResult,lblOpinion;
        private Literal ltrInfo;
        private int _executor, _receiptid;//_executor:清点有误时，发送回退任务的接收者ID;_receiptid:资产管理员办理回收后生成的回收入库单ID
        private string strBackUrl;
        private bool bSendTask = false;//标识是否已经发送确认任务

        private static string[] ShowTlist = {                                                                                                                        
                                              "物资名称:MaterialName",
                                              "规格型号:SpecificationModel",                                              
                                              "财务编码:FinanceCode",                                                                              
                                              "根/台/套/件:TotleGentaojian",
                                              "米:TotleMetre",
                                              "吨:TotleTon",
                                              "仓库:StorageName",
                                              "剁位:PileName",                                              
                                              "回收单号:RetrieveCode",
                                              "备注:Remark"
                                            };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskid);
                    _executor = tsi.TaskCreaterID;
                    _subdocid = tsi.StorageInID;
                    _stocktakingid = db.SrinStocktaking.SingleOrDefault(u => u.TaskID.Equals(_taskid)).SrinStocktakingID;
                    strBackUrl = string.Format("RiMaterialStocktaking.aspx?TaskID={0}", _taskid);
                                  
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
            //初始化ToolBar
            InitToolBar();

            //初始化spgvMaterial
            this.spgvMaterial = new SPGridView();
            this.spgvMaterial.AutoGenerateColumns = false;
            this.spgvMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");
            

            BoundField bfColumn;

            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvMaterial.Columns.Add(bfColumn);
            }

            btnOK = GetControltByMaster("btnOK") as Button;
            btnOK.Click += new EventHandler(btnOK_Click);

            ltrInfo = GetControltByMaster("ltrInfo") as Literal;

            lblResult = GetControltByMaster("lblResult") as Label;
            lblOpinion = GetControltByMaster("lblOpinion") as Label;

        }        

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息
                SrinSubDoc ssd = db.SrinSubDoc.SingleOrDefault(u => u.SrinSubDocID == _subdocid);

                ((Label)GetControltByMaster("lblCreater")).Text = ssd.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblProject")).Text = ssd.ProjectInfo.ProjectName;

                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(ssd.CreateTime.ToLongDateString(), ssd.CreateTime.ToLongTimeString());

                //初始化清点物资明细
                this.spgvMaterial.DataSource = from a in db.SrinStocktakingDetails
                                               where a.SrinStocktakingID == _stocktakingid
                                               select new
                                               {
                                                   a.SrinSubDetails.MaterialInfo.MaterialName,
                                                   a.SrinSubDetails.MaterialInfo.SpecificationModel,                                                   
                                                   a.SrinSubDetails.MaterialInfo.FinanceCode,
                                                   a.SrinSubDetails.TotleGentaojian,
                                                   a.SrinSubDetails.TotleMetre,
                                                   a.SrinSubDetails.TotleTon,
                                                   a.SrinSubDetails.RetrieveCode,
                                                   a.StorageInfo.StorageName,
                                                   a.PileInfo.PileName,
                                                   a.Remark,
                                                   a.SrinStocktakingDetailsID
                                               };
                this.spgvMaterial.DataBind();                        

                //初始化清点信息
                SrinStocktaking sst = db.SrinStocktaking.SingleOrDefault(u => u.TaskID == _taskid);
                ((Label)GetControltByMaster("lblMaterial")).Text = sst.EmpInfo.EmpName;
                lblResult.Text = sst.StocktakingResult;
                ((Label)GetControltByMaster("lblInventoryDate")).Text = string.Concat(sst.StocktakingDate.ToLongDateString(), sst.StocktakingDate.ToLongTimeString());
                lblOpinion.Text = sst.StocktakingProblem;                 
            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            if (lblResult.Text.Equals("清点有误"))
                btnOK.Text = "通知核实";           
            
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskType.Equals("物资组长确认清点结果") && u.StorageInID.Equals(_stocktakingid));
                if (tsi != null)//分支流程--已经发送任务的情况
                {
                    strBackUrl = "../../default-old.aspx";
                    btnOK.Text = "保养&检验";
                    ltrInfo.Visible = true;
                    string strMChief = string.Format("<font style=\"font-size:x-small;font-weight:bold;color:red\">{0}</font>", tsi.EmpInfo1.EmpName);
                    ltrInfo.Text = string.Format("清点结果已经发送物资组长{0}确认. . .<br/><br/>", strMChief);
                    bSendTask = true;
                    //分支流程--资产组员已经办理回收的情况
                    SrinReceipt srp = db.SrinReceipt.SingleOrDefault(u => u.SrinStocktakingConfirm.SrinStocktakingID.Equals(_stocktakingid));
                    if (srp == null)
                    {
                        btnOK.Enabled = false;
                        ltrInfo.Text = string.Format("清点结果已经发送物资组长{0}确认，请等待资产管理员办理回收入库单. . .<br/><br/>", strMChief);                        
                    }
                    else
                        _receiptid = srp.SrinReceiptID;
                }

                if (db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskid).TaskState.Equals("已完成"))
                {
                    btnOK.Visible = false;
                    ltrInfo.Visible = true;
                    ltrInfo.Text = "该任务已完成，您正在查看清点信息 . . .";
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
            if (lblResult.Text.Equals("清点有误"))//清点有误则发送回退任务               
                Response.Redirect(string.Format("CreateSrinTask.aspx?TaskID={0}&TaskType=处理清点问题&WorkID={1}&Executor={2}&TaskInfo={3}", _taskid, _subdocid, _executor, lblOpinion.Text), false);
            else//清点无误的情况
            {
                if (bSendTask)
                    Response.Redirect(string.Format("ManageRepairAndVerify.aspx?TaskID={0}",_taskid),false);
                else
                    Response.Redirect(string.Format("CreateSrinTask.aspx?TaskID={0}&WorkID={1}&TaskType=物资组长确认清点结果", _taskid, _stocktakingid), false);
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
