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
    public class StorageInDetailedEdit: System.Web.UI.Page
    {
        MMSProDBDataContext db;
        TextBox txtStorageinNum;


        //TextBox txtMaterialName;
        //TextBox txtMaterialmodel;
        TextBox txtMaterialCode;
        TextBox txtMaterialQuantity;
        TextBox txtMaterialMod;

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
        DropDownList ddlbatch;

        TextBox txtRemark;


        Button btnSave;
        Button btnQuit;
        Literal L2;
        Literal L1;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                InitControl();

                //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello ')</script>");

                if (!string.IsNullOrEmpty(Request.QueryString["StorageDetailedID"]))
                {

                    if (!IsPostBack)
                    {
                        BindUnit();
                        BindBatch();
                        BindDDL();
                        BindStorage();
                        LoadData();
                        BindPile(Convert.ToInt32(this.ddlStorage.SelectedValue));

                    }
                }
                //如果有任务则不能修改批次
                if (!string.IsNullOrEmpty(Request.QueryString["TaskStorageID"]))
                {
                    this.ddlbatch.Enabled = false;
                }
                selUser(this.txtManager, this.txtWarehouseWorker);

               
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }

        }

        private void selUser(TextBox tbox_M, TextBox tbox_W)
        {

            L1 = (Literal)GetControltByMaster("L1");
            L1.Text = JSDialogAid.GetJSForDialog(tbox_M.ClientID, tbox_W.ClientID,"", "../StorageAndPile/SelectUser.aspx");
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

        private void BindPile(int storageId)
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = from a in db.PileInfo
                           where a.StorageID ==storageId 
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

        private void BindBatch()
        {      
            this.ddlbatch.Items.Insert(0, "--请选择--");
            this.ddlbatch.Items.Insert(1, new ListItem("第一批","第一批"));
            this.ddlbatch.Items.Insert(2, new ListItem("第二批", "第二批"));
            this.ddlbatch.Items.Insert(3, new ListItem("第三批", "第三批"));
            this.ddlbatch.Items.Insert(4, new ListItem("第四批", "第四批"));
            this.ddlbatch.Items.Insert(5, new ListItem("第五批", "第五批"));
            this.ddlbatch.Items.Insert(6, new ListItem("第六批", "第六批"));
            this.ddlbatch.Items.Insert(7, new ListItem("第七批", "第七批"));
            this.ddlbatch.Items.Insert(8, new ListItem("第八批", "第八批"));
        }


        private void LoadData()
        {
            //单据ID
            int mainID = 0;
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                int id = Convert.ToInt32(Request.QueryString["StorageDetailedID"]);
                StorageInDetailed SID = db.StorageInDetailed.SingleOrDefault(a => a.StorageDetailedID == id);

           
                if (SID != null)
                {
                    //单据id
                    mainID = SID.StorageInID;
                    StorageIn SI = db.StorageIn.SingleOrDefault(u => u.StorageInID == mainID);

                    this.txtStorageinNum.Text = SI.StorageInCode;

                    this.txtMaterialCode.Text = SID.MaterialInfo.MaterialCode;
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
                    this.ddlbatch.SelectedValue = SID.BatchIndex.Trim();
                    
        
                    this.txtRemark.Text = SID.Remark.ToString();


                }
                else
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('记录不存在! ');</script>");
                    Response.Redirect("StorageInDetailedManage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "");
                }
            }
        }
        private void InitControl()
        {
            this.txtStorageinNum = (TextBox)GetControltByMaster("txtStorageinNum");
            this.txtStorageinNum.Enabled = false;

            //this.txtMaterialName = (TextBox)GetControltByMaster("txtMaterialName");
            //this.txtMaterialmodel = (TextBox)GetControltByMaster("txtMaterialmodel");
            this.txtMaterialCode = (TextBox)GetControltByMaster("txtMaterialCode");
            this.txtMaterialQuantity = (TextBox)GetControltByMaster("txtMaterialQuantity");
            this.txtMaterialMod = (TextBox)GetControltByMaster("txtMaterialMod");

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
            this.ddlbatch = (DropDownList)GetControltByMaster("ddlbatch");
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

        public void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                #region 输入验证
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
                #endregion

                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {

                    MaterialInfo mi = db.MaterialInfo.SingleOrDefault(u => u.MaterialCode == this.txtMaterialCode.Text.Trim());
                    if (mi == null)
                    {

                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('系统中不存在此物料编码，请先创建！')</script>");
                        return;
                    }

                    StorageInDetailed SID = db.StorageInDetailed.SingleOrDefault(k => k.StorageDetailedID == Convert.ToInt32(Request.QueryString["StorageDetailedID"]));
                    SID.StorageInID = Convert.ToInt32(Request.QueryString["StorageInID"]);

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

                    //财务编码重复判断
                    //StorageInDetailed code = db.StorageInDetailed.SingleOrDefault(u => u.financeCode == this.txtMaterialfinance.Text.Trim());

                    //if (code == null)
                    //{
                    //    SID.financeCode = this.txtMaterialfinance.Text.Trim();
                    //}
                    //else
                    //{
                    //    if (SID.StorageDetailedID == code.StorageDetailedID)
                    //    {
                    //        SID.financeCode = this.txtMaterialfinance.Text.Trim();
                    //    }
                    //    else
                    //    {
                    //        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('财务编码重复！')</script>");
                    //        return;
                    //    }
                    //}


                    SID.financeCode = this.txtMaterialfinance.Text.Trim();

                    

                    SID.StorageTime = this.DateTimeStorageIn.SelectedDate;

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
                    SID.BatchIndex = this.ddlbatch.SelectedValue.ToString();

                    SID.Remark = this.txtRemark.Text.Trim();


                    db.SubmitChanges();
                    if (!string.IsNullOrEmpty(Request.QueryString["TaskStorageID"]))
                    {
                        Response.Redirect("StorageInDetailedManage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "");
                    }
                    else
                    {
                        Response.Redirect("StorageInDetailedManage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "");
                    }




                }
            }
            catch(Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_UPDATEERROR));
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

        public void btnQuit_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request.QueryString["TaskStorageID"]))
            {
                Response.Redirect("StorageInDetailedManage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "");
            }
            else
            {
                Response.Redirect("StorageInDetailedManage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "");
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
