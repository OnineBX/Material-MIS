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
    public class PileEdit:System.Web.UI.Page
    {
        MMSProDBDataContext db;
        TextBox txtName;
        TextBox txtCode;
        TextBox txtSize;
        DropDownList ddlStorage;
        TextBox txtRemark;
        Button btnSave;
        Button btnQuit;
        protected void Page_Load(object sender, EventArgs e)
        {
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello ')</script>");
            InitControl();
            if (!string.IsNullOrEmpty(Request.QueryString["PileID"]))
            {

                if (!IsPostBack)
                {
                    BindDDL();
                    LoadData();
                }
            }

        }
        private void LoadData()
        {

            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                int id = Convert.ToInt32(Request.QueryString["PileID"]);
                PileInfo PI = db.PileInfo.SingleOrDefault(a => a.PileID == id);
                if (PI != null)
                {
                    this.txtName.Text = PI.PileName.ToString();
                    this.txtCode.Text = PI.PileCode.ToString();
                    this.ddlStorage.SelectedValue = PI.StorageID.ToString();
                    this.txtSize.Text = PI.PileSize.ToString();
                    this.txtRemark.Text = PI.Remark.ToString();
                }
                else
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('记录不存在! ');</script>");
                    Response.Redirect("PileManage.aspx");
                }
            }
        }
        private void InitControl()
        {
            this.txtName = (TextBox)GetControltByMaster("txtName");
            this.txtCode = (TextBox)GetControltByMaster("txtCode");
            this.ddlStorage = (DropDownList)GetControltByMaster("ddlStorage");
            this.txtSize = (TextBox)GetControltByMaster("txtSize");
            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");



            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
        }

        public void btnSave_Click(object sender, EventArgs e)
        {

            if (!string.IsNullOrEmpty(this.txtName.Text) && !string.IsNullOrEmpty(this.txtCode.Text))
            {
                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {


                    int id = Convert.ToInt32(Request.QueryString["PileID"]);
                    PileInfo PI = db.PileInfo.SingleOrDefault(a => a.PileID == id);
                    PI.PileName = this.txtName.Text.Trim();
                    PileInfo code = db.PileInfo.SingleOrDefault(u => u.PileCode == this.txtCode.Text.Trim());
                    if (code == null)
                    {
                        PI.PileCode = this.txtCode.Text.Trim();
                    }
                    else
                    {
                        if (PI.PileID == code.PileID)
                        {
                            PI.PileCode = this.txtCode.Text.Trim();
                        }
                        else
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('垛位代码重复！')</script>");
                            return;
                        }
                    }



                    if (this.ddlStorage.SelectedIndex == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择垛位所属仓库！')</script>");
                        return;
                    }
                    PI.StorageID = Convert.ToInt32(this.ddlStorage.SelectedValue);
                    PI.PileSize = this.txtSize.Text.Trim();
                    PI.Remark = this.txtRemark.Text.Trim();
                    db.SubmitChanges();
                    Response.Redirect("PileManage.aspx");


                }
            }
        }
        public void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("PileManage.aspx");
        }

        private void BindDDL()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = from a in db.StorageInfo
                           select new
                           {
                               Key = a.StorageName,
                               Value = a.StorageID
                           };

                this.ddlStorage.DataSource = temp;
                this.ddlStorage.DataTextField = "Key";
                this.ddlStorage.DataValueField = "Value";
                this.ddlStorage.DataBind();
                this.ddlStorage.Items.Insert(0, "--请选择--");
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
