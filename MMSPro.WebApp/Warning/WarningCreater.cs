using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
namespace MMSPro.WebApp
{
    public class WarningCreater : System.Web.UI.Page
    {
        //TextBox txtSupplierID;
        TextBox txtGentaojian;
        TextBox txtMetre;
        TextBox txtTon;
        Button btnSave;
        Button btnQuit;
        SPGridView spgviewMat;
        static string[] Tlist = new string[5];
        protected void Page_Load(object sender, EventArgs e)
        {
            this.spgviewMat = new SPGridView();
            this.spgviewMat.AutoGenerateColumns = false;
            Tlist[0] = "物料名称:MaterialName";
            Tlist[1] = "物料规格:SpecificationModel";
            Tlist[2] = "物料类别:MaterialChildTypeName";
            Tlist[3] = "财务编码:FinanceCode";
            Tlist[4] = "备注:Remark";
            BindGridView();
            #region 
            ////添加按钮到toolbar
            //ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarEmployee");
            ////新建
            //ToolBarButton tbarbtnAdd = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            //tbarbtnAdd.ID = "AddNewRow";
            //tbarbtnAdd.Text = "新建";
            //tbarbtnAdd.ImageUrl = "/_layouts/images/newitem.gif";
            //tbarbtnAdd.Click += new EventHandler(tbarbtnAdd_Click);
            //tbarTop.Buttons.Controls.Add(tbarbtnAdd);
            ////修改
            //ToolBarButton tbarbtnEdit = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            //tbarbtnEdit.ID = "EditRow";
            //tbarbtnEdit.Text = "修改";
            //tbarbtnEdit.ImageUrl = "/_layouts/images/edit.gif";
            //tbarbtnEdit.Click += new EventHandler(tbarbtnEdit_Click);
            //tbarTop.Buttons.Controls.Add(tbarbtnEdit);
            ////删除

            //ToolBarButton tbarbtnDelte = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            //tbarbtnDelte.ID = "DeleteRow";
            //tbarbtnDelte.Text = "删除";
            //tbarbtnDelte.ImageUrl = "/_layouts/images/delete.gif";
            //tbarbtnDelte.Click += new EventHandler(tbarbtnDelte_Click);
            //StringBuilder sbScript = new StringBuilder();
            //sbScript.Append("var aa= window.confirm('确认删除所选项?');");
            //sbScript.Append("if(aa == false){");
            //sbScript.Append("return false;}");
            //tbarbtnDelte.OnClientClick = sbScript.ToString();
            //tbarTop.Buttons.Controls.Add(tbarbtnDelte);

            //ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            //btnRefresh.ID = "btnRefresh";
            //btnRefresh.Text = "刷新";
            //btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
            //btnRefresh.Padding = "0,5,0,0";
            //btnRefresh.Click += new EventHandler(btnRefresh_Click);
            //tbarTop.RightButtons.Controls.Add(btnRefresh);
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello ')</script>");
            #endregion
            InvtControl();
          
        }

        private void InvtControl()
        {
            //this.txtSupplierID = (TextBox)GetControltByMaster("txtSupplierID");
            this.txtGentaojian = (TextBox)GetControltByMaster("txtGentaojian");
            this.txtMetre = (TextBox)GetControltByMaster("txtMetre");
            this.txtTon = (TextBox)GetControltByMaster("txtTon");

            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
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
                tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "MaterialID");
                tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
                this.spgviewMat.Columns.Add(tfieldCheckbox);
                foreach (var kvp in Tlist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.spgviewMat.Columns.Add(bfColumn);
                }
                this.spgviewMat.DataSource = from a in db.MaterialInfo
                                             join b in db.MaterialChildType on a.MaterialchildTypeID equals b.MaterialChildTypeID
                                             //join c in db.WarningList on a.MaterialID equals c.MaterialID
                                             where a.WarningList.Count<=0
                                             select new
                                             {
                                                 a.MaterialID,
                                                 a.MaterialName,
                                                 a.FinanceCode,
                                                 MaterialChildTypeName = b.MaterialMainType.MaterialMainTypeCode + b.MaterialChildTypeCode + "|" + b.MaterialMainType.MaterialMainTypeName + "-" + b.MaterialChildTypeName,
                                                 a.SpecificationModel,
                                                 a.Remark,
                                             };
                this.spgviewMat.DataBind();
                Panel p1 = (Panel)GetControltByMaster("Panel1");
                p1.Controls.Add(this.spgviewMat);

            }
        
        }

        void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("WarningManager.aspx");
        }

        void btnSave_Click(object sender, EventArgs e)
        {
            //值为0不报警
            decimal a, b, c;
            if (!decimal.TryParse(this.txtGentaojian.Text.Trim(), out a))
            {
                a = 0;
            }
            if (!decimal.TryParse(this.txtMetre.Text.Trim(), out b))
            {
                b = 0;
            }
            if (!decimal.TryParse(this.txtTon.Text.Trim(), out c))
            {
                c = 0;
            }
            if (a == 0 && b == 0 && c == 0)
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('至少填写一项预警值!')</script>");
                return;
            }
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var li = GetCheckedID();
                if (li.Count == 1)
                {
                    WarningList wl = new WarningList();
                    wl.MaterialID = int.Parse(li[0].ToolTip);
                    wl.QuantityGentaojian = a;
                    wl.QuantityMetre = b;
                    wl.QuantityTon = c;
                    db.WarningList.InsertOnSubmit(wl);
                    db.SubmitChanges();
                }
                else
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择一条,且仅一条物质作为预警设置对象!')</script>");
                }
            }
            Response.Redirect("WarningCreater.aspx");
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

