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
    public class PileManage:System.Web.UI.Page
    {
        MMSProDBDataContext db;
        SPGridView gv;
        string[] Titlelist = new string[5];
        protected void Page_Load(object sender, EventArgs e)
        {
            this.gv = new SPGridView();
            this.gv.AutoGenerateColumns = false;
            Titlelist[0] = "垛位名称:PileName";
            Titlelist[1] = "垛位编号:PileCode";
            Titlelist[2] = "垛位面积:PileSize";
            Titlelist[3] = "垛位所属仓库:StorageName";
            Titlelist[4] = "备注:Remark";
            BindGridView();

            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");
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

        protected void tbarbtnAdd_Click(object sender, EventArgs e)
        {
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");
            Response.Redirect("PileCreate.aspx");
        }
        void tbarbtnImp_Click(object sender, EventArgs e)
        {
            Response.Redirect("UploadExcelPile.aspx");
        }
        protected void tbarbtnEdit_Click(object sender, EventArgs e)
        {
            List<CheckBox> listString = GetCheckedID();
            if (listString.Count == 1)
            {
                Response.Redirect("PileEdit.aspx?PileID=" + listString[0].ToolTip);
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择一条且仅能有一条记录进行编辑!')</script>");
            }
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {
            Response.Redirect("PileManage.aspx");

        }


        /// <summary>
        /// 返回选中的列表
        /// </summary>
        /// <returns>返回list构成的列表</returns>
        private List<CheckBox> GetCheckedID()
        {
            List<CheckBox> list = new List<CheckBox>();

            foreach (GridViewRow row in this.gv.Rows)
            {

                CheckBox ck = (CheckBox)row.Cells[0].FindControl("SMItem");
                if (ck.Checked)
                {
                    list.Add(ck);
                }
            }
            return list;
        }

        protected void tbarbtnDelte_Click(object sender, EventArgs e)
        {
            List<CheckBox> listString = GetCheckedID();
            if (listString.Count > 0)
            {
                PileInfo PI;
                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    foreach (var li in listString)
                    {
                        PI = db.PileInfo.SingleOrDefault(a => a.PileID == int.Parse(li.ToolTip));
                        if (PI != null)
                        {
                            db.PileInfo.DeleteOnSubmit(PI);

                        }
                    }
                    db.SubmitChanges();
                }
                Response.Redirect("PileManage.aspx");
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择需要删除的记录!')</script>");
            }

        }

        /// <summary>
        /// 绑定Gridview
        /// </summary>
        private void BindGridView()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                BoundField bfColumn;
                //添加选择列
                TemplateField tfieldCheckbox = new TemplateField();
                tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "PileID");
                tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
                this.gv.Columns.Add(tfieldCheckbox);
                foreach (var kvp in Titlelist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.gv.Columns.Add(bfColumn);
                }
                this.gv.DataSource = from a in db.PileInfo
                                     join b in db.StorageInfo on a.StorageID equals b.StorageID
                                     select new
                                     {
                                         a.PileID,
                                         a.PileName,
                                         a.PileCode,
                                         b.StorageName,
                                         a.PileSize,
                                         a.Remark

                                     };
                this.gv.DataBind();
                Panel p1 = (Panel)GetControltByMaster("Panel1");
                p1.Controls.Add(this.gv);

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
