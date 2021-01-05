using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.WebControls;
using System.Configuration;

namespace MMSPro.WebApp
{
    public class MoreTaskOfStorageIn : System.Web.UI.Page
    {
        DropDownList ddlTaskStatus;
        SPGridView gviewMoreTaskForIn;
        Panel p1;
        protected static string[] colNames = { "TaskTitle:待办事项", "CreateTime:创建时间", "EmpName:来自", "TaskState:状态", "StorageInType:入库类型" };

        protected void Page_Load(object sender, EventArgs e)
        {

            this.ddlTaskStatus = (DropDownList)GetControltByMaster("ddlTaskStatus");
            this.ddlTaskStatus.SelectedIndexChanged += new EventHandler(ddlTaskStatus_SelectedIndexChanged);
            if (!IsPostBack)
            {
                this.ddlTaskStatus.Items.AddRange(new ListItem[] { new ListItem("全部任务"), new ListItem("未完成"), new ListItem("已完成") });

                BindGridView(this.ddlTaskStatus.SelectedItem.Text);
                this.p1 = (Panel)GetControltByMaster("Panel1");
                this.p1.Controls.Add(this.gviewMoreTaskForIn);
            }

        }

        void ddlTaskStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGridView(this.ddlTaskStatus.SelectedItem.Text);
            this.p1 = (Panel)GetControltByMaster("Panel1");
            this.p1.Controls.Add(this.gviewMoreTaskForIn);
        }
        void BindGridView(string taskFlag)
        {
            this.gviewMoreTaskForIn = new SPGridView();
            this.gviewMoreTaskForIn.AutoGenerateColumns = false;
            this.gviewMoreTaskForIn.RowDataBound += new GridViewRowEventHandler(gviewMoreTaskForIn_RowDataBound);

            //HyperLinkField hlf = new HyperLinkField();
            ////hlf.HeaderStyle.ForeColor = Color.Gray;
            //hlf.HeaderStyle.Font.Bold = true;
            //hlf.HeaderText = colNames[0].Split(':')[1];
            //hlf.DataTextField = colNames[0].Split(':')[0];
            //hlf.DataNavigateUrlFields = new string[] { "TaskStorageID", "StorageInID"};
            //hlf.DataNavigateUrlFormatString = SPContext.Current.Web.Url + "/WorkPages/StorageIn/QualityControlManage.aspx?TaskStorageID={0}&StorageInID={1}";
            TemplateField tfieldHyperLink = new TemplateField();
            tfieldHyperLink.ItemTemplate = new HyperLinkTemplate("待办事项", DataControlRowType.DataRow, "TaskType", "TaskTitle");
            tfieldHyperLink.HeaderTemplate = new HyperLinkTemplate("待办事项", DataControlRowType.Header);
            this.gviewMoreTaskForIn.Columns.Add(tfieldHyperLink);

            for (int i = 1; i < colNames.Length; i++)
            {
                BoundField bf = new BoundField();
                //bf.HeaderStyle.ForeColor = Color.Gray;
                bf.HeaderStyle.Font.Bold = true;
                bf.HeaderText = colNames[i].Split(':')[1];
                bf.DataField = colNames[i].Split(':')[0];
                if (i == 1)
                {
                    bf.DataFormatString = "{0:yyyy-MM-dd HH:mm dddd}";
                }
                //if (i == 3)
                //{
                //    TableItemStyle tis = new TableItemStyle();
                //    tis.ForeColor = System.Drawing.Color.Red;
                //    bf.DataFormatString = "";
                //}
                this.gviewMoreTaskForIn.Columns.Add(bf);
            }

            this.gviewMoreTaskForIn.AutoGenerateColumns = false;
            this.gviewMoreTaskForIn.GridLines = GridLines.None;
            this.gviewMoreTaskForIn.CssClass = "ms-vh2 padded headingfont";
            //string curLoginName = SPContext.Current.Web.CurrentUser.LoginName;
            using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                if (taskFlag == "全部任务")
                {
                    this.gviewMoreTaskForIn.DataSource = from t in dc.TaskStorageIn
                                                          join e1 in dc.EmpInfo on t.TaskTargetID equals e1.EmpID
                                                          join e2 in dc.EmpInfo on t.TaskCreaterID equals e2.EmpID
                                                          where e1.Account.ToLower() == SPContext.Current.Web.CurrentUser.LoginName.ToLower()
                                                          orderby t.TaskStorageID descending
                                                          select new
                                                          {
                                                              t.TaskStorageID,
                                                              t.TaskTitle,
                                                              t.CreateTime,
                                                              e2.EmpName,
                                                              t.TaskState,
                                                              t.StorageInID,
                                                              t.TaskType,
                                                              t.QCBatch,
                                                              t.StorageInType
                                                          };
                    this.gviewMoreTaskForIn.DataBind();
                }
                else
                {
                    this.gviewMoreTaskForIn.DataSource = from t in dc.TaskStorageIn
                                                          join e1 in dc.EmpInfo on t.TaskTargetID equals e1.EmpID
                                                          join e2 in dc.EmpInfo on t.TaskCreaterID equals e2.EmpID
                                                          where e1.Account.ToLower() == SPContext.Current.Web.CurrentUser.LoginName.ToLower() && t.TaskState == taskFlag
                                                          orderby t.TaskStorageID descending
                                                          select new
                                                          {
                                                              t.TaskStorageID,
                                                              t.TaskTitle,
                                                              t.CreateTime,
                                                              e2.EmpName,
                                                              t.TaskState,
                                                              t.StorageInID,
                                                              t.TaskType,
                                                              t.QCBatch,
                                                              t.StorageInType
                                                          };
                    this.gviewMoreTaskForIn.DataBind();
                }
            }
            int colLastIndex = this.gviewMoreTaskForIn.Columns.Count - 2;
            int rowLastIndex = this.gviewMoreTaskForIn.Rows.Count - 1;
            for (int i = 0; i <= rowLastIndex; i++)
            {
                if (this.gviewMoreTaskForIn.Rows[i].Cells[colLastIndex].Text == "已完成")
                {
                    this.gviewMoreTaskForIn.Rows[i].Cells[colLastIndex].ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    this.gviewMoreTaskForIn.Rows[i].Cells[colLastIndex].ForeColor = System.Drawing.Color.Red;
                }
            }

        }

        void gviewMoreTaskForIn_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //throw new NotImplementedException();
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                HyperLink hl = e.Row.Cells[0].FindControl("hlItem") as HyperLink;
                switch (hl.ToolTip)
                {
                    case "物资组员":
                        if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "正常入库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/Storage/StorageMaterials.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString() + "&QCBatch=" + DataBinder.Eval(e.Row.DataItem, "QCBatch").ToString();
                        else if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "委外入库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageCommitIn/QualityControlCommitIn.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString();
                        break;
                    case "物资组长":
                        if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "正常入库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/Storage/StorageMaterialsLeader.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString() + "&QCBatch=" + DataBinder.Eval(e.Row.DataItem, "QCBatch").ToString();
                        else if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "委外入库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageCommitIn/QualityControlCommitIn.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString();
                        break;
                    case "质检":
                        if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "正常入库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/Storage/StorageTest.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString() + "&QCBatch=" + DataBinder.Eval(e.Row.DataItem, "QCBatch").ToString();
                        else if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "委外入库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageCommitIn/QualityControlCommitIn.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString();
                        break;

                    case "资产组员":
                        if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "正常入库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/Storage/StorageAssets.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString() + "&QCBatch=" + DataBinder.Eval(e.Row.DataItem, "QCBatch").ToString();
                        else if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "委外入库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageCommitIn/QualityControlCommitIn.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString();
                        break;

                    case "资产组长":
                        if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "正常入库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/Storage/StorageAssetsLeader.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString() + "&QCBatch=" + DataBinder.Eval(e.Row.DataItem, "QCBatch").ToString();
                        else if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "委外入库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageCommitIn/QualityControlCommitIn.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString();
                        break;


                    //case "材料会计审核":
                    //    if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "正常入库")
                    //        hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/Storage/MaterialAccountantAudit.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString() + "&QCBatch=" + DataBinder.Eval(e.Row.DataItem, "QCBatch").ToString();
                    //    else if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "委外入库")
                    //        hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageCommitIn/AccountantCommitIn.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString();
                    //    break;
                    case "主任审核":
                        if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "正常入库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/Storage/StorageInDirector.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString() + "&QCBatch=" + DataBinder.Eval(e.Row.DataItem, "QCBatch").ToString();
                        else if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "委外入库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageCommitIn/DirectorManage.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString();
                        break;
                    case "质检前清单":
                        if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "正常入库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/Storage/StorageInDetailedManage.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString() + "&QCBatch=" + DataBinder.Eval(e.Row.DataItem, "QCBatch").ToString();
                        else if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "委外入库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageCommitIn/CommitInDetailedManage.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString();
                        break;
                    case "资产组登记":
                        hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/ViewRetrieveInDetails.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                        break;
                    case "物资组清点":
                        hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiMaterialStocktaking.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                        break;
                    case "维修保养物资组长审核":
                        hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiMaterialRepairAudit.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                        break;
                    //case "维修保养分管领导审核":
                    //    hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiAssistantRepairAudit.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                    //    break;
                    //case "维修保养主管领导审批":
                    //    hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiPrincipalRepairAudit.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                    //    break;
                    //case "维修保养物资组长审核信息":
                    //    hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiMaterialRepairAuditInfo.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                    //    break;
                    case "生产组安排质检":
                        hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiProduceArrangeVerify.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                        break;
                    //case "生产组安排维修保养":
                    //    hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiProduceRepairPlanDetails.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                    //    break;
                    case "检验员质检":
                        hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiInspectorVerifyDetails.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                        break;
                    //case "生产组维修保养":
                    //    hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiProduceRepairRealDetails.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                    //    break;
                    case "处理清点问题":
                        hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiMaterialStocktakingInfo.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                        break;
                    //case "资产组审核":
                    //    hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/ManageProduceAudit.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                    //    break;
                    case "处理物资组长审核问题":
                        hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiMaterialModifyRepairDetails.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                        break;
                    case "回收入库单资产组长确认":
                        hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiAChiefReceiptConfirm.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                        break;
                    //case "回收入库单材料会计确认":
                    //    hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiAccountReceiptConfirm.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                    //    break;
                    //case "资产组冲销":
                    //    hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiAssetWriteoffDetails.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                    //    break;
                    case "生产组申请维修":
                        hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiProduceApplyReport.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                        break;
                    case "资产组处理合格物资":
                        hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiAssetQualifiedReceipt.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                        break;
                    case "资产组处理修复合格物资":
                        hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiAssetQualifiedReceipt.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                        break;
                    case "资产组长确认合格物资":
                        hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiAChiefConfirmQReceipt.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                        break;
                    case "物资组安排维修":
                        hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiMaterialArrangeReport.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                        break;
                    case "检验员检验修复物资":
                        hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiInspectorVerifyRDetails.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                        break;
                    case "资产组办理回收":
                        hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiAssetCreateReceipt.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                        break;
                    case "物资组长确认清点结果":
                        hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/RiMChiefConfirmStocktaking.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
                        break;
                    default:
                        break;
                }
            }
        }


        /// <summary>
        /// 获取控件
        /// </summary>
        /// <param name="controlName">控件的ID名称</param>
        /// <returns>返回Control，需要强制类型转换为对应控件</returns>
        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }
    }
}
