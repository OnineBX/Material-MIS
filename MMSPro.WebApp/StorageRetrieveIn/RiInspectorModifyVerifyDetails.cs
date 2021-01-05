/*------------------------------------------------------------------------------
 * Unit Name：RiInspectorModifyVerifyDetails.cs
 * Description: 回收入库--检验人员修改质检结果的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-09-16
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
using System.Data.Linq.SqlClient;

namespace MMSPro.WebApp
{
    public class RiInspectorModifyVerifyDetails:Page
    {
        private int _taskid, _transferid;//_transferid是生产组员处理之后的回首检验传递表
        private SPGridView spgvMaterial;
        private Button btnOK;
        private TextBox txtRemark;

        private static string[] ShowTlist = {                                                                                                                        
                                                  "物资名称:MaterialName",
                                                  "规格型号:SpecificationModel",                                     
                                                  "生产厂家:ManufacturerName",
                                                  "回收数量:TotleGentaojian",
                                                  "回收日期:RetrieveTime",
                                                  "仓库:StorageName",
                                                  "垛位:PileName",   
                                                  "SrinInspectorVerifyDetailsID:SrinInspectorVerifyDetailsID"
                                             };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    _transferid = db.SrinInspectorVerifyTransfer.SingleOrDefault(u => u.TaskID.Equals(_taskid)).SrinInspectorVerifyTransferID;

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

            txtRemark = GetControltByMaster("txtRemark") as TextBox;
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

            //加入合格数量列
            TemplateField tfQualified = new TemplateField();
            tfQualified.HeaderText = "合格数量";
            tfQualified.ItemTemplate = new TextBoxTemplate("Qualified", "QualifiedGentaojian", "^(-?\\d+)(\\.\\d+)?$", 80);
            this.spgvMaterial.Columns.Insert(4, tfQualified);

            //加入待维修列
            TemplateField tfRepair = new TemplateField();
            tfRepair.HeaderText = "待维修数量";
            tfRepair.ItemTemplate = new TextBoxTemplate("Repair", "RepairGentaojian", "^(-?\\d+)(\\.\\d+)?$",80);
            this.spgvMaterial.Columns.Insert(5, tfRepair);

            //加入待报废列
            TemplateField tfReject = new TemplateField();
            tfReject.HeaderText = "待报废数量";
            tfReject.ItemTemplate = new TextBoxTemplate("Reject", "RejectGentaojian", "^(-?\\d+)(\\.\\d+)?$", 80);
            this.spgvMaterial.Columns.Insert(6, tfReject);

            //加入质检报告号列
            TemplateField tfRetrieveCode = new TemplateField();
            tfRetrieveCode.HeaderText = "质检报告号";
            tfRetrieveCode.ItemTemplate = new TextBoxTemplate("VerifyCode", DataControlRowType.DataRow,"VerifyCode");
            this.spgvMaterial.Columns.Insert(7, tfRetrieveCode);            

            //加入质检日期列
            TemplateField tfRealVerifyDate = new TemplateField();
            tfRealVerifyDate.HeaderText = "质检日期";
            tfRealVerifyDate.ItemTemplate = new DateTimeTemplate("RealVerifyTime");
            this.spgvMaterial.Columns.Insert(8, tfRealVerifyDate);

            //加入备注列            
            TemplateField tfRemark = new TemplateField();
            tfRemark.HeaderText = "备注";
            tfRemark.ItemTemplate = new TextBoxTemplate("备注", DataControlRowType.DataRow, "Remark");
            this.spgvMaterial.Columns.Insert(12, tfRemark);

        }

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {               
                //初始化表头信息
                SrinInspectorVerifyTransfer sivt = db.SrinInspectorVerifyTransfer.SingleOrDefault(u => u.TaskID == _taskid);

                ((Label)GetControltByMaster("lblProject")).Text = sivt.SrinProduceVerifyTransfer.SrinVerifyTransfer.SrinReceipt.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.ProjectInfo.ProjectName;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(sivt.SrinProduceVerifyTransfer.SrinVerifyTransfer.CreateTime.ToLongDateString(), sivt.SrinProduceVerifyTransfer.SrinVerifyTransfer.CreateTime.ToLongTimeString());
                ((Label)GetControltByMaster("lblCode")).Text = sivt.SrinProduceVerifyTransfer.SrinVerifyTransfer.SrinVerifyTransferCode;
                ((Label)GetControltByMaster("lblMaterial")).Text = sivt.SrinProduceVerifyTransfer.SrinVerifyTransfer.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblReadyWork")).Text = sivt.SrinProduceVerifyTransfer.SrinVerifyTransfer.ReadyWorkIsFinished ? "是" : "否";
                ((Label)GetControltByMaster("lblProduce")).Text = sivt.SrinProduceVerifyTransfer.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblVerifyTime")).Text = sivt.SrinProduceVerifyTransfer.VerifyTime.ToLongDateString();

                //初始化回收检验物资

                this.spgvMaterial.DataSource = from a in db.SrinInspectorVerifyDetails
                                                 where a.SrinInspectorVerifyTransferID == _transferid
                                                 select new
                                                 {
                                                     a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                                     a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                                     a.SrinMaterialVerifyDetails.Manufacturer.ManufacturerName,
                                                     a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.SrinSubDetails.TotleGentaojian,
                                                     a.QualifiedGentaojian,
                                                     a.RepairGentaojian,
                                                     a.RejectGentaojian,
                                                     a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.StorageInfo.StorageName,
                                                     a.SrinMaterialVerifyDetails.SrinDetails.SrinStocktakingDetails.PileInfo.PileName,
                                                     a.SrinMaterialVerifyDetails.RetrieveTime,
                                                     a.RealVerifyTime,
                                                     a.VerifyCode,
                                                     a.Remark,
                                                     a.SrinInspectorVerifyDetailsID
                                                 };
                this.spgvMaterial.DataBind();

                //初始化表尾
                if(!Page.IsPostBack)
                    txtRemark.Text = sivt.Remark;

            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            spgvMaterial.Columns[13].Visible = false;

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
                //将确认结果保存到数据库
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {                    
                    //修改回收检验传递表--质检人员
                    SrinInspectorVerifyTransfer sivt = db.SrinInspectorVerifyTransfer.SingleOrDefault(u => u.SrinInspectorVerifyTransferID.Equals(_transferid));                    
                    sivt.Remark = txtRemark.Text.Trim();
                    sivt.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();                    

                    //生成质检清单
                    SrinInspectorVerifyDetails sivd;
                    foreach (GridViewRow gvr in this.spgvMaterial.Rows)
                    {
                        sivd = db.SrinInspectorVerifyDetails.SingleOrDefault(u => u.SrinInspectorVerifyDetailsID.Equals(Convert.ToInt32(gvr.Cells[13].Text)));                    
                        sivd.QualifiedGentaojian = Convert.ToDecimal((gvr.Cells[4].Controls[0] as TextBox).Text.Trim());
                        sivd.RepairGentaojian = Convert.ToDecimal((gvr.Cells[5].Controls[0] as TextBox).Text.Trim());
                        sivd.RejectGentaojian = Convert.ToDecimal((gvr.Cells[6].Controls[0] as TextBox).Text.Trim());
                        sivd.VerifyCode = (gvr.Cells[7].Controls[0] as TextBox).Text.Trim();
                        sivd.RealVerifyTime = (gvr.Cells[8].Controls[0] as DateTimeControl).SelectedDate;
                        sivd.Remark = (gvr.Cells[12].Controls[0] as TextBox).Text.Trim();
                        sivd.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();                                                
                    }
                    db.SubmitChanges();
                }
                Response.Redirect(string.Format("RiInspectorVerifyDetailsMessage.aspx?TaskID={0}", _taskid), false);
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

        #endregion
    }
}
