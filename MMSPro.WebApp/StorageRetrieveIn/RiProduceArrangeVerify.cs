/*------------------------------------------------------------------------------
 * Unit Name：RiMaterialRepairAudit.cs
 * Description: 回收入库--生产组安排质检的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-09-14
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
    public class RiProduceArrangeVerify:Page
    {
        private int _taskid, _formid;
        private SPGridView spgvMaterial;
        private Button btnOK;
        private DateTimeControl dtcVerifyTime;
        private TextBox txtRemark;

        private bool bModified = false;//标识是否已经生成了生产组员的回收检验传递表

        private static string[] ShowTlist = {                                                                                                                        
                                                  "物资名称:MaterialName",
                                                  "规格型号:SpecificationModel",                                     
                                                  "生产厂家:ManufacturerName",
                                                  "回收数量:TotleGentaojian",
                                                  "回收日期:RetrieveTime",
                                                  "仓库:StorageName",
                                                  "垛位:PileName",
                                                  "备注:Remark",    
                                                  "SrinMaterialVerifyDetailsID:SrinMaterialVerifyDetailsID"
                                             };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._taskid = Convert.ToInt32(Request.QueryString["TaskID"]);
                using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    TaskStorageIn tsi = db.TaskStorageIn.SingleOrDefault(u => u.TaskStorageID.Equals(_taskid));
                    if (tsi.TaskState.Equals("已完成"))
                    {
                        Response.Redirect(string.Format("RiProduceArrangeVerifyMessage.aspx?TaskID={0}", _taskid), false);
                        return;
                    }
                    _formid = tsi.StorageInID;
                    if (db.SrinProduceVerifyTransfer.Count(u => u.TaskID.Equals(_taskid)) != 0)
                        bModified = true;
                    
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

            dtcVerifyTime = GetControltByMaster("dtcVerifyTime") as DateTimeControl;
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

        }

        private void BindDataToCustomControls()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化表头信息
                SrinVerifyTransfer svt = db.SrinVerifyTransfer.SingleOrDefault(u => u.SrinVerifyTransferID.Equals(_formid));

                ((Label)GetControltByMaster("lblProject")).Text = svt.SrinReceipt.SrinStocktakingConfirm.SrinStocktaking.SrinSubDoc.ProjectInfo.ProjectName;
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(svt.CreateTime.ToLongDateString(), svt.CreateTime.ToLongTimeString());
                ((Label)GetControltByMaster("lblCode")).Text = svt.SrinVerifyTransferCode;

                //初始化回收检验物资
                spgvMaterial.DataSource = from a in db.SrinMaterialVerifyDetails
                                          where a.SrinVerifyTransferID == _formid
                                          select new
                                          {
                                              a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.MaterialName,
                                              a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.MaterialInfo.SpecificationModel,
                                              a.Manufacturer.ManufacturerName,
                                              a.SrinDetails.SrinStocktakingDetails.SrinSubDetails.TotleGentaojian,
                                              a.SrinDetails.SrinStocktakingDetails.StorageInfo.StorageName,
                                              a.SrinDetails.SrinStocktakingDetails.PileInfo.PileName,
                                              a.RetrieveTime,
                                              a.Remark,
                                              a.SrinMaterialVerifyDetailsID
                                          };
                this.spgvMaterial.DataBind();

                //初始化表尾
                ((Label)GetControltByMaster("lblMaterial")).Text = svt.EmpInfo.EmpName;
                ((Label)GetControltByMaster("lblReadyWork")).Text = svt.ReadyWorkIsFinished ? "是" : "否";
                if (!Page.IsPostBack)
                {
                    if (bModified)//分支流程--已经生成质检清单的情况
                    {
                        SrinProduceVerifyTransfer spvt = db.SrinProduceVerifyTransfer.SingleOrDefault(u => u.TaskID == _taskid);
                        txtRemark.Text = spvt.Remark.Trim();
                        dtcVerifyTime.SelectedDate = spvt.VerifyTime;
                    }
                    else
                    {
                        txtRemark.Text = svt.Remark.Trim();
                        dtcVerifyTime.SelectedDate = DateTime.Now.Date;
                    }
                }
            }
        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);

            spgvMaterial.Columns[8].Visible = false;

            if (bModified)//分支流程--已经生成质检清单的情况
                btnOK.Text = "修改质检清单";

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
                    if (bModified)//分支流程--已经生成质检清单的情况
                    {
                        SrinProduceVerifyTransfer spvt = db.SrinProduceVerifyTransfer.SingleOrDefault(u => u.TaskID == _taskid);
                        spvt.VerifyTime = dtcVerifyTime.SelectedDate;
                        spvt.Remark = txtRemark.Text.Trim();
                        spvt.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                    }
                    else
                    {
                        SrinProduceVerifyTransfer spvt = new SrinProduceVerifyTransfer();
                        spvt.SrinVerifyTransferID = _formid;
                        spvt.VerifyTime = dtcVerifyTime.SelectedDate;
                        spvt.Remark = txtRemark.Text.Trim();
                        spvt.CreateTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { }).First();
                        spvt.Creator = db.EmpInfo.SingleOrDefault(u => u.Account == SPContext.Current.Web.CurrentUser.LoginName).EmpID;
                        spvt.TaskID = _taskid;
                        db.SrinProduceVerifyTransfer.InsertOnSubmit(spvt);
                    }
                    db.SubmitChanges();
                }
                Response.Redirect(string.Format("RiProduceArrangeVerifyMessage.aspx?TaskID={0}", _taskid), false);
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
