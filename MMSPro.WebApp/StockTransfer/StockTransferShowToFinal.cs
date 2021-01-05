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
    public class StockTransferShowToFinal : System.Web.UI.Page
    {
        private int _noticeid;
        private int _taskid;
        MMSProDBDataContext db;
        private SPGridView spgvProducingAudit;
        private CheckBox chbAgree;
        private TextBox txtOpinion;
       // private TextBox txtUser;
        private Button btnOK, btnCancel;
        StockTransferTask stStart;
        // private Panel panelUser;
        //Literal L1;
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
                        var t = db.StockTransferTask.Where(a => a.StockTransferID == stt.StockTransferID && a.TaskInType == stt.TaskInType).OrderBy(a => a.StockTransferTaskID).First();
                        stt = t;
                        //加载任务相关信息
                        Label lblReceiver = (Label)GetControltByMaster("lblReceiver");
                        lblReceiver.Text = stt.EmpInfo.EmpName;
                        Label lblDate = (Label)GetControltByMaster("lblDate");
                        lblDate.Text = stt.CreateTime.ToShortTimeString();
                        StockTransfer sts = db.StockTransfer.SingleOrDefault(a => a.StockTransferID == stt.StockTransferID);
                        if (sts != null)
                        {
                            Label lblNoticeCode = (Label)GetControltByMaster("lblNoticeCode");
                            lblNoticeCode.Text = sts.StockTransferNum;
                        }

                        //加载过往批复信息
                        //获取物资组长审核信息记录
                        stStart = db.StockTransferTask.Where(a => a.StockTransferID == stt.StockTransferID && a.TaskType == "物资组长审核信息").OrderByDescending(a => a.StockTransferTaskID).First();
                        if (stStart != null)
                        {

                            Label lblsta = (Label)GetControltByMaster("lblsta");
                            lblsta.Text = stStart.AuditStatus;
                            Label lbluser = (Label)GetControltByMaster("lbluser");
                            lbluser.Text = stStart.EmpInfo1.EmpName;
                            Label lbldete = (Label)GetControltByMaster("lbldete");
                            lbldete.Text = stStart.AcceptTime.ToString();
                            ((Label)GetControltByMaster("lblop")).Text = stStart.AuditOpinion;

                        }
                        //获取生产组长审核记录
                        //stStart = db.StockTransferTask.SingleOrDefault(a => a.StockTransferID == stt.StockTransferID && a.TaskType == "生产组长审核信息");
                        stStart = db.StockTransferTask.Where(a => a.StockTransferID == stt.StockTransferID && a.TaskType == "生产组长审核信息").OrderByDescending(a => a.StockTransferTaskID).First();
                        if (stStart != null)
                        {

                            Label lblsta0 = (Label)GetControltByMaster("lblsta0");
                            lblsta0.Text = stStart.AuditStatus;
                            Label lbluser0 = (Label)GetControltByMaster("lbluser0");
                            lbluser0.Text = stStart.EmpInfo1.EmpName;
                            Label lbldete0 = (Label)GetControltByMaster("lbldete0");
                            lbldete0.Text = stStart.AcceptTime.ToString();
                            ((Label)GetControltByMaster("lblop0")).Text = stStart.AuditOpinion;

                        }
                        //获取提交材料会计审核记录
                        stStart = db.StockTransferTask.Where(a => a.StockTransferID == stt.StockTransferID && a.TaskType == "发起人确认").OrderByDescending(a => a.StockTransferTaskID).First();
                        if (stStart != null)
                        {

                            Label lblsta0 = (Label)GetControltByMaster("lblsta1");
                            lblsta0.Text = stStart.AuditStatus;
                            Label lbluser0 = (Label)GetControltByMaster("lbluser1");
                            lbluser0.Text = stStart.EmpInfo1.EmpName;
                            Label lbldete0 = (Label)GetControltByMaster("lbldete1");
                            lbldete0.Text = stStart.AcceptTime.ToString();

                        }


                        ////判断是否已经发送过任务
                        //var n = db.StockTransferTask.Where(a => a.StockTransferID == stt.StockTransferID && a.TaskInType == stt.TaskInType).OrderByDescending(a => a.StockTransferTaskID).First();
                        btnOK = (Button)GetControltByMaster("btnOK");
                        //if (n.StockTransferTaskID > _taskid)
                        //{
                        //    //任务已发送
                        //    btnOK.Enabled = false;
                        //}
                        //else
                        //{
                        //    //任务未发送
                        //    btnOK.Enabled = true;
                        //    btnOK.Text = "申请者接收";
                        //}
                        //判断任务是否已经完成
                        StockTransferTask stt2 = db.StockTransferTask.SingleOrDefault(a => a.StockTransferTaskID == _taskid);
                        if (stt2.TaskState == "已完成")
                        {
                            ((Panel)GetControltByMaster("PanelDone")).Visible = true;
                            ((Panel)GetControltByMaster("PanelOp")).Visible = false;
                            //载入数据
                            ((Label)GetControltByMaster("lblsta2")).Text = stt2.AuditStatus;
                            ((Label)GetControltByMaster("lbluser2")).Text = stt2.EmpInfo1.EmpName;
                            ((Label)GetControltByMaster("lbldete2")).Text = stt2.AcceptTime.ToString();
                            ((Label)GetControltByMaster("lblop2")).Text = stt2.AuditOpinion;

                           // 任务已完成
                           btnOK.Enabled = false;
                        }
                        else
                        {
                            ((Panel)GetControltByMaster("PanelDone")).Visible = false;
                            ((Panel)GetControltByMaster("PanelOp")).Visible = true;
                            //任务未完成
                            btnOK.Enabled = true;
                            btnOK.Text = "材料会计审核";
                        }


                    }
                }
                //txtUser = (TextBox)GetControltByMaster("txtUser");
                //L1 = (Literal)GetControltByMaster("L1");
                //L1.Text = JSDialogAid.GetJSForDialog(txtUser.ClientID, "../StorageAndPile/SelectUser.aspx");

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
                                     "物料编码:MaterialCode",                                   
                                     "调拨数量:Quantity",   
                                      "根/套/件:QuantityGentaojian",
                                              "米:QuantityMetre",
                                              "吨:QuantityTon",
                                     "单价:UnitPrice",
                                     "金额:Amount",
                                     "所属仓库:StorageName",
                                     "所属垛位:PileCode",
                                     "到库时间:StorageTime",
                                     "供应商:SupplierName",
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




        }

        private void BindDataToCustomControls()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                // BoundField bfColumn;
                this.spgvProducingAudit.DataSource = from a in db.StockTransferDetail
                                                     //join b in db.StorageStocks on a.StocksID equals b.StocksID
                                                     where a.StockTransferID == _noticeid
                                                     select new
                                                     {
                                                         a.TableOfStocks.MaterialInfo.MaterialName,
                                                         a.TableOfStocks.MaterialInfo.MaterialCode,
                                                         a.TableOfStocks.SpecificationModel,
                                                         a.Quantity,
                                                         a.QuantityGentaojian,
                                                         a.QuantityMetre,
                                                         a.QuantityTon,
                                                         a.TableOfStocks.UnitPrice,
                                                         Amount = a.Quantity * a.TableOfStocks.UnitPrice,
                                                         a.TableOfStocks.PileInfo.StorageInfo.StorageName,
                                                         a.TableOfStocks.PileInfo.PileCode,
                                                         a.TableOfStocks.StorageTime,
                                                         a.TableOfStocks.SupplierInfo.SupplierName,
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
        void chbAgree_CheckedChanged(object sender, EventArgs e)
        {
            if (chbAgree.Checked)
            {
                //txtOpinion.Enabled = true;
                //txtOpinion.Text = "同意";
               
                this.btnOK.Text = "审核通过";
            }
            else
            {
                //txtOpinion.Enabled = true;
                //txtOpinion.Text = "请在此处填写审核意见...";
               
                this.btnOK.Text = "退还审核";
            }
        }
        void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                //将审核结果保存到数据库
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    if (this.chbAgree.Checked)
                    {
                        
                        //通過
                        //验证是否都有内容
                        _taskid = Convert.ToInt32(Request.QueryString["StockTransferTaskID"]);
                        StockTransferTask st = db.StockTransferTask.SingleOrDefault(a => a.StockTransferTaskID == _taskid);
                        bool done = false;
                        string strMessage = "";
                        //修改所有数量
                        var std = db.StockTransferDetail.Where(a => a.StockTransferID == st.StockTransferID);
                        foreach (var n in std)
                        {
                            //获取原有库存对象
                            var ts = n.TableOfStocks;
                            //判断是否足够数量进行运算                            
                            //获取原有数
                            decimal dcmT = 0;
                            decimal dcmT1 = 0;
                            decimal dcmT2 = 0;
                            decimal dcmT3 = 0;
                            decimal.TryParse(ts.QuantityGentaojian.ToString(), out dcmT);
                            decimal.TryParse(ts.QuantityMetre.ToString(), out dcmT1);
                            decimal.TryParse(ts.QuantityTon.ToString(), out dcmT2);
                            decimal.TryParse(ts.Quantity.ToString(), out dcmT3);    
                            //获取移库数
                            //获取原有数
                            decimal dcmC = 0;
                            decimal dcmC1 = 0;
                            decimal dcmC2 = 0;
                            decimal dcmC3 = 0;
                            decimal.TryParse(n.QuantityGentaojian.ToString(), out dcmC);
                            decimal.TryParse(n.QuantityMetre.ToString(), out dcmC1);
                            decimal.TryParse(n.QuantityTon.ToString(), out dcmC2);
                            decimal.TryParse(n.Quantity.ToString(), out dcmC3);
                            if (dcmT - dcmC >= 0 && dcmT1 - dcmC1 >= 0 && dcmT2 - dcmC2 >= 0 && dcmT3 - dcmC3 >= 0)
                            {
                                //插入
                                TableOfStocks tsNew = new TableOfStocks();
                                tsNew.CurUnit = ts.CurUnit;
                                tsNew.financeCode = ts.financeCode;
                                tsNew.MaterialID = ts.MaterialID;
                                tsNew.MaterialsManager = ts.MaterialsManager;
                                tsNew.NumberQualified = ts.NumberQualified;
                                tsNew.PileID = n.TargetPile;
                                tsNew.Quantity = dcmC3;
                                tsNew.QuantityGentaojian = dcmC;
                                tsNew.QuantityMetre = dcmC1;
                                tsNew.QuantityTon = dcmC2;
                                tsNew.Remark = ts.Remark;
                                tsNew.SpecificationModel = ts.SpecificationModel;
                                tsNew.StorageInID = ts.StorageInID;
                                tsNew.StorageInType = ts.StorageInType;
                                tsNew.StorageTime = ts.StorageTime;
                                tsNew.SupplierID = ts.SupplierID;
                                tsNew.UnitPrice = ts.UnitPrice;
                                tsNew.WarehouseWorker = ts.WarehouseWorker;
                                db.TableOfStocks.InsertOnSubmit(tsNew);
                                //减数
                                ts.Quantity = dcmT3 - dcmC3;
                                ts.QuantityGentaojian = dcmT - dcmC;
                                ts.QuantityMetre = dcmT1 - dcmC1;
                                ts.QuantityTon = dcmT2 - dcmC2;
                                done = true;
                            }
                            else
                            {
                                strMessage += "移库失败!从" + ts.PileInfo.StorageInfo.StorageName + "|" + ts.PileInfo.PileName + "处移动到" + n.PileInfo.StorageInfo.StorageName + "|" + n.PileInfo.PileName + "失败!请检查数量是否足够!\br";
                                done = false;
                                break;
                            }
                           
                        }
                        if (done)
                        {
                            st.AcceptTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                            st.AuditOpinion = this.txtOpinion.Text.Trim();
                            st.AuditStatus = "审核通过";
                            st.TaskState = "已完成";
                            //写入新的内容
                            db.SubmitChanges();
                        }
                        else
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('"+strMessage+"')</script>");
                            Response.AddHeader("Refresh", "0");
                        }

                    }
                    else
                    {
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
                        StockTransferTask st0 = db.StockTransferTask.Where(a => a.StockTransferID == st.StockTransferID).OrderBy(a => a.StockTransferTaskID).First();



                        StockTransferTask stt = new StockTransferTask();
                        stt.Remark = this.txtOpinion.Text.Trim();
                        stt.StockTransferID = st.StockTransferID;
                        stt.TaskCreaterID = st.TaskTargetID;
                        stt.TaskInType = "移库任务";
                        stt.TaskState = "未完成";
                        stt.TaskTargetID = st0.TaskCreaterID;
                        stt.TaskTitle = st.TaskTitle + "[材料会计审核未通过]";
                        stt.TaskType = "发起人修改";
                        stt.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                        db.StockTransferTask.InsertOnSubmit(stt);
                        db.SubmitChanges();
                        Response.Redirect("../../default-old.aspx", false);
                    }

                        //获取userid

                        // stNew.TaskTargetID = reEmpId(txtUser.Text.Trim());

                   
                }
               // Response.Redirect("../../default-old.aspx", false);
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
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
