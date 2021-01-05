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
    public class MoreMyMessage : System.Web.UI.Page
    {
        DropDownList ddlMsgStatus;
        SPGridView gviewMoreMyMsg;
        Panel p1;

        protected void Page_Load(object sender, EventArgs e)
        {

            this.ddlMsgStatus = (DropDownList)GetControltByMaster("ddlMsgStatus");
            this.ddlMsgStatus.SelectedIndexChanged += new EventHandler(ddlMsgStatus_SelectedIndexChanged);
            if (!IsPostBack)
            {
                this.ddlMsgStatus.Items.AddRange(new ListItem[] { new ListItem("全部消息"), new ListItem("未读"), new ListItem("已读") });

                BindGridView(this.ddlMsgStatus.SelectedItem.Text);
                this.p1 = (Panel)GetControltByMaster("Panel1");
                this.p1.Controls.Add(this.gviewMoreMyMsg);
            }

        }

        void BindGridView(string msgFlag)
        {
            //throw new NotImplementedException();
            this.gviewMoreMyMsg = new SPGridView();

            HyperLinkField lf = new HyperLinkField();
            lf.HeaderText = "消息主题";
            //lf.DataTextField = "MessageTitle";
            //lf.DataNavigateUrlFields = new string[] { "MessageInfoID", "ReceiverID" };
            //lf.DataNavigateUrlFormatString = "MyMessage.aspx?curMessageInfoID={0}&curReceiverID={1}";

            //DataNavigateUrlFormatString不支持javascript
            //string relativeUrl = "MyMessage.aspx?curMessageInfoID={0}&curReceiverID={1}";
            //lf.DataNavigateUrlFormatString = "javascript:window.showModalDialog('" + relativeUrl + "','0','dialogWidth:300px;dialogHeight:450px');";
            //lf.DataNavigateUrlFormatString = "javascript:window.showModalDialog('MyMessage.aspx?curMessageInfoID={0}&curReceiverID={1}','0','dialogWidth:300px;dialogHeight:450px');";

            this.gviewMoreMyMsg.Columns.Add(lf);

            BoundField bfCreateTime = new BoundField();
            bfCreateTime.HeaderText = "日期";
            bfCreateTime.DataField = "CreateTime";
            bfCreateTime.DataFormatString = "{0:yyyy-MM-dd}";
            this.gviewMoreMyMsg.Columns.Add(bfCreateTime);

            BoundField bfMsgFromEmp = new BoundField();
            bfMsgFromEmp.HeaderText = "来自";
            bfMsgFromEmp.DataField = "MsgFromEmp";
            this.gviewMoreMyMsg.Columns.Add(bfMsgFromEmp);

            BoundField bfMessageStatus = new BoundField();
            bfMessageStatus.HeaderText = "状态";
            bfMessageStatus.DataField = "MessageStatus";
            this.gviewMoreMyMsg.Columns.Add(bfMessageStatus);

            this.gviewMoreMyMsg.AutoGenerateColumns = false;
            this.gviewMoreMyMsg.GridLines = GridLines.None;
            this.gviewMoreMyMsg.CssClass = "ms-vh2 padded headingfont";
            this.gviewMoreMyMsg.RowDataBound += new GridViewRowEventHandler(gviewMoreMyMsg_RowDataBound);
            //this.gviewMoreTaskForMyMsg.AllowPaging = true;
            //this.gviewMoreTaskForMyMsg.PageSize = 1;
            //this.gviewMoreTaskForMyMsg.PageIndexChanging +=new GridViewPageEventHandler(gviewMoreTaskForMyMsg_PageIndexChanging);
            //this.gviewMoreTaskForMyMsg.PagerTemplate = new SPGridViewPagerTemplate("{0} - {1}", gviewMoreTaskForMyMsg);

            using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                if (msgFlag == "全部消息")
                {
                    this.gviewMoreMyMsg.DataSource = from m in dc.MessageInfo
                                                      join r in dc.MessageReceiver on m.MessageInfoID equals r.MessageInfoID
                                                      where m.MessageType == "私有消息" && dc.EmpInfo.SingleOrDefault(e => e.EmpID == r.ReceiverID).Account == SPContext.Current.Web.CurrentUser.LoginName
                                                      orderby m.MessageInfoID descending
                                                      select new
                                                      {
                                                          m.MessageInfoID,
                                                          r.ReceiverID,
                                                          m.MessageTitle,
                                                          m.CreateTime,
                                                          MsgFromEmp = dc.EmpInfo.SingleOrDefault(ee => ee.EmpID == m.Creater).EmpName,
                                                          m.MessageStatus
                                                      };

                    this.gviewMoreMyMsg.DataBind();
                }
                else
                {
                    this.gviewMoreMyMsg.DataSource = from m in dc.MessageInfo
                                                      join r in dc.MessageReceiver on m.MessageInfoID equals r.MessageInfoID
                                                      where m.MessageStatus == msgFlag && m.MessageType == "私有消息" && dc.EmpInfo.SingleOrDefault(e => e.EmpID == r.ReceiverID).Account == SPContext.Current.Web.CurrentUser.LoginName
                                                      orderby m.MessageInfoID descending
                                                      select new
                                                      {
                                                          m.MessageInfoID,
                                                          r.ReceiverID,
                                                          m.MessageTitle,
                                                          m.CreateTime,
                                                          MsgFromEmp = dc.EmpInfo.SingleOrDefault(ee => ee.EmpID == m.Creater).EmpName,
                                                          m.MessageStatus
                                                      };

                    this.gviewMoreMyMsg.DataBind();
                }
            }
        }

        void gviewMoreMyMsg_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Cells[0].Text = string.Format("<a href=\"javaScript:onClick=window.showModalDialog(encodeURI('MyMessage.aspx?curMessageInfoID={0}&curReceiverID={1}'),'0','resizable:true;dialogWidth:700px;dialogHeight:400px')\">{2}</a>", DataBinder.Eval(e.Row.DataItem, "MessageInfoID"), DataBinder.Eval(e.Row.DataItem, "ReceiverID"), DataBinder.Eval(e.Row.DataItem, "MessageTitle"));// e.Row.Cells[0].Text
            }
        }

        void ddlMsgStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGridView(this.ddlMsgStatus.SelectedItem.Text);
            this.p1 = (Panel)GetControltByMaster("Panel1");
            this.p1.Controls.Add(this.gviewMoreMyMsg);
        }

        void ddlTaskStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGridView(this.ddlMsgStatus.SelectedItem.Text);
            this.p1 = (Panel)GetControltByMaster("Panel1");
            this.p1.Controls.Add(this.gviewMoreMyMsg);
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
