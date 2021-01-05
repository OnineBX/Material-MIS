/*------------------------------------------------------------------------------
 * Unit Name：ViewSrinSubDetails.cs
 * Description: 回收入库--查看回收分单物资的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-07-29
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
    public class ViewSrinSubDetails:System.Web.UI.Page
    {
        private int _subdocid;
        private SPGridView spgvMaterial;     

        private static string[] ShowTlist = {                                                                                                                        
                                              "物资名称:MaterialName",
                                              "规格型号:SpecificationModel",                                              
                                              "财务编码:FinanceCode",                                                                              
                                              "根/台/套/件:TotleGentaojian",
                                              "米:TotleMetre",
                                              "吨:TotleTon",                                                                                         
                                              "回收单号:RetrieveCode",
                                              "备注:Remark"
                                            };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {                                
                _subdocid = Convert.ToInt32(Request.QueryString["SubDocID"]);

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
            //初始化ToolBar
            InitToolBar();

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
                SrinSubDoc ssd = db.SrinSubDoc.SingleOrDefault(u => u.SrinSubDocID == _subdocid);

                ((Label)GetControltByMaster("lblProject")).Text = ssd.ProjectInfo.ProjectName;
                ((Label)GetControltByMaster("lblCount")).Text = ssd.SrinSubDetails.Count.ToString();
                ((Label)GetControltByMaster("lblDate")).Text = string.Concat(ssd.CreateTime.ToLongDateString(),ssd.CreateTime.ToLongTimeString());

                //初始化回收分单中的物资
                this.spgvMaterial.DataSource = from a in db.SrinSubDetails
                                               where a.SrinSubDocID == _subdocid
                                               select new
                                               {
                                                   a.MaterialInfo.MaterialName,
                                                   a.MaterialInfo.SpecificationModel,                                                   
                                                   a.MaterialInfo.FinanceCode,                                                  
                                                   a.TotleGentaojian,
                                                   a.TotleMetre,
                                                   a.TotleTon,
                                                   a.RetrieveCode,
                                                   a.Remark
                                               };
                this.spgvMaterial.DataBind();       
            }


        }

        private void ShowCustomControls()
        {
            Panel p1 = (Panel)GetControltByMaster("Panel1");
            p1.Controls.Add(this.spgvMaterial);            

        }

        #endregion

        #region 控件事件

        void tbarbtnBack_Click(object sender, EventArgs e)
        {

            Response.Redirect("ManageSrinSubDoc.aspx", false);
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {

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
