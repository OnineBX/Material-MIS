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


namespace MMSPro.WebApp
{
    public class StockTransferDetailsSelect : System.Web.UI.Page
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


        static string[] Titlelist = {

                                     "物料名称:MaterialName",
                                     "状态:Status",
                                     "物料规格:SpecificationModel",
                                    
                                    
                                      "根/套/件[库存]:StocksGenTaojian",
                                              "米[库存]:StocksMetre",
                                              "吨[库存]:StocksTon",
                                     "单价:UnitPrice",
                                     "所属仓库:StorageName",
                                     "所属垛位:PileName",
                                     "到库时间:StorageTime",
                                     "财务编码:FinanceCode",
                                     "ID:StocksID"
                                    };
        static string[] exist = {
                                 
                                     "物料名称:MaterialName",
                                     "物料规格:SpecificationModel",
                                     "状态:Status",
                                   
                                     //"调拨数量:Quantity",
                                     //"单位:CurUnit",
                                              "根/套/件:QuantityGentaojian",
                                              "米:QuantityMetre",
                                              "吨:QuantityTon",
                                     "单价:UnitPrice",
                                     
                                     "所属仓库:StorageName",
                                     "所属垛位:PileName",
                                     "到库时间:StorageTime",
                                     "财务编码:FinanceCode",
                                     };
        //static string[] exist = {
        //                             "调拨通知单编号:StorageOutNoticeCode",
        //                             "物料名称:MaterialName",
        //                             "物料规格:SpecificationModel",
        //                             "物料编码:MaterialCode",
        //                             "库存数量:StocksCount",
        //                             "调拨数量:Quantity",
        //                             "财务编号:financeCode",
        //                             "单价:UnitPrice",
        //                             "金额:Amount",
        //                             "所属仓库:StorageName",
        //                             "所属垛位:PileCode",
        //                             "到库时间:StorageTime",
        //                             "供应商:SupplierName",
        //                             "创建者:EmpName",
        //                             "创建日期:CreateTime",
        //                            };



        protected void Page_Load(object sender, EventArgs e)
        {
            this.gvexist = new SPGridView();
            this.gvexist.AutoGenerateColumns = false;

            this.gv = new SPGridView();
            this.gv.AutoGenerateColumns = false;
            this.gv.Columns.Clear();
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.gv);
          //  p1.Controls.Add(this.spgvMaterial);
           
