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
using System.Reflection;

namespace MMSPro.WebApp
{
    public class CommitInManage:System.Web.UI.Page
    {
        MMSProDBDataContext db;
        SPGridView gv;
        static string[] Titlelist = {
                                      "备注:Remark",
                                      "创建时间:CreateTime",
                                      "ID:CommitInID"
                                      
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


        protected void tbarbtnAdd_Click(object sender, EventArgs e)
        {
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");
            Response.Redirect("CommitInCreate.aspx");
        }

        protected void tbarbtnEdit_Click(object sender, EventArgs e)
        {
            List<CheckBox> listString = GetCheckedID();
            if (listString.Count == 1)
            {
                Response.Redirect("CommitInEdit.aspx?CommitInID=" + listString[0].ToolTip);
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择一条且仅能有一条记录进行编辑!')</script>");
            }
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {
            Response.Redirect("CommitInManage.aspx");

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
            try
            {
                List<CheckBox> listString = GetCheckedID();
                if (listString.Count > 0)
                {



                    CommitIn SI;
                    int id;
                    using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {

                        foreach (var li in listString)
                        {
                            id = int.Parse(li.ToolTip);
                            if (checkInFlow(id) == false)
                            {
                                ClientScript.RegisterClientScriptBlock(typeof(string), "Messages", "<script>alert('委外入库任务已进入流程不能删除委外入库单.')</script>");
                                return;
                            }

                            //检查入库单下是否有明细物资
                            if (checkDetail(id) == false)
                            {
                                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('此入库单已存在物资,若要删除入库单,请先删除入库单下的物资!')</script>");
                                return;
                            }

                            SI = db.CommitIn.SingleOrDefault(a => a.CommitInID == id);
                            if (SI != null)
                            {
                                db.CommitIn.DeleteOnSubmit(SI);

                            }
                        }
                        db.SubmitChanges();
                    }
                    Response.Redirect("CommitInManage.aspx");
                }
                else
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择需要删除的记录!')</script>");
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));
            }

        }

        /// <summary>
        /// 检查入库单下是否有物资
        /// </summary>
        /// <returns></returns>
        private bool checkDetail(int cid)
        {


            using (MMSProDBDataContext data = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {


                var temp = from a in data.CommitInDetailed
                           where a.CommitInID == cid
                           select a;


                if (temp.ToList().Count > 0)
                {
                    return false;
                }


            }

            return true;
        }

        /// <summary>
        /// 检查任务是否进入流程
        /// </summary>
        /// <returns></returns>
        private bool checkInFlow(int cid)
        {


            using (MMSProDBDataContext data = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {


                var temp = from a in data.TaskStorageIn
                           where a.StorageInID == cid && a.StorageInType == "委外入库"
                           select a;


                if (temp.ToList().Count > 0)
                {
                    return false;
                }


            }

            return true;
        }

        /// <summary>
        /// 绑定Gridview
        /// </summary>
        private void BindGridView()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                BoundField bfColumn;

                foreach (var kvp in Titlelist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.gv.Columns.Add(bfColumn);
                }



                //添加选择列
                TemplateField tfieldCheckbox = new TemplateField();
                tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "CommitInID");
                tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
                this.gv.Columns.Insert(0,tfieldCheckbox);

                SPMenuField colMenu = new SPMenuField();
                colMenu.HeaderText = "交货通知单编号";
                colMenu.TextFields = "CommitInCode";
                colMenu.MenuTemplateId = "actionMenu";

                HyperLinkField hlTask = new HyperLinkField();
                hlTask.HeaderText = "任务详情";

                colMenu.NavigateUrlFields = "CommitInID"; //定义方式:"列名1,列名2..."
                colMenu.NavigateUrlFormat = "CommitInDetailedManage.aspx?CommitInID={0}";
                colMenu.TokenNameAndValueFields = "curCommitID=CommitInID";//定义方式:"别名1=列名1,别名2=列名2...."

                MenuTemplate menuItemCollection = new MenuTemplate();
                menuItemCollection.ID = "actionMenu";

                MenuItemTemplate createMenuItem = new MenuItemTemplate("送往质检", "/_layouts/images/newitem.gif");
                createMenuItem.ClientOnClickNavigateUrl = "../PublicPage/AuditDispatchCenter.aspx?Process=委外入库&CommitInID=%curCommitID%";     

                menuItemCollection.Controls.Add(createMenuItem);
                this.Controls.Add(menuItemCollection);
                this.gv.Columns.Insert(1,colMenu);
                this.gv.Columns.Insert(4,hlTask);

                
                this.gv.DataSource = from a in db.CommitIn
                                     select new
                                     {
                                         a.CommitInID,
                                         a.CommitInCode,
                                         a.Remark,
                                         a.CreateTime
                                     };
                this.gv.RowDataBound += new GridViewRowEventHandler(gv_RowDataBound);
                this.gv.DataBind();
                this.gv.Columns[5].Visible = false;

                Panel p1 = (Panel)GetControltByMaster("Panel1");
                p1.Controls.Add(this.gv);





            }

        }


        void gv_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Cells[4].Text = string.Format("<a href=\"javaScript:onClick=window.showModalDialog(encodeURI('../PublicPage/TaskHistoryInfo.aspx?NoticeID={0}&&TaskType=委外入库'),'0','resizable:true;dialogWidth:800px;dialogHeight:600px')\">任务详情</a>", int.Parse(e.Row.Cells[5].Text));
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
