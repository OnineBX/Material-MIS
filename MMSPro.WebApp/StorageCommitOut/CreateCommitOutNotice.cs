/*------------------------------------------------------------------------------
 * Unit Name：CreateCommitOutNotice.cs
 * Description: 委外出库--创建委外调拨单的页面
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
    public class CreateCommitOutNotice:System.Web.UI.Page
    {
        private int _userid,_noticeid;
        private DropDownList ddlReceiver;
        private Button btnSave;
        private CustomValidator vldNoticeCode;
        private TextBox txtNoticeCode, txtRemark;
        private DateTimeControl dtcCreateTime;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._userid = Convert.ToInt32(Request.QueryString["CurrentUserID"]);
                this._noticeid = Convert.ToInt32(Request.QueryString["NoticeID"]);

                InitializeCustomControls();
                if (!Page.IsPostBack)
                {
                    BindControl();
                    if (_noticeid != 0)//分支流程--修改Notice的情况
                        BindDataToCustomControls();
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
        #region 初始化和绑定数据方法

        private void InitBar()
        {
            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");

            //返回
            ToolBarButton tbarbtnBack = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnBack.ID = "btnBack";
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
            this.ddlReceiver = (DropDownList)GetControltByMaster("ddlReceiver");
            this.btnSave = (Button)GetControltByMaster("btnSave");            
            this.btnSave.Click += new EventHandler(btnSave_Click);

            this.txtNoticeCode = (TextBox)GetControltByMaster("txtNoticeCode");
            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");

            vldNoticeCode = (CustomValidator)GetControltByMaster("vldNoticeCode");
            vldNoticeCode.ServerValidate += new ServerValidateEventHandler(vldNoticeCode_ServerValidate);

            dtcCreateTime = GetControltByMaster("dtcCreateTime") as DateTimeControl;
        }

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                StorageCommitOutNotice scon = db.StorageCommitOutNotice.SingleOrDefault(u => u.StorageCommitOutNoticeID == this._noticeid);
                txtNoticeCode.Text = scon.StorageCommitOutNoticeCode;
                txtRemark.Text = scon.Remark;
                ddlReceiver.SelectedValue = scon.Receiver.ToString();
                dtcCreateTime.SelectedDate = scon.CreateTime;
                btnSave.Text = "修改";
            }
        }

        private void BindControl()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = from a in db.BusinessUnitInfo
                           where a.BusinessUnitType.BusinessUnitTypeName == "内部单位"
                           select new
                           {
                               Key = a.BusinessUnitName,
                               Value = a.BusinessUnitID
                           };

                this.ddlReceiver.DataSource = temp;
                this.ddlReceiver.DataTextField = "Key";
                this.ddlReceiver.DataValueField = "Value";
                this.ddlReceiver.DataBind();
                this.ddlReceiver.Items.Insert(0, "--请选择--");
            }
        }

        #endregion

        #region 控件事件方法

        void btnRefresh_Click(object sender, EventArgs e)
        {
            
        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("ManageCommitOutNotice.aspx", false);
        }

        void vldNoticeCode_ServerValidate(object source, ServerValidateEventArgs args)
        {
            try
            {
                if (btnSave.Text.Equals("创建"))
                {
                    string strNoticeCode = this.txtNoticeCode.Text.Trim();

                    //NoticeID为空的情况
                    if (string.IsNullOrEmpty(strNoticeCode))
                    {
                        args.IsValid = false;
                        vldNoticeCode.Text = "调拨通知单号不能为空！";
                        return;
                    }

                    //数据库中存在相同NoticeCode的情况
                    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        StorageCommitOutNotice scon = db.StorageCommitOutNotice.SingleOrDefault(u => u.StorageCommitOutNoticeCode == strNoticeCode);
                        if (scon != null)
                        {
                            args.IsValid = false;
                            vldNoticeCode.Text = "调拨通知单号已存在！";
                            return;
                        }
                    }

                    args.IsValid = true;
                }                
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));
            }            
        }

        void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid)
                {
                    if (this.ddlReceiver.SelectedIndex == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择领料单位! ');</script>");
                        return;
                    }

                    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        StorageCommitOutNotice scon;
                        if (this._noticeid == 0) //主流程--新建的情况
                        {
                            scon = new StorageCommitOutNotice();
                            scon.Creator = this._userid;
                            scon.CreateTime = dtcCreateTime.SelectedDate;
                            scon.Receiver = Convert.ToInt32(ddlReceiver.SelectedValue);
                            scon.Remark = ((TextBox)GetControltByMaster("txtRemark")).Text.Trim();
                            scon.StorageCommitOutNoticeCode = ((TextBox)GetControltByMaster("txtNoticeCode")).Text.Trim();
                            db.StorageCommitOutNotice.InsertOnSubmit(scon);
                        }
                        else
                        {
                            scon = db.StorageCommitOutNotice.SingleOrDefault(u => u.StorageCommitOutNoticeID == this._noticeid);
                            scon.CreateTime = ((DateTimeControl)GetControltByMaster("dtcCreateTime")).SelectedDate;
                            scon.Receiver = Convert.ToInt32(ddlReceiver.SelectedValue);
                            scon.Remark = txtRemark.Text.Trim();
                            scon.StorageCommitOutNoticeCode = txtNoticeCode.Text.Trim();
                        }
                        db.SubmitChanges();
                    }

                    Response.Redirect("ManageCommitOutNotice.aspx",false);
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
            }
           

        }

        #endregion                                

        #region 辅助方法
        /// <summary>
        /// 获取控件
        /// </summary>
        /// <param name="controlName">控件的ID名称</param>
        /// <returns>返回Control，需要强制类型转换为对应控件</returns>
        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }

        #endregion
    }
}
