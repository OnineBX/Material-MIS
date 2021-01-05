/*------------------------------------------------------------------------------
 * Unit Name：RiMaterialRepairAudit.cs
 * Description: 回收入库--显示物资组长维修保养计划审核信息的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-08-18
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
using System.Data.Linq.SqlClient;

namespace MMSPro.WebApp
{
    public class RiMaterialRepairAuditMessage:Page
    {
        private int _taskid, _formid,_executor;//当前任务ID和维修保养计划表ID
        private SPGridView spgvMaterial;
        private Button btnOK;
        private Label lblCode,lblOpinion, lblResult;
        private bool bfinished = false;

        private static string[] ShowTlist = {                                                                                                                        
                                                  "物资名称:MaterialName",
                                                  "规格型号:SpecificationModel",                                     
                                                  "生产厂家:ManufacturerName",
                                                  "进库时间:ArrivalTime",
                                                  "维修保养数量:Gentaojian",
                                                  "维修保养原因:RepairReason",
                                                  "计划完成时间:PlanTime",
                                                  "实际维修保养数量:RealGentaojian",
                                                  "实际完成时间:RealTime",                                                    
                                                  "备注:Remark"
                                               };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                _taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskid);
                    _formid = tsi.StorageInID;
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

        private void InitBar()
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
            InitBar();

            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);

            lblCode = (Label)GetControltByMaster("lblCode");
            lblOpinion = (Label)GetControltByMaster("lblOpinion");
            lblResult = (Label)GetControltByMaster("lblResult");

            spgvMaterial = new SPGridView();
            spgvMaterial.AutoGenerateColumns = false;
            spgvMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");

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
                //初始化表头
                SrinRepairPlan srp = db.SrinRepairPlan.SingleOrDefault(u => u.SrinRepairPlanID == _formid);
                ((Label)GetControltByMaster("lblMaterial")).Text = srp.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(srp.CreateTime.ToLongDateString(), srp.CreateTime.ToLongTimeString());
                lblCode.Text = srp.SrinRepairPlanCode;
                _executor = srp.Creator;

                spgvMaterial.DataSource = from a in db.SrinMaterialRepairDetails
                                          where a.SrinRepairPlanID == _formid
                                          select new
                                          {
                                              a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                              a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                              a.Manufacturer.ManufacturerName,
                                              a.ArrivalTime,
                                              a.Gentaojian,
                                              a.RepairReason,
                                              a.PlanTime,
                                              a.RealGentaojian,
                                              a.RealTime,
                                              a.Remark
                                          };
                spgvMaterial.DataBind();

                //初始化审核信息
                SrinMaterialRepairAudit smra = db.SrinMaterialRepairAudit.SingleOrDefault(u => u.TaskID == _taskid);
                ((Label)GetControltByMaster("lblMaterialChief")).Text = smra.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblAuditDate")).Text = string.Concat(smra.AuditTime.ToLongDateString(),smra.AuditTime.ToLongTimeString());
                lblResult.Text = smra.AuditResult;
                lblOpinion.Text = smra.AuditOpinion;
                   
            }

        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            if (bfinished)
            {
                Panel p3 = (Panel)GetControltByMaster("Panel3");
                p3.Controls.Add(new LiteralControl("<br/><Font Style=\"font-size:x-small;font-weight:bold;color:green\">该任务已完成，您正在查看维修保养计划审核信息. . .</Font>"));
                btnOK.Visible = false;
            }
            else
            {
                if (lblResult.Text.Equals("通过"))
                    btnOK.Text = "完成";
            }
                
        }

        #endregion

        #region 控件事件方法

        void btnRefresh_Click(object sender, EventArgs e)
        {
            
        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            if(bfinished)
                Response.Redirect("../../default-old.aspx", false);
            else
                Response.Redirect(string.Format("RiMaterialRepairAudit.aspx?TaskID={0}",_taskid), false);
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (lblResult.Text.Equals("未通过"))
                {
                    Response.Redirect(string.Format("CreateSrinTask.aspx?TaskID={0}&WorkID={1}&TaskType=处理物资组长审核问题&Executor={2}", _taskid, _formid, _executor), false);
                }
                else
                {
                    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID.Equals(_taskid));
                        tsi.TaskState = "已完成";
                        db.SubmitChanges();
                    }
                    Response.Redirect("../../default-old.aspx", false);
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_UPDATEERROR));
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
