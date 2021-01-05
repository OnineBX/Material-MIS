//***********************************************************
//--Description:新建物资明细，选择物资                      *
//--Created By: adonis                                      *
//--Date:2010.9.29                                          *
//--*********************************************************
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
    public class SelectMaterialall: System.Web.UI.Page
    {
        MMSProDBDataContext db;
        SPGridView gv;
        SPGridView gvexist;
        TextBox tboxName;
        TextBox tboxcode;
  
        Button btnSearch;
        Button btnOK;
        private string reValue;


        static string[] Titlelist = {

                                     "物料名称:MaterialName",
                                     "财务编码:FinanceCode",
                                     "规格型号:SpecificationModel",
                                     "根/台/套/件(数量):StocksGenTaojian",
                                     "米(数量):StocksMetre",
                                     "吨(数量):StocksTon",
                                     "库存类型:Status",
                                     "ID:StocksID"
                                    };



        protected void Page_Load(object sender, EventArgs e)
        {
            this.gvexist = new SPGridView();
            this.gvexist.AutoGenerateColumns = false;

            this.gv = new SPGridView();
            this.gv.AutoGenerateColumns = false;
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.gv);
            init();
            BindGridView();

            if (!IsPostBack)
            {

              

            }



        }

        void ddlStorage_SelectedIndexChanged(object sender, EventArgs e)
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




        private void init()
        {
            #region 初始化


            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");

            ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnRefresh.ID = "btnRefresh";
            btnRefresh.Text = "刷新";
            btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
            btnRefresh.Padding = "0,5,0,0";
            btnRefresh.Click += new EventHandler(btnRefresh_Click);
            tbarTop.RightButtons.Controls.Add(btnRefresh);

       
            btnSearch = new Button();
            btnSearch.Text = "搜索";
            btnSearch.Width = 120;
            btnSearch.Click += new EventHandler(btnSearch_Click);

            btnOK = new Button();
            btnOK.Text = "加入";
            btnOK.Width = 120;
            btnOK.Click += new EventHandler(btnOK_Click);

            tboxcode = (TextBox)GetControltByMaster("txtMaterialCode");
            tboxName = (TextBox)GetControltByMaster("txtMaterialName");


            Panel pstorage = (Panel)GetControltByMaster("Plstorage");
            Panel pile = (Panel)GetControltByMaster("Plpile");
            Panel search = (Panel)GetControltByMaster("search");
            Panel pelOK = (Panel)GetControltByMaster("pelOK");


            search.Controls.Add(btnSearch);
            pelOK.Controls.Add(btnOK);
            #endregion
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            List<CheckBox> listString = GetCheckedID();
            if (listString.Count != 1)
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('有且仅有一条记录被添加!')</script>");
                return;
            }


            int ichecked = 0;

            foreach (GridViewRow gvr in this.gv.Rows)
            {


                CheckBox chb = (CheckBox)gvr.Cells[0].Controls[0];
                if (!chb.Checked)
                    continue;
                ichecked++;

                reValue += "物料名称:(" + gvr.Cells[1].Text + ")|" + gvr.Cells[8].Text + "|" + gvr.Cells[7].Text + " ";




            }


            if (ichecked == 0)
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择调拨的记录!')</script>");
                return;
            }
            //回发模式窗口的值，并关闭模式窗口
            Page.RegisterStartupScript("success", "<script>window.returnValue='" + reValue + "';window.close()</" + "script>");


        }





        /// <summary>
        /// 绑定Gridview
        /// </summary>
        private void BindGridView()
        {
            this.gv.Columns.Clear();
            //int sid = Convert.ToInt32(Request.QueryString["StorageInID"]);

            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
               




                BoundField bfColumn;
                //添加选择列
                TemplateField tfieldCheckbox = new TemplateField();
                tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow);
                tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
                this.gv.Columns.Add(tfieldCheckbox);

                foreach (var kvp in Titlelist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.gv.Columns.Add(bfColumn);
                }
                this.gv.DataSource = from a in db.StorageStocks
                                     where a.MaterialName.Contains(this.tboxName.Text.Trim())
                                     && a.FinanceCode.Contains(this.tboxcode.Text.Trim())
                                     select new
                                     {
                                        a.StocksID,
                                        a.FinanceCode,
                                        a.MaterialName,
                                        a.SpecificationModel,
                                        a.StocksGenTaojian,
                                        a.StocksMetre,
                                        a.StocksTon,
                                        a.Status,
                                        a.Remark
                                     };

                this.gv.PageIndexChanging += new GridViewPageEventHandler(gv_PageIndexChanging);
                this.gv.AllowPaging = true;
                this.gv.PageSize = 8;

                this.gv.PagerTemplate = new PagerTemplate("{0} - {1}", gv);


                this.gv.DataBind();
                this.gv.Columns[this.gv.Columns.Count - 1].Visible = false;
              




            }

        }

        void gv_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            ((SPGridView)sender).PageIndex = e.NewPageIndex;
            ((SPGridView)sender).DataBind();
        }







        //多条件搜索
        void btnSearch_Click(object sender, EventArgs e)
        {
            //BindGridView();
        }

        void btnPassNagetive_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void btnCannel_Click(object sender, EventArgs e)
        {
            //Response.Redirect("QualityControlManage.aspx?TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&StorageInID=" + Request.QueryString["StorageInID"] + "");
        }

        void btnSend_Click(object sender, EventArgs e)
        {

            Response.Redirect("../../default-old.aspx", false);

            //Response.Redirect("QualityControlMessage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&state=材料会计审核");
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
