//***********************************************************
//--Description:物资转线上                                  *
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
    public class MaterialOnLineManage:System.Web.UI.Page
    {
        private int _noticeid;

        private SPGridView spgvMaterial, spgvExistMaterial;

        private TextBox txtMaterialName, txtfinanceCode;
      
        private Button btnSearch, btnOK;
        private CheckBox chbShowAll;
        //controls
        DropDownList ddlyn = new DropDownList();

        private static string[] Titlelist = {        
          
                                     "所属收料单:storagecode",
                                     "物料名称:MaterialName",
                                     "规格型号:SpecificationModel",     
                                     "财务编码:FinanceCode",
                                     "预期使用项目:ProjectName",
                                     "库存数量(根/台/套/件):QuantityGentaojian",
                                     "库存数量(米):QuantityMetre",
                                     "库存数量(吨):QuantityTon",                           
                                     "生产厂商:ManufacturerName",
                                     "供应商:SupplierName",
                                     "所属批次:BatchIndex",
                                     "StocksID:StocksID",
                                    };
        private static string[] ExistTitlelist = {        
                             
                                     "所属收料单:storagecode",
                                     "物料名称:MaterialName",
                                     "规格型号:SpecificationModel",
                                     "财务编码:FinanceCode",
                                     "线上物料编码:OnlineCode",                                                                     
                                     "采购订单号:OrderNum",    
                                     "线上收料时间:OnlineDate",
                                     "收料凭证号:CertificateNum",
                                     "线上计量单位:OnlineUnit",
                                     "线上收料数量:CurQuantity",  
                                     "线上收料单价:OnlineUnitPrice",   
                                     "线上收料金额:OnlineTotal",                                  
                                     "预期使用项目:ProjectName",
                                     "所属批次:BatchIndex",

                                                             
                                    };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                _noticeid = Convert.ToInt32(Request.QueryString["NoticeID"]);
                Control();
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

        #region 初始化和绑定方法

        private void InitBar()
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
        private void Control()
        {

            txtfinanceCode = (TextBox)GetControltByMaster("txtfinanceCode");
            txtMaterialName = (TextBox)GetControltByMaster("txtMaterialName");
            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);

            btnSearch = (Button)GetControltByMaster("btnSearch");
            btnSearch.Click += new EventHandler(btnSearch_Click);

            chbShowAll = (CheckBox)GetControltByMaster("chbShowAll");
            chbShowAll.CheckedChanged += new EventHandler(chbShowAll_CheckedChanged);

      
        }

        private void InitializeCustomControls()
        {
            InitBar();



            //***初始化新建物资列表***//
            this.spgvMaterial = new SPGridView();
            this.spgvMaterial.AutoGenerateColumns = false;
            this.spgvMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");
            this.spgvMaterial.RowDataBound += new GridViewRowEventHandler(spgvMaterial_RowDataBound);
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);
            this.spgvMaterial.Columns.Clear();
            //分页
            this.spgvMaterial.PageIndexChanging += new GridViewPageEventHandler(spgvMaterial_PageIndexChanging);
            this.spgvMaterial.AllowPaging = true;
            this.spgvMaterial.PageSize = 8;

            //this.spgvMaterial.PagerSettings.Mode = PagerButtons.NextPreviousFirstLast;
            //this.spgvMaterial.PagerSettings.NextPageText = "下一页";
            //this.spgvMaterial.PagerSettings.LastPageText = "尾页";
            //this.spgvMaterial.PagerSettings.PreviousPageText = "上一页";
            //this.spgvMaterial.PagerSettings.FirstPageText = "首页";
            //this.spgvMaterial.PagerSettings.Mode = PagerButtons.NextPreviousFirstLast;
            //this.spgvMaterial.PagerSettings.Position = PagerPosition.Bottom;
            //this.spgvMaterial.PagerStyle.HorizontalAlign = HorizontalAlign.Left;
            //this.spgvMaterial.PagerTemplate = null;
            this.spgvMaterial.PagerTemplate = new PagerTemplate("{0} - {1}", spgvMaterial);


           


            //添加选择列
            TemplateField tfieldCheckbox = new TemplateField();
            tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("选择", DataControlRowType.DataRow);
            tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("选择", DataControlRowType.Header);
            this.spgvMaterial.Columns.Add(tfieldCheckbox);

            BoundField bfColumn;
            foreach (var kvp in Titlelist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvMaterial.Columns.Add(bfColumn);
            }

            //采购订单号
            TemplateField tfGentaojian = new TemplateField();
            tfGentaojian.HeaderText = "采购订单号";
            tfGentaojian.ItemTemplate = new TextBoxTemplate("Gentaojian", string.Empty, "", "", 80);
            this.spgvMaterial.Columns.Insert(9, tfGentaojian);


            //线上物料编号
            TemplateField tfTon = new TemplateField();
            tfTon.HeaderText = "线上物料编号";
            tfTon.ItemTemplate = new TextBoxTemplate("Ton", string.Empty, "", "", 80);
            this.spgvMaterial.Columns.Insert(10, tfTon);


            //线上收料时间
            TemplateField time = new TemplateField();
            time.HeaderText = "线上收料时间";
            time.ItemTemplate = new DateTimeTemplate(DataControlRowType.DataRow);
            this.spgvMaterial.Columns.Insert(11, time);


            //收料凭证号
            TemplateField tfMetre = new TemplateField();
            tfMetre.HeaderText = "收料凭证号";
            tfMetre.ItemTemplate = new TextBoxTemplate("Metre", string.Empty, "", "", 80);
            this.spgvMaterial.Columns.Insert(12, tfMetre);


            
        
            //线上物资计量单位
            TemplateField unit = new TemplateField();
            unit.HeaderText = "线上计量单位";
            unit.ItemTemplate = new MulDropDownListTemplate(DataControlRowType.DataRow, "ddlyn");
            this.spgvMaterial.Columns.Insert(13, unit);




            //线上收料数量
            TemplateField num = new TemplateField();
            num.HeaderText = "线上收料数量";
            num.ItemTemplate = new TextBoxTemplate("num", string.Empty, "^(-?\\d+)(\\.\\d+)?$", "", 80);
            this.spgvMaterial.Columns.Insert(14, num);



            //线上收料金额
            TemplateField tup = new TemplateField();
            tup.HeaderText = "线上收料金额";
            tup.ItemTemplate = new TextBoxTemplate("UnitPrice", string.Empty, "^(-?\\d+)(\\.\\d+)?$", "", 80);
            this.spgvMaterial.Columns.Insert(15, tup);


            //线上收料的根台套件数量
            TemplateField _gen = new TemplateField();
            _gen.HeaderText = "根/台/套/件(线上数量)";
            _gen.ItemTemplate = new TextBoxTemplate("_gen", string.Empty, "^(-?\\d+)(\\.\\d+)?$", "", 80);
            this.spgvMaterial.Columns.Insert(16, _gen);

            //线上收料米的数量
            TemplateField _metre = new TemplateField();
            _metre.HeaderText = "米(线上数量)";
            _metre.ItemTemplate = new TextBoxTemplate("_metre", string.Empty, "^(-?\\d+)(\\.\\d+)?$", "", 80);
            this.spgvMaterial.Columns.Insert(17, _metre);

            //线上收料吨的数量
            TemplateField _ton = new TemplateField();
            _ton.HeaderText = "吨(线上数量)";
            _ton.ItemTemplate = new TextBoxTemplate("_ton", string.Empty, "^(-?\\d+)(\\.\\d+)?$", "", 80);
            this.spgvMaterial.Columns.Insert(18, _ton);
            


            





            //***初始化已加入物资列表***//
            this.spgvExistMaterial = new SPGridView();
            this.spgvExistMaterial.AutoGenerateColumns = false;
            this.spgvExistMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");
            Panel p4 = (Panel)GetControltByMaster("Panel4");
            p4.Controls.Add(this.spgvExistMaterial);
            this.spgvExistMaterial.Columns.Clear();
            this.spgvExistMaterial.AllowPaging = true;
            this.spgvExistMaterial.PageSize = 8;
            this.spgvExistMaterial.PageIndexChanging += new GridViewPageEventHandler(spgvExistMaterial_PageIndexChanging);

            this.spgvExistMaterial.PagerTemplate = new PagerTemplate("{0} - {1}", spgvExistMaterial);
            foreach (var kvp in ExistTitlelist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvExistMaterial.Columns.Add(bfColumn);
            }


      

        }

       

        void spgvMaterial_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    BindYesorNo(e, ddlyn, 13);
                }
            }
        }

        /// <summary>
        /// 绑定信息是否与线下信息一致
        /// </summary>
        /// <param name="e">e</param>
        /// <param name="name">DropDownList对象</param>
        /// <param name="cellIdx">单元格在GridView中对应行的索引</param>
        private void BindYesorNo(GridViewRowEventArgs e, DropDownList name, int cellIdx)
        {
            name = (DropDownList)e.Row.Cells[cellIdx].Controls[0];
            List<string> dataText = new List<string>();
            List<string> dataValue = new List<string>();
            dataText.Add("--请选择--");
            dataText.Add("根/台/套/件");
            dataText.Add("米");
            dataText.Add("吨");
            dataValue.Add("0");
            dataValue.Add("1");
            dataValue.Add("2");
            dataValue.Add("3");



            Dictionary<string, string> dic = new Dictionary<string, string>();
            for (int i = 0; i < dataText.Count && i < dataValue.Count; ++i)
            {
                dic.Add(dataText[i], dataValue[i]);
            }

            name.DataSource = dic;
            name.DataTextField = "Key";
            name.DataValueField = "Value";
            name.AutoPostBack = false;
            name.DataBind();



        }

        private void BindDataToCustomControls()
        {
            //初始化查询参数
           

            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {

              

                this.spgvMaterial.DataSource = from a in db.TableOfStocks
                                               where a.MaterialInfo.FinanceCode.Contains(txtfinanceCode.Text.Trim()) && a.MaterialInfo.MaterialName.Contains(txtMaterialName.Text.Trim())
                                               select new
                                               {
                                                   a.StocksID,
                                                   storagecode=(db.StorageInMain.SingleOrDefault(u=>u.StorageInID==a.StorageID)).StorageInCode,
                                                   a.MaterialInfo.MaterialName,
                                                   a.MaterialInfo.SpecificationModel,
                                                   a.MaterialInfo.FinanceCode,
                                                   a.ProjectInfo.ProjectName,
                                                   a.QuantityGentaojian,
                                                   a.QuantityMetre,
                                                   a.QuantityTon,
                                                   a.CurUnit,
                                                   a.BatchIndex,
                                                   a.Manufacturer.ManufacturerName,
                                                   a.SupplierInfo.SupplierName
                                               };
                
             

                //this.spgvMaterial.PagerSettings.NextPageText = "下一页";
                //this.spgvMaterial.PagerSettings.LastPageText = "尾页";
                //this.spgvMaterial.PagerSettings.PreviousPageText = "上一页";
                //this.spgvMaterial.PagerSettings.FirstPageText = "首页";
                //this.spgvMaterial.PagerSettings.Mode = PagerButtons.NextPreviousFirstLast;
                //this.spgvMaterial.PagerSettings.Position = PagerPosition.Bottom;
                //this.spgvMaterial.PagerStyle.HorizontalAlign = HorizontalAlign.Right;
                //this.spgvMaterial.PagerTemplate = null;


                this.spgvMaterial.DataBind();
                this.spgvMaterial.Columns[this.spgvMaterial.Columns.Count - 1].Visible = false;
                

                this.spgvExistMaterial.DataSource = from a in db.StockOnline
                                                  
                                                    select new
                                                    {
                                                        storagecode = (db.StorageInMain.SingleOrDefault(u => u.StorageInID == a.StorageID)).StorageInCode,
                                                        a.MaterialInfo.MaterialName,
                                                        a.MaterialInfo.SpecificationModel,
                                                        a.MaterialInfo.FinanceCode,
                                                        a.OnlineCode,
                                                        a.CurQuantity,
                                                        a.OrderNum,
                                                        a.CertificateNum,
                                                        a.OnlineTotal,
                                                        a.OnlineUnitPrice,
                                                        a.OnlineUnit,
                                                        a.OnlineDate,
                                                        a.ProjectInfo.ProjectName,
                                                        a.BatchIndex,
                                                       
                                                    };
                this.spgvExistMaterial.DataBind();

            }
        }

        private void ShowCustomControls()
        {
            
            //this.spgvMaterial.PagerTemplate = null;
            //this.spgvMaterial.Columns[16].Visible = false;
            //this.spgvMaterial.Columns[17].Visible = false;

            //还未加入回收物资的情况
            if (spgvExistMaterial.Rows.Count == 0)
            {
                RemoveControltFromMaster("Panel3");
                RemoveControltFromMaster("Panel4");
                RemoveControltFromMaster("Panel2");
            }
            else
            {
                
            }

            if (this.spgvMaterial.Rows.Count == 0)
                RemoveControltFromMaster("Panel1");
        }


       
        

        #endregion

        #region 控件事件

        void spgvMaterial_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            ((SPGridView)sender).PageIndex = e.NewPageIndex;
            ((SPGridView)sender).DataBind();
        }

        void spgvExistMaterial_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            ((SPGridView)sender).PageIndex = e.NewPageIndex;
            ((SPGridView)sender).DataBind();
        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
           
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {


        }

        void chbShowAll_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (!chbShowAll.Checked)
                    chbShowAll.Enabled = true;
                else
                {
                    txtfinanceCode.Text = string.Empty;
                    txtMaterialName.Text = string.Empty;

                    chbShowAll.Enabled = false;
                }
                Response.AddHeader("Refresh", "0");
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
        }

        void btnOK_Click(object sender, EventArgs e)
        {

           


            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    List<CheckBox> listString = GetCheckedID();
                    if (listString.Count != 1)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择一条且仅能有一条记录进行编辑!')</script>");
                        return;
                    }


                    CheckBox chb;
                    int iCount = 0;
                    int iSocksID;
                    //MaterialOnLine.TempDataEdit so;
                    //StockOnline so;
                    foreach (GridViewRow gvr in spgvMaterial.Rows)
                    {
                        chb = (CheckBox)gvr.Cells[0].Controls[0];
                        if (!chb.Checked)
                            continue;

                        //将选中项保存到数据库
                        iSocksID = Convert.ToInt32(gvr.Cells[this.spgvMaterial.Columns.Count - 1].Text);
                        //so = new MMSPro.WebApp.MaterialOnLine.TempDataEdit();
                        //so = new StockOnline();
                        if (MaterialOnLine.TempDataEdit.locked == false)
                        {
                            //锁住静态类
                            MaterialOnLine.TempDataEdit.locked = true;
                            MaterialOnLine.TempDataEdit.iSocksID = iSocksID;

                            MaterialOnLine.TempDataEdit.OrderNum = (gvr.Cells[9].Controls[0] as TextBox).Text.Trim();
                            MaterialOnLine.TempDataEdit.OnlineCode = (gvr.Cells[10].Controls[0] as TextBox).Text.Trim();
                            MaterialOnLine.TempDataEdit.OnlineDate = (gvr.Cells[11].Controls[0] as DateTimeControl).SelectedDate;
                            MaterialOnLine.TempDataEdit.CertificateNum = (gvr.Cells[12].Controls[0] as TextBox).Text.Trim();
                            MaterialOnLine.TempDataEdit.OnlineUnit = (gvr.Cells[13].Controls[0] as DropDownList).SelectedItem.Text;
                            MaterialOnLine.TempDataEdit.CurQuantity = Convert.ToDecimal((gvr.Cells[14].Controls[0] as TextBox).Text.Trim());
                            MaterialOnLine.TempDataEdit.OnlineTotal = Convert.ToDecimal((gvr.Cells[15].Controls[0] as TextBox).Text.Trim());


                            MaterialOnLine.TempDataEdit.StorageInID = Convert.ToInt32(db.TableOfStocks.SingleOrDefault(a => a.StocksID == iSocksID).StorageInID);
                            MaterialOnLine.TempDataEdit.StorageInType = db.TableOfStocks.SingleOrDefault(a => a.StocksID == iSocksID).StorageInType.ToString();
                            MaterialOnLine.TempDataEdit.ReceivingTypeName = db.TableOfStocks.SingleOrDefault(a => a.StocksID == iSocksID).ReceivingTypeName;
                            MaterialOnLine.TempDataEdit.StorageInCode = db.TableOfStocks.SingleOrDefault(a => a.StocksID == iSocksID).StorageInCode;
                            MaterialOnLine.TempDataEdit.BillCode = db.TableOfStocks.SingleOrDefault(a => a.StocksID == iSocksID).BillCode;
                            MaterialOnLine.TempDataEdit.MaterialID = db.TableOfStocks.SingleOrDefault(a => a.StocksID == iSocksID).MaterialID;
                            MaterialOnLine.TempDataEdit.MaterialCode = "N/A";

                            MaterialOnLine.TempDataEdit.QuantityGentaojian = Convert.ToDecimal((gvr.Cells[16].Controls[0] as TextBox).Text.Trim());
                            MaterialOnLine.TempDataEdit.QuantityMetre = Convert.ToDecimal((gvr.Cells[17].Controls[0] as TextBox).Text.Trim());
                            MaterialOnLine.TempDataEdit.QuantityTon = Convert.ToDecimal((gvr.Cells[18].Controls[0] as TextBox).Text.Trim());

                            MaterialOnLine.TempDataEdit.OfflineGentaojian -= MaterialOnLine.TempDataEdit.QuantityGentaojian;
                            MaterialOnLine.TempDataEdit.OfflineMetre -= MaterialOnLine.TempDataEdit.QuantityMetre;
                            MaterialOnLine.TempDataEdit.OfflineTon -= MaterialOnLine.TempDataEdit.QuantityTon;


                            MaterialOnLine.TempDataEdit.CurUnit = db.TableOfStocks.SingleOrDefault(a => a.StocksID == iSocksID).CurUnit;
                            MaterialOnLine.TempDataEdit.UnitPrice = db.TableOfStocks.SingleOrDefault(a => a.StocksID == iSocksID).UnitPrice;
                            MaterialOnLine.TempDataEdit.Amount = db.TableOfStocks.SingleOrDefault(a => a.StocksID == iSocksID).Amount;
                            MaterialOnLine.TempDataEdit.ExpectedProject = db.TableOfStocks.SingleOrDefault(a => a.StocksID == iSocksID).ExpectedProject;
                            MaterialOnLine.TempDataEdit.Remark = db.TableOfStocks.SingleOrDefault(a => a.StocksID == iSocksID).Remark;
                            MaterialOnLine.TempDataEdit.BatchIndex = db.TableOfStocks.SingleOrDefault(a => a.StocksID == iSocksID).BatchIndex;
                            MaterialOnLine.TempDataEdit.ManufacturerID = db.TableOfStocks.SingleOrDefault(a => a.StocksID == iSocksID).ManufacturerID;
                            MaterialOnLine.TempDataEdit.SupplierID = Convert.ToInt32(db.TableOfStocks.SingleOrDefault(a => a.StocksID == iSocksID).SupplierID);
                            MaterialOnLine.TempDataEdit.StorageID = db.TableOfStocks.SingleOrDefault(a => a.StocksID == iSocksID).StorageID;
                            MaterialOnLine.TempDataEdit.PileID = db.TableOfStocks.SingleOrDefault(a => a.StocksID == iSocksID).PileID;
                            MaterialOnLine.TempDataEdit.MaterialsManager = Convert.ToInt32(db.TableOfStocks.SingleOrDefault(a => a.StocksID == iSocksID).MaterialsManager);
                            MaterialOnLine.TempDataEdit.AssetsManager = Convert.ToInt32(db.TableOfStocks.SingleOrDefault(a => a.StocksID == iSocksID).AssetsManager);
                            MaterialOnLine.TempDataEdit.StorageTime = db.TableOfStocks.SingleOrDefault(a => a.StocksID == iSocksID).StorageTime;


                            if (MaterialOnLine.TempDataEdit.CurQuantity != 0)
                            {
                                MaterialOnLine.TempDataEdit.OnlineUnitPrice = Convert.ToDecimal(MaterialOnLine.TempDataEdit.OnlineTotal / MaterialOnLine.TempDataEdit.CurQuantity);
                            }

                            MaterialOnLine.TempDataEdit.Creator = db.EmpInfo.SingleOrDefault(u => u.Account == SPContext.Current.Web.CurrentUser.LoginName).EmpID;
                           

                        }

                        //HttpCookie UserCookie = new HttpCookie("temponlinedata");
                        //UserCookie. = so;
                        

                        //Session["temponlinedata"] = so;

                        //db.StockOnline.InsertOnSubmit(so);




#region modefy by adonis 2011-2-15
                        ////修改库存表线上状态
                        //TableOfStocks to = db.TableOfStocks.SingleOrDefault(u => u.StocksID == iSocksID);

                        ////转线上后，库存剩余数量
                        //switch (so.OnlineUnit)
                        //{
                        //    case "根/台/套/件":
                        //        to.QuantityGentaojian -= so.CurQuantity;
                        //        break;
                        //    case "米":
                        //        to.QuantityMetre -= so.CurQuantity;
                        //        break;
                        //    case "吨":
                        //        to.QuantityTon -= so.CurQuantity;
                        //        break;


                        //}
                        ////so.QuantityGentaojian = to.
                        ////修改库存表对应单位数量
                        //to.QuantityGentaojian -= so.QuantityGentaojian;
                        //to.QuantityMetre -= so.QuantityMetre;
                        //to.QuantityTon -= so.QuantityTon;
#endregion

                        iCount++;
                        //db.SubmitChanges();

                        //没有选中的情况
                        if (iCount == 0)
                        {
                            Response.Write("<script language='javaScript'>alert('没有选中要转线上的的物资明细！');</script>");
                            return;
                        }
                        Response.Redirect("MaterialsOnLineEdit.aspx?StockID=" + iSocksID + "&&Quantity=" + MaterialOnLine.TempDataEdit.CurQuantity + "&&CurUnit=" + MaterialOnLine.TempDataEdit.OnlineUnit + "", false);

                        
                    }
                    
                }
            }

            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
            }


            //转线上
            //try
            //{
            //    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            //    {
            //        CheckBox chb;
            //        int iCount = 0;
            //        int iSocksID;
            //        StockOnline so;
            //        foreach (GridViewRow gvr in spgvMaterial.Rows)
            //        {
            //            chb = (CheckBox)gvr.Cells[0].Controls[0];
            //            if (!chb.Checked)
            //                continue;

            //            //将选中项保存到数据库
            //            iSocksID = Convert.ToInt32(gvr.Cells[this.spgvMaterial.Columns.Count-1].Text);


            //            so = new StockOnline();
            //            so.TableOfStocksID = iSocksID;
            //            so.OrderNum = (gvr.Cells[9].Controls[0] as TextBox).Text.Trim();
            //            so.OnlineCode = (gvr.Cells[10].Controls[0] as TextBox).Text.Trim();
            //            so.OnlineDate = (gvr.Cells[11].Controls[0] as DateTimeControl).SelectedDate;
            //            so.CertificateNum = (gvr.Cells[12].Controls[0] as TextBox).Text.Trim();
            //            so.OnlineUnit = (gvr.Cells[13].Controls[0] as DropDownList).SelectedItem.Text;
            //            so.CurQuantity = Convert.ToDecimal((gvr.Cells[14].Controls[0] as TextBox).Text.Trim());
            //            so.OnlineTotal= Convert.ToDecimal((gvr.Cells[15].Controls[0] as TextBox).Text.Trim());
            //            if (so.CurQuantity != 0)
            //            {
            //                so.OnlineUnitPrice = so.OnlineTotal / so.CurQuantity;
            //            }
                        
            //            so.Creator = db.EmpInfo.SingleOrDefault(u => u.Account == SPContext.Current.Web.CurrentUser.LoginName).EmpID;

                        

            //            var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
            //            so.CreateTime = SevTime.First();
            //            db.StockOnline.InsertOnSubmit(so);





            //            //修改库存表线上状态
            //            TableOfStocks to = db.TableOfStocks.SingleOrDefault(u => u.StocksID == so.TableOfStocksID);
            //            //转线上后，库存剩余数量
            //            switch (so.OnlineUnit)
            //            { 
            //                case "根/台/套/件":
            //                    to.QuantityGentaojian -= so.CurQuantity;
            //                    break;
            //                case "米":
            //                    to.QuantityMetre -= so.CurQuantity;
            //                    break;
            //                case "吨":
            //                    to.QuantityTon -= so.CurQuantity;
            //                    break;

                                
            //            }
            //            //to.OnlineState = "线上";
            //            //so.TableOfStocks.OnlineState = "线上";
                   

            //            iCount++;
            //        }
            //        db.SubmitChanges();

            //        //没有选中的情况
            //        if (iCount == 0)
            //        {
            //            Response.Write("<script language='javaScript'>alert('没有选中要转线上的的物资明细！');</script>");
            //            return;
            //        }
            //    }

            //    Response.AddHeader("Refresh", "0");
            //}
            //catch (Exception ex)
            //{
            //    MethodBase mb = MethodBase.GetCurrentMethod();
            //    LogToDBHelper lhelper = LogToDBHelper.Instance;
            //    lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
            //    ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
            //}

        }

        /// <summary>
        /// 返回选中的列表
        /// </summary>
        /// <returns>返回list构成的列表</returns>
        private List<CheckBox> GetCheckedID()
        {
            List<CheckBox> list = new List<CheckBox>();

            foreach (GridViewRow row in this.spgvMaterial.Rows)
            {

                CheckBox ck = (CheckBox)row.Cells[0].FindControl("SMItem");
                if (ck.Checked)
                {
                    list.Add(ck);
                }
            }
            return list;
        }


        void btnSearch_Click(object sender, EventArgs e)
        {


            //chbShowAll.AutoPostBack = false;
            //chbShowAll.Checked = false;
            //chbShowAll.AutoPostBack = true;
            //chbShowAll.Enabled = true;

        }

        #endregion

        #region 辅助方法
        /// <summary>
        /// 获取控件
        /// </summary>
        /// <param name="controlName">控件的ID名称</param>
        /// <returns>返回Control，需要强制类型转换为对应控件</returns>
        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }

        protected void RemoveControltFromMaster(string controlName)
        {
            Control ctr = this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
            this.Master.FindControl("PlaceHolderMain").Controls.Remove(ctr);

        }

        private int GetPricingIndex(string curunit)
        {
            switch (curunit)
            {
                case "根/台/套/件":
                    return 5;
                case "米":
                    return 7;
                case "吨":
                    return 9;
                default:
                    return -1;
            }
        }

        #endregion
    }
}
