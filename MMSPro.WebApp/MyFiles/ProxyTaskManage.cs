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
    public class ProxyTaskManage:System.Web.UI.Page
    {
        MMSProDBDataContext db;
        SPGridView gv;
        string TaskType;
        DateTime timeServer;
        static string[] Titlelist = {
                                     "委托任务类型:ProxyTaskType",
                                     "委托人:Principal",
                                     "受托人:Fiduciary",
                                     "委托起始时间:StartTime",
                                     "委托起始时间:EndTime",
                                     "完成状态:TaskDispose",
                                     "委托创建时间:CreateTime",
                                     "备注:Remark",
                                     "ID:TaskProxyID"
                                    };



        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this.gv = new SPGridView();
                this.gv.AutoGenerateColumns = false;
                CheckTimeExpired();
                BindGridView();
                init();
            }
            catch(Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
           
        }
        /// <summary>
        /// 检查代理任务是否过期,
        /// 过期将修改状态
        /// </summary>
        private void CheckTimeExpired()
        {
            
            //检查时间是否过期
            using (MMSProDBDataContext data = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var SevTime = data.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                timeServer = SevTime.First();
                var temp = from a in data.TaskProxy
                           select new {a.TaskProxyID, a.EndTime};
                var list = temp.ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    //如果过期
                    if (list[i].EndTime < timeServer)
                    {
                        TaskProxy TP = data.TaskProxy.SingleOrDefault(u => u.TaskProxyID == list[i].TaskProxyID);
                        TP.TaskDispose = "已过期";
                        data.SubmitChanges();   
                    }
                }
            }
        }

        private void init()
        {
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

            //返回
            ToolBarButton tbarbtnBack = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnBack.ID = "backRow";
            tbarbtnBack.Text = "返回";
            tbarbtnBack.ImageUrl = "/_layouts/images/BACK.GIF";
            tbarbtnBack.Click += new EventHandler(tbarbtnBack_Click);
            tbarTop.Buttons.Controls.Add(tbarbtnBack);


            ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnRefresh.ID = "btnRefresh";
            btnRefresh.Text = "刷新";
            btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
            btnRefresh.Padding = "0,5,0,0";
            btnRefresh.Click += new EventHandler(btnRefresh_Click);
            tbarTop.RightButtons.Controls.Add(btnRefresh);
        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            
        }

        protected void tbarbtnAdd_Click(object sender, EventArgs e)
        {

            Response.Redirect("ProxyTaskCreate.aspx");

            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");

        }

        protected void tbarbtnEdit_Click(object sender, EventArgs e)
        {
            List<CheckBox> listString = GetCheckedID();
            if (listString.Count == 1)
            {
                Response.Redirect("ProxyTaskEdit.aspx?TaskProxyID=" + listString[0].ToolTip + "");
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择一条且仅能有一条记录进行编辑!')</script>");
            }
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {
            

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
                    TaskProxy TP;
                    using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        foreach (var li in listString)
                        {
                            TP = db.TaskProxy.SingleOrDefault(a => a.TaskProxyID == int.Parse(li.ToolTip));
                            if (TP != null)
                            {
                                db.TaskProxy.DeleteOnSubmit(TP);

                            }
                        }
                        db.SubmitChanges();
                    }
                    Response.Redirect("ProxyTaskManage.aspx");
                }
                else
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择需要删除的记录!')</script>");
                    return;
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_DELETEERROR));
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
                tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "TaskProxyID");
                tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
                this.gv.Columns.Insert(0, tfieldCheckbox);

                HyperLinkField hlTask = new HyperLinkField();
                hlTask.HeaderText = "任务详情";
               


                foreach (var kvp in Titlelist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.gv.Columns.Add(bfColumn);
                }

                this.gv.Columns.Insert(this.gv.Columns.Count-1, hlTask);
                this.gv.DataSource = from a in db.TaskProxy
 
                                     select new
                                     {
                                         a.TaskProxyID,
                                         ProxyTaskType = db.TaskProxyType.SingleOrDefault(u => u.TaskProxyTypeID == a.ProxyTaskType).TaskProxyTypeName,
                                         Principal = db.EmpInfo.SingleOrDefault(u => u.EmpID == a.ProxyPrincipal).EmpName,
                                         Fiduciary = db.EmpInfo.SingleOrDefault(u => u.EmpID == a.ProxyFiduciary).EmpName,
                                         a.StartTime,
                                         a.EndTime,
                                         a.TaskDispose,
                                         a.CreateTime,
                                         a.Remark
                                     };
                this.gv.RowDataBound += new GridViewRowEventHandler(gv_RowDataBound);
                this.gv.DataBind();
                this.gv.Columns[this.gv.Columns.Count - 1].Visible = false;
                Panel p1 = (Panel)GetControltByMaster("Panel1");
                p1.Controls.Add(this.gv);

            }

        }

        void gv_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                TaskType = e.Row.Cells[1].Text;
                e.Row.Cells[this.gv.Columns.Count - 2].Text = string.Format("<a href=\"javaScript:onClick=window.showModalDialog(encodeURI('../MyFiles/TaskProxyInfo.aspx?NoticeID={0}&&TaskType=" + TaskType + "'),'0','resizable:true;dialogWidth:800px;dialogHeight:600px')\">任务详情</a>", int.Parse(e.Row.Cells[this.gv.Columns.Count - 1].Text));
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
