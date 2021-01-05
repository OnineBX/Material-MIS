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
    public class SelectCommitMaterial : System.Web.UI.Page
    {

        MMSProDBDataContext db;
        SPGridView gv;
        SPGridView gvexist;
        TextBox tboxName;
        TextBox tboxcode;
        DropDownList ddlStorage;
        DropDownList ddlPile;
        Button btnSearch;
        Button btnOK;
        TextBox tboxQualified;
        private string storage;
        private string f_Pile;
        private int listCount = 0;
        int SocksID;
        private string reValue;
        Literal L1;

        static string[] Titlelist = {

                                     "物料名称:MaterialName",
                                     "物料编码:MaterialCode",
                                     "物料规格:SpecificationModel",
                                     "所属委外出库单:StorageCommitOutNoticeCode",

                                     "根/套/件数量:QuantityGentaojian",
                                     "米数量:QuantityMetre",
                                     "吨数量:QuantityTon",
                                     "单位数量:Quantity",
                                     "计量单位:CurUnit",

                                     "单价:UnitPrice",

                                     "所属垛位:PileCode",
                                     "所属仓库:StorageName",
                                     "财务编码:financeCode",
                                     "供应商:SupplierName",
                                     "到库时间:StorageTime",
                                     "ID:StorageCommitOutRealDetailsID"

                                    };



        protected void Page_Load(object sender, EventArgs e)
        {
            this.gvexist = new SPGridView();
            this.gvexist.AutoGenerateColumns = false;

            this.gv = new SPGridView();
            this.gv.AutoGenerateColumns = false;
            init();
            BindGridView();

            if (!IsPostBack)
            {

                BindStorage();

            }



        }

        void ddlStorage_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindPile();
        }





        void btnRefresh_Click(object sender, EventArgs e)
        {


        }
        //绑定仓库
        private void BindStorage()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = (from a in db.TableOfStocks
                            select new
                            {
                                Key = a.PileInfo.StorageInfo.StorageName,
                                Value = a.PileInfo.StorageInfo.StorageID
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
                    var temp = (from a in db.TableOfStocks
                                where a.PileInfo.StorageInfo.StorageID == Convert.ToInt32(this.ddlStorage.SelectedValue)
                                select new
                                {
                                    Key = a.PileInfo.PileCode,
                                    Value = a.PileInfo.PileID
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
        /// <summary>
        /// 检查输入选择行输入框值否为空,且财务编码是否唯一
        /// </summary>
        /// <returns></returns>
        private bool CheckStringEmpty()
        {
            using (MMSProDBDataContext dbc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                List<string> li = new List<string>();
                List<CheckBox> lbox = GetCheckedID();

                for (int i = 0; i < this.gv.Rows.Count; i++)
                {
                    CheckBox chb = (CheckBox)this.gv.Rows[i].Cells[0].Controls[0];
                    if (!chb.Checked)
                        continue;
                    for (int k = 6; k < 7; k++)
                    {
                        if (this.gv.Rows[i].Cells[k].Controls[0] is TextBox)
                        {
                            tboxQualified = (TextBox)this.gv.Rows[i].Cells[k].Controls[0];
                            if (tboxQualified.Text == string.Empty)
                            {
                                return false;
                            }
                            //财务唯一验证
                            //if (k == 5)
                            //{


                            //    TextBox f_code = (TextBox)this.gv.Rows[i].Cells[k].Controls[0];
                            //    li.Add(f_code.Text.Trim());
                            //    var tmp = from a in dbc.StorageOutDetails
                            //              where a.FinanceCode == f_code.Text.Trim()
                            //              select a;
                            //    if (tmp.ToArray().Length > 0)
                            //    {
                            //        return false;
                            //    }



                            //}
                        }
                    }


                }
                //用户填写的值如有重复
                listCount = li.Count;
                if (listCount != li.Distinct().ToList().Count)
                {
                    return false;
                }






            }

            return true;
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

            ddlStorage = new DropDownList();
            ddlStorage.AutoPostBack = true;
            this.ddlStorage.SelectedIndexChanged += new EventHandler(ddlStorage_SelectedIndexChanged);
            ddlPile = new DropDownList();
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

            pstorage.Controls.Add(ddlStorage);
            pile.Controls.Add(ddlPile);
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

                reValue += "物料名称:(" + gvr.Cells[1].Text + ")|物料编码:(" + gvr.Cells[2].Text + ")|" + gvr.Cells[16].Text + "";




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
                TemplateField tfieldCheckbox = new TemplateField();
                tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "StorageCommitOutRealDetailsID");
                tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
                this.gv.Columns.Add(tfieldCheckbox);

                foreach (var kvp in Titlelist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.gv.Columns.Add(bfColumn);
                }
                this.gv.DataSource = from a in db.CommitOutMaterial


                                     where a.MaterialCode.Contains(this.tboxcode.Text.Trim())
                                     && a.MaterialName.Contains(this.tboxName.Text.Trim())
                                     && a.StorageName == (storage == "" ? a.StorageName : storage)
                                     && a.PileCode == (f_Pile == "" ? a.PileCode : f_Pile)

                                     select new
                                     {

                                         a.StorageCommitOutRealDetailsID,
                                         a.MaterialCode,
                                         a.MaterialName,
                                         a.StorageCommitOutNoticeCode,
                                         a.SpecificationModel,
                                         a.UnitPrice,
                                         a.QuantityGentaojian,
                                         a.QuantityMetre,
                                         a.QuantityTon,
                                         a.Quantity,
                                         a.CurUnit,
                                         a.PileCode,
                                         a.StorageName,
                                         a.financeCode,
                                         a.StorageTime,
                                         a.SupplierName

                                     };
                this.gv.DataBind();
                this.gv.Columns[this.gv.Columns.Count - 1].Visible = false;
                Panel p1 = (Panel)GetControltByMaster("Panel1");



                p1.Controls.Add(this.gv);




            }

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

            Response.Redirect("../../default-old.aspx",false);

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
