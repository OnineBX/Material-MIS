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
    [Guid("c8f69e3d-a10f-4323-a351-17cf9f79156a")]
    public class MyMessage : System.Web.UI.WebControls.WebParts.WebPart
    {
        SPGridView _gviewMyMessage;
        private static string[] colNames = { "MessageInfoID:ID", "ReceiverID:ToID", "MessageTitle:消息主题", "CreateTime:日期", "FromEmp:来自" };

        public MyMessage()
        {
        }

        protected override void CreateChildControls()
        {
            //base.CreateChildControls();

            // TODO: add custom rendering code here.
            // Label label = new Label();
            // label.Text = "Hello World";
            // this.Controls.Add(label);
            this._gviewMyMessage = new SPGridView();

            HyperLinkField lf = new HyperLinkField();
            lf.HeaderText = "消息主题";
            //lf.DataTextField = "MessageTitle";
            //lf.DataNavigateUrlFields = new string[] { "MessageInfoID", "ReceiverID" };
            //lf.DataNavigateUrlFormatString = "MyMessage.aspx?curMessageInfoID={0}&curReceiverID={1}";

            //DataNavigateUrlFormatString不支持javascript
            //string relativeUrl = "MyMessage.aspx?curMessageInfoID={0}&curReceiverID={1}";
            //lf.DataNavigateUrlFormatString = "javascript:window.showModalDialog('" + relativeUrl + "','0','dialogWidth:300px;dialogHeight:450px');";
            //lf.DataNavigateUrlFormatString = "javascript:window.showModalDialog('MyMessage.aspx?curMessageInfoID={0}&curReceiverID={1}','0','dialogWidth:300px;dialogHeight:450px');";

            this._gviewMyMessage.Columns.Add(lf);

            BoundField bfCreateTime = new BoundField();
            bfCreateTime.HeaderText = "日期";
            bfCreateTime.DataField = "CreateTime";
            bfCreateTime.DataFormatString = "{0:yyyy-MM-dd}";
            this._gviewMyMessage.Columns.Add(bfCreateTime);

            BoundField bfMsgFromEmp = new BoundField();
            bfMsgFromEmp.HeaderText = "来自";
            bfMsgFromEmp.DataField = "MsgFromEmp";
            this._gviewMyMessage.Columns.Add(bfMsgFromEmp);

            BoundField bfMessageStatus = new BoundField();
            bfMessageStatus.HeaderText = "状态";
            bfMessageStatus.DataField = "MessageStatus";
            this._gviewMyMessage.Columns.Add(bfMessageStatus);

            this._gviewMyMessage.AutoGenerateColumns = false;
            this._gviewMyMessage.GridLines = GridLines.None;
            this._gviewMyMessage.CssClass = "ms-vh2 padded headingfont";
            this._gviewMyMessage.RowDataBound += new GridViewRowEventHandler(_gviewMyMessage_RowDataBound);

            //this._gviewMyMessage.AllowPaging = true;
            //this._gviewMyMessage.PageSize = 1;
            //this._gviewMyMessage.PageIndexChanging +=new GridViewPageEventHandler(_gviewMyMessage_PageIndexChanging);
            //this._gviewMyMessage.PagerTemplate = new SPGridViewPagerTemplate("{0} - {1}", _gviewMyMessage);

            using (WebPartMMSProDBDataContext dc = new WebPartMMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                this._gviewMyMessage.DataSource = (from m in dc.MessageInfo
                                                  join r in dc.MessageReceiver on m.MessageInfoID equals r.MessageInfoID
                                                  where m.MessageStatus == "未读" && m.MessageType == "私有消息" && dc.EmpInfo.SingleOrDefault(e=>e.EmpID ==r.ReceiverID).Account == SPContext.Current.Web.CurrentUser.LoginName
                                                  orderby m.MessageInfoID descending
                                                  select new
                                                  {
                                                      m.MessageInfoID,
                                                      r.ReceiverID,
                                                      m.MessageTitle,
                                                      m.CreateTime,
                                                      MsgFromEmp = dc.EmpInfo.SingleOrDefault(ee=>ee.EmpID == m.Creater).EmpName,
                                                      m.MessageStatus
                                                  }).Take(6);

                this._gviewMyMessage.DataBind();
            }
            Literal L1 = new Literal();
            L1.Text = "<table style='width:100%; text-align:right'><tr><td><a href='WorkPages/DocAndIndexManager/MoreMyMessage.aspx'>更多我的消息...</a></td></tr></table>";
            this.Controls.Add(this._gviewMyMessage);
            this.Controls.Add(L1);
            this.Title = "我的消息";
        }

        void _gviewMyMessage_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //throw new NotImplementedException();
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Cells[0].Text = string.Format("<a href=\"javaScript:onClick=window.showModalDialog(encodeURI('WorkPages/DocAndIndexManager/MyMessage.aspx?curMessageInfoID={0}&curReceiverID={1}'),'0','resizable:true;dialogWidth:700px;dialogHeight:400px')\">{2}</a>", DataBinder.Eval(e.Row.DataItem, "MessageInfoID"), DataBinder.Eval(e.Row.DataItem, "ReceiverID"), DataBinder.Eval(e.Row.DataItem, "MessageTitle"));// e.Row.Cells[0].Text
            }
        }

        //void _gviewMyMessage_PageIndexChanging(object sender, GridViewPageEventArgs e)
        //{
        //    //throw new NotImplementedException();
        //    this._gviewMyMessage.PageIndex = e.NewPageIndex;
        //    this._gviewMyMessage.DataBind();
        //}
    }
}
