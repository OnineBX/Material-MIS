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
namespace MMSPro.WebApp
{
    public class RelaProBusCreater : System.Web.UI.Page
    {
        DropDownList ddlProject;
        DropDownList ddlBusiness;    
        Button btnSave;
        Button btnQuit;
        string strType = "";
        int intId = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello ')</script>");
            InvtControl();
            if (!IsPostBack)
            {
                BindDDL();
            }
            if (!string.IsNullOrEmpty(Request.QueryString["BusinessUnitID"]) && Request.QueryString.Count == 1)
            {
                strType = "BusinessUnitID";                
                int.TryParse(Request.QueryString["BusinessUnitID"],out intId);
            }
            if (!string.IsNullOrEmpty(Request.QueryString["ProjectID"]) && Request.QueryString.Count == 1)
            {
                strType = "ProjectID";
                int.TryParse(Request.QueryString["ProjectID"], out intId);
            }
            if (intId == 0 || strType == "")
            {
                Response.Redirect("RelaProBusManager.aspx");
            }
            if (strType == "BusinessUnitID")
            {
                this.ddlBusiness.SelectedValue = intId.ToString();
                this.ddlBusiness.Enabled = false;
            }
            else
            {
                this.ddlProject.SelectedValue = intId.ToString();
                this.ddlProject.Enabled = false;

            }
        }

        private void BindDDL()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                
                this.ddlBusiness.DataSource = from a in db.BusinessUnitInfo
                                              select new
                                              {
                                                  Value = a.BusinessUnitID,
                                                  Key = a.BusinessUnitName
                                              };
                this.ddlBusiness.DataTextField = "Key";
                this.ddlBusiness.DataValueField = "Value";
                this.ddlBusiness.DataBind();
                this.ddlBusiness.Items.Insert(0, "--请选择--");


                this.ddlProject.DataSource = from b in db.ProjectInfo
                                             select new
                                             {
                                                 Value = b.ProjectID,
                                                 Key = b.ProjectName
                                             };
                this.ddlProject.DataTextField = "Key";
                this.ddlProject.DataValueField = "Value";
                this.ddlProject.DataBind();
                this.ddlProject.Items.Insert(0, "--请选择--");


              
            }
        }

        private void InvtControl()
        {
            this.ddlProject = (DropDownList)GetControltByMaster("ddlProject");
            this.ddlBusiness = (DropDownList)GetControltByMaster("ddlBusiness"); 
            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
        }
    

        void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("RelaProBusManager.aspx");
        }

        void btnSave_Click(object sender, EventArgs e)
        {

            if (this.ddlProject.SelectedIndex !=0 &&this.ddlBusiness.SelectedIndex!=0)
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))                
                {
                   //检查唯一性
                    var temp = db.RelationProjectBusiness.SingleOrDefault(a => a.ProjectID == int.Parse(this.ddlProject.SelectedValue)&&a.BusinessUnitID == int.Parse(this.ddlBusiness.SelectedValue));
                    if (temp == null)
                    {
                        RelationProjectBusiness ei = new RelationProjectBusiness();
                        ei.BusinessUnitID = int.Parse(this.ddlBusiness.SelectedValue);
                        ei.ProjectID = int.Parse(this.ddlProject.SelectedValue);
                        db.RelationProjectBusiness.InsertOnSubmit(ei);
                        db.SubmitChanges();
                        Response.Redirect("RelaProBusManager.aspx");
                    }
                    else
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('关系已存在')</script>");
                    }
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

