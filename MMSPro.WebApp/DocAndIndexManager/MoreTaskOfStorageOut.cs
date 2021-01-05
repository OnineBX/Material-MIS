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
    public class MoreTaskOfStorageOut: System.Web.UI.Page
    {
        DropDownList ddlTaskStatus;
        SPGridView gviewMoreTaskForOut;
        Panel p1;
        protected static string[] colNames = { "TaskTitle:待办事项", "CreateTime:创建时间", "EmpName:来自", "TaskState:状态", "Process:出库类型" };
        protected void Page_Load(object sender, EventArgs e)
        {

            this.ddlTaskStatus = (DropDownList)GetControltByMaster("ddlTaskStatus");
            this.ddlTaskStatus.SelectedIndexChanged += new EventHandler(ddlTaskStatus_SelectedIndexChanged);
            if (!IsPostBack)
            {
                this.ddlTaskStatus.Items.AddRange(new ListItem[] { new ListItem("全部任务"), new ListItem("未完成"), new ListItem("已完成") });

                BindGridView(this.ddlTaskStatus.SelectedItem.Text);
                this.p1 = (Panel)GetControltByMaster("Panel1");
                this.p1.Controls.Add(this.gviewMoreTaskForOut);
            }

        }

        void ddlTaskStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGridView(this.ddlTaskStatus.SelectedItem.Text);
            this.p1 = (Panel)GetControltByMaster("Panel1");
            this.p1.Controls.Add(this.gviewMoreTaskForOut);
        }
        void BindGridView(string taskFlag)
        {
            this.gviewMoreTaskForOut = new SPGridView();
            this.gviewMoreTaskForOut.AutoGenerateColumns = false;
            this.gviewMoreTaskForOut.RowDataBound += new GridViewRowEventHandler(gviewMoreTaskForOut_RowDataBound);

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
            this.gviewMoreTaskForOut.Columns.Add(tfieldHyperLink);

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
                this.gviewMoreTaskForOut.Columns.Add(bf);
            }

            this.gviewMoreTaskForOut.AutoGenerateColumns = false;
            this.gviewMoreTaskForOut.GridLines = GridLines.None;
            this.gviewMoreTaskForOut.CssClass = "ms-vh2 padded headingfont";
            //string curLoginName = SPContext.Current.Web.CurrentUser.LoginName;
            using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                if (taskFlag == "全部任务")
                {
                    this.gviewMoreTaskForOut.DataSource = from t in dc.StorageOutTask
                                                        join e1 in dc.EmpInfo on t.TaskTargetID equals e1.EmpID
                                                        join e2 in dc.EmpInfo on t.TaskCreaterID equals e2.EmpID
                                                        where e1.Account.ToLower() == SPContext.Current.Web.CurrentUser.LoginName.ToLower()
                                                        orderby t.TaskID descending
                                                        select new
                                                        {
                                                            t.TaskID,
                                                            t.TaskTitle,
                                                            t.CreateTime,
                                                            e2.EmpName,
                                                            t.TaskState,
                                                            t.NoticeID,
                                                            t.TaskType,
                                                            t.Process
                                                        };
                    this.gviewMoreTaskForOut.DataBind();
                }
                else
                {
                    this.gviewMoreTaskForOut.DataSource = from t in dc.StorageOutTask
                                                        join e1 in dc.EmpInfo on t.TaskTargetID equals e1.EmpID
                                                        join e2 in dc.EmpInfo on t.TaskCreaterID equals e2.EmpID
                                                           where e1.Account.ToLower() == SPContext.Current.Web.CurrentUser.LoginName.ToLower() && t.TaskState == taskFlag
                                                        orderby t.TaskID descending
                                                        select new
                                                        {
                                                            t.TaskID,
                                                            t.TaskTitle,
                                                            t.CreateTime,
                                                            e2.EmpName,
                                                            t.TaskState,
                                                            t.NoticeID,
                                                            t.TaskType,
                                                            t.Process
                                                        };
                    this.gviewMoreTaskForOut.DataBind();
                }
            }
            int colLastIndex = this.gviewMoreTaskForOut.Columns.Count - 2;
            int rowLastIndex = this.gviewMoreTaskForOut.Rows.Count - 1;
            for (int i = 0; i <= rowLastIndex; i++)
            {
                if (this.gviewMoreTaskForOut.Rows[i].Cells[colLastIndex].Text == "已完成")
                {
                    this.gviewMoreTaskForOut.Rows[i].Cells[colLastIndex].ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    this.gviewMoreTaskForOut.Rows[i].Cells[colLastIndex].ForeColor = System.Drawing.Color.Red;
                }
            }

        }

        void gviewMoreTaskForOut_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                HyperLink hl = e.Row.Cells[0].FindControl("hlItem") as HyperLink;
                switch (hl.ToolTip)
                {
                    case "物资调拨审核信息":
                        if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "正常出库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageOut/NormalOutProduceAuditInfo.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskID").ToString();
                        else if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "委外出库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageCommitOut/CommitOutProduceAuditInfo.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskID").ToString();
                        break;
                    case "物资调拨审核":
                        if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "正常出库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageOut/NormalOutProduceAudit.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskID").ToString();
                        else if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "委外出库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageCommitOut/CommitOutProduceAudit.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskID").ToString();
                        break;
                    case "物资出库":
                        if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "正常出库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageOut/NormalOutAssetDetails.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskID").ToString();
                        else if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "委外出库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageCommitOut/CommitOutAssetDetails.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskID").ToString();
                        break;
                    case "物资出库审核":
                        if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "正常出库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageOut/NormalOutAssetAudit.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskID").ToString();
                        else if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "委外出库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageCommitOut/CommitOutAssetAudit.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskID").ToString();
                        break;
                    case "物资出库审核信息":
                        if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "正常出库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageOut/NormalOutAssetAuditInfo.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskID").ToString();
                        else if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "委外出库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageCommitOut/CommitOutAssetAuditInfo.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskID").ToString();
                        break;
                    case "主任审批":
                        if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "正常出库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageOut/NormalOutDirectorConfirm.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskID").ToString();
                        else if (e.Row.Cells[e.Row.Cells.Count - 1].Text.Trim() == "委外出库")
                            hl.NavigateUrl = SPContext.Current.Web.Url + "/WorkPages/StorageCommitOut/CommitOutDirectorConfirm.aspx?TaskID=" + DataBinder.Eval(e.Row.DataItem, "TaskID").ToString();
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
