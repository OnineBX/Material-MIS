/*------------------------------------------------------------------------------
 * Unit Name：RiAssetReceiptMessage.cs
 * Description: 回收入库--资产管理员创建回收入库单后显示信息的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-08-20
 * Modified Date:2010-10-15
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
    public class RiAssetReceiptMessage:Page
    {
        private int _taskid,_receiptid;
        private SPGridView spgvMaterial;
        private Button btnOK;
        private Literal ltrInfo;
        private bool isSendTask = false;//是否已将回收单提交给资产组长  
        private string strBackUrl;

        private static string[] ShowTlist = {      
                                                "财务编号:FinanceCode",                                                 
                                                "物资名称:MaterialName",
                                                "规格型号:SpecificationModel",
                                                "根/台/套/件:TotleGentaojian",
                                                "米:TotleMetre",
                                                "吨:TotleTon",
                                                "单价:UnitPrice", 
                                                "金额:Amount",
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
                    _receiptid = db.SrinReceipt.SingleOrDefault(u => u.TaskID == _taskid).SrinReceiptID;                    
                    strBackUrl = string.Format("RiAssetCreateReceipt.aspx?TaskID={0}", _taskid);
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
            tbarbtnBack.ID = "btnBack";
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
           
        }

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息
                SrinReceipt srp = db.SrinReceipt.SingleOrDefault(u => u.SrinReceiptID == _receiptid);

                ((Label)GetControltByMaster("lblProject")).Text = srp.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.ProjectInfo.ProjectName;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(srp.CreateTime.ToLongDateString(), srp.CreateTime.ToLongTimeString());
                ((Label)GetControltByMaster("lblCode")).Text = srp.SrinReceiptCode;

                //初始化调拨明细
                this.spgvMaterial.DataSource = from a in db.SrinDetails
                                               where a.SrinReceiptID == _receiptid
                                               select new
                                               {
                                                   a.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.FinanceCode,                                                  
                                                   a.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                                   a.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,                                                  
                                                   a.SrinStocktakingDetails.SrinSubDetails.TotleGentaojian,
                                                   a.SrinStocktakingDetails.SrinSubDetails.TotleMetre,
                                                   a.SrinStocktakingDetails.SrinSubDetails.TotleTon,
                                                   a.UnitPrice,
                                                   a.Amount,
                                                   a.SrinStocktakingDetails.SrinSubDetails.RetrieveCode,
                                                   a.Remark
                                               };
                this.spgvMaterial.DataBind();

                //初始化表尾信息
                ((Label)GetControltByMaster("lblRemark")).Text = srp.Remark;

            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial); 
           
            //分支流程--回收入库单已经发送资产组长审核的情况
            
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskType.Equals("回收入库单资产组长确认") && u.PreviousTaskID == _taskid && u.StorageInID == _receiptid);
                if (tsi != null)
                {
                    btnOK.Text = "通知保养&检验";
                    isSendTask = true;
                    strBackUrl = "../../default-old.aspx";
                    ltrInfo.Visible = true;
                    string strAChief = string.Format("<font style=\"font-size:x-small;font-weight:bold;color:red\">{0}</font>", tsi.EmpInfo1.EmpName);
                    ltrInfo.Text = string.Format("<br/>回收入库单已经提交资产组长{0}确认. . .<br/>", strAChief); 
                    //分支流程--任务完成的情况
                    if (db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID.Equals(_taskid)).TaskState.Equals("已完成"))
                        btnOK.Visible = false;                    
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
            if (!isSendTask)
                Response.Redirect(string.Format("CreateSrinTask.aspx?TaskID={0}&TaskType=回收入库单资产组长确认&WorkID={1}", _taskid, _receiptid), false);
            else//分支流程--已经发送任务通知物资组执行保养&检验
            {
                string strReceiver;
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    strReceiver = HttpUtility.UrlEncode(string.Concat(db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID.Equals(GetPreviousTaskID(0,_taskid))).EmpInfo.Account,";"));                    
                }
                Response.Redirect(string.Format("../PublicPage/SendMessage.aspx?TaskID={0}&Receivers={1}", _taskid,strReceiver), false);
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
