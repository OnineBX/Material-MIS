﻿/*------------------------------------------------------------------------------
 * Unit Name：RiMChiefConfirmStocktakingMessage.cs
 * Description: 回收入库--显示物资组长确认清点物资信息的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-10-15
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
    public class RiMChiefConfirmStocktakingMessage:Page
    {
        private int _taskid, _stocktakingid,_confirmid;
        private SPGridView spgvMaterial;
        private Button btnOK;        

        private string strBackUrl;
        private bool bfinished = false;

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
                    _stocktakingid = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID.Equals(_taskid)).StorageInID;

                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskid);
                    if (tsi.TaskState.Equals("已完成"))//分支流程--任务已经完成的情况
                    {
                        bfinished = true;
                        strBackUrl = "../../default-old.aspx";
                    }
                    else
                        strBackUrl = string.Format("RiMChiefConfirmStocktaking.aspx?TaskID={0}", _taskid);
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
                SrinStocktaking sst = db.SrinStocktaking.SingleOrDefault(u => u.SrinStocktakingID == _stocktakingid);

                ((Label)GetControltByMaster("lblDelivery")).Text = sst.SrinSubDoc.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblProject")).Text = sst.SrinSubDoc.ProjectInfo.ProjectName;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(sst.SrinSubDoc.CreateTime.ToLongDateString(), sst.SrinSubDoc.CreateTime.ToLongTimeString());

                //初始化调拨明细
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

                //初始化表尾
                ((Label)GetControltByMaster("lblMaterial")).Text = sst.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblInventoryDate")).Text = string.Concat(sst.StocktakingDate.ToLongDateString(), sst.StocktakingDate.ToLongTimeString());
                ((Label)GetControltByMaster("lblResult")).Text = sst.StocktakingResult.Trim();
                ((Label)GetControltByMaster("lblOpinion")).Text = sst.StocktakingProblem.Trim();

                SrinStocktakingConfirm sstc = db.SrinStocktakingConfirm.SingleOrDefault(u => u.TaskID.Equals(_taskid));
                ((Label)GetControltByMaster("lblMChief")).Text = sstc.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblConfirmTime")).Text = string.Concat(sstc.ConfirmTime.ToLongDateString(), sstc.ConfirmTime.ToLongTimeString());
                _confirmid = sstc.SrinStocktakingConfirmID;
            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            if (bfinished)//分支流程--任务已经完成的情况
            {
                btnOK.Visible = false;
                Panel p2 = (Panel)GetControltByMaster("Panel2");
                p2.Controls.Add(new LiteralControl("<font style=\"color:green;font-size:x-small;font-weight:bold\">该任务已经完成,您正在浏览清点物资的确认信息. . .</font>"));
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
            Response.Redirect(string.Format("CreateSrinTask.aspx?TaskID={0}&WorkID={1}&TaskType=资产组办理回收", _taskid, _confirmid), false);
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
