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
    [Guid("8acb73f0-553d-4acb-b9d6-9bc672481eb2")]
    public class TransferAndScrap : System.Web.UI.WebControls.WebParts.WebPart
    {
        SPGridView _gviewTransferAndScrap;
        protected static string[] colNames = { "TaskTitle:待办事项", "CreateTime:创建时间", "EmpName:来自", "TaskState:状态", "TaskInType:任务类型" };
        public TransferAndScrap()
        {
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // TODO: add custom rendering code here.
            // Label label = new Label();
            // label.Text = "Hello World";
            // this.Controls.Add(label);

            this._gviewTransferAndScrap = new SPGridView();
            this._gviewTransferAndScrap.RowDataBound += new GridViewRowEventHandler(_gviewTransferAndScrap_RowDataBound);

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
            this._gviewTransferAndScrap.Columns.Add(tfieldHyperLink);

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
                this._gviewTransferAndScrap.Columns.Add(bf);
            }

            this._gviewTransferAndScrap.AutoGenerateColumns = false;
            this._gviewTransferAndScrap.GridLines = GridLines.None;
            this._gviewTransferAndScrap.CssClass = "ms-vh2 padded headingfont";
            //string curLoginName = SPContext.Current.Web.CurrentUser.LoginName;
            using (WebPartMMSProDBDataContext dc = new WebPartMMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                this._gviewTransferAndScrap.DataSource = (from t in dc.StockTransferTask
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
                                                   }).Take(7);
                this._gviewTransferAndScrap.DataBind();
            }
            int colLastIndex = this._gviewTransferAndScrap.Columns.Count - 2;
            int rowLastIndex = this._gviewTransferAndScrap.Rows.Count - 1;
            for (int i = 0; i <= rowLastIndex; i++)
            {
                if (this._gviewTransferAndScrap.Rows[i].Cells[colLastIndex].Text == "已完成")
                {
                    this._gviewTransferAndScrap.Rows[i].Cells[colLastIndex].ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    this._gviewTransferAndScrap.Rows[i].Cells[colLastIndex].ForeColor = System.Drawing.Color.Red;
                }
            }
            Literal L1 = new Literal();
            L1.Text = "<table style='width:100%; text-align:right'><tr><td><a href='workpages/DocAndIndexManager/MoreTransferAndScrap.aspx'>更多待办事项...</a></td></tr></table>";
            this.Controls.Add(this._gviewTransferAndScrap);
            this.Controls.Add(L1);
            this.Title = "移库&报废";
        }

        void _gviewTransferAndScrap_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //throw new NotImplementedException();
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
    }
}
