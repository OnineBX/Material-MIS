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
using System.Reflection;

namespace MMSPro.WebApp
{
    public class MyMessage : System.Web.UI.Page
    {
        Label lblFromProcess;
        Label lblFromTask;
        Label lblFromEmp;
        Label lblCreateTime;
        Label lblMsgTitle;
        Label lblMsgContent;
        Label lblMsgStatus;
        Button btnEditStatus;
        int curMsgInfoID;

        protected void Page_Load(object sender, EventArgs e)
        {
            this.curMsgInfoID = Convert.ToInt32(Request.QueryString["curMessageInfoID"]);
            InitControls();
        }

        protected void InitControls()
        {
            this.lblFromProcess = (Label)GetControltByMaster("lblFromProcess");
            this.lblFromTask = (Label)GetControltByMaster("lblFromTask");
            this.lblFromEmp = (Label)GetControltByMaster("lblFromEmp");
            this.lblCreateTime = (Label)GetControltByMaster("lblCreateTime");
            this.lblMsgTitle = (Label)GetControltByMaster("lblMsgTitle");
            this.lblMsgContent = (Label)GetControltByMaster("lblMsgContent");
            this.lblMsgStatus = (Label)GetControltByMaster("lblMsgStatus");
            this.btnEditStatus = (Button)GetControltByMaster("btnEditStatus");
            btnEditStatus.Click += new EventHandler(btnEditStatus_Click);

            using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                try
                {
                    MessageInfo msgInfoTemp = dc.MessageInfo.SingleOrDefault(mi => mi.MessageInfoID == this.curMsgInfoID);
                    if (msgInfoTemp != null)
                    {
                        this.lblFromProcess.Text = msgInfoTemp.MessageSource;
                        if (msgInfoTemp.MessageSource == "回收入库")
                        {
                            this.lblFromTask.Text = dc.TaskStorageIn.SingleOrDefault(tsi => tsi.TaskStorageID == msgInfoTemp.TaskID).TaskTitle;
                        }
                        else if (msgInfoTemp.MessageSource == "正常入库")
                        {
                            this.lblFromTask.Text = dc.StorageOutTask.SingleOrDefault(sot => sot.TaskID == msgInfoTemp.TaskID).TaskTitle;
                        }

                        //this.lblFromEmp.Text = dc.EmpInfo.SingleOrDefault(ei => ei.EmpID == msgInfoTemp.Creater).EmpName;
                        this.lblFromEmp.Text = msgInfoTemp.EmpInfo.EmpName;
                        this.lblCreateTime.Text = msgInfoTemp.CreateTime.ToString();
                        this.lblMsgTitle.Text += msgInfoTemp.MessageTitle;
                        this.lblMsgContent.Text = msgInfoTemp.MessageContent;
                        this.lblMsgStatus.Text = msgInfoTemp.MessageStatus;
                        if (this.lblMsgStatus.Text == "未读")
                        {
                            this.lblMsgStatus.ForeColor = System.Drawing.Color.Red;
                            this.btnEditStatus.Text = "标记为已读?";
                            this.btnEditStatus.Enabled = true;
                        }
                        else
                        {
                            this.lblMsgStatus.ForeColor = System.Drawing.Color.Green;
                            this.btnEditStatus.Text = "已读状态";
                            this.btnEditStatus.Enabled = false;
                        }

                    }
                }
                catch (Exception ex)
                {
                    MethodBase mb = MethodBase.GetCurrentMethod();
                    LogToDBHelper lhelper = LogToDBHelper.Instance;
                    lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                    ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
                }
            }
        }

        void btnEditStatus_Click(object sender, EventArgs e)
        {
            using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                MessageInfo msgInfoTemp = dc.MessageInfo.SingleOrDefault(mi => mi.MessageInfoID == this.curMsgInfoID);
                if (msgInfoTemp != null)
                {
                    msgInfoTemp.MessageStatus = "已读";
                    dc.SubmitChanges();
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
