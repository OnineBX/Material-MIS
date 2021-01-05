/*------------------------------------------------------------------------------
 * Unit Name：RiAssetCreateReceipt.cs
 * Description: 回收入库--物资管理员创建回收入库单的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-08-11
 * Modified Date:2010-10-15
 * ----------------------------------------------------------------------------*/
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
    public class RiAssetCreateReceipt:Page
    {
        private int _taskid,_confirmid;
        private SPGridView spgvMaterial;
        private Button btnOK;
        private CustomValidator vldCode;
        private TextBox txtCode;       

        private static string[] ShowTlist = {      
                                                "财务编码:FinanceCode",                                                
                                                "物资名称:MaterialName",                                              
                                                "规格型号:SpecificationModel",                                               
                                                "仓库:StorageName",
                                                "剁位:PileName",
                                                "根/台/套/件:TotleGentaojian",
                                                "米:TotleMetre",
                                                "吨:TotleTon",                                                
                                                "回收单号:RetrieveCode",                                               
                                                "ID:SrinStocktakingDetailsID"
                                            };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._taskid = Convert.ToInt32(Request.QueryString["TaskID"]);                
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID == _taskid);
                    _confirmid = tsi.StorageInID; 
                   
                    //分支流程--已经创建回收入库单的情况
                    SrinReceipt srp = db.SrinReceipt.SingleOrDefault(u => u.TaskID == _taskid);
                    if (srp != null)
                    {
                        if (db.TaskStorageIn.Count(u => u.TaskType.Equals("回收入库单资产组长确认") && u.PreviousTaskID == _taskid && u.StorageInID == srp.SrinReceiptID) != 0)//已经提交部门主管确认的情况
                            Response.Redirect(string.Format("RiAssetReceiptMessage.aspx?TaskID={0}", _taskid), false);                            
                        else//尚未提交部门主管确认的情况
                            Response.Redirect(string.Format("RiAssetModifyReceipt.aspx?TaskID={0}", _taskid), false);
                        return;
                    }
                }

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

        #region 初始化和数据绑定方法

        private void InitToolBar()
        {
            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");

            //返回
            ToolBarButton tbarbtnBack = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnBack.ID = "btnBack";
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

        private void InitializeCustomControls()
        {
            InitToolBar();           

            txtCode = GetControltByMaster("txtCode") as TextBox;

            vldCode = GetControltByMaster("vldCode") as CustomValidator;
            vldCode.ServerValidate += new ServerValidateEventHandler(vldCode_ServerValidate);

            btnOK = (Button)GetControltByMaster("btnOK");
            btnOK.Click += new EventHandler(btnOK_Click);            

            //初始化spgvMaterial
            this.spgvMaterial = new SPGridView();
            this.spgvMaterial.AutoGenerateColumns = false;
            this.spgvMaterial.Attributes.Add("style", "word-break:keep-all;word-wrap:normal");            

            BoundField bfColumn;

            foreach (var kvp in ShowTlist)
            {
                bfColumn = new BoundField();
                bfColumn.HeaderText = kvp.Split(':')[0];
                bfColumn.DataField = kvp.Split(':')[1];
                this.spgvMaterial.Columns.Add(bfColumn);
            }

            //单价
            TemplateField tfUnitPrice = new TemplateField();
            tfUnitPrice.HeaderText = "单价";
            tfUnitPrice.ItemTemplate = new TextBoxTemplate("UnitPrice", string.Empty, "^(-?\\d+)(\\.\\d+)?$", "0",80);
            this.spgvMaterial.Columns.Insert(8, tfUnitPrice);

            TemplateField tfCurUnit = new TemplateField();
            tfCurUnit.HeaderText = "计量单位";
            tfCurUnit.ItemTemplate = new DropDownListTemplate(string.Empty, DataControlRowType.DataRow,new string[]{"根/台/套/件","米","吨"});
            this.spgvMaterial.Columns.Insert(9, tfCurUnit);

            //加入备注列            
            TemplateField tfRemark = new TemplateField();
            tfRemark.HeaderText = "备注";
            tfRemark.ItemTemplate = new TextBoxTemplate("备注", DataControlRowType.DataRow, "Remark");
            this.spgvMaterial.Columns.Insert(11, tfRemark);
        }        
       
        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息
                SrinStocktakingConfirm sstc = db.SrinStocktakingConfirm.SingleOrDefault(u => u.SrinStocktakingConfirmID.Equals(_confirmid));

                ((Label)GetControltByMaster("lblCreater")).Text = sstc.SrinStocktaking.SrinSubDoc.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblProject")).Text = sstc.SrinStocktaking.SrinSubDoc.ProjectInfo.ProjectName;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(sstc.SrinStocktaking.SrinSubDoc.CreateTime.ToLongDateString(), sstc.SrinStocktaking.SrinSubDoc.CreateTime.ToLongTimeString());

                //初始化调拨明细
                this.spgvMaterial.DataSource = from a in db.SrinStocktakingDetails
                                               where a.SrinStocktakingID == sstc.SrinStocktaking.SrinStocktakingID
                                               select new
                                               {
                                                   a.SrinSubDetails.MaterialInfo.MaterialName,
                                                   a.SrinSubDetails.MaterialInfo.SpecificationModel,                                                  
                                                   a.SrinSubDetails.MaterialInfo.FinanceCode,                                                   
                                                   a.StorageInfo.StorageName,
                                                   a.PileInfo.PileName,
                                                   a.SrinSubDetails.TotleGentaojian,
                                                   a.SrinSubDetails.TotleMetre,
                                                   a.SrinSubDetails.TotleTon,                                                   
                                                   a.SrinSubDetails.RetrieveCode,
                                                   a.Remark,                                                   
                                                   a.SrinStocktakingDetailsID
                                               };
                this.spgvMaterial.DataBind();

                //初始化表尾
                ((Label)GetControltByMaster("lblMaterial")).Text = sstc.SrinStocktaking.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblInventoryDate")).Text = string.Concat(sstc.SrinStocktaking.StocktakingDate.ToLongDateString(), sstc.SrinStocktaking.StocktakingDate.ToLongTimeString());
                ((Label)GetControltByMaster("lblResult")).Text = sstc.SrinStocktaking.StocktakingResult.Trim();
                ((Label)GetControltByMaster("lblOpinion")).Text = sstc.SrinStocktaking.StocktakingProblem.Trim();                
                ((Label)GetControltByMaster("lblMChief")).Text = sstc.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblConfirmTime")).Text = string.Concat(sstc.ConfirmTime.ToLongDateString(), sstc.ConfirmTime.ToLongTimeString());
               
            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            spgvMaterial.Columns[12].Visible = false;            
        }

        #endregion

        #region 控件事件方法

        void btnRefresh_Click(object sender, EventArgs e)
        {

        }

        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("../../default-old.aspx", false);
        }         

        void vldCode_ServerValidate(object source, ServerValidateEventArgs args)
        {
            try
            {                
                    string strCode = this.txtCode.Text.Trim();

                    //回收入库单编号为空的情况
                    if (string.IsNullOrEmpty(strCode))
                    {
                        args.IsValid = false;
                        vldCode.Text = "回收入库单编号不能为空！";
                        return;
                    }

                    //数据库中存在相同回收入库单编号的情况
                    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {                        
                        if (db.SrinReceipt.Count(u => u.SrinReceiptCode.Equals(strCode)) != 0)
                        {
                            args.IsValid = false;
                            vldCode.Text = "回收入库单编号已存在！";
                            return;
                        }
                    }

                    args.IsValid = true;                
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_QUERYERROR));
            }           
        }        

        void btnOK_Click(object sender, EventArgs e)
        {           
            try
            {
                if (Page.IsValid)
                { 
                    using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        //生成回收物资设备入库单
                        SrinReceipt srp = new SrinReceipt();
                        srp.SrinReceiptCode = txtCode.Text.Trim();
                        srp.CreateTime = ((DateTimeControl)GetControltByMaster("dtcCreateTime")).SelectedDate;
                        srp.Remark = ((TextBox)GetControltByMaster("txtRemark")).Text.Trim();
                        srp.SrinStocktakingConfirmID = _confirmid;
                        srp.TaskID = _taskid;
                        srp.Creator = db.EmpInfo.SingleOrDefault(u => u.Account.Equals(SPContext.Current.Web.CurrentUser.LoginName)).EmpID;
                        db.SrinReceipt.InsertOnSubmit(srp);
                        db.SubmitChanges();

                        //添加物资明细
                        int iDetailsID;
                        SrinDetails sdl;                        
                        foreach (GridViewRow gvr in spgvMaterial.Rows)
                        {
                            iDetailsID = Convert.ToInt32(gvr.Cells[12].Text);                            
                            sdl = new SrinDetails();
                            sdl.SrinReceiptID = srp.SrinReceiptID;
                            sdl.SrinStocktakingDetailsID = iDetailsID;                            
                            sdl.UnitPrice = Convert.ToDecimal((gvr.Cells[8].Controls[0] as TextBox).Text);
                            sdl.CurUnit = (gvr.Cells[9].Controls[0] as DropDownList).Text;
                            sdl.Amount = Convert.ToDecimal(gvr.Cells[GetPricingIndex(sdl.CurUnit)].Text) * sdl.UnitPrice;
                            sdl.Remark = (gvr.Cells[11].Controls[0] as TextBox).Text.Trim();
                            sdl.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                            sdl.Creator = db.EmpInfo.SingleOrDefault(u => u.Account == SPContext.Current.Web.CurrentUser.LoginName).EmpID;
                            db.SrinDetails.InsertOnSubmit(sdl);
                        }
                        db.SubmitChanges();
                    }
                    Response.Redirect(string.Format("RiAssetReceiptMessage.aspx?TaskID={0}", _taskid), false);
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


        #endregion

        #region 辅助方法

        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }

        private int GetPricingIndex(string curunit)
        {
            switch (curunit)
            {
                case "根/台/套/件":
                    return 5;
                case "米":
                    return 6;
                case "吨":
                    return 7;
                default:
                    return -1;
            }
        }

        #endregion
    }
}