            init();
            BindGridView();
            BindGridViewExist();
            if (!IsPostBack)
            {

                BindStorage();

            }



        }

        void ddlStorage_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindPile();
        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("StockTransferDetailsManage.aspx?StockTransferID=" + Request.QueryString["StockTransferID"]);
        }




        void btnRefresh_Click(object sender, EventArgs e)
        {


        }
        //绑定仓库
        private void BindStorage()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = (from a in db.StorageInfo
                            select new
                            {
                                Key = a.StorageName,
                                Value = a.StorageID
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
                if (this.ddlStorage.SelectedIndex> 0)
                {
                    var temp = (from a in db.PileInfo
                                where a.StorageInfo.StorageID == Convert.ToInt32(this.ddlStorage.SelectedValue)
                                select new
                                {
                                    Key =a.PileName,
                                    Value = a.PileID
                                }).Distinct();

                    this.ddlPile.DataSource = temp;
                    this.ddlPile.DataTextField = "Key";
                    this.ddlPile.DataValueField = "Value";
                    this.ddlPile.DataBind();
                    this.ddlPile.Items.Insert(0, "--请选择--");
                }
                else
                {
                    this.ddlPile.Items.Insert(0, "--请选择--");
                    //if (this.ddlStorage.SelectedValue != "--请选择--")
                    //{
                    //    this.ddlPile.Items.Insert(0, "--请选择--");
                    //    this.ddlPile.SelectedIndex = 0;
                    //}
                }

            }
        }
        /// <summary>
        /// 检查输入选择行输入框值否为空
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

                    var ddl = (DropDownList)this.gv.Rows[i].Cells[12].Controls[0];
                    decimal dcmMain = 0;
                    bool done = false;
                    switch (ddl.Text)
                    {
                        case "根/台/套/件":
                            done = decimal.TryParse(((TextBox)this.gv.Rows[i].Cells[6].Controls[0]).Text,out dcmMain);
                            break;
                        case "米":
                            done = decimal.TryParse(((TextBox)this.gv.Rows[i].Cells[8].Controls[0]).Text, out dcmMain);
                            break;
                        case "吨":
                            done = decimal.TryParse(((TextBox)this.gv.Rows[i].Cells[10].Controls[0]).Text, out dcmMain);
                            break;
                    }
                    if (dcmMain <= 0|| !done )
                    {
                        return false;
                    }
                    #region 屏蔽
                    //for (int k = 4; k < 10; k++)
                    //{
                    //    if (this.gv.Rows[i].Cells[k].Controls.Count > 0)
                    //    {
                            
                    //        if (this.gv.Rows[i].Cells[k].Controls[0] is TextBox)
                    //        {
                    //            tboxQualified = (TextBox)this.gv.Rows[i].Cells[k].Controls[0];
                    //            if (tboxQualified.Text == string.Empty)
                    //            {
                    //                return false;
                    //            }

                    //        }
                    //    }
                    //}
                    #endregion
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



            tboxcode = (TextBox)GetControltByMaster("txtMaterialCode");
            tboxName = (TextBox)GetControltByMaster("txtMaterialName");
            ddlStorage = new DropDownList();
            ddlStorage.AutoPostBack = true;
            this.ddlStorage.SelectedIndexChanged += new EventHandler(ddlStorage_SelectedIndexChanged);
            ddlPile = new DropDownList();
            btnSearch = new Button();
            btnSearch.Text = "搜索";
            btnSearch.Width = 120;
            btnSearch.Click += new EventHandler(btnSearch_Click);

            btnOK = new Button();
            btnOK.Text = "完成";
            btnOK.Width = 120;
            btnOK.Click += new EventHandler(btnOK_Click);


            Panel ps = (Panel)GetControltByMaster("PlSearch");

            Panel pn = (Panel)GetControltByMaster("Plname");
            Panel pstorage = (Panel)GetControltByMaster("Plstorage");
            Panel pile = (Panel)GetControltByMaster("Plpile");
            Panel search = (Panel)GetControltByMaster("search");
            Panel pelOK = (Panel)GetControltByMaster("pelOK");
            ps.Controls.Add(tboxcode);
            pn.Controls.Add(tboxName);
            pstorage.Controls.Add(ddlStorage);
            pile.Controls.Add(ddlPile);
            search.Controls.Add(btnSearch);
            pelOK.Controls.Add(btnOK);
            #endregion
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (CheckStringEmpty() != true)
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请将选择的数据填写完整!')</script>");
                return;
            }

            List<CheckBox> listString = GetCheckedID();
            if (listString.Count == 0)
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择调拨的记录!')</script>");
                return;
            }

            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                foreach (GridViewRow gvr in this.gv.Rows)
                {


                    CheckBox chb = (CheckBox)gvr.Cells[0].Controls[0];
                    if (!chb.Checked)
                        continue;
                 
                    //M roro
                    var tbSelect = (TextBox)(gvr.Cells[4].Controls[0]);
                    //var 
                    var tboxNums = (TextBox)(gvr.Cells[6].Controls[0]);
                    var tboxNum = (TextBox)(gvr.Cells[8].Controls[0]);
                    var tboxNumq = (TextBox)(gvr.Cells[10].Controls[0]);
                    var ddlType = (DropDownList)(gvr.Cells[12].Controls[0]);
                    var strT = tbSelect.Text.Split('|');
                    string strPileID = "";
                    if (strT.Length == 3)
                    {
                        strPileID = strT[2];
                    }

                    PileInfo pi = db.PileInfo.SingleOrDefault(a => a.PileCode == strPileID);
                    if (pi == null)
                    {
                        return;
                    }
                    StockTransferDetail sod = db.StockTransferDetail.SingleOrDefault(u => u.StockTransferID == Convert.ToInt32(Request.QueryString["StockTransferID"]) && u.StocksID == int.Parse(chb.ToolTip) && u.TargetPile == pi.PileID);
                    //end M roro
                    if (sod == null)
                    {
                        StockTransferDetail st = new StockTransferDetail();
                        st.DetailType = "移库任务";
                        decimal dcmT = 0;
                        if (decimal.TryParse(tboxNums.Text,out dcmT))
                            st.QuantityGentaojian = dcmT;
                        if (decimal.TryParse(tboxNum.Text, out dcmT))
                            st.QuantityMetre = dcmT;
                       if( decimal.TryParse(tboxNumq.Text, out dcmT))
                            st.QuantityTon  = dcmT;
                        //st.Quantity
                        //switch (ddlType.Text)
                        //{
                        //    case "根/套/件":
                        //        st.Quantity =decimal.Parse( st.QuantityGentaojian.ToString());
                        //        break;
                        //    case "米":
                        //        st.Quantity = decimal.Parse(st.QuantityMetre.ToString());
                        //        break;
                        //    case "吨":
                        //        st.Quantity = decimal.Parse(st.QuantityTon.ToString());
                        //        break;
                        //}                        
                        st.StocksID = int.Parse(chb.ToolTip);
                        st.StocksStatus = gvr.Cells[2].Text;
                        st.StockTransferID = Convert.ToInt32(Request.QueryString["StockTransferID"]);
                        st.TargetPile = pi.PileID;
                        db.StockTransferDetail.InsertOnSubmit(st);        
                    }
                    else
                    {

                    }


                    db.SubmitChanges();

                }

                Response.Redirect("StockTransferDetailsManage.aspx?StockTransferID=" + Request.QueryString["StockTransferID"]);
            }

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
                if (this.ddlStorage.SelectedIndex>0)
                {
                    storage = this.ddlStorage.SelectedItem.Text.ToString();

                }
                else
                {
                    storage = "";
                }
                if (this.ddlPile.SelectedIndex >0)
                {
                    f_Pile = this.ddlPile.SelectedItem.Text.ToString();
                }
                else
                {
                    f_Pile = "";
                }
               



                //TemplateField tfieldTextBox = new TemplateField();
                //tfieldTextBox.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "StocksID", "txtCount");
                //tfieldTextBox.HeaderTemplate = new MulTextBoxTemplate("调拨数量", DataControlRowType.Header);
                //tfieldTextBox.ItemStyle.Width = 150;

                TemplateField code_fieldTextBox = new TemplateField();
                code_fieldTextBox.ItemTemplate = new TextBoxWithImage("请选择", DataControlRowType.DataRow, "", "StocksID", "txtCode");
                code_fieldTextBox.HeaderTemplate = new TextBoxWithImage("调拨垛位(仓库|垛位|垛位号)", DataControlRowType.Header);
                code_fieldTextBox.ItemStyle.Width = 250;

                //TemplateField imgSelect = new TemplateField();
                // imgSelect.ItemTemplate = new mul

                BoundField bfColumn;
                //添加选择列
                TemplateField tfieldCheckbox = new TemplateField();
                tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "StocksID");
                tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
                this.gv.Columns.Add(tfieldCheckbox);

                foreach (var kvp in Titlelist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.gv.Columns.Add(bfColumn);
                }
                this.gv.Columns.Insert(4, code_fieldTextBox);

                TemplateField tfQuantityGtj = new TemplateField();
                tfQuantityGtj.HeaderText = "根/套/件";
                tfQuantityGtj.ItemTemplate = new TextBoxTemplate("QuantityGentaojian", string.Empty, "^(-?\\d+)(\\.\\d+)?$",50);
                this.gv.Columns.Insert(6, tfQuantityGtj);

                //加入回收数量--米列
                TemplateField tfQuantityMetre = new TemplateField();
                tfQuantityMetre.HeaderText = "米";
                tfQuantityMetre.ItemTemplate = new TextBoxTemplate("QuantityMetre", string.Empty, "^(-?\\d+)(\\.\\d+)?$",50);
                this.gv.Columns.Insert(8, tfQuantityMetre);

                //加入回收数量--吨列
                TemplateField tfQuantityTon = new TemplateField();
                tfQuantityTon.HeaderText = "吨";
                tfQuantityTon.ItemTemplate = new TextBoxTemplate("QuantityTon", string.Empty, "^(-?\\d+)(\\.\\d+)?$",50);
                this.gv.Columns.Insert(10, tfQuantityTon);

                //加入当前计量单位
                TemplateField tfCurUnit = new TemplateField();
                tfCurUnit.HeaderText = "计算单位";
                string[] units = new string[] { "根/台/套/件", "米", "吨" };
                tfCurUnit.ItemTemplate = new DropDownListTemplate("CurUnit", DataControlRowType.DataRow, units,false);
                this.gv.Columns.Insert(12, tfCurUnit);

                //因出库未完成无法显示现有库存量

               // this.gv.Columns.Insert(6, tfieldTextBox);
                this.gv.DataSource = from a in db.StorageStocks
                                     where a.FinanceCode.Contains(this.tboxcode.Text.Trim())
                                     && a.MaterialName.Contains(this.tboxName.Text.Trim())
                                     && a.StorageName == (storage == "" ? a.StorageName : storage)
                                     && a.PileName == (f_Pile == "" ? a.PileName : f_Pile)
                                     select new
                                     {
                                         a.MaterialName,
                                         a.Status,
                                         a.SpecificationModel,                                          
                                         a.UnitPrice,
                                         a.StorageName,
                                         a.PileName,
                                         a.StorageTime,
                                         
                                         a.Remark,
                                         a.StocksID,
                                         a.CurUnit,
                                         a.StocksGenTaojian,    
                                         a.StocksMetre,
                                         a.StocksTon,
                                            a.FinanceCode,
                                     };

                //分页
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

        private void BindGridViewExist()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                BoundField bfColumn;



                foreach (var kvp in exist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.gvexist.Columns.Add(bfColumn);
                }


                this.gvexist.DataSource = from a in db.StockTransferDetail
                                          join b in db.StockTransfer on a.StockTransferID equals b.StockTransferID
                                          join c in db.StorageStocks on a.StocksID equals c.StocksID
                                          where a.StockTransferID == Convert.ToInt32(Request.QueryString["StockTransferID"])
                                          && a.StocksID == c.StocksID
                                          && a.StocksStatus == c.Status
                                          select new
                                          {
                                              a.StockTransferDetailID,
                                              b.StockTransferNum,
                                              c.ManufacturerName,
                                              c.MaterialCode,
                                              c.SpecificationModel,
                                              c.StorageName,
                                              c.MaterialName,
                                              c.PileName,
                                              c.CurUnit,
                                              a.QuantityGentaojian,
                                              a.QuantityMetre,
                                              a.QuantityTon,
                                              c.FinanceCode,
                                              c.Status,
                                              c.UnitPrice,
                                              c.StorageTime,
                                              //c.SupplierName,
                                              //a.
                                              target = a.PileInfo.StorageInfo.StorageName + "|" + a.PileInfo.PileName + "|" + a.PileInfo.PileCode
                                          };
                this.gvexist.DataBind();
                Panel p2 = (Panel)GetControltByMaster("Panel2");
                p2.Controls.Add(this.gvexist);

            }

        }





        //多条件搜索
        void btnSearch_Click(object sender, EventArgs e)
        {
           BindGridView();
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
