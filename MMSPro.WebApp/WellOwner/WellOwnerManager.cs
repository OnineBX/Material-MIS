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
    public class WellOwnerManager : System.Web.UI.Page
    {
        //SPGridView spgviewWellOwner;        
        //static string[] Tlist = new string[2];        
        //protected void Page_Load(object sender, EventArgs e)
        //{
        //    this.spgviewWellOwner = new SPGridView();
        //    this.spgviewWellOwner.AutoGenerateColumns = false;
        //  //  this.spgviewMatMainType.RowCreated += new GridViewRowEventHandler(spgviewSupplierType_RowCreated);
        //    Tlist[0] = "业主单位名称:WellOwnerName";
        //    Tlist[1] = "业主单位编码:WellOwnerCode";
        //   // Tlist[2] = "备注:Remark";
        //    BindGridView();
        //    //添加按钮到toolbar
        //    ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarEmployee");
        //    //新建
        //    ToolBarButton tbarbtnAdd = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
        //    tbarbtnAdd.ID = "AddNewRow";
        //    tbarbtnAdd.Text = "新建";
        //    tbarbtnAdd.ImageUrl = "/_layouts/images/newitem.gif";
        //    tbarbtnAdd.Click += new EventHandler(tbarbtnAdd_Click);
        //    tbarTop.Buttons.Controls.Add(tbarbtnAdd);
        //    //修改
        //    ToolBarButton tbarbtnEdit = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
        //    tbarbtnEdit.ID = "EditRow";
        //    tbarbtnEdit.Text = "修改";
        //    tbarbtnEdit.ImageUrl = "/_layouts/images/edit.gif";
        //    tbarbtnEdit.Click += new EventHandler(tbarbtnEdit_Click);
        //    tbarTop.Buttons.Controls.Add(tbarbtnEdit);
        //    //删除

        //    ToolBarButton tbarbtnDelte = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
        //    tbarbtnDelte.ID = "DeleteRow";
        //    tbarbtnDelte.Text = "删除";
        //    tbarbtnDelte.ImageUrl = "/_layouts/images/delete.gif";
        //    tbarbtnDelte.Click += new EventHandler(tbarbtnDelte_Click);
        //    StringBuilder sbScript = new StringBuilder();
        //    sbScript.Append("var aa= window.confirm('确认删除所选项?');");
        //    sbScript.Append("if(aa == false){");
        //    sbScript.Append("return false;}");
        //    tbarbtnDelte.OnClientClick = sbScript.ToString();
        //    tbarTop.Buttons.Controls.Add(tbarbtnDelte);

        //    ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
        //    btnRefresh.ID = "btnRefresh";
        //    btnRefresh.Text = "刷新";
        //    btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
        //    btnRefresh.Padding = "0,5,0,0";
        //    btnRefresh.Click += new EventHandler(btnRefresh_Click);
        //    tbarTop.RightButtons.Controls.Add(btnRefresh);
        //}

        ////void spgviewSupplierType_RowCreated(object sender, GridViewRowEventArgs e)
        ////{
        ////  //  e.Row.Attributes.Add("onclick", "SmtGridSelectItem(this)");
        ////}

        //void btnRefresh_Click(object sender, EventArgs e)
        //{
        //    Response.Redirect("WellOwnerManager.aspx");

        //}
        ///// <summary>
        ///// 获取控件
        ///// </summary>
        ///// <param name="controlName">控件的ID名称</param>
        ///// <returns>返回Control，需要强制类型转换为对应控件</returns>
        //protected Control GetControltByMaster(string controlName)
        //{
        //    return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        //}
        //protected void tbarbtnDelte_Click(object sender, EventArgs e)
        //{
        //    List<CheckBox> listString = GetCheckedID();
        //    if (listString.Count > 0)
        //    {
        //         WellOwner di;
        //        using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
        //        {
                    
        //            foreach (var li in listString)
        //            {
                        
                        
        //                //if(db.MaterialChildType.FirstOrDefault(a=>a.MaterialMainTypeID == int.Parse(li.ToolTip) )!= null)
        //                //{
        //                //    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('所删除的记录包含有供应商,请先移除所有供应商后删除类别!')</script>");
        //                //    return;
        //                //}
        //                di = db.WellOwner.SingleOrDefault(a => a.WellOwnerID == int.Parse(li.ToolTip));
        //                if (di != null)
        //                {
        //                    db.WellOwner.DeleteOnSubmit(di);

        //                }
        //            }
        //            db.SubmitChanges();
        //        }
        //        Response.Redirect("WellOwnerManager.aspx");
        //    }
        //    else
        //    {
        //        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择需要删除的记录!')</script>");
        //    }

        //}

        //protected void tbarbtnEdit_Click(object sender, EventArgs e)
        //{
        //    List<CheckBox> listString = GetCheckedID();
        //    if (listString.Count == 1)
        //    {
        //        Response.Redirect("WellOwnerEditer.aspx?WellOwnerID=" + listString[0].ToolTip);
        //    }
        //    else
        //    {
        //        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择一条且仅能有一条记录进行编辑!')</script>");
        //    }
        //}

        //protected void tbarbtnAdd_Click(object sender, EventArgs e)
        //{
        //    //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");
        //    Response.Redirect("WellOwnerCreater.aspx");
        //}

        ///// <summary>
        ///// 绑定Gridview
        ///// </summary>
        //private void BindGridView()
        //{
        //    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
        //    {
        //        BoundField bfColumn;
        //        //添加选择列
        //        TemplateField tfieldCheckbox = new TemplateField();
        //        tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "WellOwnerID");
        //        tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
        //        this.spgviewWellOwner.Columns.Add(tfieldCheckbox);
        //        foreach (var kvp in Tlist)
        //        {
        //            bfColumn = new BoundField();
        //            bfColumn.HeaderText = kvp.Split(':')[0];
        //            bfColumn.DataField = kvp.Split(':')[1];
        //            this.spgviewWellOwner.Columns.Add(bfColumn);
        //        }
        //        this.spgviewWellOwner.DataSource = from a in db.WellOwner
        //                                              select a;
        //        this.spgviewWellOwner.DataBind();
        //        Panel p1 = (Panel)GetControltByMaster("Panel1");
        //        p1.Controls.Add(this.spgviewWellOwner);

        //    }
      
        //}
        ///// <summary>
        ///// 返回选中的列表
        ///// </summary>
        ///// <returns>返回list构成的列表</returns>
        //private List<CheckBox> GetCheckedID()
        //{
        //    List<CheckBox> list = new List<CheckBox>();

        //    foreach (GridViewRow row in this.spgviewWellOwner.Rows)
        //    {

        //        CheckBox ck = (CheckBox)row.Cells[0].FindControl("SMItem");
        //        if (ck.Checked)
        //        {
        //            list.Add(ck);
        //        }
        //    }
        //    return list;
        //}

    }
}
