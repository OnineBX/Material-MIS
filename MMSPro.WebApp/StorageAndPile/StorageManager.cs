using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Configuration;

namespace MMSPro.WebApp
{
    public class StorageManager : System.Web.UI.Page
    {
        SPGridView _gviewStorage;
        static string[] _colNames = { "StorageName:仓库名称", "StorageCode:仓库编码", "EmpName:库管员", "Remark:备注" };

        protected void Page_Load(object sender, EventArgs e)
        {
            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarStorageManager");
            //新建
            ToolBarButton btnAdd = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnAdd.ID = "AddNewRow";
            btnAdd.Text = "新建";
            btnAdd.ImageUrl = "/_layouts/images/newitem.gif";
            btnAdd.Click += new EventHandler(btnAdd_Click);
            tbarTop.Buttons.Controls.Add(btnAdd);

            //修改
            ToolBarButton btnEdit = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnEdit.ID = "EditRow";
            btnEdit.Text = "修改";
            btnEdit.ImageUrl = "/_layouts/images/edit.gif";
            btnEdit.Click += new EventHandler(btnEdit_Click);
            tbarTop.Buttons.Controls.Add(btnEdit);
            //删除

            ToolBarButton btnDelte = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnDelte.ID = "DeleteRow";
            btnDelte.Text = "删除";
            btnDelte.ImageUrl = "/_layouts/images/delete.gif";
            btnDelte.Click += new EventHandler(btnDelte_Click);
            StringBuilder sbScript = new StringBuilder();
            sbScript.Append("var aa= window.confirm('确认删除所选项?');");
            sbScript.Append("if(aa == false){");
            sbScript.Append("return false;}");
            btnDelte.OnClientClick = sbScript.ToString();
            tbarTop.Buttons.Controls.Add(btnDelte);

            ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnRefresh.ID = "btnRefresh";
            btnRefresh.Text = "刷新";
            btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
            btnRefresh.Padding = "0,5,0,0";
            btnRefresh.Click += new EventHandler(btnRefresh_Click);
            tbarTop.RightButtons.Controls.Add(btnRefresh);
            BindGridView();
        }
        protected void BindGridView()
        {
            try
            {
               this._gviewStorage = (SPGridView)this.Master.FindControl("PlaceHolderMain").FindControl("Panel1").FindControl("gviewStorage");
               this._gviewStorage.Columns.Clear();

                using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    BoundField bf;

                    //添加CheckBox选择列
                    TemplateField tfCheckbox = new TemplateField();
                    tfCheckbox.ItemTemplate = new CheckBoxTemplate("选择所有/取消", DataControlRowType.DataRow, "StorageID");
                    tfCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
                    this._gviewStorage.Columns.Add(tfCheckbox);

                    //SPMenuField colMenu = new SPMenuField();
                    //colMenu.HeaderText = "仓库名称";
                    //colMenu.TextFields = "StorageName";
                    //colMenu.MenuTemplateId = "actionMenu";
                    ////colMenu.NavigateUrlFields = "StorageID,StorageName"; //定义方式:"列名1,列名2..."
                    ////colMenu.NavigateUrlFormat = "StorageManager.aspx?curStorageID={0}&curStorageName={1}";
                    //colMenu.TokenNameAndValueFields = "curStorageID=StorageID";//定义方式:"别名1=列名1,别名2=列名2...."

                    //MenuTemplate menuItemCollection = new MenuTemplate();
                    //menuItemCollection.ID = "actionMenu";

                    //MenuItemTemplate editMenuItem = new MenuItemTemplate("编辑", "/_layouts/images/edit.gif");
                    //editMenuItem.ClientOnClickNavigateUrl = "StorageEdit.aspx?StorageID=%curStorageID%";
                    ////editMenuItem.ClientOnClickScript = "if(!window.confirm('确认删除所选项?')) return false;window.location.href='StorageEdit.aspx?StorageID=%curStorageID%'";//%curStorageID%代表别名curStorageID，而StorageID代表数据库的表中的列名
                    ////editMenuItem.ClientOnClickScript = "window.location.href='StorageEdit.aspx?StorageID=%curStorageID%&curTime=" + DateTime.Now.ToString() + "'";
                    //menuItemCollection.Controls.Add(editMenuItem);
                    //this.Controls.Add(menuItemCollection);
                    //this._gviewStorage.Columns.Add(colMenu);

                    for (int i = 0; i < StorageManager._colNames.Length; i++)
                    {
                        bf = new BoundField();
                        bf.HeaderText = _colNames[i].Split(':')[1];
                        bf.DataField = _colNames[i].Split(':')[0];
                        this._gviewStorage.Columns.Add(bf);
                    }

                    //HyperLinkField lf = new HyperLinkField();
                    //lf.HeaderText = "aaa";
                    ////lf.DataTextField = "";
                    //lf.Text = "删除";
                    //lf.DataNavigateUrlFields = new string[] { "StorageID", "StorageName" };
                    //lf.DataNavigateUrlFormatString = "StorageManager.axps?curStorageID={0}&curStorageName={1}";
                    //this._gviewStorage.Columns.Add(lf);

                    this._gviewStorage.DataSource = from u in dc.StorageInfo
                                                    join emp in dc.EmpInfo on u.EmpID equals emp.EmpID
                                                    select new
                                                    {
                                                        u.StorageID,
                                                        u.StorageName,
                                                        u.StorageCode,
                                                        emp.EmpName,
                                                        u.Remark
                                                    };
                    this._gviewStorage.PageIndexChanging += new GridViewPageEventHandler(_gviewStorage_PageIndexChanging);
                    this._gviewStorage.PageIndexChanged += new EventHandler(_gviewStorage_PageIndexChanged);
                    this._gviewStorage.AllowPaging = true;
                    this._gviewStorage.PageSize = 15;
                    this._gviewStorage.PagerSettings.NextPageText = "下一页";
                    this._gviewStorage.PagerSettings.LastPageText = "尾页";
                    this._gviewStorage.PagerSettings.PreviousPageText = "上一页";
                    this._gviewStorage.PagerSettings.FirstPageText = "首页";
                    this._gviewStorage.PagerSettings.Mode = PagerButtons.NextPreviousFirstLast;
                    this._gviewStorage.PagerStyle.Font.Size = new FontUnit(Unit.Pixel(5));
                    this._gviewStorage.PagerSettings.Position = PagerPosition.Bottom;
                    this._gviewStorage.PagerStyle.HorizontalAlign = HorizontalAlign.Right;
                    this._gviewStorage.PagerTemplate = null;

                    this._gviewStorage.DataBind();
                    Panel p1 = (Panel)GetControltByMaster("Panel1");
                    p1.Controls.Add(this._gviewStorage);

                }
            }
            catch (Exception ex)
            {
                System.Reflection.MethodBase mb = System.Reflection.MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }

        }

