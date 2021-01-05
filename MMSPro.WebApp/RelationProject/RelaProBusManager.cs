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
    public class RelaProBusManager : System.Web.UI.Page
    {
        SPGridView spgviewRelation;
        RadioButton rbtnBusiness;
        RadioButton rbtnProject;
        ListBox lboxLeft;
        static string[] Tlist = new string[3];        
        protected void Page_Load(object sender, EventArgs e)
        {
            this.spgviewRelation = new SPGridView();
            this.spgviewRelation.AutoGenerateColumns = false;
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarEmployee");
            //新建
            ToolBarButton tbarbtnAdd = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnAdd.ID = "AddNewRow";
            tbarbtnAdd.Text = "新建";
            tbarbtnAdd.ImageUrl = "/_layouts/images/newitem.gif";
            tbarbtnAdd.Click += new EventHandler(tbarbtnAdd_Click);
            tbarTop.Buttons.Controls.Add(tbarbtnAdd);
            //修改
            ToolBarButton tbarbtnEdit = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnEdit.ID = "EditRow";
            tbarbtnEdit.Text = "修改";
            tbarbtnEdit.ImageUrl = "/_layouts/images/edit.gif";
            tbarbtnEdit.Click += new EventHandler(tbarbtnEdit_Click);
            tbarTop.Buttons.Controls.Add(tbarbtnEdit);
            //删除

            ToolBarButton tbarbtnDelte = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnDelte.ID = "DeleteRow";
            tbarbtnDelte.Text = "删除";
            tbarbtnDelte.ImageUrl = "/_layouts/images/delete.gif";
            tbarbtnDelte.Click += new EventHandler(tbarbtnDelte_Click);
            StringBuilder sbScript = new StringBuilder();
            sbScript.Append("var aa= window.confirm('确认删除所选项?');");
            sbScript.Append("if(aa == false){");
            sbScript.Append("return false;}");
            tbarbtnDelte.OnClientClick = sbScript.ToString();
            tbarTop.Buttons.Controls.Add(tbarbtnDelte);

            ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnRefresh.ID = "btnRefresh";
            btnRefresh.Text = "刷新";
            btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
            btnRefresh.Padding = "0,5,0,0";
            btnRefresh.Click += new EventHandler(btnRefresh_Click);
            tbarTop.RightButtons.Controls.Add(btnRefresh);

            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgviewRelation);
            GetContrls();
            if (!IsPostBack)
            {
               
                BindList();
               
                //  this.spgviewMatMainType.RowCreated += new GridViewRowEventHandler(spgviewSupplierType_RowCreated);

                // Tlist[2] = "备注:Remark";
               // BindGridView();
                //添加按钮到toolbar
              
            }
        }
        /// <summary>
        /// 根据raidobutton选择绑定内容
        /// </summary>
        private void BindList()
        {
            if (this.rbtnBusiness.Checked)
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    this.lboxLeft.DataSource = from a in db.BusinessUnitInfo
                                               //where //a.BusinessUnitID// a.BusinessUnitName
                                               select a;
                    //this.lboxLeft.DataMember = "BusinessUnitID";
                    this.lboxLeft.DataTextField = "BusinessUnitName";
                    this.lboxLeft.DataValueField = "BusinessUnitID";
                    this.lboxLeft.DataBind();
                }
            }
            else
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    this.lboxLeft.DataSource = from a in db.ProjectInfo
                                               //where a.ProjectName//a.ProjectID 
                                               select a;
                    //this.lboxLeft.DataMember = "BusinessUnitID";
                    this.lboxLeft.DataTextField = "ProjectName";
                    this.lboxLeft.DataValueField = "ProjectID";
                    this.lboxLeft.DataBind();
                }
            }
        }

        private void GetContrls()
        {
            this.rbtnBusiness = (RadioButton)GetControltByMaster("rbtnBusiness");
            this.rbtnProject = (RadioButton)GetControltByMaster("rbtnProject");
            this.lboxLeft = (ListBox)GetControltByMaster("lboxLeft");
            this.rbtnBusiness.CheckedChanged += new EventHandler(rbtnBusiness_CheckedChanged);
            this.rbtnProject.CheckedChanged += new EventHandler(rbtnBusiness_CheckedChanged);
            this.lboxLeft.SelectedIndexChanged += new EventHandler(lboxLeft_SelectedIndexChanged);
            
        }

     

        void rbtnBusiness_CheckedChanged(object sender, EventArgs e)
        {
            
            BindList();
           // this.spgviewRelation.Columns.Clear();
        }

        void lboxLeft_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGridView();
        }

        //void spgviewSupplierType_RowCreated(object sender, GridViewRowEventArgs e)
        //{
        //  //  e.Row.Attributes.Add("onclick", "SmtGridSelectItem(this)");
        //}

        void btnRefresh_Click(object sender, EventArgs e)
        {
            Response.Redirect("RelaProBusManager.aspx");

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
        protected void tbarbtnDelte_Click(object sender, EventArgs e)
        {
            List<CheckBox> listString = GetCheckedID();
            if (listString.Count > 0)
            {
                RelationProjectBusiness di;
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    
                    foreach (var li in listString)
                    {
                        if (rbtnBusiness.Checked)
                        {
                            di = db.RelationProjectBusiness.SingleOrDefault(a => a.ProjectID == int.Parse(li.ToolTip) && a.BusinessUnitID == int.Parse(this.lboxLeft.SelectedValue));
                        }
                        else
                        {
                            di = db.RelationProjectBusiness.SingleOrDefault(a => a.BusinessUnitID == int.Parse(li.ToolTip) && a.ProjectID == int.Parse(this.lboxLeft.SelectedValue)); ;
                        }
                        if (di != null)
                        {
                            db.RelationProjectBusiness.DeleteOnSubmit(di);

                        }
                    }
                    db.SubmitChanges();
                }
                Response.Redirect("RelaProBusManager.aspx");
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择需要删除的记录!')</script>");
            }

        }

        protected void tbarbtnEdit_Click(object sender, EventArgs e)
        {
            List<CheckBox> listString = GetCheckedID();
            if (listString.Count == 1)
            {
                Response.Redirect("ProjectEditer.aspx?ProjectID=" + listString[0].ToolTip);
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择一条且仅能有一条记录进行编辑!')</script>");
            }
        }

        protected void tbarbtnAdd_Click(object sender, EventArgs e)
        {
            if (this.lboxLeft.SelectedItem == null)
            {
                return;
            }
            string strTemp = "?";
            if(this.rbtnBusiness.Checked)
            {
                strTemp += "BusinessUnitID=" + this.lboxLeft.SelectedValue;
            }
            else
            {
                strTemp += "ProjectID=" + this.lboxLeft.SelectedValue;
            }
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");
            Response.Redirect("RelaProBusCreater.aspx"+strTemp);
        }

        /// <summary>
        /// 绑定Gridview
        /// </summary>
        private void BindGridView()
        {
            this.spgviewRelation.Columns.Clear();
            if (this.rbtnBusiness.Checked)
            {
                Tlist[0] = "项目名称:ProjectName";
                Tlist[1] = "项目编码:ProjectCode";
                Tlist[2] = "井性:ProjectProperty";
            }
            else
            {
                Tlist[0] = "施工方名称:BusinessUnitName";
                Tlist[1] = "施工方编码:BusinessUnitCode";
                Tlist[2] = "施工方所属单位:BusinessUnitTypeName";

            }
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                BoundField bfColumn;
                //添加选择列
                TemplateField tfieldCheckbox = new TemplateField();
                if (this.rbtnBusiness.Checked)
                {
                    tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "ProjectID");
                }
                else
                {
                    tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "BusinessUnitID");
                }
                tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
                this.spgviewRelation.Columns.Add(tfieldCheckbox);
                foreach (var kvp in Tlist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.spgviewRelation.Columns.Add(bfColumn);
                }
                if (this.rbtnBusiness.Checked)
                {
                    this.spgviewRelation.DataSource = from a in db.RelationProjectBusiness
                                                      where  a.BusinessUnitID == int.Parse( this.lboxLeft.SelectedValue)
                                                      select new {
                                                          a.ProjectID,
                                                          a.ProjectInfo.ProjectCode,
                                                          a.ProjectInfo.ProjectName,
                                                          a.ProjectInfo.ProjectProperty
                                                      };
                }
                else
                {
                    this.spgviewRelation.DataSource = from a in db.RelationProjectBusiness
                                                      where a.ProjectID == int.Parse(this.lboxLeft.SelectedValue)
                                                      select new
                                                      {
                                                          a.BusinessUnitID,
                                                          a.BusinessUnitInfo.BusinessUnitName,
                                                          a.BusinessUnitInfo.BusinessUnitCode,
                                                          a.BusinessUnitInfo.BusinessUnitType.BusinessUnitTypeName
                                                      };    
                }

                this.spgviewRelation.DataBind();
               
                //SPMenuField spm = new SPMenuField();
                
            }
      
        }
        /// <summary>
        /// 返回选中的列表
        /// </summary>
        /// <returns>返回list构成的列表</returns>
        private List<CheckBox> GetCheckedID()
        {
            List<CheckBox> list = new List<CheckBox>();

            foreach (GridViewRow row in this.spgviewRelation.Rows)
            {

                CheckBox ck = (CheckBox)row.Cells[0].FindControl("SMItem");
                if (ck.Checked)
                {
                    list.Add(ck);
                }
            }
            return list;
        }

    }
}
