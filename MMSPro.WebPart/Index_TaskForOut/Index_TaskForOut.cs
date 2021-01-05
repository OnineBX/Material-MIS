using System;
using System.Runtime.InteropServices;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Serialization;

using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.WebPartPages;
using System.Configuration;
using System.Linq;

namespace MMSPro.WebPart
{
    [Guid("e2ea86e2-7ff6-4824-a1ae-3f11e242f5d7")]
    public class Index_TaskForOut : System.Web.UI.WebControls.WebParts.WebPart
    {
        SPGridView _gviewTaskForOut;
        protected static string[] colNames = { "TaskTitle:待办事项", "CreateTime:创建时间", "EmpName:来自", "TaskState:状态" ,"Process:出库类型"};

        public Index_TaskForOut()
        {
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // TODO: add custom rendering code here.
            // Label label = new Label();
            // label.Text = "Hello World";
            // this.Controls.Add(label);

            this._gviewTaskForOut = new SPGridView();
            this._gviewTaskForOut.RowDataBound += new GridViewRowEventHandler(_gviewTaskForOut_RowDataBound);

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
            this._gviewTaskForOut.Columns.Add(tfieldHyperLink);

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
                this._gviewTaskForOut.Columns.Add(bf);
            }

            this._gviewTaskForOut.AutoGenerateColumns = false;
            this._gviewTaskForOut.GridLines = GridLines.None;
            this._gviewTaskForOut.CssClass = "ms-vh2 padded headingfont";
            //string curLoginName = SPContext.Current.Web.CurrentUser.LoginName;
            using (WebPartMMSProDBDataContext dc = new WebPartMMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //this._gviewTaskForOut.DataSource =  from t in dc.StorageOutTask
                //                                    join e1 in dc.EmpInfo on t equals e1.EmpID
                this._gviewTaskForOut.DataSource = (from t in dc.StorageOutTask
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
                                                    }).Take(8);
                this._gviewTaskForOut.DataBind();
            }
            int colLastIndex = this._gviewTaskForOut.Columns.Count - 2;
            int rowLastIndex = this._gviewTaskForOut.Rows.Count - 1;
            for (int i = 0; i <= rowLastIndex; i++)
            {
                if (this._gviewTaskForOut.Rows[i].Cells[colLastIndex].Text == "已完成")
                {
                    this._gviewTaskForOut.Rows[i].Cells[colLastIndex].ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    this._gviewTaskForOut.Rows[i].Cells[colLastIndex].ForeColor = System.Drawing.Color.Red;
                }
            }
            Literal L1 = new Literal();
            L1.Text = "<table style='width:100%; text-align:right'><tr><td><a href='WorkPages/DocAndIndexManager/MoreTaskOfStorageOut.aspx'>更多待办事项...</a></td></tr></table>";
            this.Controls.Add(this._gviewTaskForOut);
            this.Controls.Add(L1);
            this.Title = "出库待办事项";
        }

        void _gviewTaskForOut_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //throw new NotImplementedException();
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
    }
}
