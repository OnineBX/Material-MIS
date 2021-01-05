/*------------------------------------------------------------------------------
 * Unit Name：CommitOutAuditInfo.cs
 * Description: 委外出库--物资管理员处理主任审批返回信息的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-07-06
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
    public class CommitOutAuditInfo:System.Web.UI.Page
    {
        private int _noticeid;
        private int _taskid;
        private int _auditid;
        private int _audittype;

        private SPGridView spgvMaterial;
        private Button btnCancel;
        private Label lblDirectorResult;

        private static string[] ShowTlist =  { 
                                             "财务编码:FinanceCode",
                                             "物资编码:MaterialCode",
                                             "物资名称:MaterialName",
                                             "规格型号:SpecificationModel",
                                             "生产厂家:SupplierName",
                                             "所属仓库:StorageName",
                                             "所在垛位:PileName",
                                             "到库日期:StorageTime",                                                                                                                                       
                                             "出库数量(根/台/套/件):RealGentaojian",                                                                                          
                                             "出库数量(米):RealMetre",
                                             "出库数量(吨):RealTon",
                                             "单价:UnitPrice",
                                             "实际金额:RealAmount"
                                           };


        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                _taskid = Convert.ToInt32(Request.QueryString["TaskID"]);

                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    StorageOutTask sot = db.StorageOutTask.SingleOrDefault(u => u.StorageOutTaskID == this._taskid);
                    _noticeid = sot.StorageOutNoticeID;
                    _auditid = sot.StorageOutAuditID.Value;
                    _audittype = sot.StorageOutAuditType;
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
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>",LogToDBHelper.LOG_MSG_LOADERROR));   
            }                

        }

        #region 初始化和绑定函数
        private void InitializeCustomControls()
        {
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

            btnCancel = (Button)GetControltByMaster("btnCancel");
            btnCancel.Click += new EventHandler(btnCancel_Click);

            lblDirectorResult = (Label)GetControltByMaster("lblDirectorResult");            

        }        

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息
                StorageCommitOutNotice scon = db.StorageCommitOutNotice.SingleOrDefault(u => u.StorageCommitOutNoticeID == this._noticeid);

                ((Label)GetControltByMaster("lblReceiver")).Text = db.BusinessUnitInfo.SingleOrDefault(u => u.BusinessUnitID == scon.Receiver).BusinessUnitName;
                ((Label)GetControltByMaster("lblNoticeCode")).Text = scon.StorageCommitOutNoticeCode;
                ((Label)GetControltByMaster("lblDate")).Text = scon.CreateTime.ToLongDateString();

                //初始化审核列表
                this.spgvMaterial.DataSource = from a in db.StorageCommitOutRealDetails
                                               where a.StorageCommitOutNoticeID == _noticeid
                                               select new
                                               {
                                                   //a.StorageCommitOutDetails.FinanceCode,
                                                   //a.StorageCommitOutDetails.TableOfStocks.MaterialCode,
                                                   //a.StorageCommitOutDetails.TableOfStocks.MaterialInfo.MaterialName,
                                                   //a.StorageCommitOutDetails.TableOfStocks.MaterialInfo.SpecificationModel,
                                                   //a.StorageCommitOutDetails.TableOfStocks.SupplierInfo.SupplierName,
                                                   //a.StorageCommitOutDetails.TableOfStocks.PileInfo.StorageInfo.StorageName,
                                                   //a.StorageCommitOutDetails.TableOfStocks.PileInfo.PileName,
                                                   //a.StorageCommitOutDetails.TableOfStocks.StorageTime,
                                                   //a.RealGentaojian,
                                                   //a.RealMetre,
                                                   //a.RealTon,
                                                   //a.RealAmount,
                                                   //a.StorageCommitOutDetails.TableOfStocks.UnitPrice
                                               };
                this.spgvMaterial.DataBind();

                //初始化主任审批信息
                StorageCommitOutRemove scor = db.StorageCommitOutRemove.SingleOrDefault(u => u.StorageCommitOutRemoveID == this._auditid);
                ((Label)GetControltByMaster("lblDirectorOpinion")).Text = scor.DirectorOpinion;
                ((Label)GetControltByMaster("lblDirectorResult")).Text = scor.DirectorAuditStatus;
                ((Label)GetControltByMaster("lblDirector")).Text = scor.EmpInfo.EmpName ;


                //初始化物资组长审核信息                
                ((Label)GetControltByMaster("lblMaterialOpinion")).Text = scor.StorageCommitOutMaterialAudit.MaterialAuditOpinion;
                ((Label)GetControltByMaster("lblMaterialResult")).Text = scor.StorageCommitOutMaterialAudit.MaterialAuditStatus;
                ((Label)GetControltByMaster("lblMaterialChief")).Text = scor.StorageCommitOutMaterialAudit.EmpInfo.EmpName;

                //初始化生产组长审核信息                
                ((Label)GetControltByMaster("lblProduceOpinion")).Text = scor.StorageCommitOutProducingAudit.AuditOpinion;
                ((Label)GetControltByMaster("lblProduceResult")).Text = scor.StorageCommitOutProducingAudit.AuditStatus;
                ((Label)GetControltByMaster("lblProduceChief")).Text = scor.StorageCommitOutProducingAudit.EmpInfo.EmpName;

            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            //分支流程--任务已完成的情况
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                Panel p5 = (Panel)GetControltByMaster("Panel5");
                StorageOutTask sot = db.StorageOutTask.SingleOrDefault(u => u.StorageOutTaskID == this._taskid);
                if (sot.TaskState.Equals("已完成"))
                {
                    btnCancel.Text = "返回";
                    if (lblDirectorResult.Text.Equals("通过"))
                        p5.Controls.AddAt(0, new LiteralControl("<BR/><font size = 2pt color = green>信息：该任务已完成，物资已出库...</font><BR/><BR/>"));
                    else
                        p5.Controls.AddAt(0, new LiteralControl("<BR/><font size = 2pt color = green>信息：该任务已完成，因未通过主任审批，物资出库数目清零..</font><BR/><BR/>"));
                    return;
                }
                //主流程--任务未完成的情况
                if (lblDirectorResult.Text.Equals("通过"))
                    p5.Controls.AddAt(0, new LiteralControl("<BR/><font size = 2pt color = red>提示：主任通过审批，请通知相关人员执行物资出库操作...</font><BR/><BR/>"));
                else
                    p5.Controls.AddAt(0, new LiteralControl("<BR/><font size = 2pt color = red>提示：未通过主任审批，将执行物资出库数目清零操作..</font><BR/><BR/>"));
            }
            
        }
        #endregion

        #region 控件事件方法
        void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    //分支流程--任务已完成的情况
                    StorageOutTask sot = db.StorageOutTask.SingleOrDefault(u => u.StorageOutTaskID == this._taskid);
                    if (sot.TaskState.Equals("未完成") && lblDirectorResult.Text.Equals("未通过"))//主流程--任务未完成而且主任审批未通过则将出库数目清零                
                        db.ExecuteCommand("Update StorageCommitOutRealDetails Set RealGentaojian = 0,RealMetre = 0,RealTon = 0,RealAmount = 0 Where StorageCommitOutNoticeID = {0}", this._noticeid);
                    if (sot.TaskState.Equals("未完成"))
                        sot.TaskState = "已完成";
                    db.SubmitChanges();
                }

                Response.Redirect("../../default-old.aspx", false);
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));
            }
        }        
        #endregion

        #region 辅助函数
        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }
       
        #endregion
    }
}
