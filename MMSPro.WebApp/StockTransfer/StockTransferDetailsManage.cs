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
    public class StockTransferDetailsManage : System.Web.UI.Page
    {
        MMSProDBDataContext db;
        SPGridView gv;
        Panel PanelMsg;
        Label lblTitle;
        TextBox tboxMsg;
        Button btnOk;
        Button btnCannel;
        Button btnBack;
        bool bolEdit = false;
        static string[] Titlelist = {
                                     "单据编号:StockTransferNum",
                                     "物料名称:MaterialName",
                                     "物料规格:SpecificationModel",
                                     "物料编码:MaterialCode",                                    
                                     "所属仓库:StorageName",
                                     "所属垛位:PileName",
                                     "目标垛位(仓库|垛位|垛位号):target",                                     
                                    
                                    };



        protected void Page_Load(object sender, EventArgs e)
        {
            
            this.gv = new SPGridView();
            this.gv.AutoGenerateColumns = false;
           
            //判断是否允许修改
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                int ID = int.Parse(Request.QueryString["StockTransferID"]);
                //允许的情况:
                //a:没有进入流程
                //b:被拒绝,要求修改                
                var t = db.StockTransferTask.Where(a => a.TaskInType == "移库任务" && a.StockTransferID == ID);
                if (t.ToList().Count==0)
                {
                    bolEdit = true;
                }
                else
                {
                    var a = t.OrderByDescending(b => b.StockTransferTaskID).First();
                    if (a.TaskType == "发起人修改"&& a.TaskState == "未完成")
                    {
                        ((Label)GetControltByMaster("lblInfo")).Text =a.TaskTitle;
                        ((Panel)GetControltByMaster("PanelDone")).Visible = true;
                        var m = t.OrderByDescending(b => b.StockTransferTaskID).Skip(1).First();
                        ((Label)GetControltByMaster("lblsta0")).Text = m.AuditStatus;
                        ((Label)GetControltByMaster("lbluser0")).Text = m.EmpInfo1.EmpName;
                        ((Label)GetControltByMaster("lbldete0")).Text = m.AcceptTime.ToString();
                        ((Label)GetControltByMaster("lblop0")).Text = m.AuditOpinion;

                        ((Panel)GetControltByMaster("Panelback")).Visible = true;
                        bolEdit = true;
                    }
                }


                //****************************


            }

            InitToolBar(bolEdit);
            

            InitMsgControls();
            BindGridView(bolEdit);
            #region //
            ////通过任务表
            //if (!string.IsNullOrEmpty(Request.QueryString["StockTransferID"]))
            //{
                
            //    //using (MMSProDBDataContext data = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            //    //{
            //    //    StorageOutTask SOT = data.StorageOutTask.SingleOrDefault(u => u.StorageOutTaskID == Convert.ToInt32(Request.QueryString["StorageOutTaskID"].ToString()));
            //    //    StorageOutProducingAudit SOA = data.StorageOutProducingAudit.SingleOrDefault(u => u.StorageOutProducingAuditID == SOT.StorageOutAuditID);
            //    //    if (SOA.AuditStatus == "未通过")
            //    //    {
            //    //        this.PanelMsg.Visible = true;
            //    //        this.tboxMsg.Text = SOA.AuditOpinion.ToString();
            //    //        this.btnOk.Visible = false;
            //    //    }
            //    //    else
            //    //    {
            //    //        this.PanelMsg.Visible = true;
            //    //        this.tboxMsg.Text = SOA.AuditOpinion.ToString();
            //    //        this.btnOk.Visible = true;
                        
            //    //    }
                    
            //    //}
               
            //    BindGridView();
            //}
            ////正常进入
            //else
            //{
            //    BindGridView();
            //    //检测任务中是否已存在此调拨单，有则提示
            //    if (isInTask(Request.QueryString["StorageOutNoticeID"]))
            //    {
            //        Label lbtask = (Label)GetControltByMaster("intask");
            //        lbtask.Visible = true;
            //        InitToolBar(false);
            //        btnBack = new Button();
            //        btnBack.Text = "返回";
            //        btnBack.Width =120;
            //        btnBack.Click+=new EventHandler(btnBack_Click);
            //        Panel Panelback = (Panel)GetControltByMaster("Panelback");
            //        Panelback.Controls.Add(btnBack);
            //    }
            //}
            //初始化调拨数量
            #endregion
        //    initQuantity();
            #region //

            ////如果任务已完成
            //using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            //{
            //    if (!string.IsNullOrEmpty(Request.QueryString["StorageOutTaskID"]))
            //    {
            //        StorageOutTask sot = db.StorageOutTask.SingleOrDefault(u => u.StorageOutTaskID == Convert.ToInt32(Request.QueryString["StorageOutTaskID"]));
            //        if (sot != null)
            //        {
            //            if (sot.TaskState == "已完成")
            //            {
            //                this.btnOk.Enabled = false;
            //                this.tboxMsg.Enabled = false;
            //                btnCannel = (Button)GetControltByMaster("btnCancel");
            //                btnCannel.Click += new EventHandler(btnCannel_Click);
                          
            //            }
            //        }
            //    }
            //}
            #endregion

        }

        

        private void InitMsgControls()
        {
            PanelMsg = (Panel)GetControltByMaster("PanelMsg");
            lblTitle = (Label)GetControltByMaster("lblTitle");
            tboxMsg = (TextBox)GetControltByMaster("txtOpinion");

            btnOk = (Button)GetControltByMaster("btnSend");            
            this.btnOk.Click += new EventHandler(btnOk_Click);

           Button btnref = (Button)GetControltByMaster("btnRef");
           btnref.Click += new EventHandler(btnRefresh_Click);

           Button btnView = (Button)GetControltByMaster("btnView");
           btnView.Click += new EventHandler(btnView_Click);

            PanelMsg.Visible = false;
            
        }

        void btnView_Click(object sender, EventArgs e)
        {
            upDataChanges(false);
            ((Button)GetControltByMaster("btnSend")).Enabled = true;
            ((Button)GetControltByMaster("btnRef")).Enabled = false;
           // bolEdit = false;
            
        }

        void btnOk_Click(object sender, EventArgs e)
        {
            Response.Redirect("StockTransferCreateTask.aspx?StockTransferID=" + Request.QueryString["StockTransferID"] + "&&TaskType=物资组长审核信息&&BackUrl=" + HttpContext.Current.Request.Url.PathAndQuery);
        }

        /// <summary>
        /// 初始化toolbar
        /// </summary>
        private void InitToolBar(bool bo)
        {
            
                //添加按钮到toolbar
                ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");

                //新建
                ToolBarButton tbarbtnAdd = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
                tbarbtnAdd.ID = "AddNewRow";
                tbarbtnAdd.Text = "新建";
                tbarbtnAdd.ImageUrl = "/_layouts/images/newitem.gif";
                tbarbtnAdd.Click += new EventHandler(tbarbtnAdd_Click);
                
                //修改
                //ToolBarButton tbarbtnEdit = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
                //tbarbtnEdit.ID = "EditRow";
                //tbarbtnEdit.Text = "修改";
                //tbarbtnEdit.ImageUrl = "/_layouts/images/edit.gif";
                //tbarbtnEdit.Click += new EventHandler(tbarbtnEdit_Click);
                //tbarTop.Buttons.Controls.Add(tbarbtnEdit);
                //删除


                ToolBarButton tbarbtnDelte = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
                tbarbtnDelte.ID = "DeleteRow";
                tbarbtnDelte.Text = "删除";
                tbarbtnDelte.ImageUrl = "/_layouts/images/delete.gif";
                tbarbtnDelte.Click += new EventHandler(tbarbtnDelte_Click);
                StringBuilder sbScript = new StringBuilder();
                sbScript.Append("var aa= window.confirm('确认删除所选项?');");
                sbScript.Append("if(aa == false){");
                sbScript.Append("return false;}");
                tbarbtnDelte.OnClientClick = sbScript.ToString();
              

                //返回
                ToolBarButton tbarbtnBack = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
                tbarbtnBack.ID = "backRow";
                tbarbtnBack.Text = "确认并返回";
                tbarbtnBack.ImageUrl = "/_layouts/images/BACK.GIF";
                tbarbtnBack.Click += new EventHandler(tbarbtnBack_Click);
              
             //返回
                ToolBarButton tbarBack = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
                tbarBack.ID = "backRow";
                tbarBack.Text = "返回";
                tbarBack.ImageUrl = "/_layouts/images/BACK.GIF";
                tbarBack.Click += new EventHandler(tbarBack_Click);

                ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
                btnRefresh.ID = "btnRefresh";
                btnRefresh.Text = "刷新";
                btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
                btnRefresh.Padding = "0,5,0,0";
                btnRefresh.Click += new EventHandler(btnRefresh_Click);
                tbarTop.RightButtons.Controls.Add(btnRefresh);
                if (bo)
                {
                    tbarTop.Buttons.Controls.Add(tbarbtnAdd);
                    tbarTop.Buttons.Controls.Add(tbarbtnDelte);
                    if (!((Panel)GetControltByMaster("PanelDone")).Visible)
                    {
                        tbarTop.Buttons.Controls.Add(tbarbtnBack);
                    }
                    
                }
                else
                {
                     tbarTop.Buttons.Controls.Add(tbarBack);
                     ((Label)GetControltByMaster("intask")).Visible = true;
                }
             //   tbarTop.Visible = bo;
                
        }

        void tbarBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("StockTransferManager.aspx");
        }
        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            //更新数据
            upDataChanges(true);

            //返回
          
        }

        private void upDataChanges(bool bolJump)
        {
            try
            {
                int intFail = 0;
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                   
                    foreach (GridViewRow gvr in this.gv.Rows)
                    {
                        StockTransferDetail sod = db.StockTransferDetail.SingleOrDefault(u => u.StockTransferDetailID == Convert.ToInt32(((CheckBox)gvr.Cells[0].FindControl("SMItem")).ToolTip));


                        var tboxNums = (TextBox)(gvr.Cells[6].Controls[0]);
                        var tboxNum = (TextBox)(gvr.Cells[7].Controls[0]);
                        var tboxNumq = (TextBox)(gvr.Cells[8].Controls[0]);
                        var ddlType = (DropDownList)(gvr.Cells[9].Controls[0]);

                        decimal dcmT = 0;
                        decimal dcmT1 = 0;
                        decimal dcmT2 = 0;
                        decimal.TryParse(tboxNums.Text, out dcmT);
                        decimal.TryParse(tboxNum.Text, out dcmT1);                        
                        decimal.TryParse(tboxNumq.Text, out dcmT2);                            

                        bool done = false;
                        switch (ddlType.Text)
                        {
                            case "根/台/套/件":
                                if (dcmT > 0)
                                {
                                    done = true;
                                  
                                }
                                break;
                            case "米":
                                if (dcmT1 > 0)
                                {
                                    done = true;
                                    
                                }
                                break;
                            case "吨":
                                if (dcmT2 > 0)
                                {
                                    done = true;
                                   
                                }
                                //  sod.Quantity = decimal.Parse(st.QuantityTon.ToString());
                                break;
                        }
                        if (done)
                        {
                            sod.QuantityGentaojian = dcmT;
                            sod.QuantityMetre = dcmT1;
                            sod.QuantityTon = dcmT2;
                            db.SubmitChanges();
                        }
                        else
                        {
                          //  StockTransferDetail sd = db.StockTransferDetail.SingleOrDefault(u => u.StockTransferDetailID == Convert.ToInt32(((CheckBox)gvr.Cells[0].FindControl("SMItem")).ToolTip));
                            intFail++;
                            continue;
                        }
                       // sod.Quantity = Convert.ToDecimal(((TextBox)gvr.Cells[6].Controls[0]).Text);
                        //sod.FinanceCode = ((TextBox)gvr.Cells[7].Controls[0]).Text.Trim();
                        //sod.Amount = sod.Quantity * Convert.ToDecimal(gvr.Cells[8].Text);
                        //sod.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                        
                    }
                }
                if (intFail > 0)
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('您修改的记录中"+intFail.ToString()+"条数据修改不符合规范,关键单位的数量必须大于0!请检查!')</script>");
                    Response.AddHeader("Refresh", "0");
                }
                else
                {
                  if( bolJump)
                    Response.Redirect("StockTransferManager.aspx");
                  else
                      BindGridView(false);
                }
            }
            catch (Exception ex)
            {
                //MethodBase mb = MethodBase.GetCurrentMethod();
                //LogToDBHelper lhelper = LogToDBHelper.Instance;
                //lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                //ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));
            }

        }


        protected void tbarbtnAdd_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request.QueryString["StockTransferID"]))
            {
                Response.Redirect("StockTransferDetailsSelect.aspx?StockTransferID=" + Request.QueryString["StockTransferID"]);
                //if (!string.IsNullOrEmpty(Request.QueryString["StockTransferID"]))
                //{
                //    Response.Redirect("StorageOutDetailsSelect.aspx?StockTransferID=" + Request.QueryString["StockTransferID"]);
                //}
                //else
                //{
                   
                //}
                
            }
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello! ')</script>");

        }

      

        void btnRefresh_Click(object sender, EventArgs e)
        {

            Response.AddHeader("Refresh", "0");
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

        protected void tbarbtnDelte_Click(object sender, EventArgs e)
        {
            List<CheckBox> listString = GetCheckedID();
            if (listString.Count > 0)
            {
                StockTransferDetail SID;
                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    foreach (var li in listString)
                    {
                        SID = db.StockTransferDetail.SingleOrDefault(a => a.StockTransferDetailID == int.Parse(li.ToolTip));
                        if (SID != null)
                        {
                            db.StockTransferDetail.DeleteOnSubmit(SID);

                        }
                    }
                    db.SubmitChanges();
                }
                Response.Redirect("StockTransferDetailsManage.aspx?StockTransferID=" + Request.QueryString["StockTransferID"] + "");
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择需要删除的记录!')</script>");
                return;
            }

        }

        /// <summary>
        /// 绑定Gridview
        /// </summary>
        private void BindGridView(bool bolCanEdit)
        {
            this.gv.Columns.Clear();
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                BoundField bfColumn;
                //添加选择列
                TemplateField tfieldCheckbox = new TemplateField();
                tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "StockTransferDetailID");
                tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);


                TemplateField tfieldTextBox = new TemplateField();
                tfieldTextBox.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "Quantity", "StockTransferDetailID", "txtCount");
                tfieldTextBox.HeaderTemplate = new MulTextBoxTemplate("调拨数量", DataControlRowType.Header);
                tfieldTextBox.ItemStyle.Width = 150;

                //TemplateField code_fieldTextBox = new TemplateField();
                //code_fieldTextBox.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "StorageOutDetailsID", "txtCode");
                //code_fieldTextBox.HeaderTemplate = new MulTextBoxTemplate("财务编号", DataControlRowType.Header);
                //code_fieldTextBox.ItemStyle.Width = 150;

                if (bolCanEdit)
                    this.gv.Columns.Add(tfieldCheckbox);
                foreach (var kvp in Titlelist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.gv.Columns.Add(bfColumn);
                }
                if (bolCanEdit)
                {
                    //加入回收数量--根套件列
                    TemplateField tfQuantityGtj = new TemplateField();
                    tfQuantityGtj.HeaderText = "根/套/件";
                    tfQuantityGtj.ItemTemplate = new TextBoxTemplate("QuantityGentaojian", "QuantityGentaojian", "^(-?\\d+)(\\.\\d+)?$");
                    this.gv.Columns.Insert(6, tfQuantityGtj);

                    //加入回收数量--米列
                    TemplateField tfQuantityMetre = new TemplateField();
                    tfQuantityMetre.HeaderText = "米";
                    tfQuantityMetre.ItemTemplate = new TextBoxTemplate("QuantityMetre", "QuantityMetre", "^(-?\\d+)(\\.\\d+)?$");
                    this.gv.Columns.Insert(7, tfQuantityMetre);

                    //加入回收数量--吨列
                    TemplateField tfQuantityTon = new TemplateField();
                    tfQuantityTon.HeaderText = "吨";
                    tfQuantityTon.ItemTemplate = new TextBoxTemplate("QuantityTon", "QuantityTon", "^(-?\\d+)(\\.\\d+)?$");
                    this.gv.Columns.Insert(8, tfQuantityTon);

                    //加入当前计量单位
                    TemplateField tfCurUnit = new TemplateField();
                    tfCurUnit.HeaderText = "计算单位";
                    string[] units = new string[] { "根/台/套/件", "米", "吨" };
                    tfCurUnit.ItemTemplate = new DropDownListTemplate("CurUnit", DataControlRowType.DataRow, units, false);
                    this.gv.Columns.Insert(9, tfCurUnit);
                }
                else
                {

                    bfColumn = new BoundField();
                    bfColumn.HeaderText ="根/套/件";
                    bfColumn.DataField = "QuantityGentaojian";
                    this.gv.Columns.Insert(6,bfColumn);

                    bfColumn = new BoundField();
                    bfColumn.HeaderText = "米";
                    bfColumn.DataField = "QuantityMetre";
                    this.gv.Columns.Insert(7, bfColumn);

                    bfColumn = new BoundField();
                    bfColumn.HeaderText ="吨";
                    bfColumn.DataField = "QuantityTon";
                    this.gv.Columns.Insert(8, bfColumn);

                    bfColumn = new BoundField();
                    bfColumn.HeaderText = "计算单位";
                    bfColumn.DataField = "CurUnit";
                    this.gv.Columns.Insert(9, bfColumn);
                }

               // this.gv.Columns.Insert(6, tfieldTextBox);
                //this.gv.Columns.Insert(7, code_fieldTextBox);

                var Dsource = from a in db.StockTransferDetail
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
                                  c.MaterialName,
                                  c.MaterialCode,
                                  c.SpecificationModel,
                                  c.StorageName,
                                  c.PileName,
                                  c.CurUnit,
                                  a.QuantityGentaojian,
                                  a.QuantityMetre,
                                  a.QuantityTon,
                                  //a.
                                  target = a.PileInfo.StorageInfo.StorageName + "|" + a.PileInfo.PileName + "|" + a.PileInfo.PileCode
                              };
                 this.gv.DataSource = Dsource;
                this.gv.DataBind();

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
