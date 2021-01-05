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
    public class WarningManager : System.Web.UI.Page
    {
        SPGridView spgviewMat;
        static string[] Tlist = new string[6];
        protected void Page_Load(object sender, EventArgs e)
        {
            this.spgviewMat = new SPGridView();
            this.spgviewMat.AutoGenerateColumns = false;
            Tlist[0] = "物料名称:MaterialName";
            Tlist[1] = "物料规格:SpecificationModel";
            Tlist[2] = "物料类别:MaterialChildTypeName";
            Tlist[3] = "根/套/件:QuantityGentaojian";
            Tlist[4] = "米:QuantityMetre";
            Tlist[5] = "吨:QuantityTon";
     
            BindGridView();
            //添加按钮到toolbar
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
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {
            Response.Redirect("WarningManager.aspx");

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
                WarningList di;
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    foreach (var li in listString)
                    {
                        di = db.WarningList.SingleOrDefault(a => a.WarningID == int.Parse(li.ToolTip));
                        if (di != null)
                        {
                            db.WarningList.DeleteOnSubmit(di);

                        }
                    }
                    db.SubmitChanges();
                }
                Response.Redirect("WarningManager.aspx");
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
                Response.Redirect("WarningEditer.aspx?WarningID=" + listString[0].ToolTip);
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择一条且仅能有一条记录进行编辑!')</script>");
            }
        }

        protected void tbarbtnAdd_Click(object sender, EventArgs e)
        {
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");
            Response.Redirect("WarningCreater.aspx");
        }

        /// <summary>
        /// 绑定Gridview
        /// </summary>
        private void BindGridView()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                BoundField bfColumn;
                //添加选择列
                TemplateField tfieldCheckbox = new TemplateField();
                tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "WarningID");
                tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
                this.spgviewMat.Columns.Add(tfieldCheckbox);
                foreach (var kvp in Tlist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.spgviewMat.Columns.Add(bfColumn);
                }
                this.spgviewMat.DataSource = from a in db.WarningList                                             
                                             select new
                                             {
                                                 a.WarningID,
                                                 a.QuantityGentaojian,
                                                 a.QuantityMetre,
                                                 a.QuantityTon,
                                                 a.MaterialInfo.FinanceCode,
                                                 a.MaterialInfo.SpecificationModel,
                                                 a.MaterialInfo.MaterialName,
                                                 MaterialChildTypeName = a.MaterialInfo.MaterialChildType.MaterialMainType.MaterialMainTypeCode + a.MaterialInfo.MaterialChildType.MaterialChildTypeCode + "|" + a.MaterialInfo.MaterialChildType.MaterialMainType.MaterialMainTypeName + "-" + a.MaterialInfo.MaterialChildType.MaterialChildTypeName,
                                                
                                             };
                this.spgviewMat.DataBind();
                Panel p1 = (Panel)GetControltByMaster("Panel1");
                p1.Controls.Add(this.spgviewMat);

            }

        }
        /// <summary>
        /// 返回选中的列表
        /// </summary>
        /// <returns>返回list构成的列表</returns>
        private List<CheckBox> GetCheckedID()
        {
            List<CheckBox> list = new List<CheckBox>();

            foreach (GridViewRow row in this.spgviewMat.Rows)
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
