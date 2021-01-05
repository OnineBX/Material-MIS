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
    public class MaterialsOnLineEdit:System.Web.UI.Page
    {


        MMSProDBDataContext db;
        SPGridView gv;
        int UserLoginId;
        int _stockID;
        decimal _quantity;
        string _CurUnit;
        Button btnOK;
        Label lblInfo;
        Label lblout;
        static string[] Titlelist = {
                                     "入库单号:StorageCommitOutNoticeCode",
                                      "项目名称:ProjectName",
                                      "根/台/套/件 数量:RealGentaojian",
                                      "米 数量:RealMetre",
                                      "吨 数量:RealTon",
             
                                      "入库类型:type",
                                      "PID:ProjectID",
                                      "ID:StocksID"
                                      
                                    };
        protected void Page_Load(object sender, EventArgs e)
        {
            _stockID = Convert.ToInt32(Request.QueryString["StockID"]);
            if (!string.IsNullOrEmpty(Request.QueryString["Quantity"].ToString().Trim()))
            {
                _quantity = Convert.ToDecimal(Request.QueryString["Quantity"].ToString().Trim());
            }
            _CurUnit = Request.QueryString["CurUnit"];
            this.gv = new SPGridView();
            this.gv.AutoGenerateColumns = false;

            try
            {
                //获取登录用户ID
                UserLoginId = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);

                BindGridView();

                LoadDate();
                autoComplite();
                initControls();
            }

            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
        }

        private void initControls()
        {

            this.lblInfo = (Label)GetControltByMaster("lblInfo");
            this.lblInfo.Text = "需要转为线上物资的数量：" + _quantity;

            this.lblout = (Label)GetControltByMaster("lblout");

            this.btnOK = (Button)GetControltByMaster("btnOK");
            this.btnOK.Click += new EventHandler(btnOK_Click);

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

        void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    //decimal count = 0;
                    decimal countcell = 0;
                    for (int a = 0; a < this.gv.Rows.Count; a++)
                    {
                        //switch (this.gv.Rows[a].Cells[6].Text.Trim())
                        //{ 
                        //    case "根/台/套/件":
                        //        count += Convert.ToDecimal(this.gv.Rows[a].Cells[2].Text.Trim());
                                
                        //        break;
                        //    case "米":
                        //        count += Convert.ToDecimal(this.gv.Rows[a].Cells[3].Text.Trim());

                        //        break;
                        //    case "吨":
                        //        count += Convert.ToDecimal(this.gv.Rows[a].Cells[4].Text.Trim());
                        //        break;
                        //}
                        countcell += Convert.ToDecimal((this.gv.Rows[a].Cells[5].Controls[0] as TextBox).Text.Trim());
                       
                    }
                    if (countcell > _quantity)
                    {
                        lblout.Visible = true;
                        lblout.Text = "默认填写物资数量为：" + countcell + "，已超出待转线上数值，请修改！";
                        return;
                    }


                    #region modify by adonis 2011-2-15
                    //写入线上库存

                    StockOnline so = new StockOnline();
                    so.OrderNum = MaterialOnLine.TempDataEdit.OrderNum;



                    so.OnlineCode = MaterialOnLine.TempDataEdit.OnlineCode;
                    so.OnlineDate = MaterialOnLine.TempDataEdit.OnlineDate;
                    so.CertificateNum = MaterialOnLine.TempDataEdit.CertificateNum;
                    so.OnlineUnit = MaterialOnLine.TempDataEdit.OnlineUnit;
                    so.CurQuantity = MaterialOnLine.TempDataEdit.CurQuantity;
                    so.OnlineTotal = MaterialOnLine.TempDataEdit.OnlineTotal;


                    so.StorageInID = MaterialOnLine.TempDataEdit.StorageInID;
                    so.StorageInType = MaterialOnLine.TempDataEdit.StorageInType;
                    so.ReceivingTypeName = MaterialOnLine.TempDataEdit.ReceivingTypeName;
                    so.StorageInCode = MaterialOnLine.TempDataEdit.StorageInCode;
                    so.BillCode = MaterialOnLine.TempDataEdit.BillCode;
                    so.MaterialID = MaterialOnLine.TempDataEdit.MaterialID;
                    so.MaterialCode = "N/A";

                    so.QuantityGentaojian = MaterialOnLine.TempDataEdit.OnlineTotal;
                    so.QuantityMetre = MaterialOnLine.TempDataEdit.OnlineTotal;
                    so.QuantityTon = MaterialOnLine.TempDataEdit.OnlineTotal;

                    so.OfflineGentaojian -= MaterialOnLine.TempDataEdit.QuantityGentaojian;
                    so.OfflineMetre -= MaterialOnLine.TempDataEdit.QuantityMetre;
                    so.OfflineTon -= MaterialOnLine.TempDataEdit.QuantityTon;


                    so.CurUnit = MaterialOnLine.TempDataEdit.CurUnit;
                    so.UnitPrice = MaterialOnLine.TempDataEdit.UnitPrice;
                    so.Amount = MaterialOnLine.TempDataEdit.Amount;
                    so.ExpectedProject = MaterialOnLine.TempDataEdit.ExpectedProject;
                    so.Remark = MaterialOnLine.TempDataEdit.Remark;
                    so.BatchIndex = MaterialOnLine.TempDataEdit.BatchIndex;
                    so.ManufacturerID = MaterialOnLine.TempDataEdit.ManufacturerID;
                    so.SupplierID = MaterialOnLine.TempDataEdit.SupplierID;
                    so.StorageID = MaterialOnLine.TempDataEdit.StorageID;
                    so.PileID = MaterialOnLine.TempDataEdit.PileID;
                    so.MaterialsManager = MaterialOnLine.TempDataEdit.MaterialsManager;
                    so.AssetsManager =  MaterialOnLine.TempDataEdit.AssetsManager;
                    so.StorageTime = MaterialOnLine.TempDataEdit.StorageTime;

                    so.OnlineUnitPrice = MaterialOnLine.TempDataEdit.OnlineUnitPrice;
                    so.Creator = MaterialOnLine.TempDataEdit.Creator;

                    var SaveTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                    so.CreateTime = SaveTime.First();
                    db.StockOnline.InsertOnSubmit(so);
                    

                    //修改库存表线上状态
                    TableOfStocks to = db.TableOfStocks.SingleOrDefault(u => u.StocksID == MaterialOnLine.TempDataEdit.iSocksID);

                    //转线上后，库存剩余数量
                    switch (so.OnlineUnit)
                    {
                        case "根/台/套/件":
                            to.QuantityGentaojian -= so.CurQuantity;
                            break;
                        case "米":
                            to.QuantityMetre -= so.CurQuantity;
                            break;
                        case "吨":
                            to.QuantityTon -= so.CurQuantity;
                            break;


                    }
                    //so.QuantityGentaojian = to.
                    //修改库存表对应单位数量
                    to.QuantityGentaojian -= so.QuantityGentaojian;
                    to.QuantityMetre -= so.QuantityMetre;
                    to.QuantityTon -= so.QuantityTon;



                    #endregion




                    for (int i = 0; i < this.gv.Rows.Count; i++)
                    {
                        FlowDetailsOffline fdo = new FlowDetailsOffline();
                        fdo.TableOfStocksID = Convert.ToInt32(this.gv.Rows[i].Cells[this.gv.Columns.Count - 1].Text.Trim());
                        fdo.StorageType = this.gv.Rows[i].Cells[7].Text.Trim();
                        fdo.StorageOutCode = this.gv.Rows[i].Cells[0].Text.Trim();
                        fdo.StorageOutProject = Convert.ToInt32(this.gv.Rows[i].Cells[this.gv.Columns.Count - 2].Text.Trim());
                        //fdo.CurUnit = this.gv.Rows[i].Cells[6].Text.Trim();

                        TextBox tboxCurQuantity = (TextBox)this.gv.Rows[i].Cells[5].Controls[0];
                        fdo.CurQuantity = Convert.ToDecimal(tboxCurQuantity.Text.Trim());

                        TextBox Curunit = (TextBox)this.gv.Rows[i].Cells[6].Controls[0];
                        fdo.CurUnit = Curunit.Text.Trim();

                        fdo.RealGentaojian = Convert.ToDecimal(this.gv.Rows[i].Cells[2].Text.Trim());
                        fdo.RealMetre = Convert.ToDecimal(this.gv.Rows[i].Cells[3].Text.Trim());
                        fdo.RealTon = Convert.ToDecimal(this.gv.Rows[i].Cells[4].Text.Trim());
                        fdo.Creator = db.EmpInfo.SingleOrDefault(u => u.Account == SPContext.Current.Web.CurrentUser.LoginName).EmpID;
                        var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                        fdo.CreateTime = SevTime.First();
                        db.FlowDetailsOffline.InsertOnSubmit(fdo);
                        db.SubmitChanges();
                    }
                    //解开静态类锁
                    MaterialOnLine.TempDataEdit.locked = false;

                }
                Response.Redirect("MaterialOnLineManage.aspx", false);
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
            }
            //写入物资追踪表
            
        }

        private void LoadDate()
        {
            for (int i = 0; i < this.gv.Rows.Count; i++)
            {
                TextBox Curunit = (TextBox)this.gv.Rows[i].Cells[6].Controls[0];
                Curunit.Text = _CurUnit;
            }
        }

        /// <summary>
        /// 自动完成数量
        /// </summary>
        private void autoComplite()
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    for (int i = 0; i < this.gv.Rows.Count; i++)
                    {
                        TextBox tboxCount = (TextBox)(this.gv.Rows[i].Cells[5].Controls[0]);

                        switch ((this.gv.Rows[i].Cells[6].Controls[0] as TextBox).Text.Trim())
                        { 
                            case "根/台/套/件":
                                tboxCount.Text = this.gv.Rows[i].Cells[2].Text;
                                break;
                            case "米":
                                tboxCount.Text = this.gv.Rows[i].Cells[3].Text;
                                break;
                            case "吨":
                                tboxCount.Text = this.gv.Rows[i].Cells[4].Text;
                                break;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance; lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
        }


        void tbarbtnBack_Click(object sender, EventArgs e)
        {

            Response.Redirect("MaterialOnLineManage.aspx", false);
        }

        private int reEmpId(string Emptbox)
        {
            int reID = 0;
            using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {

                EmpInfo ei = dc.EmpInfo.SingleOrDefault(u => u.Account == Emptbox);
                if (ei == null)
                {
                    return 0;
                }
                reID = ei.EmpID;

            }
            return reID;
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


                //线上收料数量
                TemplateField num = new TemplateField();
                num.HeaderText = "转线上数量";
                num.ItemTemplate = new TextBoxTemplate("num", string.Empty, "^(-?\\d+)(\\.\\d+)?$", "", 80);
                this.gv.Columns.Insert(5, num);

                //线上收料单位
                TemplateField unit = new TemplateField();
                unit.HeaderText = "计量单位";
                unit.ItemTemplate = new TextBoxTemplate("unit", string.Empty, "", "", 80,false);
                this.gv.Columns.Insert(6, unit);


                this.gv.DataSource = from a in db.FlowDetails
                                     where a.StocksID == _stockID
                                     select new
                                     {
                                         a.StocksID,
                                         a.ProjectID,
                                         a.StorageCommitOutNoticeCode,
                                         a.ProjectName,
                                         a.RealGentaojian,
                                         a.RealMetre,
                                         a.RealTon,
                                         a.type,

                                     };
                this.gv.DataBind();
                this.gv.Columns[this.gv.Columns.Count - 1].Visible = false;
                this.gv.Columns[this.gv.Columns.Count - 2].Visible = false;
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
