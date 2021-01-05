/*------------------------------------------------------------------------------
 * Unit Name：ViewStorageOutDetails.cs
 * Description: 正常出库--通过调拨单查看物资调拨明细的页面
 * Author: Xu Chun Lei
 * Created Date: 2010-10-28
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
using org.in2bits.MyXls;
using System.Data;
namespace MMSPro.WebApp
{
    public class ViewStorageOutDetails:System.Web.UI.Page
    {        
        private int _noticeid;
        private SPGridView spgvMaterial;

        private static string[] ShowTlist =  { 
                                                 "财务编码:FinanceCode", 
                                                 "物资名称:MaterialName",
                                                 "规格型号:SpecificationModel",                                                                                         
                                                 "库存数量(根/台/套/件):StocksGentaojian",
                                                 "调拨数量(根/台/套/件):Gentaojian",   
                                                 "库存数量(米):StocksMetre",
                                                 "调拨数量(米):Metre",
                                                 "库存数量(吨):StocksTon",
                                                 "调拨数量(吨):Ton",
                                                 "备注:Remark"
                                              };

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this._noticeid = Convert.ToInt32(Request.QueryString["NoticeID"]);
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

            ToolBarButton btnDown = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnDown.ID = "btnDown";
            btnDown.Text = "生成调拨单";
            btnDown.ImageUrl = "/_layouts/images/lg_icxls.gif";
            btnDown.Padding = "0,5,0,0";
            btnDown.Click += new EventHandler(btnDown_Click);
            tbarTop.Buttons.Controls.Add(btnDown);

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
                //初始化调拨明细
                this.spgvMaterial.DataSource = ( from a in db.StorageOutDetails
                                                 join b in db.StorageStocks on a.MaterialID equals b.MaterialID
                                                 where a.StorageOutNoticeID == _noticeid
                                                 select new
                                                 {
                                                     a.MaterialInfo.FinanceCode,
                                                     a.MaterialInfo.MaterialName,
                                                     a.MaterialInfo.SpecificationModel,
                                                     a.Gentaojian,
                                                     a.Metre,
                                                     a.Ton,
                                                     StocksGenTaojian = (from c in db.StorageStocks
                                                                         where c.MaterialID == a.MaterialID
                                                                         select c).Sum(u => u.StocksGenTaojian),
                                                     StocksMetre = (from c in db.StorageStocks
                                                                    where c.MaterialID == a.MaterialID
                                                                    select c).Sum(u => u.StocksMetre),
                                                     StocksTon = (from c in db.StorageStocks
                                                                  where c.MaterialID == a.MaterialID
                                                                  select c).Sum(u => u.StocksTon),
                                                     a.Remark
                                                 }).Distinct();
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

            Response.Redirect("ManageStorageOutNotice.aspx",false);
        }        

        void btnRefresh_Click(object sender, EventArgs e)
        {


        }
        void btnDown_Click(object sender, EventArgs e)
        {
            XlsDocument xd = new XlsDocument();
            #region  单元格样式
            #region 表头
            XF xf1 = xd.NewXF();//为xls生成一个XF实例（XF是cell格式对象）
            xf1.HorizontalAlignment = HorizontalAlignments.Centered;//设定文字居中
            xf1.Font.Height = 12 * 20;//设定字大小（字体大小是以 1/20 point 为单位的）
            xf1.UseBorder = true;//使用边框
            xf1.BottomLineStyle = 2;//设定边框底线为粗线      
            xf1.LeftLineStyle = 2; //设定边框左线为粗线
            xf1.TopLineStyle = 2; //
            xf1.RightLineStyle = 2; //
            xf1.Font.Bold = true;
            #endregion


            #region 表头
            XF xf2 = xd.NewXF();//为xls生成一个XF实例（XF是cell格式对象）
            xf2.HorizontalAlignment = HorizontalAlignments.Centered;//设定文字居中
            xf2.Font.Height = 10 * 20;//设定字大小（字体大小是以 1/20 point 为单位的）
            xf2.UseBorder = true;//使用边框
            xf2.BottomLineStyle = 2;//设定边框底线为粗线      
            xf2.LeftLineStyle = 2; //设定边框左线为粗线
            xf2.TopLineStyle = 2; //
            xf2.RightLineStyle = 2; //
            #endregion

            #region 表头
            XF xf3 = xd.NewXF();//为xls生成一个XF实例（XF是cell格式对象）
            xf3.HorizontalAlignment = HorizontalAlignments.Centered;//设定文字居中
            xf3.Font.Height = 16 * 20;//设定字大小（字体大小是以 1/20 point 为单位的）

            #endregion
            #endregion
            xd.FileName = HttpUtility.UrlEncode("出库调拨单");
            Worksheet ws;
            DataTable dt = new DataTable();
            int i = 0;
            int intRN;
            ws = xd.Workbook.Worksheets.Add("出库调拨单");
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                 var t =  db.StorageOutNotice.SingleOrDefault(a=>a.StorageOutNoticeID == _noticeid);
            #region 表头
            ws.Cells.Add(1, 1, "西南油气分公司物资供应处", xf3);
            ws.Cells.Add(2, 1, "物资设备调拨单", xf3);
            ws.Cells.Add(3, 1, "项目名称：" + t.ProjectInfo.ProjectName);
            ws.Cells.Add(3, 2, null);
            ws.Cells.Add(3, 3, null);
            ws.Cells.Add(3, 4, null);
            ws.Cells.Add(3, 5, "业主单位："+t.BusinessUnitInfo1.BusinessUnitName);
            ws.Cells.Add(3, 6, null);
            ws.Cells.Add(3, 7, null);
            ws.Cells.Add(3, 8, null);
            ws.Cells.Add(3, 9, null);
            ws.Cells.Add(3, 10, null);
            ws.Cells.Add(3, 11, null);
            ws.Cells.Add(3, 12, "日  期："+DateTime.Now.ToString());
            ws.Cells.Add(3, 13, null);
            ws.Cells.Add(3, 14, null);
            ws.Cells.Add(3, 15, null);
            ws.Cells.Add(4, 1, "项目性质："+t.ProjectInfo.ProjectProperty);
            ws.Cells.Add(4, 2, null);
            ws.Cells.Add(4, 3, null);
            ws.Cells.Add(4, 4, null);
            ws.Cells.Add(4, 5, null);
            ws.Cells.Add(4, 6, null);
            ws.Cells.Add(4, 7, null);
            ws.Cells.Add(4, 8, null);
            ws.Cells.Add(4, 9, null);
            ws.Cells.Add(4, 10, null);
            ws.Cells.Add(4, 11, null);
            ws.Cells.Add(4, 12, null);
            ws.Cells.Add(4, 13, null);
            ws.Cells.Add(4, 14, null);
            ws.Cells.Add(4, 15, null);
            ws.Cells.Add(5, 1, "项目阶段："+t.ProjectStage);
            ws.Cells.Add(5, 2, null);
            ws.Cells.Add(5, 3, null);
            ws.Cells.Add(5, 4, null);
            ws.Cells.Add(5, 5, "施工单位："+t.BusinessUnitInfo.BusinessUnitName);
            ws.Cells.Add(5, 6, null);
            ws.Cells.Add(5, 7, null);
            ws.Cells.Add(5, 8, null);
            ws.Cells.Add(5, 9, null);
            ws.Cells.Add(5, 10, null);
            ws.Cells.Add(5, 11, null);
            ws.Cells.Add(5, 12, "编  号："+t.StorageOutNoticeCode);
            ws.Cells.Add(5, 13, null);
            ws.Cells.Add(5, 14, null);
            ws.Cells.Add(5, 15, null);
            ws.Cells.Add(6, 1, "财务编码", xf1);
            ws.Cells.Add(6, 2, "物料编码", xf1);
            ws.Cells.Add(6, 3, "物资名称", xf1);
            ws.Cells.Add(6, 4, "规格型号", xf1);
            ws.Cells.Add(6, 5, "生产厂家", xf1);
            ws.Cells.Add(6, 6, "进库日期", xf1);
            ws.Cells.Add(6, 7, "预发数", xf1);
            ws.Cells.Add(6, 8, "", xf1);
            ws.Cells.Add(6, 9, "", xf1);
            ws.Cells.Add(6, 10, "实发数量", xf1);
            ws.Cells.Add(6, 11, "", xf1);
            ws.Cells.Add(6, 12, "", xf1);
            ws.Cells.Add(6, 13, "计量单位", xf1);
            ws.Cells.Add(6, 14, "单价（元）", xf1);
            ws.Cells.Add(6, 15, "金额（元）", xf1);
            ws.Cells.Add(7, 1, "", xf1);
            ws.Cells.Add(7, 2, "", xf1);
            ws.Cells.Add(7, 3, "", xf1);
            ws.Cells.Add(7, 4, "", xf1);
            ws.Cells.Add(7, 5, "", xf1);
            ws.Cells.Add(7, 6, "", xf1);
            ws.Cells.Add(7, 7, "根/件/套", xf1);
            ws.Cells.Add(7, 8, "米", xf1);
            ws.Cells.Add(7, 9, "吨", xf1);
            ws.Cells.Add(7, 10, "根/件/套", xf1);
            ws.Cells.Add(7, 11, "米", xf1);
            ws.Cells.Add(7, 12, "吨", xf1);
            ws.Cells.Add(7, 13, "", xf1);
            ws.Cells.Add(7, 14, "", xf1);
            ws.Cells.Add(7, 15, "", xf1);
            ws.AddMergeArea(new MergeArea(1, 1, 1, 15));
            ws.AddMergeArea(new MergeArea(2, 2, 1, 15));
            ws.AddMergeArea(new MergeArea(6, 7, 1, 1));
            ws.AddMergeArea(new MergeArea(6, 7, 2, 2));
            ws.AddMergeArea(new MergeArea(6, 7, 3, 3));
            ws.AddMergeArea(new MergeArea(6, 7, 4, 4));
            ws.AddMergeArea(new MergeArea(6, 7, 5, 5));
            ws.AddMergeArea(new MergeArea(6, 7, 6, 6));
            ws.AddMergeArea(new MergeArea(6, 6, 7, 9));
            ws.AddMergeArea(new MergeArea(6, 6, 10, 12));
            ws.AddMergeArea(new MergeArea(6, 7, 13, 13));
            ws.AddMergeArea(new MergeArea(6, 7, 14, 14));
            ws.AddMergeArea(new MergeArea(6, 7, 15, 15));
            #endregion

            
           
            #region 数据源 

            #region 绑定主体数据
           
                var souce = (from a in db.StorageOutDetails
                             join b in db.StorageStocks on a.MaterialID equals b.MaterialID
                             where a.StorageOutNoticeID == _noticeid
                             select new
                             {

                                 a.MaterialInfo.FinanceCode,
                                 mcode = "",
                                 a.MaterialInfo.MaterialName,
                                 a.MaterialInfo.SpecificationModel,
                                 madefr = "",
                                 b.StorageTime,
                                 a.Gentaojian,
                                 a.Metre,
                                 a.Ton,
                                 G="",
                                 M="",
                                 T="",
                                 cunit = "",
                                 uprice = "",
                                 sum="",
                                 
                             }).Distinct();


           

                dt = ReporterHelper.LinqQueryToDataTable(souce);
                if (dt.Rows.Count == 0)
                    return;


               
                ReporterHelper.ReadFromDataTable(ref dt, ref ws, 8, 1, xf2);
#endregion


                intRN = 8 + dt.Rows.Count;

            }
            #endregion
            #region 表尾
            ws.Cells.Add(intRN, 1, "备注：");
            ws.Cells.Add(intRN, 2, "");
            ws.Cells.Add(intRN, 3, "");
            ws.Cells.Add(intRN, 4, "");
            ws.Cells.Add(intRN, 5, "");
            ws.Cells.Add(intRN, 6, "");
            ws.Cells.Add(intRN, 7, "");
            ws.Cells.Add(intRN, 8, "");
            ws.Cells.Add(intRN, 9, "");
            ws.Cells.Add(intRN, 10, "");
            ws.Cells.Add(intRN, 11, "");
            ws.Cells.Add(intRN, 12, "");
            ws.Cells.Add(intRN, 13, "");
            ws.Cells.Add(intRN, 14, "");
            ws.Cells.Add(intRN, 15, "");
            ws.Cells.Add(intRN + 1, 1, "生产技术主管：");
            ws.Cells.Add(intRN + 1, 2, "");
            ws.Cells.Add(intRN + 1, 3, "");
            ws.Cells.Add(intRN + 1, 4, "调度员：");
            ws.Cells.Add(intRN + 1, 5, "物资主管：");
            ws.Cells.Add(intRN + 1, 6, "");
            ws.Cells.Add(intRN + 1, 7, "");
            ws.Cells.Add(intRN + 1, 8, "");
            ws.Cells.Add(intRN + 1, 9, "");
            ws.Cells.Add(intRN + 1, 10, "");
            ws.Cells.Add(intRN + 1, 11, "");
            ws.Cells.Add(intRN + 1, 12, "");
            ws.Cells.Add(intRN + 1, 13, "");
            ws.Cells.Add(intRN + 1, 14, "领料：");
            ws.Cells.Add(intRN + 1, 15, "");
            ws.Cells.Add(intRN + 2, 1, "");
            ws.Cells.Add(intRN + 2, 2, "");
            ws.Cells.Add(intRN + 2, 3, "");
            ws.Cells.Add(intRN + 2, 4, "");
            ws.Cells.Add(intRN + 2, 5, "");
            ws.Cells.Add(intRN + 2, 6, "");
            ws.Cells.Add(intRN + 2, 7, "");
            ws.Cells.Add(intRN + 2, 8, "");
            ws.Cells.Add(intRN + 2, 9, "");
            ws.Cells.Add(intRN + 2, 10, "");
            ws.Cells.Add(intRN + 2, 11, "");
            ws.Cells.Add(intRN + 2, 12, "");
            ws.Cells.Add(intRN + 2, 13, "");
            ws.Cells.Add(intRN + 2, 14, "");
            ws.Cells.Add(intRN + 2, 15, "");
            ws.Cells.Add(intRN + 3, 1, "");
            ws.Cells.Add(intRN + 3, 2, "");
            ws.Cells.Add(intRN + 3, 3, "");
            ws.Cells.Add(intRN + 3, 4, "");
            ws.Cells.Add(intRN + 3, 5, "");
            ws.Cells.Add(intRN + 3, 6, "");
            ws.Cells.Add(intRN + 3, 7, "");
            ws.Cells.Add(intRN + 3, 8, "");
            ws.Cells.Add(intRN + 3, 9, "");
            ws.Cells.Add(intRN + 3, 10, "");
            ws.Cells.Add(intRN + 3, 11, "");
            ws.Cells.Add(intRN + 3, 12, "");
            ws.Cells.Add(intRN + 3, 13, "");
            ws.Cells.Add(intRN + 3, 14, "");
            ws.Cells.Add(intRN + 3, 15, "");
            ws.Cells.Add(intRN + 4, 1, "第一联：生产技术组");
            ws.Cells.Add(intRN + 4, 2, "");
            ws.Cells.Add(intRN + 4, 3, "");
            ws.Cells.Add(intRN + 4, 4, "第二联：");
            ws.Cells.Add(intRN + 4, 5, "物资管理员");
            ws.Cells.Add(intRN + 4, 6, "");
            ws.Cells.Add(intRN + 4, 7, "");
            ws.Cells.Add(intRN + 4, 8, "第四联：资产组做帐");
            ws.Cells.Add(intRN + 4, 9, "");
            ws.Cells.Add(intRN + 4, 10, "");
            ws.Cells.Add(intRN + 4, 11, "第四联：资产组做帐");
            ws.Cells.Add(intRN + 4, 12, "");
            ws.Cells.Add(intRN + 4, 13, "");
            ws.Cells.Add(intRN + 4, 14, "");
            ws.Cells.Add(intRN + 4, 15, "第五联：领料凭证");

            ws.AddMergeArea(new MergeArea(intRN, intRN, 1, 15));

            #endregion
            xd.Send();
            Response.Flush();
            Response.End();
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