        void _gviewStorage_PageIndexChanged(object sender, EventArgs e)
        {
            
        }

        void _gviewStorage_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            //this._gviewStorage.PageIndex = e.NewPageIndex;
            //this._gviewStorage.DataBind();

            ((SPGridView)sender).PageIndex = e.NewPageIndex;
            ((SPGridView)sender).DataBind();
        }
        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        protected void btnDelte_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            List<CheckBox> listCheckBoxs = GetCheckedID();
            if (listCheckBoxs.Count > 0)
            {
                StorageInfo si;
                using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    foreach (var chb in listCheckBoxs)
                    {
                        si = dc.StorageInfo.SingleOrDefault(u => u.StorageID == int.Parse(chb.ToolTip));
                        if (si != null)
                        {
                            dc.StorageInfo.DeleteOnSubmit(si);

                        }
                    }
                    dc.SubmitChanges();
                }
                Page.RegisterStartupScript("DeleteOk", "<script>alert('删除成功!          ');window.location.href='StorageManager.aspx?dt=" + DateTime.Now.ToString("yyyyMMddhhmmss") + "'</" + "script>");
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择需要删除的记录!')</script>");
                //Page.RegisterClientScriptBlock("ShowMessage", "<script>alert('请选择需要删除的记录!')</script>");
            }

        }

        protected void btnEdit_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            List<CheckBox> listCheckBoxs = GetCheckedID();
            if (listCheckBoxs.Count == 1)
            {
                Response.Redirect("StorageEdit.aspx?StorageID=" + listCheckBoxs[0].ToolTip);
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择一条且仅能有一条记录进行编辑!')</script>");
            }
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            Response.Redirect("StorageCreate.aspx");
        }

        /// <summary>
        /// 返回选中的列表
        /// </summary>
        /// <returns>返回list构成的列表</returns>
        private List<CheckBox> GetCheckedID()
        {
            List<CheckBox> listCheckBoxs = new List<CheckBox>();

            foreach (GridViewRow row in this._gviewStorage.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    CheckBox ck = (CheckBox)row.Cells[0].FindControl("SMItem");
                    if (ck.Checked)
                    {
                        listCheckBoxs.Add(ck);
                    }
                }
            }
            return listCheckBoxs;
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
