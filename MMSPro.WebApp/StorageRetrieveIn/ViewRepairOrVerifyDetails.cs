/*------------------------------------------------------------------------------
 * Unit Name：ViewRepairOrVerifyDetails.cs
 * Description: 回收入库--查看维修保养计划表或回收检验传递表物资的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-10-13
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
    public class ViewRepairOrVerifyDetails:Page
    {
        private int _formid;
        private string _type;

        private SPGridView spgvMaterial;
        private static string[] ShowRepairTlist = {                                                                                                                        
                                                      "物资名称:MaterialName",
                                                      "规格型号:SpecificationModel",                                     
                                                      "生产厂家:ManufacturerName",
                                                      "进库时间:ArrivalTime",
                                                      "回收数量:RetrieveInQuantity",
                                                      "维修保养数量(根/台/套/件):Gentaojian",
                                                      "维修/保养原因:RepairReason",
                                                      "计划完成时间:PlanTime",
                                                      "实际维修保养数量:RealGentaojian",
                                                      "实际完成时间:RealTime",                                                      
                                                      "备注:Remark"                                                      
                                                   };

        private static string[] ShowVerifyTlist = {                                                                                                                        
                                                      "物资名称:MaterialName",
                                                      "规格型号:SpecificationModel",                                     
                                                      "生产厂家:ManufacturerName",
                                                      "回收数量:TotleGentaojian",
                                                      "回收日期:RetrieveTime",
                                                      "仓库:StorageName",
                                                      "垛位:PileName",
                                                      "备注:Remark"                                                     
                                                   };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                _formid = Convert.ToInt32(Request.QueryString["FormID"]);
                _type = Request.QueryString["Type"];

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

        private void InitializeCustomControls()
        {
            InitToolBar();

            //初始化物资列表
            this.spgvMaterial = new SPGridView();
            this.spgvMaterial.AutoGenerateColumns = false;
            this.spgvMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");           

            BoundField bfColumn;

            switch (_type)
            {
                case "维修保养":
                    foreach (var kvp in ShowRepairTlist)
                    {
                        bfColumn = new BoundField();
                        bfColumn.HeaderText = kvp.Split(':')[0];
                        bfColumn.DataField = kvp.Split(':')[1];
                        this.spgvMaterial.Columns.Add(bfColumn);
                    }                                                                            
                    break;
                case "回收检验":
                    foreach (var kvp in ShowVerifyTlist)
                    {
                        bfColumn = new BoundField();
                        bfColumn.HeaderText = kvp.Split(':')[0];
                        bfColumn.DataField = kvp.Split(':')[1];
                        this.spgvMaterial.Columns.Add(bfColumn);
                    }
                    break;
            }

        }

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                switch (_type)
                {
                    case "维修保养":
                        spgvMaterial.DataSource = from a in db.SrinMaterialRepairDetails
                                                  where a.SrinRepairPlanID == _formid
                                                  select new
                                                  {
                                                      a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                                      a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                                      a.Manufacturer.ManufacturerName,
                                                      a.ArrivalTime,
                                                      RetrieveInQuantity = a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.TotleGentaojian,
                                                      a.Gentaojian,
                                                      a.RepairReason,
                                                      a.PlanTime,
                                                      a.RealGentaojian,
                                                      a.RealTime,
                                                      a.Remark,
                                                      a.SrinMaterialRepairDetailsID
                                                  };
                        break;
                    case "回收检验":
                        spgvMaterial.DataSource = from a in db.SrinMaterialVerifyDetails
                                                  where a.SrinVerifyTransferID == _formid
                                                  select new
                                                  {
                                                      a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                                      a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                                      a.Manufacturer.ManufacturerName,
                                                      a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.TotleGentaojian,
                                                      a.SrinDetails.SrinStocktakingDetails.StorageInfo.StorageName,
                                                      a.SrinDetails.SrinStocktakingDetails.PileInfo.PileName,
                                                      a.RetrieveTime,
                                                      a.Remark,
                                                      a.SrinMaterialVerifyDetailsID
                                                  };
                        break;
                }

                spgvMaterial.DataBind();
            }
        }


        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            switch (_type)
            {
                case"维修保养":
                    (GetControltByMaster("lblInfo") as Label).Text = "该维修保养计划表已经提交物资组长审核，您正在查看维修保养物资明细信息. . .";
                    break;
                case"回收检验":
                    (GetControltByMaster("lblInfo") as Label).Text = "该回收检验传递表已经提交生产组办理，您正在查看回收检验物资明细信息. . .";
                    break;
            }
        }

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

        #endregion

        #region 控件事件方法

        void btnRefresh_Click(object sender, EventArgs e)
        {

        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {           
            Response.Redirect(string.Format("ManageRepairAndVerify.aspx?TaskID={0}", GetCurrentTaskID()), false);
        }       

        #endregion

        #region 辅助方法

        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }

        private int GetCurrentTaskID()
        {
            int taskid = 0;
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    switch (_type)
                    {
                        case "维修保养":
                            taskid = db.SrinRepairPlan.SingleOrDefault(u => u.SrinRepairPlanID == _formid).TaskID;                            
                            break;
                        case "回收检验":
                            taskid = db.SrinVerifyTransfer.SingleOrDefault(u => u.SrinVerifyTransferID == _formid).TaskID;                            
                            break;
                    }                    

                }
                return taskid;

            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_UPDATEERROR));
                return taskid;
            }

        }

        #endregion
    }
}
