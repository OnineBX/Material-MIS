/*------------------------------------------------------------------------------
 * Unit Name：RiMaterialStocktaking.cs
 * Description: 回收入库--物资管理员修改清点回收物资信息的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-10-15
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
    public class RiMaterialModifyStocktaking:Page
    {
        private int _taskid, _stocktakingid;
        private SPGridView spgvMaterial;
        private Button btnOK;
        private CheckBox chbCheck;
        private Label lblProblem;
        private TextBox txtProblem;

        private static string[] ShowTlist = {                                                                                                                        
                                              "物资名称:MaterialName",
                                              "规格型号:SpecificationModel",                                     
                                              "财务编码:FinanceCode",                                      
                                              "根/台/套/件:TotleGentaojian",
                                              "米:TotleMetre",
                                              "吨:TotleTon",                                                                                         
                                              "回收单号:RetrieveCode",
                                              "ID:SrinStocktakingDetailsID"
                                            };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    _stocktakingid = db.SrinStocktaking.SingleOrDefault(u => u.TaskID.Equals(_taskid)).SrinStocktakingID;
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

        #region 初始化和数据绑定方法

        private void InitToolBar()
        {
            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");

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

        private void InitializeCustomControls()
        {
            //初始化ToolBar
            InitToolBar();

            //初始化spgvMaterial
            this.spgvMaterial = new SPGridView();
            this.spgvMaterial.AutoGenerateColumns = false;
            this.spgvMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");
            this.spgvMaterial.RowDataBound += new GridViewRowEventHandler(spgvMaterial_RowDataBound);

            BoundField bfColumn;

            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvMaterial.Columns.Add(bfColumn);
            }

            //加入仓库列
            TemplateField tfStorage = new TemplateField();
            tfStorage.HeaderText = "仓库";
            tfStorage.ItemTemplate = new MulDropDownListTemplate(DataControlRowType.DataRow, "DDLStorage", "StorageID");
            this.spgvMaterial.Columns.Insert(6, tfStorage);

            //加入垛位列
            TemplateField tfPile = new TemplateField();
            tfPile.HeaderText = "垛位";
            tfPile.ItemTemplate = new MulDropDownListTemplate(DataControlRowType.DataRow, "DDLPile", "PileID");
            this.spgvMaterial.Columns.Insert(7, tfPile);

            //加入备注列            
            TemplateField tfRemark = new TemplateField();
            tfRemark.HeaderText = "备注";
            tfRemark.ItemTemplate = new TextBoxTemplate("备注", DataControlRowType.DataRow, "Remark");
            this.spgvMaterial.Columns.Insert(9, tfRemark);

            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);
            btnOK.OnClientClick = "return VerifyDDL()";
            (GetControltByMaster("ltrJS") as Literal).Text = JSDialogAid.GetVerifyDDLJSForBtn("--请选择--", "请为物资选择要存放的仓库或垛位！");

            chbCheck = (CheckBox)GetControltByMaster("chbCheck");
            chbCheck.CheckedChanged += new EventHandler(chbCheck_CheckedChanged);

            lblProblem = (Label)GetControltByMaster("lblProblem");
            txtProblem = (TextBox)GetControltByMaster("txtProblem");
        }

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息                
                SrinSubDoc ssd = db.SrinSubDoc.SingleOrDefault(u => u.SrinSubDocID == db.TaskStorageIn.SingleOrDefault(v => v.TaskStorageID.Equals(_taskid)).StorageInID);

                ((Label)GetControltByMaster("lblCreater")).Text = ssd.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblProject")).Text = ssd.ProjectInfo.ProjectName;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(ssd.CreateTime.ToLongDateString(), ssd.CreateTime.ToLongTimeString());

                //初始化回收分单中的物资
                this.spgvMaterial.DataSource = from a in db.SrinStocktakingDetails
                                               where a.SrinStocktakingID == _stocktakingid
                                               select new
                                               {
                                                   a.SrinSubDetails.MaterialInfo.MaterialName,
                                                   a.SrinSubDetails.MaterialInfo.SpecificationModel,
                                                   a.SrinSubDetails.MaterialInfo.FinanceCode,
                                                   a.SrinSubDetails.TotleGentaojian,
                                                   a.SrinSubDetails.TotleMetre,
                                                   a.SrinSubDetails.TotleTon,
                                                   a.SrinSubDetails.RetrieveCode,
                                                   a.StorageID,
                                                   a.PileID,
                                                   a.Remark,
                                                   a.SrinStocktakingDetailsID
                                               };
                this.spgvMaterial.DataBind();

                //初始化清点信息
                if (!Page.IsPostBack)
                {
                    SrinStocktaking sst = db.SrinStocktaking.SingleOrDefault(u => u.TaskID == _taskid);
                    if (sst != null)
                    {
                        chbCheck.AutoPostBack = false;
                        chbCheck.Checked = sst.StocktakingResult.Equals("清点有误") ? true : false;
                        chbCheck.AutoPostBack = true;
                        txtProblem.Visible = sst.StocktakingResult.Equals("清点有误") ? true : false;
                        txtProblem.Text = sst.StocktakingProblem;
                        btnOK.Text = "修改清点表单";
                    }
                }
            }


        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            this.spgvMaterial.Columns[10].Visible = false;
        }

        #endregion

        #region 控件事件

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("../../default-old.aspx", false);
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {

        }

        void spgvMaterial_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    //绑定仓库
                    DropDownList ddlStorage = (DropDownList)e.Row.Cells[6].Controls[0];
                    ddlStorage.SelectedIndexChanged += new EventHandler(ddlStorage_SelectedIndexChanged);
                    ddlStorage.Items.Clear();
                    ddlStorage.DataSource = from s in db.StorageInfo
                                            select new
                                            {
                                                s.StorageID,
                                                s.StorageName
                                            };
                    ddlStorage.DataTextField = "StorageName";
                    ddlStorage.DataValueField = "StorageID";
                    ddlStorage.DataBind();
                    ddlStorage.Items.Insert(0, new ListItem("--请选择--", "0"));

                    //绑定垛位
                    DropDownList ddlPile = (DropDownList)e.Row.Cells[7].Controls[0];
                    ddlPile.Items.Clear();
                    ddlPile.DataSource = from p in db.PileInfo
                                         where p.StorageID == Convert.ToInt32(ddlStorage.SelectedValue)
                                         select new
                                         {
                                             p.PileID,
                                             p.PileName
                                         };
                    ddlPile.DataTextField = "PileName";
                    ddlPile.DataValueField = "PileID";
                    ddlPile.DataBind();
                    ddlPile.Items.Insert(0, new ListItem("--请选择--", "0"));
                }

            }
        }

        void ddlStorage_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            DropDownList ddlStorage = (DropDownList)sender;//获取现在的事件触发者
            GridViewRow gvr = (GridViewRow)ddlStorage.NamingContainer;//同属于在一个NamingContainer下
            DropDownList ddlPile = (DropDownList)gvr.Cells[7].Controls[0];//找到字段的DropDownList
            ddlPile.Items.Clear();
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    ddlPile.DataSource = from p in db.PileInfo
                                         where p.StorageID == Convert.ToInt32(ddlStorage.SelectedValue)
                                         select new
                                         {
                                             p.PileID,
                                             p.PileName
                                         };
                    ddlPile.DataTextField = "PileName";
                    ddlPile.DataValueField = "PileID";
                    ddlPile.DataBind();
                    ddlPile.Items.Insert(0, new ListItem("--请选择--", "0"));
                }
            }
            catch
            {
                ddlPile.Items.Insert(0, new ListItem("--请选择--", "0"));
                ddlPile.SelectedValue = "0";
            }
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                //保存清点结果
                SrinStocktaking sst;
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    sst = db.SrinStocktaking.SingleOrDefault(u => u.TaskID == _taskid);
                    sst.StocktakingDate = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                    sst.StocktakingResult = chbCheck.Checked ? "清点有误" : "清点无误";
                    sst.StocktakingProblem = txtProblem.Text.Trim();

                    //修改清点物资明细
                    int iDetailsID;
                    SrinStocktakingDetails sstd;
                    foreach (GridViewRow gvr in this.spgvMaterial.Rows)
                    {
                        iDetailsID = Convert.ToInt32(gvr.Cells[10].Text);

                        sstd = db.SrinStocktakingDetails.SingleOrDefault(u => u.SrinStocktakingDetailsID.Equals(iDetailsID));                            
                        sstd.StorageID = Convert.ToInt32(((DropDownList)gvr.Cells[6].Controls[0]).SelectedValue);
                        sstd.PileID = Convert.ToInt32(((DropDownList)gvr.Cells[7].Controls[0]).SelectedValue);
                        sstd.Remark = ((TextBox)gvr.Cells[9].Controls[0]).Text.Trim();
                        sstd.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();                                                       
                    }
                    db.SubmitChanges();
                  
                }

                Response.Redirect(string.Format("RiMaterialStocktakingMessage.aspx?TaskID={0}", _taskid), false);
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
            }
        }

        void chbCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (chbCheck.Checked)
            {
                lblProblem.Visible = true;
                txtProblem.Visible = true;
                txtProblem.Text = "请在此描述清点问题...";
            }
            else
            {
                lblProblem.Visible = false;
                txtProblem.Visible = false;
                txtProblem.Text = "无";
            }
        }

        #endregion

        #region 辅助方法

        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }

        #endregion
    }
}
