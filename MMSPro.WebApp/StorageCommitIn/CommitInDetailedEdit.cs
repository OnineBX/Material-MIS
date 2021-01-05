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
    public class CommitInDetailedEdit:System.Web.UI.Page
    {
        MMSProDBDataContext db;
        TextBox txtStorageinNum;

        TextBox txtMaterialCode;
        TextBox txtMaterialQuantity;
        TextBox txtMaterialMod;
        TextBox txtID;

        TextBox txtGTJ;
        TextBox txtMetre;
        TextBox txtTon;
        DropDownList ddlUnit;

        TextBox txtMaterialUnitPrice;
        TextBox txtMaterialAmount;
        DropDownList ddlStorage;
        DropDownList ddlPile;
        TextBox txtMaterialfinance;
        DateTimeControl DateTimeStorageIn;

        DropDownList ddlSupplier;
        TextBox txtManager;
        TextBox txtWarehouseWorker;
        TextBox txtRemark;
        TextBox txtCommitMaterialCode;

        Button btnSave;
        Button btnQuit;


        Literal L1;
        Literal L2;
        Literal L3;

        string cmid;
        string strCmit;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                InitControl();
               
                //再次新建初始化委外物料名称
                if (!string.IsNullOrEmpty(Request.QueryString["CommitMid"]))
                {
                    cmid = Request.QueryString["CommitMid"];
                    using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        StorageCommitOutRealDetails scord = db.StorageCommitOutRealDetails.SingleOrDefault(u => u.StorageCommitOutRealDetailsID == Convert.ToInt32(cmid));
                        strCmit = "物料名称:" + scord.StorageCommitOutDetails.TableOfStocks.MaterialInfo.MaterialName + " | " + "物料编码:" + scord.StorageCommitOutDetails.TableOfStocks.MaterialCode;

                        this.txtCommitMaterialCode.Text = strCmit;
                        this.txtID.Text = scord.StorageCommitOutRealDetailsID.ToString();
                    }

                }



                selUser(this.txtManager, this.txtWarehouseWorker);
                selectUser(this.txtCommitMaterialCode, this.txtID, "../StorageCommitIn/SelectCommitMaterial.aspx");

                if (!IsPostBack)
                {
                    BindUnit();
                    BindDefaultDate();
                    BindDDL();
                    BindStorage();
                    LoadData();
                    BindPile(Convert.ToInt32(this.ddlStorage.SelectedValue));
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
        private void selUser(TextBox tbox_M, TextBox tbox_W)
        {

            L1 = (Literal)GetControltByMaster("L1");
            L1.Text = JSDialogAid.GetJSForDialog(tbox_M.ClientID, tbox_W.ClientID, "", "../StorageAndPile/SelectUser.aspx");
        }
        /// <summary>
        /// 接受模式窗体返回数据
        /// </summary>
        /// <param name="tb">textbox对象</param>
        /// <param name="lb">lable对象</param>
        /// <param name="url">url</param>
        private void selectUser(TextBox tb, TextBox txtid, string url)
        {

            L3 = (Literal)GetControltByMaster("L3");
            L3.Text = JSDialogAid.GetDialogInfo(tb.ClientID, txtid.ClientID, url);
        }
        private void BindDefaultDate()
        {
            int id = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["CommitInID"]))
            {
                id = Convert.ToInt32(Request.QueryString["CommitInID"]);
                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    CommitIn temp = db.CommitIn.SingleOrDefault(u => u.CommitInID == id);
                    this.txtStorageinNum.Text = temp.CommitInCode.ToString();
                    this.txtStorageinNum.Enabled = false;
                }
            }
            else
            {
                Response.Redirect("CommitInManage.aspx");
            }


        }

        private void BindPile(int storageId)
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = from a in db.PileInfo
                           where a.StorageID == storageId
                           select new
                           {
                               Key = a.PileCode,
                               Value = a.PileID
                           };

                this.ddlPile.DataSource = temp;
                this.ddlPile.DataTextField = "Key";
                this.ddlPile.DataValueField = "Value";
                this.ddlPile.DataBind();
                this.ddlPile.Items.Insert(0, "--请选择--");
            }
        }

        private void LoadData()
        {
            //单据ID
            int mainID = 0;
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                int id = Convert.ToInt32(Request.QueryString["CommitDetailedID"]);
                CommitInDetailed SID = db.CommitInDetailed.SingleOrDefault(a => a.CommitDetailedID == id);


                if (SID != null)
                {
                    //单据id
                    mainID = SID.CommitInID;
                    CommitIn SI = db.CommitIn.SingleOrDefault(u => u.CommitInID == mainID);

                    this.txtStorageinNum.Text = SI.CommitInCode;

                    this.txtMaterialCode.Text = SID.MaterialInfo.FinanceCode;
                    this.txtMaterialMod.Text = SID.SpecificationModel;
                    this.txtMaterialQuantity.Text = SID.Quantity.ToString();

                    this.txtGTJ.Text = SID.QuantityGentaojian.ToString();
                    this.txtMetre.Text = SID.QuantityMetre.ToString();
                    this.txtTon.Text = SID.QuantityTon.ToString();
                    this.ddlUnit.SelectedIndex = reIndex(SID.CurUnit.ToString());

                    this.txtMaterialUnitPrice.Text = SID.UnitPrice.ToString();
                    this.txtMaterialAmount.Text = SID.Amount.ToString();
                    this.ddlStorage.SelectedValue = SID.PileInfo.StorageID.ToString();

                    this.ddlPile.SelectedValue = SID.PileInfo.PileID.ToString();
                    this.txtMaterialfinance.Text = SID.financeCode.ToString();
                    this.DateTimeStorageIn.SelectedDate = SID.StorageTime;
                    this.ddlSupplier.SelectedValue = SID.SupplierInfo.SupplierID.ToString();
                    this.txtManager.Text = db.EmpInfo.SingleOrDefault(u => u.EmpID == SID.MaterialsManager).Account;
                    this.txtWarehouseWorker.Text = db.EmpInfo.SingleOrDefault(u => u.EmpID == SID.WarehouseWorker).Account;
                    this.txtRemark.Text = SID.Remark.ToString();

                    //再次新建初始化委外物料名称
                    //关系表存放的新建物料索引值唯一
                    RelationCommitIn rci = db.RelationCommitIn.SingleOrDefault(u => u.CommitMaterial == SID.CommitDetailedID);

                    StorageCommitOutRealDetails scord = db.StorageCommitOutRealDetails.SingleOrDefault(u => u.StorageCommitOutRealDetailsID == rci.CommitOutMaterial);
                    strCmit = "物料名称:" + scord.StorageCommitOutDetails.TableOfStocks.MaterialInfo.MaterialName + " | " + "物料编码:" + scord.StorageCommitOutDetails.TableOfStocks.MaterialInfo.MaterialCode;

                    this.txtCommitMaterialCode.Text = strCmit;
                    this.txtID.Text = scord.StorageCommitOutRealDetailsID.ToString();




                }
                else
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('记录不存在! ');</script>");
                    Response.Redirect("CommitInDetailedManage.aspx?CommitInID=" + Request.QueryString["CommitInID"] + "");
                }
            }
        }

        /// <summary>
        /// 返回下来列表索引
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private int reIndex(string value)
        {
            switch (value)
            {
                case "根/套/件":
                    return 1;
                case "米":
                    return 2;
                case "吨":
                    return 3;
                default:
                    return 0;

            }
        }

        private void InitControl()
        {

            this.txtStorageinNum = (TextBox)GetControltByMaster("txtStorageinNum");
            this.txtMaterialCode = (TextBox)GetControltByMaster("txtMaterialCode");
            this.txtCommitMaterialCode = (TextBox)GetControltByMaster("txtCommitMaterialCode");
            this.txtMaterialMod = (TextBox)GetControltByMaster("txtMaterialMod");
            this.txtMaterialQuantity = (TextBox)GetControltByMaster("txtMaterialQuantity");

            this.txtID = (TextBox)GetControltByMaster("txtID");

            this.txtGTJ = (TextBox)GetControltByMaster("txtGTJ");
            this.txtMetre = (TextBox)GetControltByMaster("txtMetre");
            this.txtTon = (TextBox)GetControltByMaster("txtTon");
            this.ddlUnit = (DropDownList)GetControltByMaster("ddlUnit");
            this.ddlUnit.SelectedIndexChanged += new EventHandler(ddlUnit_SelectedIndexChanged);

            this.txtMaterialUnitPrice = (TextBox)GetControltByMaster("txtMaterialUnitPrice");
            this.txtMaterialAmount = (TextBox)GetControltByMaster("txtMaterialAmount");

            this.ddlStorage = (DropDownList)GetControltByMaster("ddlStorage");
            this.ddlStorage.SelectedIndexChanged += new EventHandler(ddlStorage_SelectedIndexChanged);
            this.ddlPile = (DropDownList)GetControltByMaster("ddlPile");

            this.txtMaterialfinance = (TextBox)GetControltByMaster("txtMaterialfinance");
            this.DateTimeStorageIn = (DateTimeControl)GetControltByMaster("DateTimeStorageIn");

            this.ddlSupplier = (DropDownList)GetControltByMaster("ddlSupplier");
            this.txtManager = (TextBox)GetControltByMaster("txtManager");
            this.txtWarehouseWorker = (TextBox)GetControltByMaster("txtWarehouseWorker");


            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");



            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);


            StringBuilder sb = new StringBuilder();
            sb.Append("<script type=\"text/javascript\">");
            sb.Append("function Product()");
            sb.Append("{");
            sb.Append("var numAccounting = document.getElementById('" + this.txtMaterialAmount.ClientID + "');");
            sb.Append("var numlAmount = document.getElementById('" + this.txtMaterialQuantity.ClientID + "');");
            sb.Append("var numQuantity = document.getElementById('" + this.txtMaterialUnitPrice.ClientID + "');");
            sb.Append("if(numlAmount.value !='' && numQuantity.value !='')");
            sb.Append("{");
            sb.Append("var price = parseFloat(numQuantity.value);");
            sb.Append("var count = parseFloat(numlAmount.value)*price;");
            //对结果四舍五入
            sb.Append("count =Math.round(count*100)/100;");
            sb.Append("numAccounting.value =count;");
            sb.Append("}");
            sb.Append("}");
            sb.Append("</script>");

            L2 = (Literal)GetControltByMaster("L2");
            L2.Text = sb.ToString();
            //计算金额
            this.txtMaterialQuantity.Attributes.Add("onpropertychange", "Product()");
            this.txtMaterialUnitPrice.Attributes.Add("onpropertychange", "Product()");
        }

        void ddlUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.ddlUnit.SelectedItem.Text)
            {
                case "--请选择--":
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择单位')</script>");
                    this.txtMaterialQuantity.Text = string.Empty;
                    return;

                case "根/套/件":
                    if (this.txtGTJ.Text.Trim() != string.Empty)
                    {
                        this.txtMaterialQuantity.Text = this.txtGTJ.Text.Trim();
                    }
                    break;
                case "米":
                    if (this.txtMetre.Text.Trim() != string.Empty)
                    {
                        this.txtMaterialQuantity.Text = this.txtMetre.Text.Trim();
                    }
                    break;
                case "吨":
                    if (this.txtTon.Text.Trim() != string.Empty)
                    {
                        this.txtMaterialQuantity.Text = this.txtTon.Text.Trim();
                    }
                    break;

            }
        }

        /// <summary>
        /// 绑定单位类型
        /// </summary>
        private void BindUnit()
        {
            List<string> dataText = new List<string>();
            List<string> dataValue = new List<string>();
            dataText.Add("--请选择--");
            dataText.Add("根/套/件");
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

            ddlUnit.DataSource = dic;
            ddlUnit.DataTextField = "Key";
            ddlUnit.DataValueField = "Value";
            ddlUnit.DataBind();



        }

        void ddlStorage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ddlStorage.SelectedIndex != 0)
            {
                BindPile();
            }
        }

        private void BindDDL()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = from a in db.SupplierInfo
                           select new
                           {
                               Key = a.SupplierName,
                               Value = a.SupplierID
                           };

                this.ddlSupplier.DataSource = temp;
                this.ddlSupplier.DataTextField = "Key";
                this.ddlSupplier.DataValueField = "Value";
                this.ddlSupplier.DataBind();
                this.ddlSupplier.Items.Insert(0, "--请选择--");
            }
        }


        private void BindStorage()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = from a in db.StorageInfo
                           select new
                           {
                               Key = a.StorageName,
                               Value = a.StorageID
                           };

                this.ddlStorage.DataSource = temp;
                this.ddlStorage.DataTextField = "Key";
                this.ddlStorage.DataValueField = "Value";
                this.ddlStorage.DataBind();
                this.ddlStorage.Items.Insert(0, "--请选择--");
            }
        }

        private void BindPile()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = from a in db.PileInfo
                           where a.StorageID == Convert.ToInt32(this.ddlStorage.SelectedValue)
                           select new
                           {
                               Key = a.PileCode,
                               Value = a.PileID
                           };

                this.ddlPile.DataSource = temp;
                this.ddlPile.DataTextField = "Key";
                this.ddlPile.DataValueField = "Value";
                this.ddlPile.DataBind();
                this.ddlPile.Items.Insert(0, "--请选择--");
            }
        }


        public void btnSave_Click(object sender, EventArgs e)
        {

            try
            {

                if (!PageValidate.IsNumberTwoDecimal(this.txtMaterialQuantity.Text.Trim()))
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('数量只能是数字 ')</script>");
                    return;
                }

                if (!PageValidate.IsNumberTwoDecimal(this.txtMaterialUnitPrice.Text.Trim()))
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('单价只能是数字,且只能有两位小数 ')</script>");
                    return;
                }

                if (!PageValidate.IsNumberTwoDecimal(this.txtMaterialAmount.Text.Trim()))
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('金额只能是数字,且只能有两位小数 ')</script>");
                    return;
                }



                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {

                    MaterialInfo mi = db.MaterialInfo.SingleOrDefault(u => u.MaterialCode == this.txtMaterialCode.Text.Trim());
                    if (mi == null)
                    {

                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('系统中不存在此物料编码，请先创建！')</script>");
                        return;
                    }

                    CommitInDetailed SID = db.CommitInDetailed.SingleOrDefault(u => u.CommitDetailedID == Convert.ToInt32(Request.QueryString["CommitDetailedID"]));
                    SID.CommitInID = Convert.ToInt32(Request.QueryString["CommitInID"]);

                    SID.MaterialID = mi.MaterialID;
                    SID.SpecificationModel = this.txtMaterialMod.Text.Trim();

                    SID.QuantityGentaojian = Convert.ToDecimal(this.txtGTJ.Text.Trim());
                    SID.QuantityMetre = Convert.ToDecimal(this.txtMetre.Text.Trim());
                    SID.QuantityTon = Convert.ToDecimal(this.txtTon.Text.Trim());
                    if (this.ddlUnit.SelectedItem.Text == "--请选择--")
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('所选单位不能为空')</script>");
                        return;
                    }
                    SID.CurUnit = this.ddlUnit.SelectedItem.Text;

                    SID.Quantity = Convert.ToDecimal(this.txtMaterialQuantity.Text.Trim());
                    SID.UnitPrice = Convert.ToDecimal(this.txtMaterialUnitPrice.Text.Trim());
                    SID.Amount = Convert.ToDecimal(this.txtMaterialAmount.Text.Trim());

                    if (this.ddlStorage.SelectedIndex == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择所属仓库！')</script>");
                        return;
                    }

                    if (this.ddlPile.SelectedIndex == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择所属垛位！')</script>");
                        return;
                    }


                    SID.PileID = Convert.ToInt32(this.ddlPile.SelectedValue.Trim());
                    ////财务编码重复
                    //CommitInDetailed code = db.CommitInDetailed.SingleOrDefault(u => u.financeCode == this.txtMaterialfinance.Text.Trim());
                    //if (code != null)
                    //{
                    //    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('财务编码重复！')</script>");
                    //    return;
                    //}

                    SID.financeCode = this.txtMaterialfinance.Text.Trim();

                    SID.StorageTime = this.DateTimeStorageIn.SelectedDate;

                    if (this.ddlSupplier.SelectedIndex == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择供应商！')</script>");
                        return;
                    }
                    SID.SupplierID = Convert.ToInt32(this.ddlSupplier.SelectedValue.Trim());


                    if (this.ddlSupplier.SelectedIndex == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择供应商！')</script>");
                        return;
                    }
                    SID.SupplierID = Convert.ToInt32(this.ddlSupplier.SelectedValue.Trim());
                    if (reEmpId(this.txtWarehouseWorker.Text.Trim()) == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('不存在物资管理员,请同步AD账户！')</script>");
                        return;
                    }
                    if (reEmpId(this.txtManager.Text.Trim()) == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('不存在仓库员,请同步AD账户！')</script>");
                        return;
                    }

                    SID.MaterialsManager = reEmpId(this.txtManager.Text.Trim());
                    SID.WarehouseWorker = reEmpId(this.txtWarehouseWorker.Text.Trim());

                    SID.Remark = this.txtRemark.Text.Trim();








                    db.SubmitChanges();


                    if (!string.IsNullOrEmpty(this.txtID.Text.Trim()))
                    {
                        //更新委外关系表
                        RelationCommitIn rc = db.RelationCommitIn.SingleOrDefault(u => u.CommitMaterial == Convert.ToInt32(Request.QueryString["CommitDetailedID"]));
                        rc.CommitMaterial = SID.CommitDetailedID;
                        rc.CommitOutMaterial = Convert.ToInt32(this.txtID.Text.Trim());
                        rc.CreateTime = this.DateTimeStorageIn.SelectedDate;
                        db.SubmitChanges();
                    }
                    else
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('委外数据获取异常！')</script>");
                        return;
                    }



                    //判断是否有任务
                    if (!string.IsNullOrEmpty(Request.QueryString["TaskStorageID"]))
                    {
                        Response.Redirect("CommitInDetailedManage.aspx?CommitInID=" + Request.QueryString["CommitInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&CommitMid=" + this.txtID.Text.Trim() + "");
                    }
                    else
                    {
                        Response.Redirect("CommitInDetailedManage.aspx?CommitInID=" + Request.QueryString["CommitInID"] + "&&CommitMid=" + this.txtID.Text.Trim() + "");
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
        public void btnQuit_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request.QueryString["TaskStorageID"]))
            {
                Response.Redirect("CommitInDetailedManage.aspx?CommitInID=" + Request.QueryString["CommitInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "");
            }
            else
            {
                Response.Redirect("CommitInDetailedManage.aspx?CommitInID=" + Request.QueryString["CommitInID"] + "");
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
