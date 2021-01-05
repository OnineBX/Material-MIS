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
   public class StockTransferShow : System.Web.UI.Page
    {
        private int _noticeid;
        private int _taskid;
        MMSProDBDataContext db;
        private SPGridView spgvProducingAudit;
        private CheckBox chbAgree;
        private TextBox txtOpinion;
     //   private TextBox txtUser;
        private Button btnOK, btnCancel;
      //  private Panel panelUser;
       // Literal L1;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                _taskid = Convert.ToInt32(Request.QueryString["StockTransferTaskID"]);
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    StockTransferTask stt = db.StockTransferTask.SingleOrDefault(a => a.StockTransferTaskID == _taskid);
                    if (stt != null)
                    {
                       // StockTransfer st = db.StockTransfer.SingleOrDefault(a => a.StockTransferID == stt.StockTransferID);
                        _noticeid = stt.StockTransferID;
                        var t = db.StockTransferTask.Where(a => a.StockTransferID == stt.StockTransferID && a.TaskInType == stt.TaskInType).OrderBy(a =>a.StockTransferTaskID).First();
                        stt = t;
                        //加载任务相关信息
                        Label lblReceiver = (Label)GetControltByMaster("lblReceiver");
                        lblReceiver.Text = stt.EmpInfo.EmpName;
                        Label lblDate = (Label)GetControltByMaster("lblDate");
                        lblDate.Text = stt.CreateTime.ToString();

                        StockTransfer st = db.StockTransfer.SingleOrDefault(a => a.StockTransferID == stt.StockTransferID);
                        if (st != null)
                        {
                            Label lblNoticeCode = (Label)GetControltByMaster("lblNoticeCode");
                            lblNoticeCode.Text = st.StockTransferNum;
                        }
                        //判断是否已经发送过任务
                        var n = db.StockTransferTask.Where(a => a.StockTransferID == stt.StockTransferID && a.TaskInType == stt.TaskInType).OrderByDescending(a => a.StockTransferTaskID).First();
                        btnOK = (Button)GetControltByMaster("btnOK");
                        if (n.StockTransferTaskID > _taskid)
                        {
                            //任务已发送
                              btnOK.Enabled = false;                              
                        }
                        else
                        {
                            //任务未发送
                            btnOK.Enabled = true;
                            
                        }
                        //判断任务是否已经完成
                        StockTransferTask stt2 = db.StockTransferTask.SingleOrDefault(a => a.StockTransferTaskID == _taskid);
                        if (stt2.TaskState == "已完成")
                        {
                            ((Panel)GetControltByMaster("PanelDone")).Visible = true;
                            ((Panel)GetControltByMaster("PanelOp")).Visible = false;
                            //载入数据
                            ((Label)GetControltByMaster("lblsta")).Text = stt2.AuditStatus;
                            ((Label)GetControltByMaster("lbluser")).Text = stt2.EmpInfo1.EmpName;
                            ((Label)GetControltByMaster("lbldete")).Text = stt2.AcceptTime.ToString();
                            ((Label)GetControltByMaster("lblop")).Text = stt2.AuditOpinion;
                           // btnOK.Text = "组长审核";
                            btnOK.Visible = false;
                        }
                        else
                        {
                            ((Panel)GetControltByMaster("PanelDone")).Visible = false;
                            ((Panel)GetControltByMaster("PanelOp")).Visible = true;
                        }
                      //= n.StockTransferTaskID > _taskid?false:true;       
                        
                    }
                }
                //txtUser = (TextBox)GetControltByMaster("txtUser");
                //L1 = (Literal)GetControltByMaster("L1");
                //L1.Text = JSDialogAid.GetJSForDialog(txtUser.ClientID,"../StorageAndPile/SelectUser.aspx");                

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

        private void InitializeCustomControls()
        {
            this.spgvProducingAudit = new SPGridView();
            this.spgvProducingAudit.AutoGenerateColumns = false;
            this.spgvProducingAudit.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");

            string[] ShowTlist =  { 
                                        "物料名称:MaterialName",
                                     "物料规格:SpecificationModel",
                                     "状态:Status",                                   
                                     
                                       "根/套/件:QuantityGentaojian",
                                              "米:QuantityMetre",
                                              "吨:QuantityTon",
                                     "单价:UnitPrice",
                                     
                                     "所属仓库:StorageName",
                                     "所属垛位:PileName",
                                     "到库时间:StorageTime",
                                     //"供应商:SupplierName",
                                           };
            BoundField bfColumn;

            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvProducingAudit.Columns.Add(bfColumn);
            }

            chbAgree = (CheckBox)GetControltByMaster("chbAgree");
            chbAgree.CheckedChanged += new EventHandler(chbAgree_CheckedChanged);

            txtOpinion = (TextBox)GetControltByMaster("txtOpinion");
            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);

            btnCancel = (Button)GetControltByMaster("btnCancel");
            btnCancel.Click += new EventHandler(btnCancel_Click);

            //panelUser = (Panel)GetControltByMaster("panel5");
           

        }

        private void BindDataToCustomControls()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
               // BoundField bfColumn;
                this.spgvProducingAudit.DataSource = from a in db.StockTransferDetail
                                                     //join b in db.StorageStocks on a.StocksID equals b.StocksID
                                                     join b in db.StorageStocks on a.StocksID equals b.StocksID
                                                     where a.StockTransferID == _noticeid
                                                     && a.StocksID == b.StocksID
                                                     && a.StocksStatus == b.Status
                                                     select new
                                                     {
                                                         
                                                         b.MaterialName,
                                                        b.MaterialCode,
                                                         b.SpecificationModel,
                                                        b.Status,
                                                         a.QuantityGentaojian,
                                                         a.QuantityMetre,
                                                         a.QuantityTon,
                                                         b.UnitPrice,
                                                       
                                                         b.StorageName,
                                                         b.PileName,
                                                         b.StorageTime,
                                                         
                                                     };
                this.spgvProducingAudit.DataBind();
            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvProducingAudit);
        }

        #endregion

        #region 控件事件方法
        void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("../../default-old.aspx", false);
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                
                //将审核结果保存到数据库
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    _taskid = Convert.ToInt32(Request.QueryString["StockTransferTaskID"]);
                    StockTransferTask st1 = db.StockTransferTask.SingleOrDefault(a => a.StockTransferTaskID == _taskid);
                    //页面已处理完毕就直接跳转到发送任务界面
                    if (((Panel)GetControltByMaster("PanelDone")).Visible == true)
                    {
                        Response.Redirect("StockTransferCreateTask.aspx?StockTransferID=" + st1.StockTransferID + "&&TaskType=生产组长审核信息&&BackUrl=" + HttpContext.Current.Request.Url.PathAndQuery);
                    }



                    if (this.chbAgree.Checked)
                    {
                        //通過
                        //验证是否都有内容
                        _taskid = Convert.ToInt32(Request.QueryString["StockTransferTaskID"]);
                        StockTransferTask st = db.StockTransferTask.SingleOrDefault(a => a.StockTransferTaskID==_taskid);
                        st.AcceptTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                        st.AuditOpinion = this.txtOpinion.Text.Trim();
                        st.AuditStatus = "审核通过";
                        st.TaskState = "已完成";
                        //db.SubmitChanges();
                        var all = from a in db.StockTransferDetail
                                  where a.StockTransferID == st.StockTransferID
                                  select a;
                        bool done = true;
                        foreach (var single in all)
                        {
                           
                            switch (single.StocksStatus)
                            {
                                case "线上":
                                    #region 改线上
                                    var ul = db.StockOnline.SingleOrDefault(a => a.StockOnlineID == single.StocksID);
                                    decimal gen, meter, ton;
                                    gen = decimal.Parse((ul.QuantityGentaojian -single.QuantityGentaojian).ToString());
                                    meter = decimal.Parse((ul.QuantityMetre - single.QuantityMetre).ToString());
                                    ton = decimal.Parse((ul.QuantityTon - single.QuantityTon).ToString());
                                    if (gen < 0 || meter < 0 || ton < 0)
                                    {
                                        done = false;
                                    }
                                    //创建新纪录//修改原纪录
                                    StockOnline so = new StockOnline();

                                      so.Amount = ul.Amount;
                                    so.AssetsManager = ul.AssetsManager;
                                    so.BatchIndex = ul.BatchIndex;
                                    so.BillCode = ul.BillCode;

                                    so.CurUnit = ul.CurUnit;
                                    so.ExpectedProject = ul.ExpectedProject;
                                    so.ManufacturerID = ul.ManufacturerID;
                                    so.MaterialCode = ul.MaterialCode;
                                    so.MaterialID = ul.MaterialID;
                                    so.MaterialsManager = ul.MaterialsManager;
                                    so.OfflineGentaojian = ul.OfflineGentaojian;
                                    so.OfflineMetre = ul.OfflineMetre;
                                    so.OfflineTon = ul.OfflineTon;
                                    so.ReceivingTypeName = ul.ReceivingTypeName;
                                    so.Remark = ul.Remark;
                                    so.StorageInCode = ul.StorageInCode;
                                    so.StorageInID = ul.StorageInID;
                                    so.StorageInType = ul.StorageInType;
                                    so.StorageTime = ul.StorageTime;
                                    so.SupplierID = ul.SupplierID;
                                    so.UnitPrice = ul.UnitPrice;
                                    so.CertificateNum = ul.CertificateNum;
                                    so.CreateTime = DateTime.Now;
                                    so.Creator = ul.Creator;                                    
                                    switch (ul.OnlineUnit)
                                    {
                                        case "根/台/套/件":
                                            so.CurQuantity = single.QuantityGentaojian;
                                            
                                            ul.CurQuantity = gen;
                                            // so.OnlineTotal = gen * ul.OnlineUnitPrice;
                                            break;
                                        case "米":
                                            so.CurQuantity = single.QuantityMetre;
                                            
                                            ul.CurQuantity = meter;
                                            break;
                                        case "吨":
                                            so.CurQuantity = single.QuantityTon;
                                            
                                            ul.CurQuantity = ton;
                                            break;
                                    }                                    
                                    so.OnlineCode = ul.OnlineCode;                                    
                                    so.OnlineDate = ul.OnlineDate;
                                    so.OnlineTotal = so.CurQuantity * ul.OnlineUnitPrice;
                                    ul.OnlineTotal = ul.CurQuantity * ul.OnlineUnitPrice;
                                    so.OnlineUnit = ul.OnlineUnit;
                                    so.OnlineUnitPrice = ul.OnlineUnitPrice;
                                    so.OrderNum = ul.OrderNum;
                                    so.QuantityGentaojian = single.QuantityGentaojian;
                                    ul.QuantityGentaojian = gen;
                                    so.QuantityMetre = single.QuantityMetre;
                                    ul.QuantityMetre = meter;
                                    so.QuantityTon = single.QuantityTon;
                                    ul.QuantityTon = ton;
                                  
                                  //  so.TableOfStocksID = ul.TableOfStocksID;
                                    so.PileID = single.TargetPile;
                                    db.StockOnline.InsertOnSubmit(so);
                                    
                                    #endregion
                                    break;
                                case "线下":
                                    #region 改线下
                                    var un = db.TableOfStocks.SingleOrDefault(a => a.StocksID == single.StocksID);
                                    gen = decimal.Parse((un.QuantityGentaojian - single.QuantityGentaojian).ToString());
                                    meter = decimal.Parse((un.QuantityMetre - single.QuantityMetre).ToString());
                                    ton = decimal.Parse((un.QuantityTon - single.QuantityTon).ToString());
                                    if (gen < 0 || meter < 0 || ton < 0)
                                    {
                                        done = false;
                                    }
                                    //创建新纪录//修改原纪录
                                    TableOfStocks tos = new TableOfStocks();
                                    tos.AssetsManager = un.AssetsManager;
                                    tos.BatchIndex = un.BatchIndex;
                                    tos.BillCode = un.BillCode;
                                    tos.CreateTime = DateTime.Now;
                                    tos.Creator = un.Creator;
                                    tos.CurUnit = un.CurUnit;
                                    tos.ExpectedProject = un.ExpectedProject;
                                    tos.ManufacturerID = un.ManufacturerID;
                                    tos.MaterialCode = un.MaterialCode;
                                    tos.MaterialID = un.MaterialID;
                                    tos.MaterialsManager = un.MaterialsManager;
                                    tos.PileID = single.TargetPile;
                                    tos.QuantityGentaojian = single.QuantityGentaojian;
                                    un.QuantityGentaojian = gen;
                                    tos.QuantityMetre = single.QuantityMetre;
                                    un.QuantityMetre = meter;
                                    tos.QuantityTon = single.QuantityTon;
                                    un.QuantityTon = ton;
                                    tos.ReceivingTypeName = un.ReceivingTypeName;
                                    tos.Remark = un.Remark;
                                    tos.StorageID = single.PileInfo.StorageInfo.StorageID;
                                    tos.StorageInCode = un.StorageInCode;
                                    tos.StorageInID = un.StorageInID;
                                    tos.StorageInType = un.StorageInType;
                                    tos.StorageTime = un.StorageTime;
                                    tos.SupplierID = un.SupplierID;
                                    tos.UnitPrice = un.UnitPrice;
                                    switch(tos.CurUnit)
                                    {
                                        case "根/台/套/件":
                                            tos.Amount =decimal.Parse( tos.QuantityGentaojian.ToString()) * tos.UnitPrice;
                                            un.Amount = gen * un.UnitPrice;
                                            // so.OnlineTotal = gen * ul.OnlineUnitPrice;
                                            break;
                                        case "米":
                                            tos.Amount = decimal.Parse(tos.QuantityMetre.ToString()) * tos.UnitPrice;
                                            un.Amount = meter * un.UnitPrice;
                                            break;
                                        case "吨":
                                            tos.Amount = decimal.Parse(tos.QuantityTon.ToString()) * tos.UnitPrice;
                                            un.Amount = ton * un.UnitPrice;
                                            break;
                                    }
                                    db.TableOfStocks.InsertOnSubmit(tos);
                                    #endregion 
                                    break;
                                case "回收合格":
                                    #region  回收
                                    var ur = db.QualifiedStocks.SingleOrDefault(a => a.StocksID == single.StocksID);
                                    gen = decimal.Parse((ur.Gentaojian - single.QuantityGentaojian).ToString());
                                    meter = decimal.Parse((ur.Metre - single.QuantityMetre).ToString());
                                    ton = decimal.Parse((ur.Ton - single.QuantityTon).ToString());
                                    if (gen < 0 || meter < 0 || ton < 0)
                                    {
                                        done = false;
                                    }
                                    //创建新纪录//修改原纪录
                                    QualifiedStocks qs = new QualifiedStocks();
                                    qs.CurUnit = ur.CurUnit;
                                    qs.ManufactureID = ur.ManufactureID;
                                    qs.MaterialID = ur.MaterialID;
                                    qs.PileID = single.TargetPile;
                                    qs.Remark = ur.Remark;
                                    qs.RetrieveProjectID = ur.RetrieveProjectID;
                                    qs.RetrieveTime = ur.RetrieveTime;
                                    qs.StorageID = single.PileInfo.StorageInfo.StorageID;
                                    qs.StorageTime = ur.StorageTime;
                                    qs.UnitPrice = ur.UnitPrice;                                    
                                    qs.Gentaojian =decimal.Parse( single.QuantityGentaojian.ToString());
                                    ur.Gentaojian = gen;
                                    qs.Metre =decimal.Parse( single.QuantityMetre.ToString());
                                    ur.Metre = meter;
                                    qs.Ton =decimal.Parse(  single.QuantityTon.ToString());
                                    ur.Ton = ton;
                                    switch (qs.CurUnit)
                                    {
                                        case "根/台/套/件":
                                            qs.Amount = decimal.Parse(qs.Gentaojian.ToString()) * qs.UnitPrice;
                                            ur.Amount = gen * ur.UnitPrice;
                                            // so.OnlineTotal = gen * ul.OnlineUnitPrice;
                                            break;
                                        case "米":
                                            qs.Amount = decimal.Parse(qs.Metre.ToString()) * qs.UnitPrice;
                                            ur.Amount = meter * ur.UnitPrice;
                                            break;
                                        case "吨":
                                            qs.Amount = decimal.Parse(qs.Ton.ToString()) * qs.UnitPrice;
                                            ur.Amount = ton * ur.UnitPrice;
                                            break;
                                    }
                                    db.QualifiedStocks.InsertOnSubmit(qs);
                                    #endregion
                                    break;
                            }
                            if (!done)
                            {
                                break;
                            }
                        }
                        if (done)
                        {
                            db.SubmitChanges();
                        }
                        else
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>","移动数目超过库存数,请检查数据!" ));
                        }
                        //Response.Redirect("StockTransferCreateTask.aspx?StockTransferID=" + st.StockTransferID + "&&TaskType=生产组长审核信息&&BackUrl=" + HttpContext.Current.Request.Url.PathAndQuery);
                          

                    }
                    else {
                        //未通過
                        //通過
                        //验证是否都有内容
                        _taskid = Convert.ToInt32(Request.QueryString["StockTransferTaskID"]);
                        StockTransferTask st = db.StockTransferTask.SingleOrDefault(a => a.StockTransferTaskID == _taskid);
                        st.AcceptTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                        st.AuditOpinion = this.txtOpinion.Text.Trim();
                        st.AuditStatus = "审核未通过";
                        st.TaskState = "已完成";
                        //写入新的内容

                        StockTransferTask stt = new StockTransferTask();
                        stt.Remark = this.txtOpinion.Text.Trim();
                        stt.StockTransferID = st.StockTransferID;
                        stt.TaskCreaterID = st.TaskTargetID;
                        stt.TaskInType = "移库任务";
                        stt.TaskState = "未完成";
                        stt.TaskTargetID = st.TaskCreaterID;
                        stt.TaskTitle = st.TaskTitle + "[审核未通过]";
                        stt.TaskType = "发起人修改";
                        stt.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                        db.StockTransferTask.InsertOnSubmit(stt);
                        db.SubmitChanges();
                    }
                }
                Response.Redirect("../../default-old.aspx", false);
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
            }

        }

        void chbAgree_CheckedChanged(object sender, EventArgs e)
        {
            if (chbAgree.Checked)
            {
                txtOpinion.Enabled = true;
                //txtOpinion.Text = "同意";
                //panelUser.Visible = true;
                this.btnOK.Text = "审核通过";
            }
            else
            {
                txtOpinion.Enabled = true;
                //txtOpinion.Text = "请在此处填写审核意见...";
               // panelUser.Visible = false;
                this.btnOK.Text = "退还审核";
            }
        }
        #endregion

        #region 辅助函数
        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }
        private int reEmpId(string Emp)
        {
            int valueEmp = 0;
            using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                EmpInfo EI = dc.EmpInfo.SingleOrDefault(u => u.Account == Emp);
                if (EI != null)
                {
                    valueEmp = EI.EmpID;
                }

            }

            return valueEmp;
        }

        #endregion
    }
}
