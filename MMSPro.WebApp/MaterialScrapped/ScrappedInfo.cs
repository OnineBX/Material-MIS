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
    public class ScrappedInfo : System.Web.UI.Page
    {
        MMSProDBDataContext db;
        SPGridView gv;


        //搜索
        DropDownList ddlStorage;
        DropDownList ddlPile;
        TextBox txtMaterialCode;
        TextBox txtMaterialName;
        private string storage;
        private string f_Pile;


        static string[] Titlelist = {
                                     "物料名称:MaterialName",
                                     "财务编码:FinanceCode",
                                     "物料规格:SpecificationModel",
                                     "待报废报告号:ScrapReportNum",
                                     "报废数量:Gentaojian",
                                     "所属仓库:StorageName",
                                     "所属垛位:PileCode",
                                     "回收项目:ProjectName",
                                     "生产厂家:ManufacturerName",
                                     "报废状态:state",
                                     "报废文件号:ScrappedNum",
                                     "所属库存物资名称:materialName",
                                     "所属库存类型:StockType",
                                     "ID:ScrappedID"
                                    };



        protected void Page_Load(object sender, EventArgs e)
        {
            this.gv = new SPGridView();
            this.gv.AutoGenerateColumns = false;
            try
            {
                toolbarInit();
                initControl();
                BindGridView();
                if (!IsPostBack)
                {

                    BindStorage();

                }


            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
        }
        private void initControl()
        {

            this.txtMaterialCode = (TextBox)GetControltByMaster("txtMaterialCode");
            this.txtMaterialName = (TextBox)GetControltByMaster("txtMaterialName");
            this.ddlStorage = (DropDownList)GetControltByMaster("ddlStorage");
            this.ddlStorage.SelectedIndexChanged += new EventHandler(ddlStorage_SelectedIndexChanged);
            this.ddlPile = (DropDownList)GetControltByMaster("ddlPile");

        }

        void ddlStorage_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindPile();
        }

        //绑定仓库
        private void BindStorage()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = (from a in db.Scrapped
                            select new
                            {
                                Key = a.AwaitScrap.PileInfo.StorageInfo.StorageName,
                                Value = a.AwaitScrap.PileInfo.StorageInfo.StorageID
                            }).Distinct();

                this.ddlStorage.DataSource = temp;
                this.ddlStorage.DataTextField = "Key";
                this.ddlStorage.DataValueField = "Value";
                this.ddlStorage.DataBind();
                this.ddlStorage.Items.Insert(0, "--请选择--");
            }
        }
        //绑定垛位
        private void BindPile()
        {
            this.ddlPile.Items.Clear();
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                if (this.ddlStorage.SelectedValue != "--请选择--" && this.ddlStorage.SelectedValue != "")
                {
                    var temp = (from a in db.Scrapped
                                where a.AwaitScrap.PileInfo.StorageInfo.StorageID == Convert.ToInt32(this.ddlStorage.SelectedValue)
                                select new
                                {
                                    Key = a.AwaitScrap.PileInfo.PileCode,
                                    Value = a.AwaitScrap.PileInfo.PileID
                                }).Distinct();

                    this.ddlPile.DataSource = temp;
                    this.ddlPile.DataTextField = "Key";
                    this.ddlPile.DataValueField = "Value";
                    this.ddlPile.DataBind();
                    this.ddlPile.Items.Insert(0, "--请选择--");
                }
                else
                {
                    if (this.ddlStorage.SelectedValue != "--请选择--")
                    {
                        this.ddlPile.Items.Insert(0, "--请选择--");
                        this.ddlPile.SelectedIndex = 0;
                    }
                }

            }
        }

        private void toolbarInit()
        {

            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");

            //返回
            ToolBarButton tbarbtnAdd = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnAdd.ID = "AddNewRow";
            tbarbtnAdd.Text = "返回";
            tbarbtnAdd.ImageUrl = "/_layouts/images/newitem.gif";
            tbarbtnAdd.Click += new EventHandler(tbarbtnAdd_Click);
            tbarTop.Buttons.Controls.Add(tbarbtnAdd);



            //刷新

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

            Response.Redirect("AwaitScrapManage.aspx", false);
        }


        protected void tbarbtnEdit_Click(object sender, EventArgs e)
        {
           
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
            //try
            //{
            //    List<CheckBox> listString = GetCheckedID();
            //    if (listString.Count > 0)
            //    {
            //        StorageInDetailed SID;
            //        using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            //        {
            //            foreach (var li in listString)
            //            {
            //                SID = db.StorageInDetailed.SingleOrDefault(a => a.StorageDetailedID == int.Parse(li.ToolTip));
            //                if (SID != null)
            //                {
            //                    db.StorageInDetailed.DeleteOnSubmit(SID);

            //                }
            //            }
            //            db.SubmitChanges();
            //        }
            //        Response.Redirect("StorageInDetailedManage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "");
            //    }
            //    else
            //    {
            //        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择需要删除的记录!')</script>");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MethodBase mb = MethodBase.GetCurrentMethod();
            //    LogToDBHelper lhelper = LogToDBHelper.Instance; lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
            //    ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_DELETEERROR));
            //}

        }

        /// <summary>
        /// 绑定Gridview
        /// </summary>
        private void BindGridView()
        {

            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {

                if (this.ddlStorage.SelectedValue != "--请选择--" && this.ddlStorage.SelectedValue != "")
                {
                    storage = this.ddlStorage.SelectedItem.Text.ToString();

                }
                else
                {
                    storage = string.Empty;
                }
                if (this.ddlPile.SelectedValue != "--请选择--" && this.ddlPile.SelectedValue != "")
                {
                    f_Pile = this.ddlPile.SelectedItem.Text.ToString();
                }
                else
                {
                    f_Pile = string.Empty;
                }


                BoundField bfColumn;
                //添加选择列



                foreach (var kvp in Titlelist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.gv.Columns.Add(bfColumn);
                }
                this.gv.DataSource = from a in db.Scrapped
                                     where a.AwaitScrap.State == "已报废"
                                     && a.AwaitScrap.MaterialInfo.FinanceCode.Contains(this.txtMaterialCode.Text.Trim())
                                     && a.AwaitScrap.MaterialInfo.MaterialName.Contains(this.txtMaterialName.Text.Trim())
                                     && a.AwaitScrap.StorageInfo.StorageName == (storage == "" ? a.AwaitScrap.StorageInfo.StorageName : storage)
                                     && a.AwaitScrap.PileInfo.PileCode == (f_Pile == "" ? a.AwaitScrap.PileInfo.PileCode : f_Pile)
                                     select new
                                     {
                                         a.ScrappedID,
                                         a.AwaitScrap.ScrapReportNum,
                                         a.AwaitScrap.MaterialInfo.MaterialName,
                                         a.AwaitScrap.MaterialInfo.FinanceCode,
                                         a.AwaitScrap.Gentaojian,
                                         a.AwaitScrap.MaterialInfo.SpecificationModel,
                                         a.AwaitScrap.StorageInfo.StorageName,
                                         a.AwaitScrap.PileInfo.PileCode,
                                         a.AwaitScrap.ProjectInfo.ProjectName,
                                         a.AwaitScrap.Manufacturer.ManufacturerName,
                                         a.AwaitScrap.State,
                                         a.ScrappedNum,
                                         materialName = (db.StorageStocks.SingleOrDefault(p=>p.StocksID==a.StockID && p.Status == a.StockType)).MaterialName,
                                         a.StockType,
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
