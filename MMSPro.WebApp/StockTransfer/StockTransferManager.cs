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

   public class StockTransferManager : System.Web.UI.Page
    {
      
        MMSProDBDataContext db;
        SPGridView gv;
        static string[] Titlelist = {                                        
                                      "备注:Remark",
                                      "创建时间:CreateTime",
                                             "ID:StockTransferID"
                                    };
        protected void Page_Load(object sender, EventArgs e)
        {
            this.gv = new SPGridView();
            this.gv.AutoGenerateColumns = false;

            try
            {
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




                ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
                btnRefresh.ID = "btnRefresh";
                btnRefresh.Text = "刷新";
                btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
                btnRefresh.Padding = "0,5,0,0";
                btnRefresh.Click += new EventHandler(btnRefresh_Click);
                tbarTop.RightButtons.Controls.Add(btnRefresh);
            }

            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
        }
        //void tbarbtnView_Click(object sender, EventArgs e)
        //{
        //    List<CheckBox> listString = GetCheckedID();
        //    if (listString.Count == 1)
        //    {
        //        Response.Redirect("StorageInDetailedManage.aspx?StorageInID=" + listString[0].ToolTip);
        //    }
        //    else
        //    {
        //        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择一条且仅能有一条记录进行编辑!')</script>");
        //    }
        //}

        protected void tbarbtnAdd_Click(object sender, EventArgs e)
        {
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");
            Response.Redirect("StockTransferApply.aspx");
        }

        protected void tbarbtnEdit_Click(object sender, EventArgs e)
        {
            List<CheckBox> listString = GetCheckedID();
            if (listString.Count == 1)
            {
                Response.Redirect("StockTransferApply.aspx?StockTransferID=" + listString[0].ToolTip+"&&BackUrl="+HttpContext.Current.Request.Url.PathAndQuery);
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择一条且仅能有一条记录进行编辑!')</script>");
            }
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {
            Response.Redirect("StockTransferManager.aspx");

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
            int n = 0;
            List<CheckBox> listString = GetCheckedID();
            foreach (CheckBox cb in listString)
            {
                //3表删除                
                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    int intID = int.Parse(cb.ToolTip);
                    StockTransfer st = db.StockTransfer.SingleOrDefault(a => a.StockTransferID == intID);
                    var stds = db.StockTransferDetail.Where(a => a.StockTransferID == st.StockTransferID);
                    var stts = db.StockTransferTask.Where(a => a.StockTransferID == st.StockTransferID && a.TaskInType == "移库任务");
                    //判断stts的数量
                    if (stts.ToList().Count > 0)
                    {
                        
                        n++;
                        continue;
                    }
                    db.StockTransferDetail.DeleteAllOnSubmit(stds);
                    db.StockTransferTask.DeleteAllOnSubmit(stts);
                    db.StockTransfer.DeleteOnSubmit(st);
                    db.SubmitChanges();

                }
               
            }
            if(n>0)
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", "<script>alert('所选择的调拨单中,有"+n.ToString()+"条记录已进入移库流程的不能删除！')</script>");
            //Response.Redirect("StockTransferManager.aspx");
            Response.AddHeader("Refresh", "0");
            

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
                tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "StockTransferID");
                tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
                this.gv.Columns.Add(tfieldCheckbox);
                SPMenuField colMenu = new SPMenuField();
                colMenu.HeaderText = "单据编号";
                colMenu.TextFields = "StockTransferNum";
                colMenu.MenuTemplateId = "actionMenu";
                colMenu.NavigateUrlFields = "StockTransferID"; //定义方式:"列名1,列名2..."
                colMenu.NavigateUrlFormat = "StockTransferDetailsManage.aspx?StockTransferID={0}";
                colMenu.TokenNameAndValueFields = "cStockTransferID=StockTransferID";//定义方式:"别名1=列名1,别名2=列名2...."

                HyperLinkField hlTask = new HyperLinkField();
                hlTask.HeaderText = "任务详情";
                
                //hlTask.DataNavigateUrlFields = new string []{ "StockTransferID" };
                //hlTask.DataNavigateUrlFormatString = "javaScript:onClick=window.showModalDialog(encodeURI('../PublicPage/TaskHistoryInfo.aspx?NoticeID={0}&&TaskType=正常入库'),'0','resizable:true;dialogWidth:800px;dialogHeight:600px')";
                //hlTask.DataNavigateUrlFormatString = "www.google.com?a={0}";
                //hlTask.Text = "任务详情";
                //hlTask.Text = "<a href=\"javaScript:onClick=window.showModalDialog(encodeURI('../PublicPage/TaskHistoryInfo.aspx?NoticeID={0}&&TaskType=正常入库'),'0','resizable:true;dialogWidth:800px;dialogHeight:600px')\">任务详情</a>";
              
                MenuTemplate menuItemCollection = new MenuTemplate();
                menuItemCollection.ID = "actionMenu";

                MenuItemTemplate createMenuItem = new MenuItemTemplate("组长审批", "/_layouts/images/newitem.gif");
                createMenuItem.ClientOnClickNavigateUrl = "StockTransferCreateTask.aspx?StockTransferID=%cStockTransferID%&&TaskType=物资组长审核信息&&BackUrl=" + HttpContext.Current.Request.Url.PathAndQuery;
                //editMenuItem.ClientOnClickScript = "if(!window.confirm('确认删除所选项?')) return false;window.location.href='StorageEdit.aspx?StorageID=%curStorageID%'";//%curStorageID%代表别名curStorageID，而StorageID代表数据库的表中的列名
                //editMenuItem.ClientOnClickScript = "window.location.href='StorageEdit.aspx?StorageID=%curStorageID%&curTime=" + DateTime.Now.ToString() + "'";

                menuItemCollection.Controls.Add(createMenuItem);
                this.Controls.Add(menuItemCollection);
                this.gv.Columns.Add(colMenu);

                
                foreach (var kvp in Titlelist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.gv.Columns.Add(bfColumn);
                }
                this.gv.Columns.Insert(4,hlTask);
                this.gv.DataSource = from a in db.StockTransfer
                                     select a;

                this.gv.RowDataBound += new GridViewRowEventHandler(gv_RowDataBound);
                this.gv.DataBind();
                this.gv.Columns[5].Visible = false;

                Panel p1 = (Panel)GetControltByMaster("Panel1");
                p1.Controls.Add(this.gv);





            }

        }


        void gv_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)           {



                e.Row.Cells[4].Text = string.Format("<a href=\"javaScript:onClick=window.showModalDialog(encodeURI('../PublicPage/TaskHistoryInfo.aspx?NoticeID={0}&&TaskType=移库任务'),'0','resizable:true;dialogWidth:800px;dialogHeight:600px')\">任务详情</a>", int.Parse(e.Row.Cells[5].Text));
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
