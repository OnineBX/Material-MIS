/*------------------------------------------------------------------------------
 * Unit Name：CreateStorageOutNotice.cs
 * Description: 正常出库--创建委外调拨单的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-10-27
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
    public class CreateStorageOutNotice:System.Web.UI.Page
    {
        private int _userid,_noticeid;
        private DropDownList ddlProprietor,ddlConstructor,ddlProject,ddlProjectStage;
        private Button btnSave;
        private CustomValidator vldNoticeCode;
        private TextBox txtNoticeCode, txtRemark, txtProperty;
        private DateTimeControl dtcCreateTime;
      
        private static ListItem[] Stages = new ListItem[]{ 
                                                               new ListItem("--请选择--"),
                                                               new ListItem("钻井"), 
                                                               new ListItem("完井"), 
                                                               new ListItem("测试"), 
                                                               new ListItem("地面建设"), 
                                                               new ListItem("其他") 
                                                          };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._userid = Convert.ToInt32(Request.QueryString["CurrentUserID"]);
                this._noticeid = Convert.ToInt32(Request.QueryString["NoticeID"]);

                InitializeCustomControl();
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

        private void InitializeCustomControl()
        {
            InitBar();

            this.ddlProprietor = GetControltByMaster("ddlProprietor") as DropDownList;
            this.ddlProprietor.SelectedIndexChanged += new EventHandler(ddlProprietor_SelectedIndexChanged);

            this.ddlConstructor = GetControltByMaster("ddlConstructor") as DropDownList;

            this.ddlProject = GetControltByMaster("ddlProject") as DropDownList;
            this.ddlProject.SelectedIndexChanged += new EventHandler(ddlProject_SelectedIndexChanged);

            this.ddlProjectStage = GetControltByMaster("ddlProjectStage") as DropDownList;

            this.txtProperty = GetControltByMaster("txtProperty") as TextBox;

            this.btnSave = GetControltByMaster("btnSave") as Button;
            this.btnSave.Click += new EventHandler(btnSave_Click);

            this.txtNoticeCode = GetControltByMaster("txtNoticeCode") as TextBox;
            this.txtRemark = GetControltByMaster("txtRemark") as TextBox;

            this.dtcCreateTime = GetControltByMaster("dtcCreateTime") as DateTimeControl;

            vldNoticeCode = GetControltByMaster("vldNoticeCode") as CustomValidator;
            vldNoticeCode.ClientValidationFunction = "vldDDL";
            vldNoticeCode.ServerValidate += new ServerValidateEventHandler(vldNoticeCode_ServerValidate);

            (GetControltByMaster("ltrJS") as Literal).Text = JSDialogAid.GetVerifyDDLJSForVld("--请选择--","您填入的信息不完整，请核实后重新提交！");
        }              

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化项目阶段
                ddlProjectStage.Items.AddRange(Stages);

                StorageOutNotice son = db.StorageOutNotice.SingleOrDefault(u => u.StorageOutNoticeID == this._noticeid);
                txtNoticeCode.Text = son.StorageOutNoticeCode;
                txtRemark.Text = son.Remark;
                ddlProprietor.SelectedValue = son.Proprietor.ToString();
                ddlConstructor.SelectedValue = son.Constructor.ToString();
                dtcCreateTime.SelectedDate = son.CreateTime;

                //初始化项目列表
                this.ddlProject.DataSource = from a in db.ProjectInfo
                                             where a.Owner == Convert.ToInt32(ddlProprietor.SelectedValue)
                                             select new
                                             {
                                                 Key = a.ProjectName,
                                                 Value = a.ProjectID,
                                             };
                this.ddlProject.DataTextField = "Key";
                this.ddlProject.DataValueField = "Value";
                this.ddlProject.DataBind();
                this.ddlProject.Items.Insert(0, "--请选择--");


                ddlProject.SelectedValue = son.ProjectID.ToString();
                txtProperty.Text = son.ProjectInfo.ProjectProperty;
                ddlProjectStage.Text = son.ProjectStage.Trim();
                btnSave.Text = "修改";
            }            
        }

        private void BindControl()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //绑定业主单位
                this.ddlProprietor.DataSource = from a in db.BusinessUnitInfo
                                                where a.BusinessUnitType.BusinessUnitTypeName == "业主单位"
                                                select new
                                                {
                                                    Key = a.BusinessUnitName,
                                                    Value = a.BusinessUnitID
                                                };

                this.ddlProprietor.DataTextField = "Key";
                this.ddlProprietor.DataValueField = "Value";
                this.ddlProprietor.DataBind();
                this.ddlProprietor.Items.Insert(0, new ListItem("--请选择--","0"));


                //绑定施工单位
                this.ddlConstructor.DataSource = from a in db.BusinessUnitInfo
                                                where a.BusinessUnitType.BusinessUnitTypeName == "施工单位"
                                                select new
                                                {
                                                    Key = a.BusinessUnitName,
                                                    Value = a.BusinessUnitID
                                                };

                this.ddlConstructor.DataTextField = "Key";
                this.ddlConstructor.DataValueField = "Value";
                this.ddlConstructor.DataBind();
                this.ddlConstructor.Items.Insert(0, "--请选择--");                

            }
        }

        #endregion

        #region 控件事件方法

        void btnRefresh_Click(object sender, EventArgs e)
        {
            
        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("ManageStorageOutNotice.aspx", false);
        }

        void ddlProprietor_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    this.ddlProject.DataSource = from a in db.ProjectInfo
                                                 where a.Owner == Convert.ToInt32(ddlProprietor.SelectedValue)
                                                 select new {
                                                                Key = a.ProjectName,
                                                                Value = a.ProjectID,
                                                            };
                    this.ddlProject.DataTextField = "Key";
                    this.ddlProject.DataValueField = "Value";
                    this.ddlProject.DataBind();
                    this.ddlProject.Items.Insert(0, "--请选择--");

                    this.ddlProjectStage.Items.Clear();
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

        void ddlProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    txtProperty.Text = db.ProjectInfo.SingleOrDefault(u => u.ProjectID.Equals(Convert.ToInt32(ddlProject.SelectedValue))).ProjectProperty;
                                       
                    InitProjectStage();         
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
                        StorageOutNotice son = db.StorageOutNotice.SingleOrDefault(u => u.StorageOutNoticeCode == strNoticeCode);
                        if (son != null)
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
                    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        StorageOutNotice son;
                        if (this._noticeid == 0) //主流程--新建的情况
                        {
                            son = new StorageOutNotice();
                            son.StorageOutNoticeCode = txtNoticeCode.Text.Trim();
                            son.Proprietor = Convert.ToInt32(ddlProprietor.SelectedValue);
                            son.ProjectID = Convert.ToInt32(ddlProject.SelectedValue);
                            son.ProjectStage = ddlProjectStage.Text;
                            son.Constructor = Convert.ToInt32(ddlConstructor.SelectedValue);
                            son.Creator = this._userid;
                            son.CreateTime = dtcCreateTime.SelectedDate;
                            son.Remark = txtRemark.Text.Trim();                            
                            db.StorageOutNotice.InsertOnSubmit(son);
                        }
                        else
                        {
                            son = db.StorageOutNotice.SingleOrDefault(u => u.StorageOutNoticeID == this._noticeid);
                            son.StorageOutNoticeCode = txtNoticeCode.Text.Trim();
                            son.Proprietor = Convert.ToInt32(ddlProprietor.SelectedValue);
                            son.ProjectID = Convert.ToInt32(ddlProject.SelectedValue);
                            son.ProjectStage = ddlProjectStage.Text;
                            son.Constructor = Convert.ToInt32(ddlConstructor.SelectedValue);
                            son.Remark = txtRemark.Text.Trim();
                            son.CreateTime = ((DateTimeControl)GetControltByMaster("dtcCreateTime")).SelectedDate;
                            
                        }
                        db.SubmitChanges();
                    }

                    Response.Redirect("ManageStorageOutNotice.aspx",false);
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

        private void InitProjectStage()
        {
            //绑定项目阶段
            if (ddlProject.Text.Equals("--请选择--"))
            {
                if (ddlProjectStage.Items.Count != 0)
                    ddlProjectStage.Items.Clear();
            }
            else
            {
                if (ddlProjectStage.Items.Count == 0)
                    this.ddlProjectStage.Items.AddRange(Stages);
                this.ddlProjectStage.SelectedIndex = 0;   
            }                     
        }

        #endregion
    }
}
