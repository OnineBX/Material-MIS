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
    public class MoreTransferAndScrap : System.Web.UI.Page
    {
        DropDownList ddlTaskStatus;
        SPGridView gviewTransferAndScrap;
        Panel p1;
        protected static string[] colNames = { "TaskTitle:待办事项", "CreateTime:创建时间", "EmpName:来自", "TaskState:状态", "TaskInType:任务类型" };

        protected void Page_Load(object sender, EventArgs e)
        {
            this.ddlTaskStatus = (DropDownList)GetControltByMaster("ddlTaskStatus");
            this.ddlTaskStatus.SelectedIndexChanged += new EventHandler(ddlTaskStatus_SelectedIndexChanged);
            if (!IsPostBack)
            {
                this.ddlTaskStatus.Items.AddRange(new ListItem[] { new ListItem("全部任务"), new ListItem("未完成"), new ListItem("已完成") });

                BindGridView(this.ddlTaskStatus.SelectedItem.Text);
                this.p1 = (Panel)GetControltByMaster("Panel1");
                this.p1.Controls.Add(this.gviewTransferAndScrap);
            }
        }

        void ddlTaskStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGridView(this.ddlTaskStatus.SelectedItem.Text);
            this.p1 = (Panel)GetControltByMaster("Panel1");
            this.p1.Controls.Add(this.gviewTransferAndScrap);
        }

        void BindGridView(string taskFlag)
        {
            this.gviewTransferAndScrap = new SPGridView();
            this.gviewTransferAndScrap.RowDataBound += new GridViewRowEventHandler(gviewTransferAndScrap_RowDataBound);

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
            this.gviewTransferAndScrap.Columns.Add(tfieldHyperLink);

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
                this.gviewTransferAndScrap.Columns.Add(bf);
            }

            this.gviewTransferAndScrap.AutoGenerateColumns = false;
            this.gviewTransferAndScrap.GridLines = GridLines.None;
            this.gviewTransferAndScrap.CssClass = "ms-vh2 padded headingfont";

            using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                if (taskFlag == "全部任务")
                {
                    this.gviewTransferAndScrap.DataSource = from t in dc.StockTransferTask
                                                              join e1 in dc.EmpInfo on t.TaskTargetID equals e1.EmpID
                                                              join e2 in dc.EmpInfo on t.TaskCreaterID equals e2.EmpID
                                                              where e1.Account.ToLower() == SPContext.Current.Web.CurrentUser.LoginName.ToLower()
                                                              orderby t.StockTransferTaskID descending
                                                              select new
                                                              {
                                                                  t.StockTransferTaskID,
                                                                  t.TaskTitle,
                                                                  t.CreateTime,
                                                                  e2.EmpName,
                                                                  t.TaskState,
                                                                  t.StockTransferID,
                                                                  t.TaskType,
                                                                  t.TaskInType
                                                              };
                    this.gviewTransferAndScrap.DataBind();
                }
                else
                {
                    this.gviewTransferAndScrap.DataSource = from t in dc.StockTransferTask
                                                            join e1 in dc.EmpInfo on t.TaskTargetID equals e1.EmpID
                                                            join e2 in dc.EmpInfo on t.TaskCreaterID equals e2.EmpID
                                                            where e1.Account.ToLower() == SPContext.Current.Web.CurrentUser.LoginName.ToLower() && t.TaskState == taskFlag
                                                            orderby t.StockTransferTaskID descending
                                                            select new
                                                            {
                                                                t.StockTransferTaskID,
                                                                t.TaskTitle,
                                                                t.CreateTime,
                                                                e2.EmpName,
                                                                t.TaskState,
                                                                t.StockTransferID,
                                                                t.TaskType,
                                                                t.TaskInType
                                                            };
                    this.gviewTransferAndScrap.DataBind();
                }
            }
            int colLastIndex = this.gviewTransferAndScrap.Columns.Count - 2;
            int rowLastIndex = this.gviewTransferAndScrap.Rows.Count - 1;
            for (int i = 0; i <= rowLastIndex; i++)
            {
                if (this.gviewTransferAndScrap.Rows[i].Cells[colLastIndex].Text == "已完成")
                {
                    this.gviewTransferAndScrap.Rows[i].Cells[colLastIndex].ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    this.gviewTransferAndScrap.Rows[i].Cells[colLastIndex].ForeColor = System.Drawing.Color.Red;
                }
            }

        }

        void gviewTransferAndScrap_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                HyperLink hl = e.Row.Cells[0].FindControl("hlItem") as HyperLink;
                switch (hl.ToolTip)
                {
                    case "发起人确认":
                        if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "移库任务")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StockTransfer/StockTransferShowToStart.aspx?StockTransferTaskID=" + DataBinder.Eval(e.Row.DataItem, "StockTransferTaskID").ToString();
                        else if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "委外入库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageCommitIn/QualityControlCommitIn.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString();
                        break;
                    case "材料会计审核":
                        if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "移库任务")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StockTransfer/StockTransferShowToFinal.aspx?StockTransferTaskID=" + DataBinder.Eval(e.Row.DataItem, "StockTransferTaskID").ToString();
                        else if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "委外入库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageCommitIn/StorageInDirectorManage.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString();
                        break;
                    case "物资组长审核信息":
                        if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "移库任务")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StockTransfer/StockTransferShow.aspx?StockTransferTaskID=" + DataBinder.Eval(e.Row.DataItem, "StockTransferTaskID").ToString();
                        else if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "委外入库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageCommitIn/StorageInDirectorManage.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString();
                        break;
                    case "生产组长审核信息":
                        if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "移库任务")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StockTransfer/StockTransferShowToCreater.aspx?StockTransferTaskID=" + DataBinder.Eval(e.Row.DataItem, "StockTransferTaskID").ToString();
                        else if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "委外入库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageCommitIn/StorageInDetailedManage.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString();
                        break;
                    case "发起人修改":
                        if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "移库任务")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StockTransfer/StockTransferDetailsManage.aspx?StockTransferID=" + DataBinder.Eval(e.Row.DataItem, "StockTransferID").ToString();
                        else if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "委外入库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageCommitIn/StorageInDetailedManage.aspx?TaskStorageID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString() + "&StorageInID=" + DataBinder.Eval(e.Row.DataItem, "StorageInID").ToString();
                        break;
                    case "资产组登记":
                        hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageRetrieveIn/ViewRetrieveInDetails.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskStorageID").ToString();
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
