/*------------------------------------------------------------------------------
 * Unit Name：ManageSrinSubDoc.cs
 * Description: 回收入库--配送组员创建回收分单的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-08-19
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
    public class CreateSrinSubDoc:Page
    {
        private int _userid, _subdocid;
        private DropDownList ddlProject, ddlCompany;
        private Button btnSave;        
        private TextBox txtTaker,txtRemark;
        private DateTimeControl dtcCreateTime;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._userid = Convert.ToInt32(Request.QueryString["CurrentUserID"]);
                this._subdocid = Convert.ToInt32(Request.QueryString["SubDocID"]);

                InitControl();
                if (!Page.IsPostBack)
                {
                    BindCompany();
                    if (_subdocid != 0)//分支流程--修改Notice的情况
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
            tbarbtnBack.ID = "backRow";
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

        private void InitControl()
        {
            InitBar();

            this.ddlProject = (DropDownList)GetControltByMaster("ddlProject");
            this.ddlCompany = (DropDownList)GetControltByMaster("ddlCompany");
            this.ddlCompany.SelectedIndexChanged += new EventHandler(ddlCompany_SelectedIndexChanged);
            this.dtcCreateTime = GetControltByMaster("dtcCreateTime") as DateTimeControl;
            this.btnSave = (Button)GetControltByMaster("btnSave");            
            this.btnSave.Click += new EventHandler(btnSave_Click);
      
            this.txtTaker = (TextBox)GetControltByMaster("txtTaker");
            ((Literal)GetControltByMaster("ltrTaker")).Text = JSDialogAid.GetJSForDialog(txtTaker.ClientID, "../StorageAndPile/SelectUser.aspx");

            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
           
        }

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                SrinSubDoc ssd = db.SrinSubDoc.SingleOrDefault(u => u.SrinSubDocID == this._subdocid);
                txtTaker.Text = ssd.EmpInfo1.Account;
                txtRemark.Text = ssd.Remark;
                ddlCompany.SelectedValue = ssd.ProjectInfo.Owner.ToString();
                dtcCreateTime.SelectedDate = ssd.CreateTime;
                BindProject();
                ddlProject.SelectedValue = ssd.Project.ToString();
                
                btnSave.Text = "修改";
            }
        }

        private void BindProject()
        {
            try
            {
                this.ddlProject.Items.Clear();

                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    this.ddlProject.DataSource = from a in db.ProjectInfo
                                                 where a.Owner == Convert.ToInt32(this.ddlCompany.SelectedValue)
                                                 select new
                                                 {
                                                     a.ProjectID,
                                                     a.ProjectName
                                                 };

                    this.ddlProject.DataTextField = "ProjectName";
                    this.ddlProject.DataValueField = "ProjectID";
                    this.ddlProject.DataBind();
                    if (ddlProject.Items.Count != 0)
                        this.ddlProject.Items.Insert(0, "--请选择--");
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

        private void BindCompany()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                this.ddlCompany.DataSource = from a in db.BusinessUnitInfo
                                             where a.BusinessUnitType.BusinessUnitTypeName == "业主单位"
                                             select new
                                             {
                                                 a.BusinessUnitID,
                                                 a.BusinessUnitName
                                             };
                this.ddlCompany.DataTextField = "BusinessUnitName";
                this.ddlCompany.DataValueField = "BusinessUnitID";
                this.ddlCompany.DataBind();
                this.ddlCompany.Items.Insert(0, new ListItem("--请选择--", "-1"));

            }
        }

        #endregion

        #region 控件事件方法

        void btnRefresh_Click(object sender, EventArgs e)
        {

        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("ManageSrinSubDoc.aspx", false);
        }

        void ddlCompany_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindProject();
        }       

        void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid)
                {
                    if (this.ddlProject.SelectedIndex == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "提示", "<script>alert('请选择回收项目! ');</script>");
                        return;
                    }

                    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        SrinSubDoc ssd;
                        if (this._subdocid == 0) //主流程--新建的情况
                        {
                            ssd = new SrinSubDoc();
                            ssd.Creator = this._userid;
                            ssd.CreateTime = ((DateTimeControl)GetControltByMaster("dtcCreateTime")).SelectedDate;
                            ssd.Project = Convert.ToInt32(ddlProject.SelectedValue);
                            ssd.Remark = ((TextBox)GetControltByMaster("txtRemark")).Text.Trim();
                            ssd.Taker = db.EmpInfo.SingleOrDefault(u => u.Account.Equals(txtTaker.Text.Trim())).EmpID;
                            db.SrinSubDoc.InsertOnSubmit(ssd);
                        }
                        else
                        {
                            ssd = db.SrinSubDoc.SingleOrDefault(u => u.SrinSubDocID == this._subdocid);
                            ssd.CreateTime = ((DateTimeControl)GetControltByMaster("dtcCreateTime")).SelectedDate;
                            ssd.Project = Convert.ToInt32(ddlProject.SelectedValue);
                            ssd.Remark = txtRemark.Text.Trim();
                            ssd.Taker = db.EmpInfo.SingleOrDefault(u => u.Account.Equals(txtTaker.Text.Trim())).EmpID;
                        }
                        db.SubmitChanges();
                    }

                    Response.Redirect("ManageSrinSubDoc.aspx", false);
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
       
        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }

        #endregion
    }
}
