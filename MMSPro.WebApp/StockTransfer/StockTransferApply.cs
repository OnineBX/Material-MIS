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
    public class StockTransferApply : System.Web.UI.Page
    {
        MMSProDBDataContext db;
        TextBox txtBillCode;
        DateTimeControl DateTimeStorageIn;
        TextBox txtRemark;
        Button btnSave;
        Button btnQuit;
        int ID;
        protected void Page_Load(object sender, EventArgs e)
        {
            
             
          
            InitControl();
            if (!IsPostBack)
            {
                
                //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello ')</script>");
                if (!string.IsNullOrEmpty(Request.QueryString["StockTransferID"]))
                {
                   // string BackUrl = Request.QueryString["BackUrl"];
                    //要求回到工作平台
                    string BackUrl = "../../default-old.aspx";
                    ID = int.Parse(Request.QueryString["StockTransferID"]);
                    //检查是否有详细列表
                    using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        var n = db.StockTransferDetail.Where(a => a.StockTransferID == ID);
                        if (n.ToList().Count > 0)
                        {
                            Response.Redirect(string.Format("../PublicPage/ErrorInfo.aspx?ErrorInfo=该调拨单中已包含调拨物质无法修改！&DisposeUrl={0}&BackUrl={1}", BackUrl, BackUrl), false);
                            return;
                        }
                        else
                        {
                            //获取目标
                            var t = db.StockTransfer.SingleOrDefault(a => a.StockTransferID == ID);
                            if (t != null)
                            {
                                this.txtBillCode.Text = t.StockTransferNum;
                                this.txtRemark.Text = t.Remark;
                                this.DateTimeStorageIn.SelectedDate = t.CreateTime;
                                this.btnSave.Text = "修改";
                                //            stt.CreateTime = DateTime.Now;
                                //stt.StockTransferNum = this.txtBillCode.Text.Trim();
                                //stt.Remark = this.txtRemark.Text.Trim();
                                //stt.Creater = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
                                //db.StockTransfer.InsertOnSubmit(stt);
                                //db.SubmitChanges();
                            }
                        }
                    }
                }
            }
        }
        private void InitControl()
        {
            this.txtBillCode = (TextBox)GetControltByMaster("txtBillCode");
            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
            this.DateTimeStorageIn = (DateTimeControl)GetControltByMaster("DateTimeStorageIn");
            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
        }

        public void btnSave_Click(object sender, EventArgs e)
        {

            if (!string.IsNullOrEmpty(this.txtBillCode.Text))
            {
                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    if (!string.IsNullOrEmpty(Request.QueryString["StockTransferID"]))
                    {
                        var t = db.StockTransfer.SingleOrDefault(a => a.StockTransferID == int.Parse(Request.QueryString["StockTransferID"]));
                        //if (db.StorageIn.SingleOrDefault(u => u.StorageInCode == this.txtBillCode.Text.Trim()) != null)
                        //{
                        //    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('单据编码重复！')</script>");
                        //    return;
                        //}                        
                        t.CreateTime = this.DateTimeStorageIn.SelectedDate;
                        t.StockTransferNum = this.txtBillCode.Text.Trim();
                        t.Remark = this.txtRemark.Text.Trim();
                        t.Creater = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
                        db.SubmitChanges();
                        Response.Redirect("StockTransferManager.aspx");
                    }
                    else
                    {
                        StockTransfer stt = new StockTransfer();                       
                        stt.CreateTime = this.DateTimeStorageIn.SelectedDate;
                        stt.StockTransferNum = this.txtBillCode.Text.Trim();
                        stt.Remark = this.txtRemark.Text.Trim();
                        stt.Creater = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
                        db.StockTransfer.InsertOnSubmit(stt);
                        db.SubmitChanges();
                        Response.Redirect("StockTransferManager.aspx");
                    }

                }
            }

        }
        public void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("StockTransferManager.aspx");
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
