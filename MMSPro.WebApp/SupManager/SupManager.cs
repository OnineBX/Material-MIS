﻿using System;
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
    public class SupManager : System.Web.UI.Page
    {
        SPGridView spgviewSupplier;        
        static string[] Tlist = new string[8];        
        protected void Page_Load(object sender, EventArgs e)
        {
            this.spgviewSupplier = new SPGridView();
            this.spgviewSupplier.AutoGenerateColumns = false;
            Tlist[0] = "供应商名称:SupplierName";
            Tlist[1] = "供应商名称编码:SupplierCode";
            Tlist[2] = "供应商类别:SupplierTypeName";
            Tlist[3] = "联系地址1:SupplierAddress1";
            Tlist[4] = "联系地址2:SupplierAddress2";
            Tlist[5] = "联系电话:SupplierPhone";
            Tlist[6] = "负责人:InCharge";
            Tlist[7] = "备注:Remark";
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
            ////导入
            ToolBarButton tbarbtnImp = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnImp.ID = "ImpExcel";
            tbarbtnImp.Text = "导入";
            tbarbtnImp.ImageUrl = "/_layouts/images/edit.gif";
            tbarbtnImp.Click += new EventHandler(tbarbtnImp_Click);
            tbarTop.Buttons.Controls.Add(tbarbtnImp);

            ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnRefresh.ID = "btnRefresh";
            btnRefresh.Text = "刷新";
            btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
            btnRefresh.Padding = "0,5,0,0";
            btnRefresh.Click += new EventHandler(btnRefresh_Click);
            tbarTop.RightButtons.Controls.Add(btnRefresh);
        }
        void tbarbtnImp_Click(object sender, EventArgs e)
        {
            Response.Redirect("UploadExcelSup.aspx");
        }
        void btnRefresh_Click(object sender, EventArgs e)
        {
            Response.Redirect("SupManager.aspx");
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
                SupplierInfo di;
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    foreach (var li in listString)
                    {
                        di = db.SupplierInfo.SingleOrDefault(a => a.SupplierID == int.Parse(li.ToolTip));
                        if (di != null)
                        {
                            db.SupplierInfo.DeleteOnSubmit(di);

                        }
                    }
                    db.SubmitChanges();
                }
                Response.Redirect("SupManager.aspx");
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
                Response.Redirect("SupEditer.aspx?SupplierID=" + listString[0].ToolTip);
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择一条且仅能有一条记录进行编辑!')</script>");
            }
        }

        protected void tbarbtnAdd_Click(object sender, EventArgs e)
        {
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");
            Response.Redirect("SupCreater.aspx");
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
                tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "SupplierID");
                tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
                this.spgviewSupplier.Columns.Add(tfieldCheckbox);
                foreach (var kvp in Tlist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.spgviewSupplier.Columns.Add(bfColumn);
                }
                this.spgviewSupplier.DataSource = from a in db.SupplierInfo
                                                  join b in db.SupplierType on a.SupplierTypeID equals b.SupplierTypeID
                                                  select new
                                                  {
                                                      a.SupplierID,
                                                      a.SupplierName,
                                                      a.SupplierCode,
                                                      b.SupplierTypeName,
                                                      a.SupplierAddress1,
                                                      a.SupplierAddress2,
                                                      a.SupplierPhone,
                                                      a.InCharge,
                                                      a.Remark,
                                                  };
                this.spgviewSupplier.DataBind();
                Panel p1 = (Panel)GetControltByMaster("Panel1");
                p1.Controls.Add(this.spgviewSupplier);

            }
      
        }
        /// <summary>
        /// 返回选中的列表
        /// </summary>
        /// <returns>返回list构成的列表</returns>
        private List<CheckBox> GetCheckedID()
        {
            List<CheckBox> list = new List<CheckBox>();

            foreach (GridViewRow row in this.spgviewSupplier.Rows)
            {

                CheckBox ck = (CheckBox)row.Cells[0].FindControl("SMItem");
                if (ck.Checked)
                {
                    list.Add(ck);
                }
            }
            return list;
        }
        private void done()
        {

        }
    }
}
