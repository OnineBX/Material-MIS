/*------------------------------------------------------------------------------
 * Unit Name：NormalOutProduceAuditInfo.cs
 * Description: 正常出库--生产组长审核的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-10-28
 * ----------------------------------------------------------------------------*/
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
    public class NormalOutProduceAuditInfo:System.Web.UI.Page
    {
        private int _noticeid;
        private int _taskid;

        private SPGridView spgvMaterial;
        private Button btnOK;

        private Label lblProduceOpinion, lblProduceResult, lblProduceAuditTitle;

        private static string[] ShowTlist =  { 
                                                 "财务编码:FinanceCode",                                            
                                                 "物资名称:MaterialName",
                                                 "规格型号:SpecificationModel",
                                                 "库存数量(根/台/套/件):StocksGentaojian",
                                                 "库存数量(米):StocksMetre",
                                                 "库存数量(吨):StocksTon",
                                                 "StorageOutDetailsID:StorageOutDetailsID"
                                             };


        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                _taskid = Convert.ToInt32(Request.QueryString["TaskID"]);

                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    StorageOutTask sot = db.StorageOutTask.SingleOrDefault(u => u.TaskID == this._taskid);
                    if (sot.TaskState.Equals("已完成"))
                    {
                        Response.Redirect(string.Format("NormalOutProduceDetailsMessage.aspx?TaskID={0}",_taskid),false);
                        return;
                    }
                    _noticeid = sot.NoticeID;
                }

                InitializeCustomControls();
                BindDataToCustomControls();
                ShowCustomControls();
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }           

        }

        #region 初始化和绑定函数

        private void InitBar()
        {
            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");            

            //返回
            ToolBarButton tbarbtnBack = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnBack.ID = "btnBack";
            tbarbtnBack.Text = "返回";
            tbarbtnBack.ImageUrl = "/_layouts/images/BACK.GIF";
            tbarbtnBack.Click += new EventHandler(tbarbtnBack_Click);
            tbarTop.Buttons.Controls.Add(tbarbtnBack);

            //新建
            ToolBarButton tbarbtnAdd = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnAdd.ID = "btnAdd";
            tbarbtnAdd.Text = "新建";
            tbarbtnAdd.ImageUrl = "/_layouts/images/newitem.gif";
            tbarbtnAdd.Click += new EventHandler(tbarbtnAdd_Click);
            tbarTop.Buttons.Controls.Add(tbarbtnAdd);


            //删除
            ToolBarButton tbarbtnDelete = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnDelete.ID = "btnDelete";
            tbarbtnDelete.Text = "删除";
            tbarbtnDelete.ImageUrl = "/_layouts/images/delete.gif";
            tbarbtnDelete.Click += new EventHandler(tbarbtnDelete_Click);
            StringBuilder sbScript = new StringBuilder();
            sbScript.Append("var aa= window.confirm('确认删除所选项?');");
            sbScript.Append("if(aa == false){");
            sbScript.Append("return false;}");
            tbarbtnDelete.OnClientClick = sbScript.ToString();
            tbarTop.Buttons.Controls.Add(tbarbtnDelete);

            ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnRefresh.ID = "btnRefresh";
            btnRefresh.Text = "刷新";
            btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
            btnRefresh.Padding = "0,5,0,0";
            btnRefresh.Click += new EventHandler(btnRefresh_Click);
            tbarTop.RightButtons.Controls.Add(btnRefresh);
        }        

        private void InitializeCustomControls()
        {
            InitBar();

            this.spgvMaterial = new SPGridView();
            this.spgvMaterial.AutoGenerateColumns = false;
            this.spgvMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");
            
            BoundField bfColumn;

            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvMaterial.Columns.Add(bfColumn);
            }

            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);

            lblProduceAuditTitle = (Label)GetControltByMaster("lblProduceAuditTitle");
            lblProduceResult = (Label)GetControltByMaster("lblProduceResult");
            lblProduceOpinion = (Label)GetControltByMaster("lblProduceOpinion");

            //添加选择列
            TemplateField tfSelect = new TemplateField();
            tfSelect.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow);
            tfSelect.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
            this.spgvMaterial.Columns.Insert(0, tfSelect);

            //加入调拨数量(根/台/套/件)列
            TemplateField tfGentaojian = new TemplateField();
            tfGentaojian.HeaderText = "调拨数量(根/台/套/件)";
            tfGentaojian.ItemTemplate = new TextBoxTemplate("Gentaojian", "Gentaojian", "^(-?\\d+)(\\.\\d+)?$", "0", 80);
            this.spgvMaterial.Columns.Insert(5, tfGentaojian);

            //加入调拨数量(米)列
            TemplateField tfMetre = new TemplateField();
            tfMetre.HeaderText = "调拨数量(米)";
            tfMetre.ItemTemplate = new TextBoxTemplate("Metre", "Metre", "^(-?\\d+)(\\.\\d+)?$", "0", 80);
            this.spgvMaterial.Columns.Insert(7, tfMetre);

            //加入调拨数量(根/台/套/件)列
            TemplateField tfTon = new TemplateField();
            tfTon.HeaderText = "调拨数量(吨)";
            tfTon.ItemTemplate = new TextBoxTemplate("Ton", "Ton", "^(-?\\d+)(\\.\\d+)?$", "0", 80);
            this.spgvMaterial.Columns.Insert(9, tfTon);

            //加入备注列
            TemplateField tfRemark = new TemplateField();
            tfRemark.HeaderText = "备注";
            tfRemark.ItemTemplate = new TextBoxTemplate("Remark", DataControlRowType.DataRow, "Remark");
            this.spgvMaterial.Columns.Insert(10, tfRemark);

        }
        
        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息
                StorageOutProduceAudit sopa = db.StorageOutProduceAudit.SingleOrDefault( u =>u.TaskID.Equals(GetPreviousTaskID(0,_taskid)));

                (GetControltByMaster("lblConstructor") as Label).Text = sopa.StorageOutNotice.BusinessUnitInfo1.BusinessUnitName;
                (GetControltByMaster("lblProprietor") as Label).Text = sopa.StorageOutNotice.BusinessUnitInfo.BusinessUnitName;
                (GetControltByMaster("lblProject") as Label).Text = string.Format("{0}({1}阶段)", sopa.StorageOutNotice.ProjectInfo.ProjectName, sopa.StorageOutNotice.ProjectStage);
                (GetControltByMaster("lblNoticeCode") as Label).Text = sopa.StorageOutNotice.StorageOutNoticeCode;
                (GetControltByMaster("lblProperty") as Label).Text = sopa.StorageOutNotice.ProjectInfo.ProjectProperty;
                (GetControltByMaster("lblDate") as Label).Text = sopa.StorageOutNotice.CreateTime.ToLongDateString();   

                //初始化审核列表
                this.spgvMaterial.DataSource = from a in db.StorageOutDetails                                               
                                               where a.StorageOutNoticeID == _noticeid
                                               select new
                                               {
                                                   a.MaterialInfo.FinanceCode,
                                                   a.MaterialInfo.MaterialName,
                                                   a.MaterialInfo.SpecificationModel,
                                                   StocksGenTaojian = (from c in db.StorageStocks
                                                                       where c.MaterialID == a.MaterialID
                                                                       select c).Sum(u => u.StocksGenTaojian),
                                                   StocksMetre = (from c in db.StorageStocks
                                                                  where c.MaterialID == a.MaterialID
                                                                  select c).Sum(u => u.StocksMetre),
                                                   StocksTon = (from c in db.StorageStocks
                                                                where c.MaterialID == a.MaterialID
                                                                select c).Sum(u => u.StocksTon),
                                                   a.Gentaojian,
                                                   a.Metre,
                                                   a.Ton,                                                   
                                                   a.Remark,
                                                   a.StorageOutDetailsID
                                               };
                this.spgvMaterial.DataBind();

                //初始化生产组长审核信息                
                (GetControltByMaster("txtProduceOpinion") as TextBox).Text = sopa.AuditOpinion;
                (GetControltByMaster("lblProduceResult") as Label).Text = sopa.AuditStatus;
                (GetControltByMaster("lblProduceChief") as Label).Text = sopa.EmpInfo.EmpName;

            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);
            this.spgvMaterial.Columns[11].Visible = false;           
        }

        #endregion

        #region 控件事件方法

        void tbarbtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    int ichecked = 0;
                    StorageOutDetails sod;
                    CheckBox chb;
                    foreach (GridViewRow gvr in spgvMaterial.Rows)
                    {
                        chb = (CheckBox)gvr.Cells[0].Controls[0];
                        if (!chb.Checked)
                            continue;
                        sod = db.StorageOutDetails.SingleOrDefault(a => a.StorageOutDetailsID == int.Parse(gvr.Cells[11].Text));
                        db.StorageOutDetails.DeleteOnSubmit(sod);
                        ichecked++;

                    }
                    if (ichecked != 0)
                        db.SubmitChanges();
                    else
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择需要删除的记录!')</script>");
                }
                Response.AddHeader("Refresh", "0");
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_DELETEERROR));
            }
        }

        void tbarbtnAdd_Click(object sender, EventArgs e)
        {
            string strBackUrl = string.Format("NormalOutProduceAuditInfo.aspx?TaskID={0}", _taskid);
            Response.Redirect(string.Format("SelectStorageOutDetails.aspx?NoticeID={0}&BackUrl={1}", _noticeid,HttpUtility.UrlEncode(strBackUrl)), false);
        }   


        void btnRefresh_Click(object sender, EventArgs e)
        {
            
        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("../../default-old.aspx", false);
        }       

        void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    StorageOutDetails sod;
                    foreach (GridViewRow gvr in this.spgvMaterial.Rows)
                    {
                        sod = db.StorageOutDetails.SingleOrDefault(u => u.StorageOutDetailsID == Convert.ToInt32(gvr.Cells[11].Text));
                        sod.Gentaojian = Convert.ToDecimal((gvr.Cells[5].Controls[0] as TextBox).Text.Trim());
                        sod.Metre = Convert.ToDecimal((gvr.Cells[7].Controls[0] as TextBox).Text.Trim());
                        sod.Ton = Convert.ToDecimal((gvr.Cells[9].Controls[0] as TextBox).Text.Trim());
                        sod.Remark = (gvr.Cells[10].Controls[0] as TextBox).Text.Trim();
                        sod.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();

                        db.SubmitChanges();
                    }
                }
                Response.Redirect(string.Format("NormalOutProduceDetailsMessage.aspx?TaskID={0}",_taskid), false);

            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_UPDATEERROR));
            } 

        }


        #endregion

        #region 辅助函数

        private int GetPreviousTaskID(int step, int taskid)
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                int tid = db.StorageOutTask.SingleOrDefault(u => u.TaskID == taskid).PreviousTaskID;
                if (step == 0)
                    return tid;
                return GetPreviousTaskID(--step, tid);
            }
        }

        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }
        
        #endregion
    }
}
