/*------------------------------------------------------------------------------
 * Unit Name：RiAChiefReceiptConfirm.cs
 * Description: 回收入库--资产组长确认回收入库单物资信息的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-09-01
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
    public class RiAChiefReceiptConfirm:Page
    {
        private int _taskid, _receiptid;
        private SPGridView spgvMaterial;
        private Button btnOK;

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
                    _receiptid = db.SrinReceipt.SingleOrDefault(u => u.TaskID == this.GetPreviousTaskID(0,_taskid)).SrinReceiptID;

                    //分支流程--已经确认过的情况
                    if (db.SrinAssetReceiptConfirm.Count(u => u.TaskID == _taskid) != 0)
                    {
                        Response.Redirect(string.Format("RiAChiefReceiptConfirmMessage.aspx?TaskID={0}", _taskid), false); 
                        return;
                    }
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

                //初始化表尾
                (GetControltByMaster("lblAsset") as Label).Text = srp.EmpInfo.EmpName;                

            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);            

        }

        #endregion

        #region 控件事件方法

        void btnRefresh_Click(object sender, EventArgs e)
        {

        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("../../default-old.aspx", false);
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                //将确认结果保存到数据库
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    SrinAssetReceiptConfirm smrc = new SrinAssetReceiptConfirm();
                    smrc.SrinReceiptID = _receiptid;
                    smrc.TaskID = _taskid;
                    smrc.MaterialChief = (db.EmpInfo.SingleOrDefault(u => u.EmpName == SPContext.Current.Web.CurrentUser.Name)).EmpID;
                    smrc.ConfirmTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                    db.SrinAssetReceiptConfirm.InsertOnSubmit(smrc);
                    db.SubmitChanges();
                }
                Response.Redirect(string.Format("RiAChiefReceiptConfirmMessage.aspx?TaskID={0}", _taskid), false);
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

        /// <summary>
        /// 得到前序第Step步任务ID
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
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
