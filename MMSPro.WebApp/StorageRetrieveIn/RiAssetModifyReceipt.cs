/*------------------------------------------------------------------------------
 * Unit Name：RiAssetModifyReceipt.cs
 * Description: 回收入库--物资管理员修改回收入库单及其物资的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-08-23
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
    public class RiAssetModifyReceipt : Page
    {
        private int _taskid, _receiptid;
        private SPGridView spgvMaterial;
        private TextBox txtCode, txtRemark;
        private DateTimeControl dtcCreateTime;
        private Button btnOK;

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
                                              "ID:SrinDetailsID"
                                            };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    SrinReceipt srp = db.SrinReceipt.SingleOrDefault(u => u.TaskID == _taskid);
                    _receiptid = srp.SrinReceiptID;
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

        private void InitializeCustomControls()
        {
            InitToolBar();

            txtCode = (TextBox)GetControltByMaster("txtCode");
            txtRemark = (TextBox)GetControltByMaster("txtRemark");
            dtcCreateTime = (DateTimeControl)GetControltByMaster("dtcCreateTime");

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
            tfUnitPrice.ItemTemplate = new TextBoxTemplate("UnitPrice", "UnitPrice", "^(-?\\d+)(\\.\\d+)?$", "0", 80);
            this.spgvMaterial.Columns.Insert(8, tfUnitPrice);

            TemplateField tfCurUnit = new TemplateField();
            tfCurUnit.HeaderText = "计量单位";
            tfCurUnit.ItemTemplate = new DropDownListTemplate("CurUnit", DataControlRowType.DataRow, new string[] { "根/台/套/件", "米", "吨" });
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
                SrinReceipt srp = db.SrinReceipt.SingleOrDefault(u => u.SrinReceiptID == _receiptid);
                
                ((Label)GetControltByMaster("lblCreater")).Text = srp.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblProject")).Text = srp.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.ProjectInfo.ProjectName;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(srp.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.CreateTime.ToLongDateString(), srp.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.CreateTime.ToLongTimeString());

                //初始化调拨明细
                this.spgvMaterial.DataSource = from a in db.SrinDetails
                                               where a.SrinReceiptID == _receiptid
                                               select new
                                               {
                                                   a.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.FinanceCode,                                                   
                                                   a.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                                   a.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                                   a.CurUnit,
                                                   a.UnitPrice,
                                                   a.SrinStocktakingDetails.SrinSubDetails.TotleGentaojian,
                                                   a.SrinStocktakingDetails.SrinSubDetails.TotleMetre,
                                                   a.SrinStocktakingDetails.SrinSubDetails.TotleTon,
                                                   a.SrinStocktakingDetails.SrinSubDetails.RetrieveCode,                                                   
                                                   a.SrinStocktakingDetails.StorageInfo.StorageName,
                                                   a.SrinStocktakingDetails.PileInfo.PileName,
                                                   a.Remark,                                                   
                                                   a.SrinDetailsID
                                               };
                this.spgvMaterial.DataBind();

                //初始化回收入库单信息

                if (!Page.IsPostBack)
                {
                    txtCode.Text = srp.SrinReceiptCode;
                    dtcCreateTime.SelectedDate = srp.CreateTime;
                    txtRemark.Text = srp.Remark;
                }

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
        
        void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    //修改回收物资设备入库单
                    SrinReceipt srp = db.SrinReceipt.SingleOrDefault(u => u.SrinReceiptID == _receiptid);
                    srp.SrinReceiptCode = txtCode.Text.Trim();
                    srp.CreateTime = dtcCreateTime.SelectedDate;
                    srp.Remark = txtRemark.Text.Trim();
                    db.SubmitChanges();
                    //修改物资明细                    
                    SrinDetails sdl;                    

                    foreach (GridViewRow gvr in spgvMaterial.Rows)
                    {                        
                        sdl = db.SrinDetails.SingleOrDefault(u => u.SrinDetailsID == Convert.ToInt32(gvr.Cells[12].Text));
                        sdl.UnitPrice = Convert.ToDecimal((gvr.Cells[8].Controls[0] as TextBox).Text);
                        sdl.CurUnit = (gvr.Cells[9].Controls[0] as DropDownList).Text;
                        sdl.Amount = Convert.ToDecimal(gvr.Cells[GetPricingIndex(sdl.CurUnit)].Text) * sdl.UnitPrice;
                        sdl.Remark = ((TextBox)gvr.Cells[11].Controls[0]).Text.Trim();
                        sdl.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                        db.SubmitChanges();
                    }                    
                }
                Response.Redirect(string.Format("RiAssetReceiptMessage.aspx?TaskID={0}", _taskid), false);
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
