using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Configuration;
using MMSPro.ADHelper.DirectoryServices;
using org.in2bits.MyXls;
using org.in2bits.MyOle2;
using System.Data;
namespace MMSPro.WebApp
{
    public class ShowReport : System.Web.UI.Page
    {
        /// <summary>
        /// 入库报表
        /// </summary>
        enum ReportTypeIn
        {
            
            正常入库物资统计报表,
            待检物资入库统计表,
            移入入库统计表,
            委外入库统计报表,
           // 回收物资入库统计表,
        }
        /// <summary>
        /// 实物库存报表
        /// </summary>
        enum ReportTypeSt
        {
          
            //实物库存汇总表,
            //实物库存报表,
            线上库存表,
            线下库存表,
            预警信息报表,
        }
        /// <summary>
        /// 出库报表
        /// </summary>
        enum ReportTypeOut
        {
            
            单井工作统计表,
           // 物资消耗总表,
            委外出库报表,
            项目发料出库报表,
            销售出库报表,
            移库出库报表,
        }
        DropDownList ddlIn;
        DropDownList ddlSt;
        DropDownList ddlOut;
        RadioButtonList rblType;
        protected void Page_Load(object sender, EventArgs e)
        {
            ddlIn = (DropDownList)this.GetControltByMaster("ddlIn");
            ddlSt = (DropDownList)this.GetControltByMaster("ddlSt");
            ddlOut = (DropDownList)this.GetControltByMaster("ddlOut");
            rblType = (RadioButtonList)this.GetControltByMaster("rblType");
            rblType.SelectedIndexChanged += new EventHandler(rblType_SelectedIndexChanged);
            Button btnReturnUser = (Button)this.GetControltByMaster("btnBuild");
            btnReturnUser.Click += new EventHandler(btnReturnUser_Click);
            if (!IsPostBack)
            {
                BindDDL();
                rblType.SelectedIndex = 0;
            }
        }

        void rblType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (rblType.Text)
            {
                case "入库报表":
                    ddlIn.Visible = true;
                    ddlSt.Visible = false;
                    ddlOut.Visible = false;
                    break;
                case "实物库存报表":
                    ddlIn.Visible = false;
                    ddlSt.Visible = true;
                    ddlOut.Visible = false;
                    break;
                case "出库报表":
                    ddlIn.Visible = false;
                    ddlSt.Visible = false;
                    ddlOut.Visible = true;
                    break;
            }
        }

        private void BindDDL()        
        {

            this.ddlIn.DataSource = Enum.GetNames(typeof(ReportTypeIn));
            this.ddlIn.DataBind();
            this.ddlSt.DataSource = Enum.GetNames(typeof(ReportTypeSt));
            this.ddlSt.DataBind();
            this.ddlOut.DataSource = Enum.GetNames(typeof(ReportTypeOut));
            this.ddlOut.DataBind();
        }

        void btnReturnUser_Click(object sender, EventArgs e)
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
           
           
            Worksheet ws;
            Cell c;
            int i = 0;
            DataTable dt = new DataTable();
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                switch (rblType.Text)
                {
                    case "入库报表":
                        #region 入库报表
                        xd.FileName = HttpUtility.UrlEncode(this.ddlIn.Text);
                        switch ((ReportTypeIn)Enum.Parse(typeof(ReportTypeIn), this.ddlIn.Text))
                        {
                            case ReportTypeIn.正常入库物资统计报表:
                                #region 正常入库物资统计报表 --完工,待测试
                                ws = xd.Workbook.Worksheets.Add("正常入库物资统计报表");
                                #region Creat by Bot
                                ws.Cells.Add(1, 1, "物资供应处川西物资配送中心正常入库物资统计表", xf3);
                                ws.Cells.Add(2, 1, "填报单位：物资供应处川西物资配送中心");
                                ws.Cells.Add(2, 10, "填报日期：");
                                ws.Cells.Add(2, 15, "截止日期：");
                                ws.Cells.Add(3, 1, "序号", xf1);
                                ws.Cells.Add(3, 2, "进库通知单号", xf1);
                                ws.Cells.Add(3, 3, "名称", xf1);
                                ws.Cells.Add(3, 4, "规格型号", xf1);
                                ws.Cells.Add(3, 5, "物料编码", xf1);
                                ws.Cells.Add(3, 6, "财务编码", xf1);
                                ws.Cells.Add(3, 7, "进库日期", xf1);
                                ws.Cells.Add(3, 8, "供应商", xf1);
                                ws.Cells.Add(3, 9, "生产厂家", xf1);
                                ws.Cells.Add(3, 10, "入库单号", xf1);
                                ws.Cells.Add(3, 11, "计量单位", xf1);
                                ws.Cells.Add(3, 12, "数量", xf1);
                                ws.Cells.Add(3, 13, null, xf1);
                                ws.Cells.Add(3, 14, null, xf1);
                                ws.Cells.Add(3, 15, "价值（元）", xf1);
                                ws.Cells.Add(3, 16, null, xf1);
                                ws.Cells.Add(3, 17, "备注", xf1);
                                ws.Cells.Add(4, 1, null, xf1);
                                ws.Cells.Add(4, 2, null, xf1);
                                ws.Cells.Add(4, 3, null, xf1);
                                ws.Cells.Add(4, 4, null, xf1);
                                ws.Cells.Add(4, 5, null, xf1);
                                ws.Cells.Add(4, 6, null, xf1);
                                ws.Cells.Add(4, 7, null, xf1);
                                ws.Cells.Add(4, 8, null, xf1);
                                ws.Cells.Add(4, 9, null, xf1);
                                ws.Cells.Add(4, 10, null, xf1);
                                ws.Cells.Add(4, 11, null, xf1);
                                ws.Cells.Add(4, 12, "根/台/套", xf1);
                                ws.Cells.Add(4, 13, "米", xf1);
                                ws.Cells.Add(4, 14, "吨", xf1);
                                ws.Cells.Add(4, 15, "单价", xf1);
                                ws.Cells.Add(4, 16, "金额", xf1);
                                ws.Cells.Add(4, 17, null, xf1);
                                ws.AddMergeArea(new MergeArea(1, 1, 1, 17));
                                ws.AddMergeArea(new MergeArea(2, 2, 10, 14));
                                ws.AddMergeArea(new MergeArea(2, 2, 15, 16));
                                ws.AddMergeArea(new MergeArea(3, 4, 1, 1));
                                ws.AddMergeArea(new MergeArea(3, 4, 2, 2));
                                ws.AddMergeArea(new MergeArea(3, 4, 3, 3));
                                ws.AddMergeArea(new MergeArea(3, 4, 4, 4));
                                ws.AddMergeArea(new MergeArea(3, 4, 5, 5));
                                ws.AddMergeArea(new MergeArea(3, 4, 6, 6));
                                ws.AddMergeArea(new MergeArea(3, 4, 7, 7));
                                ws.AddMergeArea(new MergeArea(3, 4, 8, 8));
                                ws.AddMergeArea(new MergeArea(3, 4, 9, 9));
                                ws.AddMergeArea(new MergeArea(3, 4, 10, 10));
                                ws.AddMergeArea(new MergeArea(3, 4, 11, 11));
                                ws.AddMergeArea(new MergeArea(3, 3, 12, 14));
                                ws.AddMergeArea(new MergeArea(3, 3, 15, 16));
                                ws.AddMergeArea(new MergeArea(3, 4, 17, 17));
                                #endregion

                                #region Creat Table
                                var tos = from a in db.NormalIn
                                          where a.ReceivingTypeName.Contains("正常")
                                          select new
                                          {
                                              index = "",
                                              a.StorageInCode,
                                              a.MaterialName,
                                              a.SpecificationModel,
                                              matnum = "",
                                              a.FinanceCode,
                                              a.StorageTime,
                                              a.SupplierName,
                                              a.ManufacturerName,
                                              a.BillCode,
                                              a.CurUnit,
                                              a.TestGentaojian,
                                              a.TestMetre,
                                              a.TestTon,
                                              a.UnitPrice,
                                              a.Amount,
                                              remark = "",
                                          };

                                dt = ReporterHelper.LinqQueryToDataTable(tos);
                                if (dt.Rows.Count == 0)
                                    break;


                                if (dt.Rows.Count > 0)
                                {
                                    i = 0;
                                    foreach (DataRow drtos in dt.Rows)
                                    {
                                        i++;
                                        drtos["index"] = i.ToString();


                                    }
                                }
                                ReporterHelper.ReadFromDataTable(ref dt, ref ws, 5, 1, xf2);
                                #endregion



                                // ReporterHelper.SetXF(ref ws, 3, 1, (ushort)(4 + dt.Rows.Count), 11, xf1);
                                #endregion
                                break;


                            case ReportTypeIn.待检物资入库统计表:
                                #region 待检物资入库统计表 --完工,4个列空数据,客户做保留字段,无需生成数据, 需要测试数据测试
                                ws = xd.Workbook.Worksheets.Add("待检物资入库统计表");
                                #region Creat by Bot
                                ws.Cells.Add(1, 1, "物资供应处川西物资配送中心待检物资入库统计表", xf3);
                                ws.Cells.Add(2, 1, "填报单位:物资供应处川西物资配送中心");
                                ws.Cells.Add(2, 9, "报出日期：");
                                ws.Cells.Add(2, 13, "截止日期：");
                                ws.Cells.Add(3, 1, "序号", xf1);
                                ws.Cells.Add(3, 2, "进库通知单号", xf1);
                                ws.Cells.Add(3, 3, "名称", xf1);
                                ws.Cells.Add(3, 4, "规格型号", xf1);
                                ws.Cells.Add(3, 5, "物料编码", xf1);
                                ws.Cells.Add(3, 6, "财务编码", xf1);
                                ws.Cells.Add(3, 7, "进库日期", xf1);
                                ws.Cells.Add(3, 8, "供应商", xf1);
                                ws.Cells.Add(3, 9, "生产厂家", xf1);
                                ws.Cells.Add(3, 10, "计量单位", xf1);
                                ws.Cells.Add(3, 11, "数量", xf1);
                                ws.Cells.Add(3, 12, null, xf1);
                                ws.Cells.Add(3, 13, null, xf1);
                                ws.Cells.Add(3, 14, "价值（元）", xf1);
                                ws.Cells.Add(3, 15, null, xf1);
                                ws.Cells.Add(3, 16, "备注", xf1);
                                ws.Cells.Add(4, 1, null, xf1);
                                ws.Cells.Add(4, 2, null, xf1);
                                ws.Cells.Add(4, 3, null, xf1);
                                ws.Cells.Add(4, 4, null, xf1);
                                ws.Cells.Add(4, 5, null, xf1);
                                ws.Cells.Add(4, 6, null, xf1);
                                ws.Cells.Add(4, 7, null, xf1);
                                ws.Cells.Add(4, 8, null, xf1);
                                ws.Cells.Add(4, 9, null, xf1);
                                ws.Cells.Add(4, 10, null, xf1);
                                ws.Cells.Add(4, 11, "根/台/套", xf1);
                                ws.Cells.Add(4, 12, "米", xf1);
                                ws.Cells.Add(4, 13, "吨", xf1);
                                ws.Cells.Add(4, 14, "单价", xf1);
                                ws.Cells.Add(4, 15, "金额", xf1);
                                ws.Cells.Add(4, 16, null, xf1);
                                ws.AddMergeArea(new MergeArea(1, 1, 1, 16));
                                ws.AddMergeArea(new MergeArea(2, 2, 1, 4));
                                ws.AddMergeArea(new MergeArea(3, 4, 1, 1));
                                ws.AddMergeArea(new MergeArea(3, 4, 2, 2));
                                ws.AddMergeArea(new MergeArea(3, 4, 3, 3));
                                ws.AddMergeArea(new MergeArea(3, 4, 4, 4));
                                ws.AddMergeArea(new MergeArea(3, 4, 5, 5));
                                ws.AddMergeArea(new MergeArea(3, 4, 6, 6));
                                ws.AddMergeArea(new MergeArea(3, 4, 7, 7));
                                ws.AddMergeArea(new MergeArea(3, 4, 8, 8));
                                ws.AddMergeArea(new MergeArea(3, 4, 9, 9));
                                ws.AddMergeArea(new MergeArea(3, 4, 10, 10));
                                ws.AddMergeArea(new MergeArea(3, 3, 11, 13));
                                ws.AddMergeArea(new MergeArea(3, 3, 14, 15));
                                ws.AddMergeArea(new MergeArea(3, 4, 16, 16));
                                #endregion

                                #region Creat Table

                                var wft = from a in db.WaitForTest
                                          select new
                                          {
                                              a.StorageInCode,
                                              a.MaterialName,
                                              a.SpecificationModel,
                                              a.FinanceCode,
                                              a.StorageTime,
                                              a.SupplierName,
                                              a.ManufacturerName,
                                              a.QuantityGentaojian,
                                              a.QuantityMetre,
                                              a.QuantityTon,
                                              a.Remark,
                                          };
                                dt = ReporterHelper.LinqQueryToDataTable(wft);
                                if (dt.Rows.Count == 0)
                                    break;
                                dt.Columns.Add("index").SetOrdinal(0);
                                dt.Columns.Add("mcode").SetOrdinal(4);
                                dt.Columns.Add("cur").SetOrdinal(9);
                                dt.Columns.Add("price").SetOrdinal(13);
                                dt.Columns.Add("sum").SetOrdinal(14);
                                if (dt.Rows.Count > 0)
                                {
                                    i = 0;
                                    foreach (DataRow drwft in dt.Rows)
                                    {
                                        i++;
                                        drwft["index"] = i.ToString();
                                    }
                                }
                                ReporterHelper.ReadFromDataTable(ref dt, ref ws, 5, 1, xf2);
                                #endregion

                                #endregion
                                break;

                                #region 回收物资入库统计表 后期扩展
                                /*
                            case ReportTypeIn.回收物资入库统计表:
                                #region 回收物资入库统计表
                                ws = xd.Workbook.Worksheets.Add("回收物资入库统计表");
                                #region Creat by Bot
                                ws.Cells.Add(1, 1, "物资供应处川西物资配送中心回收物资统计表", xf3);
                                ws.Cells.Add(2, 1, "填报单位：物资供应处川西物资配送中心");
                                ws.Cells.Add(2, 16, "报出日期：");
                                ws.Cells.Add(2, 32, "截止日期：");
                                ws.Cells.Add(3, 1, "序号", xf1);
                                ws.Cells.Add(3, 2, "回收单号", xf1);
                                ws.Cells.Add(3, 3, "名称", xf1);
                                ws.Cells.Add(3, 4, "规格型号", xf1);
                                ws.Cells.Add(3, 5, "物料编码", xf1);
                                ws.Cells.Add(3, 6, "财务编码", xf1);
                                ws.Cells.Add(3, 7, "回收日期", xf1);
                                ws.Cells.Add(3, 8, "回收项目", xf1);
                                ws.Cells.Add(3, 9, "供货商", xf1);
                                ws.Cells.Add(3, 10, "计量单位", xf1);
                                ws.Cells.Add(3, 11, "单价（元）", xf1);
                                ws.Cells.Add(3, 12, "回收总数", xf1);
                                ws.Cells.Add(3, 13, null, xf1);
                                ws.Cells.Add(3, 14, null, xf1);
                                ws.Cells.Add(3, 15, null, xf1);
                                ws.Cells.Add(3, 16, "回收合格数", xf1);
                                ws.Cells.Add(3, 17, null, xf1);
                                ws.Cells.Add(3, 18, null, xf1);
                                ws.Cells.Add(3, 19, null, xf1);
                                ws.Cells.Add(3, 20, null, xf1);
                                ws.Cells.Add(3, 21, "回收修复合格数", xf1);
                                ws.Cells.Add(3, 22, null, xf1);
                                ws.Cells.Add(3, 23, null, xf1);
                                ws.Cells.Add(3, 24, null, xf1);
                                ws.Cells.Add(3, 25, null, xf1);
                                ws.Cells.Add(3, 26, "回收待报废/报废数", xf1);
                                ws.Cells.Add(3, 27, null, xf1);
                                ws.Cells.Add(3, 28, null, xf1);
                                ws.Cells.Add(3, 29, null, xf1);
                                ws.Cells.Add(3, 30, null, xf1);
                                ws.Cells.Add(3, 31, "回收待修复/成套数", xf1);
                                ws.Cells.Add(3, 32, null, xf1);
                                ws.Cells.Add(3, 33, null, xf1);
                                ws.Cells.Add(3, 34, null, xf1);
                                ws.Cells.Add(3, 35, null, xf1);
                                ws.Cells.Add(3, 36, "备注", xf1);
                                ws.Cells.Add(4, 1, null, xf1);
                                ws.Cells.Add(4, 2, null, xf1);
                                ws.Cells.Add(4, 3, null, xf1);
                                ws.Cells.Add(4, 4, null, xf1);
                                ws.Cells.Add(4, 5, null, xf1);
                                ws.Cells.Add(4, 6, null, xf1);
                                ws.Cells.Add(4, 7, null, xf1);
                                ws.Cells.Add(4, 8, null, xf1);
                                ws.Cells.Add(4, 9, null, xf1);
                                ws.Cells.Add(4, 10, null, xf1);
                                ws.Cells.Add(4, 11, null, xf1);
                                ws.Cells.Add(4, 12, "根/台/套", xf1);
                                ws.Cells.Add(4, 13, "米", xf1);
                                ws.Cells.Add(4, 14, "吨", xf1);
                                ws.Cells.Add(4, 15, "金额（元）", xf1);
                                ws.Cells.Add(4, 16, "入库单号", xf1);
                                ws.Cells.Add(4, 17, "根/台/套", xf1);
                                ws.Cells.Add(4, 18, "米", xf1);
                                ws.Cells.Add(4, 19, "吨", xf1);
                                ws.Cells.Add(4, 20, "金额（元）", xf1);
                                ws.Cells.Add(4, 21, "入库单号", xf1);
                                ws.Cells.Add(4, 22, "根/台/套", xf1);
                                ws.Cells.Add(4, 23, "米", xf1);
                                ws.Cells.Add(4, 24, "吨", xf1);
                                ws.Cells.Add(4, 25, "金额（元）", xf1);
                                ws.Cells.Add(4, 26, "入库单号", xf1);
                                ws.Cells.Add(4, 27, "根/台/套", xf1);
                                ws.Cells.Add(4, 28, "米", xf1);
                                ws.Cells.Add(4, 29, "吨", xf1);
                                ws.Cells.Add(4, 30, "金额（元）", xf1);
                                ws.Cells.Add(4, 31, "入库单号", xf1);
                                ws.Cells.Add(4, 32, "根/台/套", xf1);
                                ws.Cells.Add(4, 33, "米", xf1);
                                ws.Cells.Add(4, 34, "吨", xf1);
                                ws.Cells.Add(4, 35, "金额（元）", xf1);
                                ws.Cells.Add(4, 36, null, xf1);
                                ws.AddMergeArea(new MergeArea(1, 1, 1, 36));
                                ws.AddMergeArea(new MergeArea(2, 2, 1, 7));
                                ws.AddMergeArea(new MergeArea(2, 2, 16, 18));
                                ws.AddMergeArea(new MergeArea(3, 4, 1, 1));
                                ws.AddMergeArea(new MergeArea(3, 4, 2, 2));
                                ws.AddMergeArea(new MergeArea(3, 4, 3, 3));
                                ws.AddMergeArea(new MergeArea(3, 4, 4, 4));
                                ws.AddMergeArea(new MergeArea(3, 4, 5, 5));
                                ws.AddMergeArea(new MergeArea(3, 4, 6, 6));
                                ws.AddMergeArea(new MergeArea(3, 4, 7, 7));
                                ws.AddMergeArea(new MergeArea(3, 4, 8, 8));
                                ws.AddMergeArea(new MergeArea(3, 4, 9, 9));
                                ws.AddMergeArea(new MergeArea(3, 4, 10, 10));
                                ws.AddMergeArea(new MergeArea(3, 4, 11, 11));
                                ws.AddMergeArea(new MergeArea(3, 3, 12, 15));
                                ws.AddMergeArea(new MergeArea(3, 3, 16, 20));
                                ws.AddMergeArea(new MergeArea(3, 3, 21, 25));
                                ws.AddMergeArea(new MergeArea(3, 3, 26, 30));
                                ws.AddMergeArea(new MergeArea(3, 3, 31, 35));
                                ws.AddMergeArea(new MergeArea(3, 4, 36, 36));
                                #endregion
                                #endregion
                                break;
                            #endregion
                            case ReportTypeIn.移入入库统计表:
                                #region 移入入库统计表  --完工,待测试
                                ws = xd.Workbook.Worksheets.Add("移入入库统计表");
                                #region Creat by Bot
                                ws.Cells.Add(1, 1, "物资供应处川西物资配送中心移库入库物资统计表", xf3);
                                ws.Cells.Add(2, 1, "填报单位：物资供应处川西物资配送中心");
                                ws.Cells.Add(2, 8, "报出日期：");
                                ws.Cells.Add(2, 14, "截止日期：");
                                ws.Cells.Add(3, 1, "序号", xf1);
                                ws.Cells.Add(3, 2, "进库通知单号", xf1);
                                ws.Cells.Add(3, 3, "名称", xf1);
                                ws.Cells.Add(3, 4, "规格型号", xf1);
                                ws.Cells.Add(3, 5, "物料编码", xf1);
                                ws.Cells.Add(3, 6, "财务编码", xf1);
                                ws.Cells.Add(3, 7, "进库日期", xf1);
                                ws.Cells.Add(3, 8, "供应商", xf1);
                                ws.Cells.Add(3, 9, "生产厂家", xf1);
                                ws.Cells.Add(3, 10, "入库单号", xf1);
                                ws.Cells.Add(3, 11, "计量单位", xf1);
                                ws.Cells.Add(3, 12, "数量", xf1);
                                ws.Cells.Add(3, 13, "", xf1);
                                ws.Cells.Add(3, 14, "", xf1);
                                ws.Cells.Add(3, 15, "价值（元）", xf1);
                                ws.Cells.Add(3, 16, "", xf1);
                                ws.Cells.Add(3, 17, "备注", xf1);
                                ws.Cells.Add(4, 1, "", xf1);
                                ws.Cells.Add(4, 2, "", xf1);
                                ws.Cells.Add(4, 3, "", xf1);
                                ws.Cells.Add(4, 4, "", xf1);
                                ws.Cells.Add(4, 5, "", xf1);
                                ws.Cells.Add(4, 6, "", xf1);
                                ws.Cells.Add(4, 7, "", xf1);
                                ws.Cells.Add(4, 8, "", xf1);
                                ws.Cells.Add(4, 9, "", xf1);
                                ws.Cells.Add(4, 10, "", xf1);
                                ws.Cells.Add(4, 11, "", xf1);
                                ws.Cells.Add(4, 12, "根/台/套", xf1);
                                ws.Cells.Add(4, 13, "米", xf1);
                                ws.Cells.Add(4, 14, "吨", xf1);
                                ws.Cells.Add(4, 15, "单价", xf1);
                                ws.Cells.Add(4, 16, "金额", xf1);
                                ws.Cells.Add(4, 17, "", xf1);
                                ws.AddMergeArea(new MergeArea(1, 1, 1, 17));
                                ws.AddMergeArea(new MergeArea(2, 2, 1, 3));
                                ws.AddMergeArea(new MergeArea(3, 4, 1, 1));
                                ws.AddMergeArea(new MergeArea(3, 4, 2, 2));
                                ws.AddMergeArea(new MergeArea(3, 4, 3, 3));
                                ws.AddMergeArea(new MergeArea(3, 4, 4, 4));
                                ws.AddMergeArea(new MergeArea(3, 4, 5, 5));
                                ws.AddMergeArea(new MergeArea(3, 4, 6, 6));
                                ws.AddMergeArea(new MergeArea(3, 4, 7, 7));
                                ws.AddMergeArea(new MergeArea(3, 4, 8, 8));
                                ws.AddMergeArea(new MergeArea(3, 4, 9, 9));
                                ws.AddMergeArea(new MergeArea(3, 4, 10, 10));
                                ws.AddMergeArea(new MergeArea(3, 4, 11, 11));
                                ws.AddMergeArea(new MergeArea(3, 3, 12, 14));
                                ws.AddMergeArea(new MergeArea(3, 3, 15, 16));
                                ws.AddMergeArea(new MergeArea(3, 4, 17, 17));
                                #endregion

                                #region Creat Table
                                tos = from a in db.NormalIn
                                      where a.ReceivingTypeName.Contains("移入")
                                      select new
                                      {
                                          index = "",
                                          a.StorageInCode,
                                          a.MaterialName,
                                          a.SpecificationModel,
                                          matnum = "",
                                          a.FinanceCode,
                                          a.StorageTime,
                                          a.SupplierName,
                                          a.ManufacturerName,
                                          a.BillCode,
                                          a.CurUnit,
                                          a.TestGentaojian,
                                          a.TestMetre,
                                          a.TestTon,
                                          a.UnitPrice,
                                          a.Amount,
                                          remark = "",
                                      };

                                dt = ReporterHelper.LinqQueryToDataTable(tos);
                                if (dt.Rows.Count == 0)
                                    break;


                                if (dt.Rows.Count > 0)
                                {
                                    i = 0;
                                    foreach (DataRow drtos in dt.Rows)
                                    {
                                        i++;
                                        drtos["index"] = i.ToString();


                                    }
                                }
                                ReporterHelper.ReadFromDataTable(ref dt, ref ws, 5, 1, xf2);
                                #endregion

                                 break;
                                */
                                #endregion


                            case ReportTypeIn.委外入库统计报表:
                                #region 委外入库统计报表  --完工 待测试
                                ws = xd.Workbook.Worksheets.Add("委外入库统计报表");
                                #region Creat by Bot
                                ws.Cells.Add(1, 1, "物资供应处川西物资配送中心委外入库统计表", xf3);
                                ws.Cells.Add(2, 1, "填报单位：物资供应处川西物资配送中心");
                                ws.Cells.Add(2, 8, "报出日期：");
                                ws.Cells.Add(2, 14, "截止日期:");
                                ws.Cells.Add(3, 1, "序号", xf1);
                                ws.Cells.Add(3, 2, "进库通知单号", xf1);
                                ws.Cells.Add(3, 3, "名称", xf1);
                                ws.Cells.Add(3, 4, "规格型号", xf1);
                                ws.Cells.Add(3, 5, "物料编码", xf1);
                                ws.Cells.Add(3, 6, "财务编码", xf1);
                                ws.Cells.Add(3, 7, "进库日期", xf1);
                                ws.Cells.Add(3, 8, "供应商", xf1);
                                ws.Cells.Add(3, 9, "生产厂家", xf1);
                                ws.Cells.Add(3, 10, "入库单号", xf1);
                                ws.Cells.Add(3, 11, "计量单位", xf1);
                                ws.Cells.Add(3, 12, "数量", xf1);
                                ws.Cells.Add(3, 13, null, xf1);
                                ws.Cells.Add(3, 14, null, xf1);
                                ws.Cells.Add(3, 15, "价值（元）", xf1);
                                ws.Cells.Add(3, 16, null, xf1);
                                ws.Cells.Add(3, 17, "备注", xf1);
                                ws.Cells.Add(4, 1, null, xf1);
                                ws.Cells.Add(4, 2, null, xf1);
                                ws.Cells.Add(4, 3, null, xf1);
                                ws.Cells.Add(4, 4, null, xf1);
                                ws.Cells.Add(4, 5, null, xf1);
                                ws.Cells.Add(4, 6, null, xf1);
                                ws.Cells.Add(4, 7, null, xf1);
                                ws.Cells.Add(4, 8, null, xf1);
                                ws.Cells.Add(4, 9, null, xf1);
                                ws.Cells.Add(4, 10, null, xf1);
                                ws.Cells.Add(4, 11, null, xf1);
                                ws.Cells.Add(4, 12, "根/台/套", xf1);
                                ws.Cells.Add(4, 13, "米", xf1);
                                ws.Cells.Add(4, 14, "吨", xf1);
                                ws.Cells.Add(4, 15, "单价", xf1);
                                ws.Cells.Add(4, 16, "金额", xf1);
                                ws.Cells.Add(4, 17, null, xf1);
                                ws.AddMergeArea(new MergeArea(1, 1, 1, 17));
                                ws.AddMergeArea(new MergeArea(2, 2, 1, 3));
                                ws.AddMergeArea(new MergeArea(3, 4, 1, 1));
                                ws.AddMergeArea(new MergeArea(3, 4, 2, 2));
                                ws.AddMergeArea(new MergeArea(3, 4, 3, 3));
                                ws.AddMergeArea(new MergeArea(3, 4, 4, 4));
                                ws.AddMergeArea(new MergeArea(3, 4, 5, 5));
                                ws.AddMergeArea(new MergeArea(3, 4, 6, 6));
                                ws.AddMergeArea(new MergeArea(3, 4, 7, 7));
                                ws.AddMergeArea(new MergeArea(3, 4, 8, 8));
                                ws.AddMergeArea(new MergeArea(3, 4, 9, 9));
                                ws.AddMergeArea(new MergeArea(3, 4, 10, 10));
                                ws.AddMergeArea(new MergeArea(3, 4, 11, 11));
                                ws.AddMergeArea(new MergeArea(3, 3, 12, 14));
                                ws.AddMergeArea(new MergeArea(3, 3, 15, 16));
                                ws.AddMergeArea(new MergeArea(3, 4, 17, 17));
                                #endregion

                                #region Creat Table
                                tos = from a in db.ViewCommitIn
                                      select new
                                      {
                                          index = "",
                                          a.StorageInCode,
                                          a.MaterialName,
                                          a.SpecificationModel,
                                          matnum = "",
                                          a.FinanceCode,
                                          a.StorageTime,
                                          a.SupplierName,
                                          a.ManufacturerName,
                                          a.BillCode,
                                          a.CurUnit,
                                          a.TestGentaojian,
                                          a.TestMetre,
                                          a.TestTon,
                                          a.UnitPrice,
                                          a.Amount,
                                          remark = "",
                                      };

                                dt = ReporterHelper.LinqQueryToDataTable(tos);
                                if (dt.Rows.Count == 0)
                                    break;


                                if (dt.Rows.Count > 0)
                                {
                                    i = 0;
                                    foreach (DataRow drtos in dt.Rows)
                                    {
                                        i++;
                                        drtos["index"] = i.ToString();


                                    }
                                }
                                ReporterHelper.ReadFromDataTable(ref dt, ref ws, 5, 1, xf2);
                                #endregion




                                #endregion
                                break;
                        }
                        #endregion
                        break;
                    case "实物库存报表":
                        #region 实物库存报表
                        xd.FileName = HttpUtility.UrlEncode(this.ddlSt.Text);
                        switch ((ReportTypeSt)Enum.Parse(typeof(ReportTypeSt), this.ddlSt.Text))
                        {
                            case ReportTypeSt.预警信息报表:
                                #region 预警信息报表 --完工
                                ws = xd.Workbook.Worksheets.Add("物资预警报告明细");//sheet 名
                                #region Creat by Bot
                                ws.Cells.Add(1, 1, "物资供应处川西物资配送中心常用物资预警报告明细", xf3);
                                ws.Cells.Add(2, 1, "填报单位：物资供应处川西物资配送中心");
                                ws.Cells.Add(2, 10, "填报日期：");
                                ws.Cells.Add(3, 1, "序号", xf1);
                                ws.Cells.Add(3, 2, "物质名称", xf1);
                                ws.Cells.Add(3, 3, "规格型号", xf1);
                                ws.Cells.Add(3, 4, "预警数量", xf1);
                                ws.Cells.Add(3, 5, null, xf1);
                                ws.Cells.Add(3, 6, null, xf1);
                                ws.Cells.Add(3, 7, "实际库存", xf1);
                                ws.Cells.Add(3, 8, null, xf1);
                                ws.Cells.Add(3, 9, null, xf1);
                                ws.Cells.Add(3, 10, "技术状况", xf1);
                                ws.Cells.Add(3, 11, "备注", xf1);
                                ws.Cells.Add(4, 1, null, xf1);
                                ws.Cells.Add(4, 2, null, xf1);
                                ws.Cells.Add(4, 3, null, xf1);
                                ws.Cells.Add(4, 4, "根/套/件", xf1);
                                ws.Cells.Add(4, 5, "米", xf1);
                                ws.Cells.Add(4, 6, "吨", xf1);
                                ws.Cells.Add(4, 7, "根/套/件", xf1);
                                ws.Cells.Add(4, 8, "米", xf1);
                                ws.Cells.Add(4, 9, "吨", xf1);
                                ws.Cells.Add(4, 10, null, xf1);
                                ws.Cells.Add(4, 11, null, xf1);
                                ws.AddMergeArea(new MergeArea(1, 1, 1, 11));
                                ws.AddMergeArea(new MergeArea(3, 4, 1, 1));
                                ws.AddMergeArea(new MergeArea(3, 4, 2, 2));
                                ws.AddMergeArea(new MergeArea(3, 4, 3, 3));
                                ws.AddMergeArea(new MergeArea(3, 3, 4, 6));
                                ws.AddMergeArea(new MergeArea(3, 3, 7, 9));
                                ws.AddMergeArea(new MergeArea(3, 4, 10, 10));
                                ws.AddMergeArea(new MergeArea(3, 4, 11, 11));
                                #endregion

                                #region 构建datatable
                                dt.Columns.Add("序号", typeof(int));
                                dt.Columns.Add("类型名");
                                dt.Columns.Add("型号名称");
                                dt.Columns.Add("预警根套件", typeof(decimal));
                                dt.Columns.Add("预警米", typeof(decimal));
                                dt.Columns.Add("预警吨", typeof(decimal));
                                dt.Columns.Add("实际根套件", typeof(decimal));
                                dt.Columns.Add("实际米", typeof(decimal));
                                dt.Columns.Add("实际吨", typeof(decimal));
                                dt.Columns.Add("状态");
                                dt.Columns.Add("备注");

                                DataRow dr;

                                var all = db.GetTable<WarningList>();
                                var n = db.ExecuteQuery<StorageStocks>(
                                    @"select sum(StocksGenTaojian) as StocksGenTaojian 
                            ,sum(StocksMetre) as StocksMetre 
                            ,sum(StocksTon) as StocksTon,
                            Materialname                    
                            from storagestocks group by Materialname").ToList();


                                foreach (var t in all)
                                {

                                    i++;
                                    dr = dt.NewRow();
                                    dr["序号"] = i.ToString();
                                    var t1 = db.MaterialInfo.SingleOrDefault(a => a.MaterialID == t.MaterialID);
                                    dr["类型名"] = t1.MaterialChildType.MaterialMainType.MaterialMainTypeName + "-" + t1.MaterialChildType.MaterialChildTypeName;
                                    dr["型号名称"] = t1.MaterialName;
                                    dr["预警根套件"] = t.QuantityGentaojian;
                                    dr["预警米"] = t.QuantityMetre;
                                    dr["预警吨"] = t.QuantityTon;
                                    if (n.Where(a => a.MaterialName == t.MaterialInfo.MaterialName).ToList().Count == 1)
                                    {
                                        dr["实际根套件"] = n.SingleOrDefault(a => a.MaterialName == t.MaterialInfo.MaterialName).StocksGenTaojian;
                                        dr["实际米"] = n.SingleOrDefault(a => a.MaterialName == t.MaterialInfo.MaterialName).StocksMetre;
                                        dr["实际吨"] = n.SingleOrDefault(a => a.MaterialName == t.MaterialInfo.MaterialName).StocksTon;
                                    }
                                    else
                                    {
                                        dr["实际根套件"] = 0;
                                        dr["实际米"] = 0;
                                        dr["实际吨"] = 0;
                                    }
                                    //数据库中需要添加状态列   
                                    //dr["状态"] = 
                                    //此处的备注留空给客户自己填写
                                    //dr["备注"] = 
                                    dt.Rows.Add(dr);
                                    dt.AcceptChanges();
                                }


                                #endregion
                                //                    ws.Write(dt, 5, 1);
                                ReporterHelper.ReadFromDataTable(ref dt, ref ws, 5, 1, xf2);
                                ReporterHelper.SetXF(ref ws, 3, 1, (ushort)(4 + dt.Rows.Count), 11, xf1);
                                #endregion
                                break;
                            case ReportTypeSt.线上库存表:
                                #region 线上库存表 --完工,待测试
                                ws = xd.Workbook.Worksheets.Add("线上库存表");
                                #region Creat by Bot
                                ws.Cells.Add(1, 1, "物资供应处川西物资配送中心线上物资统计表", xf3);
                                ws.Cells.Add(2, 1, "填报单位：物资供应处川西物资配送中心");
                                ws.Cells.Add(2, 10, "填报日期：");
                                ws.Cells.Add(2, 15, "截止日期：");
                                ws.Cells.Add(3, 1, "序号", xf1);
                                ws.Cells.Add(3, 2, "进库通知单号", xf1);
                                ws.Cells.Add(3, 3, "名称", xf1);
                                ws.Cells.Add(3, 4, "规格型号", xf1);
                                ws.Cells.Add(3, 5, "物料编码", xf1);
                                ws.Cells.Add(3, 6, "财务编码", xf1);
                                ws.Cells.Add(3, 7, "进库日期", xf1);
                                ws.Cells.Add(3, 8, "供应商", xf1);
                                ws.Cells.Add(3, 9, "生产厂家", xf1);
                                ws.Cells.Add(3, 10, "入库单号", xf1);
                                ws.Cells.Add(3, 11, "计量单位", xf1);
                                ws.Cells.Add(3, 12, "数量", xf1);
                                ws.Cells.Add(3, 13, null, xf1);
                                ws.Cells.Add(3, 14, null, xf1);
                                ws.Cells.Add(3, 15, "价值（元）", xf1);
                                ws.Cells.Add(3, 16, null, xf1);
                                ws.Cells.Add(3, 17, "备注", xf1);
                                ws.Cells.Add(4, 1, null, xf1);
                                ws.Cells.Add(4, 2, null, xf1);
                                ws.Cells.Add(4, 3, null, xf1);
                                ws.Cells.Add(4, 4, null, xf1);
                                ws.Cells.Add(4, 5, null, xf1);
                                ws.Cells.Add(4, 6, null, xf1);
                                ws.Cells.Add(4, 7, null, xf1);
                                ws.Cells.Add(4, 8, null, xf1);
                                ws.Cells.Add(4, 9, null, xf1);
                                ws.Cells.Add(4, 10, null, xf1);
                                ws.Cells.Add(4, 11, null, xf1);
                                ws.Cells.Add(4, 12, "根/台/套", xf1);
                                ws.Cells.Add(4, 13, "米", xf1);
                                ws.Cells.Add(4, 14, "吨", xf1);
                                ws.Cells.Add(4, 15, "单价", xf1);
                                ws.Cells.Add(4, 16, "金额", xf1);
                                ws.Cells.Add(4, 17, null, xf1);
                                ws.AddMergeArea(new MergeArea(1, 1, 1, 17));
                                ws.AddMergeArea(new MergeArea(2, 2, 10, 14));
                                ws.AddMergeArea(new MergeArea(2, 2, 15, 16));
                                ws.AddMergeArea(new MergeArea(3, 4, 1, 1));
                                ws.AddMergeArea(new MergeArea(3, 4, 2, 2));
                                ws.AddMergeArea(new MergeArea(3, 4, 3, 3));
                                ws.AddMergeArea(new MergeArea(3, 4, 4, 4));
                                ws.AddMergeArea(new MergeArea(3, 4, 5, 5));
                                ws.AddMergeArea(new MergeArea(3, 4, 6, 6));
                                ws.AddMergeArea(new MergeArea(3, 4, 7, 7));
                                ws.AddMergeArea(new MergeArea(3, 4, 8, 8));
                                ws.AddMergeArea(new MergeArea(3, 4, 9, 9));
                                ws.AddMergeArea(new MergeArea(3, 4, 10, 10));
                                ws.AddMergeArea(new MergeArea(3, 4, 11, 11));
                                ws.AddMergeArea(new MergeArea(3, 3, 12, 14));
                                ws.AddMergeArea(new MergeArea(3, 3, 15, 16));
                                ws.AddMergeArea(new MergeArea(3, 4, 17, 17));
                                #endregion

                                #region Creat Table
                                var ss = from a in db.ReportStocks
                                         where a.Status == "线上"

                                         select new
                                         {
                                             index = "",
                                             a.StorageInCode,
                                             a.MaterialName,
                                             a.SpecificationModel,
                                             a.MaterialCode,
                                             a.FinanceCode,
                                             a.StorageTime,
                                             a.SupplierName,
                                             a.ManufacturerName,
                                             a.BillCode,
                                             a.CurUnit,
                                             a.StocksGenTaojian,
                                             a.StocksMetre,
                                             a.StocksTon,
                                             a.UnitPrice,
                                             sum = "",
                                             remark = "",
                                         };

                                dt = ReporterHelper.LinqQueryToDataTable(ss);
                                if (dt.Rows.Count == 0)
                                    break;


                                if (dt.Rows.Count > 0)
                                {
                                    i = 0;
                                    foreach (DataRow drtos in dt.Rows)
                                    {
                                        i++;
                                        drtos["index"] = i.ToString();
                                        switch (drtos["CurUnit"].ToString())
                                        {
                                            case "根/台/套/件":
                                                drtos["sum"] = decimal.Parse(drtos["UnitPrice"].ToString()) * decimal.Parse(drtos["StocksGenTaojian"].ToString());
                                                break;
                                            case "米":
                                                drtos["sum"] = decimal.Parse(drtos["UnitPrice"].ToString()) * decimal.Parse(drtos["StocksMetre"].ToString());
                                                break;
                                            case "吨":
                                                drtos["sum"] = decimal.Parse(drtos["UnitPrice"].ToString()) * decimal.Parse(drtos["StocksTon"].ToString());
                                                break;
                                        }
                                        //drtos["sum"] = 

                                    }
                                }
                                ReporterHelper.ReadFromDataTable(ref dt, ref ws, 5, 1, xf2);
                                #endregion




                                // ReporterHelper.SetXF(ref ws, 3, 1, (ushort)(4 + dt.Rows.Count), 11, xf1);
                                #endregion
                                break;
                            case ReportTypeSt.线下库存表:
                                #region 线下库存表 --完工,待测试
                                ws = xd.Workbook.Worksheets.Add("线下库存表");
                                #region Creat by Bot
                                ws.Cells.Add(1, 1, "物资供应处川西物资配送中心线下物资统计表", xf3);
                                ws.Cells.Add(2, 1, "填报单位：物资供应处川西物资配送中心");
                                ws.Cells.Add(2, 10, "填报日期：");
                                ws.Cells.Add(2, 15, "截止日期：");
                                ws.Cells.Add(3, 1, "序号", xf1);
                                ws.Cells.Add(3, 2, "进库通知单号", xf1);
                                ws.Cells.Add(3, 3, "名称", xf1);
                                ws.Cells.Add(3, 4, "规格型号", xf1);
                                ws.Cells.Add(3, 5, "物料编码", xf1);
                                ws.Cells.Add(3, 6, "财务编码", xf1);
                                ws.Cells.Add(3, 7, "进库日期", xf1);
                                ws.Cells.Add(3, 8, "供应商", xf1);
                                ws.Cells.Add(3, 9, "生产厂家", xf1);
                                ws.Cells.Add(3, 10, "入库单号", xf1);
                                ws.Cells.Add(3, 11, "计量单位", xf1);
                                ws.Cells.Add(3, 12, "数量", xf1);
                                ws.Cells.Add(3, 13, null, xf1);
                                ws.Cells.Add(3, 14, null, xf1);
                                ws.Cells.Add(3, 15, "价值（元）", xf1);
                                ws.Cells.Add(3, 16, null, xf1);
                                ws.Cells.Add(3, 17, "备注", xf1);
                                ws.Cells.Add(4, 1, null, xf1);
                                ws.Cells.Add(4, 2, null, xf1);
                                ws.Cells.Add(4, 3, null, xf1);
                                ws.Cells.Add(4, 4, null, xf1);
                                ws.Cells.Add(4, 5, null, xf1);
                                ws.Cells.Add(4, 6, null, xf1);
                                ws.Cells.Add(4, 7, null, xf1);
                                ws.Cells.Add(4, 8, null, xf1);
                                ws.Cells.Add(4, 9, null, xf1);
                                ws.Cells.Add(4, 10, null, xf1);
                                ws.Cells.Add(4, 11, null, xf1);
                                ws.Cells.Add(4, 12, "根/台/套", xf1);
                                ws.Cells.Add(4, 13, "米", xf1);
                                ws.Cells.Add(4, 14, "吨", xf1);
                                ws.Cells.Add(4, 15, "单价", xf1);
                                ws.Cells.Add(4, 16, "金额", xf1);
                                ws.Cells.Add(4, 17, null, xf1);
                                ws.AddMergeArea(new MergeArea(1, 1, 1, 17));
                                ws.AddMergeArea(new MergeArea(2, 2, 10, 14));
                                ws.AddMergeArea(new MergeArea(2, 2, 15, 16));
                                ws.AddMergeArea(new MergeArea(3, 4, 1, 1));
                                ws.AddMergeArea(new MergeArea(3, 4, 2, 2));
                                ws.AddMergeArea(new MergeArea(3, 4, 3, 3));
                                ws.AddMergeArea(new MergeArea(3, 4, 4, 4));
                                ws.AddMergeArea(new MergeArea(3, 4, 5, 5));
                                ws.AddMergeArea(new MergeArea(3, 4, 6, 6));
                                ws.AddMergeArea(new MergeArea(3, 4, 7, 7));
                                ws.AddMergeArea(new MergeArea(3, 4, 8, 8));
                                ws.AddMergeArea(new MergeArea(3, 4, 9, 9));
                                ws.AddMergeArea(new MergeArea(3, 4, 10, 10));
                                ws.AddMergeArea(new MergeArea(3, 4, 11, 11));
                                ws.AddMergeArea(new MergeArea(3, 3, 12, 14));
                                ws.AddMergeArea(new MergeArea(3, 3, 15, 16));
                                ws.AddMergeArea(new MergeArea(3, 4, 17, 17));
                                #endregion
                                #region Creat Table
                                ss = from a in db.ReportStocks
                                     where a.Status == "线下"

                                     select new
                                     {
                                         index = "",
                                         a.StorageInCode,
                                         a.MaterialName,
                                         a.SpecificationModel,
                                         a.MaterialCode,
                                         a.FinanceCode,
                                         a.StorageTime,
                                         a.SupplierName,
                                         a.ManufacturerName,
                                         a.BillCode,
                                         a.CurUnit,
                                         a.StocksGenTaojian,
                                         a.StocksMetre,
                                         a.StocksTon,
                                         a.UnitPrice,
                                         sum = "",
                                         remark = "",
                                     };

                                dt = ReporterHelper.LinqQueryToDataTable(ss);
                                if (dt.Rows.Count == 0)
                                    break;


                                if (dt.Rows.Count > 0)
                                {
                                    i = 0;
                                    foreach (DataRow drtos in dt.Rows)
                                    {
                                        i++;
                                        drtos["index"] = i.ToString();
                                        switch (drtos["CurUnit"].ToString())
                                        {
                                            case "根/台/套/件":
                                                drtos["sum"] = decimal.Parse(drtos["UnitPrice"].ToString()) * decimal.Parse(drtos["StocksGenTaojian"].ToString());
                                                break;
                                            case "米":
                                                drtos["sum"] = decimal.Parse(drtos["UnitPrice"].ToString()) * decimal.Parse(drtos["StocksMetre"].ToString());
                                                break;
                                            case "吨":
                                                drtos["sum"] = decimal.Parse(drtos["UnitPrice"].ToString()) * decimal.Parse(drtos["StocksTon"].ToString());
                                                break;
                                        }
                                        //drtos["sum"] = 

                                    }
                                }
                                ReporterHelper.ReadFromDataTable(ref dt, ref ws, 5, 1, xf2);
                                #endregion






                                // ReporterHelper.SetXF(ref ws, 3, 1, (ushort)(4 + dt.Rows.Count), 11, xf1);
                                #endregion
                                break;
                                #region 后期扩展
                                /*
                            case ReportTypeSt.实物库存报表:
                                #region 实物库存报表
                                ws = xd.Workbook.Worksheets.Add("实物库存报表");
                                #region Creat by Bot
                                ws.Cells.Add(1, 1, "物资供应处川西物资配送中心实物库存表", xf3);
                                ws.Cells.Add(2, 1, "填报单位：物资供应处川西物资配送中心");
                                ws.Cells.Add(2, 8, "截止日期：");
                                ws.Cells.Add(3, 1, "序号", xf1);
                                ws.Cells.Add(3, 2, "物资大类", xf1);
                                ws.Cells.Add(3, 3, "财务编号", xf1);
                                ws.Cells.Add(3, 4, "名称", xf1);
                                ws.Cells.Add(3, 5, "规格型号", xf1);
                                ws.Cells.Add(3, 6, "实物库存", xf1);
                                ws.Cells.Add(3, 7, null, xf1);
                                ws.Cells.Add(3, 8, null, xf1);
                                ws.Cells.Add(3, 9, null, xf1);
                                ws.Cells.Add(3, 10, null, xf1);
                                ws.Cells.Add(3, 11, null, xf1);
                                ws.Cells.Add(3, 12, null, xf1);
                                ws.Cells.Add(3, 13, "ERP实物库存", xf1);
                                ws.Cells.Add(3, 14, null, xf1);
                                ws.Cells.Add(3, 15, null, xf1);
                                ws.Cells.Add(3, 16, null, xf1);
                                ws.Cells.Add(3, 17, null, xf1);
                                ws.Cells.Add(3, 18, null, xf1);
                                ws.Cells.Add(3, 19, null, xf1);
                                ws.Cells.Add(3, 20, null, xf1);
                                ws.Cells.Add(3, 21, null, xf1);
                                ws.Cells.Add(3, 22, "待上线实物库存", xf1);
                                ws.Cells.Add(3, 23, null, xf1);
                                ws.Cells.Add(3, 24, null, xf1);
                                ws.Cells.Add(3, 25, null, xf1);
                                ws.Cells.Add(3, 26, null, xf1);
                                ws.Cells.Add(3, 27, null, xf1);
                                ws.Cells.Add(3, 28, null, xf1);
                                ws.Cells.Add(3, 29, null, xf1);
                                ws.Cells.Add(3, 30, "回收合格实物库存", xf1);
                                ws.Cells.Add(3, 31, null, xf1);
                                ws.Cells.Add(3, 32, null, xf1);
                                ws.Cells.Add(3, 33, null, xf1);
                                ws.Cells.Add(3, 34, null, xf1);
                                ws.Cells.Add(3, 35, null, xf1);
                                ws.Cells.Add(3, 36, null, xf1);
                                ws.Cells.Add(3, 37, null, xf1);
                                ws.Cells.Add(3, 38, "计划项目", xf1);
                                ws.Cells.Add(3, 39, "无指向 库存", xf1);
                                ws.Cells.Add(3, 40, null, xf1);
                                ws.Cells.Add(3, 41, null, xf1);
                                ws.Cells.Add(3, 42, null, xf1);
                                ws.Cells.Add(3, 43, null, xf1);
                                ws.Cells.Add(3, 44, "货位信息", xf1);
                                ws.Cells.Add(3, 45, null, xf1);
                                ws.Cells.Add(3, 46, "备注", xf1);
                                ws.Cells.Add(4, 1, null, xf1);
                                ws.Cells.Add(4, 2, null, xf1);
                                ws.Cells.Add(4, 3, null, xf1);
                                ws.Cells.Add(4, 4, null, xf1);
                                ws.Cells.Add(4, 5, null, xf1);
                                ws.Cells.Add(4, 6, "根/台/套", xf1);
                                ws.Cells.Add(4, 7, "米", xf1);
                                ws.Cells.Add(4, 8, "吨", xf1);
                                ws.Cells.Add(4, 9, "金额（元）", xf1);
                                ws.Cells.Add(4, 10, "物资状态", xf1);
                                ws.Cells.Add(4, 11, null, xf1);
                                ws.Cells.Add(4, 12, null, xf1);
                                ws.Cells.Add(4, 13, "物料编码", xf1);
                                ws.Cells.Add(4, 14, "根/台/套", xf1);
                                ws.Cells.Add(4, 15, "米", xf1);
                                ws.Cells.Add(4, 16, "吨", xf1);
                                ws.Cells.Add(4, 17, "单价（元）", xf1);
                                ws.Cells.Add(4, 18, "金额（元）", xf1);
                                ws.Cells.Add(4, 19, "物资状态", xf1);
                                ws.Cells.Add(4, 20, null, xf1);
                                ws.Cells.Add(4, 21, null, xf1);
                                ws.Cells.Add(4, 22, "根/台/套", xf1);
                                ws.Cells.Add(4, 23, "米", xf1);
                                ws.Cells.Add(4, 24, "吨", xf1);
                                ws.Cells.Add(4, 25, "单价（元）", xf1);
                                ws.Cells.Add(4, 26, "金额（元）", xf1);
                                ws.Cells.Add(4, 27, "物资状态", xf1);
                                ws.Cells.Add(4, 28, null, xf1);
                                ws.Cells.Add(4, 29, null, xf1);
                                ws.Cells.Add(4, 30, "根/台/套", xf1);
                                ws.Cells.Add(4, 31, "米", xf1);
                                ws.Cells.Add(4, 32, "吨", xf1);
                                ws.Cells.Add(4, 33, "单价（元）", xf1);
                                ws.Cells.Add(4, 34, "金额（元）", xf1);
                                ws.Cells.Add(4, 35, "物资状态", xf1);
                                ws.Cells.Add(4, 36, null, xf1);
                                ws.Cells.Add(4, 37, null, xf1);
                                ws.Cells.Add(4, 38, null, xf1);
                                ws.Cells.Add(4, 39, null, xf1);
                                ws.Cells.Add(4, 40, null, xf1);
                                ws.Cells.Add(4, 41, null, xf1);
                                ws.Cells.Add(4, 42, null, xf1);
                                ws.Cells.Add(4, 43, null, xf1);
                                ws.Cells.Add(4, 44, null, xf1);
                                ws.Cells.Add(4, 45, null, xf1);
                                ws.Cells.Add(4, 46, null, xf1);
                                ws.Cells.Add(5, 1, null, xf1);
                                ws.Cells.Add(5, 2, null, xf1);
                                ws.Cells.Add(5, 3, null, xf1);
                                ws.Cells.Add(5, 4, null, xf1);
                                ws.Cells.Add(5, 5, null, xf1);
                                ws.Cells.Add(5, 6, null, xf1);
                                ws.Cells.Add(5, 7, null, xf1);
                                ws.Cells.Add(5, 8, null, xf1);
                                ws.Cells.Add(5, 9, null, xf1);
                                ws.Cells.Add(5, 10, "合格", xf1);
                                ws.Cells.Add(5, 11, "待保养", xf1);
                                ws.Cells.Add(5, 12, "待修", xf1);
                                ws.Cells.Add(5, 13, null, xf1);
                                ws.Cells.Add(5, 14, null, xf1);
                                ws.Cells.Add(5, 15, null, xf1);
                                ws.Cells.Add(5, 16, null, xf1);
                                ws.Cells.Add(5, 17, null, xf1);
                                ws.Cells.Add(5, 18, null, xf1);
                                ws.Cells.Add(5, 19, "合格", xf1);
                                ws.Cells.Add(5, 20, "待保养", xf1);
                                ws.Cells.Add(5, 21, "待修", xf1);
                                ws.Cells.Add(5, 22, null, xf1);
                                ws.Cells.Add(5, 23, null, xf1);
                                ws.Cells.Add(5, 24, null, xf1);
                                ws.Cells.Add(5, 25, null, xf1);
                                ws.Cells.Add(5, 26, null, xf1);
                                ws.Cells.Add(5, 27, "合格", xf1);
                                ws.Cells.Add(5, 28, "待保养", xf1);
                                ws.Cells.Add(5, 29, "待修", xf1);
                                ws.Cells.Add(5, 30, null, xf1);
                                ws.Cells.Add(5, 31, null, xf1);
                                ws.Cells.Add(5, 32, null, xf1);
                                ws.Cells.Add(5, 33, null, xf1);
                                ws.Cells.Add(5, 34, null, xf1);
                                ws.Cells.Add(5, 35, "合格", xf1);
                                ws.Cells.Add(5, 36, "待保养", xf1);
                                ws.Cells.Add(5, 37, "待修", xf1);
                                ws.Cells.Add(5, 38, null, xf1);
                                ws.Cells.Add(5, 39, "根/台/套", xf1);
                                ws.Cells.Add(5, 40, "米", xf1);
                                ws.Cells.Add(5, 41, "吨", xf1);
                                ws.Cells.Add(5, 42, "单价（元）", xf1);
                                ws.Cells.Add(5, 43, "金额（元）", xf1);
                                ws.Cells.Add(5, 44, "垛位", xf1);
                                ws.Cells.Add(5, 45, "数量", xf1);
                                ws.Cells.Add(5, 46, null, xf1);
                                ws.AddMergeArea(new MergeArea(1, 1, 1, 46));
                                ws.AddMergeArea(new MergeArea(2, 2, 8, 46));
                                ws.AddMergeArea(new MergeArea(3, 5, 1, 1));
                                ws.AddMergeArea(new MergeArea(3, 5, 2, 2));
                                ws.AddMergeArea(new MergeArea(3, 5, 3, 3));
                                ws.AddMergeArea(new MergeArea(3, 5, 4, 4));
                                ws.AddMergeArea(new MergeArea(3, 5, 5, 5));
                                ws.AddMergeArea(new MergeArea(3, 3, 6, 12));
                                ws.AddMergeArea(new MergeArea(3, 3, 13, 21));
                                ws.AddMergeArea(new MergeArea(3, 3, 22, 29));
                                ws.AddMergeArea(new MergeArea(3, 3, 30, 37));
                                ws.AddMergeArea(new MergeArea(3, 5, 38, 38));
                                ws.AddMergeArea(new MergeArea(3, 4, 39, 43));
                                ws.AddMergeArea(new MergeArea(3, 4, 44, 45));
                                ws.AddMergeArea(new MergeArea(3, 5, 46, 46));
                                ws.AddMergeArea(new MergeArea(4, 5, 6, 6));
                                ws.AddMergeArea(new MergeArea(4, 5, 7, 7));
                                ws.AddMergeArea(new MergeArea(4, 5, 8, 8));
                                ws.AddMergeArea(new MergeArea(4, 5, 9, 9));
                                ws.AddMergeArea(new MergeArea(4, 4, 10, 12));
                                ws.AddMergeArea(new MergeArea(4, 5, 13, 13));
                                ws.AddMergeArea(new MergeArea(4, 5, 14, 14));
                                ws.AddMergeArea(new MergeArea(4, 5, 15, 15));
                                ws.AddMergeArea(new MergeArea(4, 5, 16, 16));
                                ws.AddMergeArea(new MergeArea(4, 5, 17, 17));
                                ws.AddMergeArea(new MergeArea(4, 5, 18, 18));
                                ws.AddMergeArea(new MergeArea(4, 4, 19, 21));
                                ws.AddMergeArea(new MergeArea(4, 5, 22, 22));
                                ws.AddMergeArea(new MergeArea(4, 5, 23, 23));
                                ws.AddMergeArea(new MergeArea(4, 5, 24, 24));
                                ws.AddMergeArea(new MergeArea(4, 5, 25, 25));
                                ws.AddMergeArea(new MergeArea(4, 5, 26, 26));
                                ws.AddMergeArea(new MergeArea(4, 4, 27, 29));
                                ws.AddMergeArea(new MergeArea(4, 5, 30, 30));
                                ws.AddMergeArea(new MergeArea(4, 5, 31, 31));
                                ws.AddMergeArea(new MergeArea(4, 5, 32, 32));
                                ws.AddMergeArea(new MergeArea(4, 5, 33, 33));
                                ws.AddMergeArea(new MergeArea(4, 5, 34, 34));
                                ws.AddMergeArea(new MergeArea(4, 4, 35, 37));
                                #endregion



                                #endregion
                                break;
                            case ReportTypeSt.实物库存汇总表:
                                #region 实物库存汇总表
                                ws = xd.Workbook.Worksheets.Add("实物库存汇总表");
                                #region Creat by Bot
                                ws.Cells.Add(1, 1, "物资供应处川西物资配送中心实物库存汇总表", xf3);
                                ws.Cells.Add(2, 1, "填报日期：物资供应处川西物资配送中心");
                                ws.Cells.Add(2, 9, "填报日期：");
                                ws.Cells.Add(2, 17, "截止日期：");
                                ws.Cells.Add(3, 1, "项  目", xf1);
                                ws.Cells.Add(3, 2, "实物库存", xf1);
                                ws.Cells.Add(3, 3, null, xf1);
                                ws.Cells.Add(3, 4, null, xf1);
                                ws.Cells.Add(3, 5, null, xf1);
                                ws.Cells.Add(3, 6, "ERP实物库存", xf1);
                                ws.Cells.Add(3, 7, null, xf1);
                                ws.Cells.Add(3, 8, null, xf1);
                                ws.Cells.Add(3, 9, null, xf1);
                                ws.Cells.Add(3, 10, "待上线实物库存", xf1);
                                ws.Cells.Add(3, 11, null, xf1);
                                ws.Cells.Add(3, 12, null, xf1);
                                ws.Cells.Add(3, 13, null, xf1);
                                ws.Cells.Add(3, 14, "回收合格实物库存", xf1);
                                ws.Cells.Add(3, 15, null, xf1);
                                ws.Cells.Add(3, 16, null, xf1);
                                ws.Cells.Add(3, 17, null, xf1);
                                ws.Cells.Add(3, 18, "备注", xf1);
                                ws.Cells.Add(4, 1, null, xf1);
                                ws.Cells.Add(4, 2, "根/台/套", xf1);
                                ws.Cells.Add(4, 3, "米", xf1);
                                ws.Cells.Add(4, 4, "吨", xf1);
                                ws.Cells.Add(4, 5, "金额（元）", xf1);
                                ws.Cells.Add(4, 6, "根/台/套", xf1);
                                ws.Cells.Add(4, 7, "米", xf1);
                                ws.Cells.Add(4, 8, "吨", xf1);
                                ws.Cells.Add(4, 9, "金额（元）", xf1);
                                ws.Cells.Add(4, 10, "根/台/套", xf1);
                                ws.Cells.Add(4, 11, "米", xf1);
                                ws.Cells.Add(4, 12, "吨", xf1);
                                ws.Cells.Add(4, 13, "金额（元）", xf1);
                                ws.Cells.Add(4, 14, "根/台/套", xf1);
                                ws.Cells.Add(4, 15, "米", xf1);
                                ws.Cells.Add(4, 16, "吨", xf1);
                                ws.Cells.Add(4, 17, "金额（元）", xf1);
                                ws.Cells.Add(4, 18, null, xf1);
                                ws.Cells.Add(5, 1, "油管及附件 合计", xf1);
                                ws.Cells.Add(5, 2, null, xf1);
                                ws.Cells.Add(5, 3, null, xf1);
                                ws.Cells.Add(5, 4, null, xf1);
                                ws.Cells.Add(5, 5, null, xf1);
                                ws.Cells.Add(5, 6, null, xf1);
                                ws.Cells.Add(5, 7, null, xf1);
                                ws.Cells.Add(5, 8, null, xf1);
                                ws.Cells.Add(5, 9, null, xf1);
                                ws.Cells.Add(5, 10, null, xf1);
                                ws.Cells.Add(5, 11, null, xf1);
                                ws.Cells.Add(5, 12, null, xf1);
                                ws.Cells.Add(5, 13, null, xf1);
                                ws.Cells.Add(5, 14, null, xf1);
                                ws.Cells.Add(5, 15, null, xf1);
                                ws.Cells.Add(5, 16, null, xf1);
                                ws.Cells.Add(5, 17, null, xf1);
                                ws.Cells.Add(5, 18, null, xf1);
                                ws.Cells.Add(6, 1, "套管及附件 合计", xf1);
                                ws.Cells.Add(6, 2, null, xf1);
                                ws.Cells.Add(6, 3, null, xf1);
                                ws.Cells.Add(6, 4, null, xf1);
                                ws.Cells.Add(6, 5, null, xf1);
                                ws.Cells.Add(6, 6, null, xf1);
                                ws.Cells.Add(6, 7, null, xf1);
                                ws.Cells.Add(6, 8, null, xf1);
                                ws.Cells.Add(6, 9, null, xf1);
                                ws.Cells.Add(6, 10, null, xf1);
                                ws.Cells.Add(6, 11, null, xf1);
                                ws.Cells.Add(6, 12, null, xf1);
                                ws.Cells.Add(6, 13, null, xf1);
                                ws.Cells.Add(6, 14, null, xf1);
                                ws.Cells.Add(6, 15, null, xf1);
                                ws.Cells.Add(6, 16, null, xf1);
                                ws.Cells.Add(6, 17, null, xf1);
                                ws.Cells.Add(6, 18, null, xf1);
                                ws.Cells.Add(7, 1, "井控 合计", xf1);
                                ws.Cells.Add(7, 2, null, xf1);
                                ws.Cells.Add(7, 3, null, xf1);
                                ws.Cells.Add(7, 4, null, xf1);
                                ws.Cells.Add(7, 5, null, xf1);
                                ws.Cells.Add(7, 6, null, xf1);
                                ws.Cells.Add(7, 7, null, xf1);
                                ws.Cells.Add(7, 8, null, xf1);
                                ws.Cells.Add(7, 9, null, xf1);
                                ws.Cells.Add(7, 10, null, xf1);
                                ws.Cells.Add(7, 11, null, xf1);
                                ws.Cells.Add(7, 12, null, xf1);
                                ws.Cells.Add(7, 13, null, xf1);
                                ws.Cells.Add(7, 14, null, xf1);
                                ws.Cells.Add(7, 15, null, xf1);
                                ws.Cells.Add(7, 16, null, xf1);
                                ws.Cells.Add(7, 17, null, xf1);
                                ws.Cells.Add(7, 18, null, xf1);
                                ws.Cells.Add(8, 1, "油建 合计", xf1);
                                ws.Cells.Add(8, 2, null, xf1);
                                ws.Cells.Add(8, 3, null, xf1);
                                ws.Cells.Add(8, 4, null, xf1);
                                ws.Cells.Add(8, 5, null, xf1);
                                ws.Cells.Add(8, 6, null, xf1);
                                ws.Cells.Add(8, 7, null, xf1);
                                ws.Cells.Add(8, 8, null, xf1);
                                ws.Cells.Add(8, 9, null, xf1);
                                ws.Cells.Add(8, 10, null, xf1);
                                ws.Cells.Add(8, 11, null, xf1);
                                ws.Cells.Add(8, 12, null, xf1);
                                ws.Cells.Add(8, 13, null, xf1);
                                ws.Cells.Add(8, 14, null, xf1);
                                ws.Cells.Add(8, 15, null, xf1);
                                ws.Cells.Add(8, 16, null, xf1);
                                ws.Cells.Add(8, 17, null, xf1);
                                ws.Cells.Add(8, 18, null, xf1);
                                ws.Cells.Add(9, 1, "设备 合计", xf1);
                                ws.Cells.Add(9, 2, null, xf1);
                                ws.Cells.Add(9, 3, null, xf1);
                                ws.Cells.Add(9, 4, null, xf1);
                                ws.Cells.Add(9, 5, null, xf1);
                                ws.Cells.Add(9, 6, null, xf1);
                                ws.Cells.Add(9, 7, null, xf1);
                                ws.Cells.Add(9, 8, null, xf1);
                                ws.Cells.Add(9, 9, null, xf1);
                                ws.Cells.Add(9, 10, null, xf1);
                                ws.Cells.Add(9, 11, null, xf1);
                                ws.Cells.Add(9, 12, null, xf1);
                                ws.Cells.Add(9, 13, null, xf1);
                                ws.Cells.Add(9, 14, null, xf1);
                                ws.Cells.Add(9, 15, null, xf1);
                                ws.Cells.Add(9, 16, null, xf1);
                                ws.Cells.Add(9, 17, null, xf1);
                                ws.Cells.Add(9, 18, null, xf1);
                                ws.Cells.Add(10, 1, "专用工具 合计", xf1);
                                ws.Cells.Add(10, 2, null, xf1);
                                ws.Cells.Add(10, 3, null, xf1);
                                ws.Cells.Add(10, 4, null, xf1);
                                ws.Cells.Add(10, 5, null, xf1);
                                ws.Cells.Add(10, 6, null, xf1);
                                ws.Cells.Add(10, 7, null, xf1);
                                ws.Cells.Add(10, 8, null, xf1);
                                ws.Cells.Add(10, 9, null, xf1);
                                ws.Cells.Add(10, 10, null, xf1);
                                ws.Cells.Add(10, 11, null, xf1);
                                ws.Cells.Add(10, 12, null, xf1);
                                ws.Cells.Add(10, 13, null, xf1);
                                ws.Cells.Add(10, 14, null, xf1);
                                ws.Cells.Add(10, 15, null, xf1);
                                ws.Cells.Add(10, 16, null, xf1);
                                ws.Cells.Add(10, 17, null, xf1);
                                ws.Cells.Add(10, 18, null, xf1);
                                ws.Cells.Add(11, 1, "总计", xf1);
                                ws.Cells.Add(11, 2, null, xf1);
                                ws.Cells.Add(11, 3, null, xf1);
                                ws.Cells.Add(11, 4, null, xf1);
                                ws.Cells.Add(11, 5, null, xf1);
                                ws.Cells.Add(11, 6, null, xf1);
                                ws.Cells.Add(11, 7, null, xf1);
                                ws.Cells.Add(11, 8, null, xf1);
                                ws.Cells.Add(11, 9, null, xf1);
                                ws.Cells.Add(11, 10, null, xf1);
                                ws.Cells.Add(11, 11, null, xf1);
                                ws.Cells.Add(11, 12, null, xf1);
                                ws.Cells.Add(11, 13, null, xf1);
                                ws.Cells.Add(11, 14, null, xf1);
                                ws.Cells.Add(11, 15, null, xf1);
                                ws.Cells.Add(11, 16, null, xf1);
                                ws.Cells.Add(11, 17, null, xf1);
                                ws.Cells.Add(11, 18, null, xf1);
                                ws.AddMergeArea(new MergeArea(1, 1, 1, 18));
                                ws.AddMergeArea(new MergeArea(2, 2, 1, 4));
                                ws.AddMergeArea(new MergeArea(3, 4, 1, 1));
                                ws.AddMergeArea(new MergeArea(3, 3, 2, 5));
                                ws.AddMergeArea(new MergeArea(3, 3, 6, 9));
                                ws.AddMergeArea(new MergeArea(3, 3, 10, 13));
                                ws.AddMergeArea(new MergeArea(3, 3, 14, 17));
                                ws.AddMergeArea(new MergeArea(3, 4, 18, 18));
                                #endregion


                                #endregion
                                break;
                                */
                            #endregion
                        }
                        #endregion
                        break;
                    case "出库报表":
                        #region 出库报表
                        xd.FileName = HttpUtility.UrlEncode(this.ddlOut.Text);
                        switch ((ReportTypeOut)Enum.Parse(typeof(ReportTypeOut), this.ddlOut.Text))
                        {
                                #region 后期扩展
                                /*
                            case ReportTypeOut.物资消耗总表:
                                #region 物资消耗总表
                                ws = xd.Workbook.Worksheets.Add("物资消耗总表");
                                #region Creat by Bot
                                ws.Cells.Add(1, 1, "物资供应处川西物资配送中心物资消耗汇总表", xf3);
                                ws.Cells.Add(2, 1, "填报单位：物资供应处川西物资配送中心");
                                ws.Cells.Add(2, 4, "报出日期：");
                                ws.Cells.Add(2, 5, "截止日期：");
                                ws.Cells.Add(3, 1, "项  目", xf1);
                                ws.Cells.Add(3, 2, "物资消耗量", xf1);
                                ws.Cells.Add(3, 3, null, xf1);
                                ws.Cells.Add(3, 4, null, xf1);
                                ws.Cells.Add(3, 5, null, xf1);
                                ws.Cells.Add(3, 6, "备注", xf1);
                                ws.Cells.Add(4, 1, null, xf1);
                                ws.Cells.Add(4, 2, "根/台/套", xf1);
                                ws.Cells.Add(4, 3, "米", xf1);
                                ws.Cells.Add(4, 4, "吨", xf1);
                                ws.Cells.Add(4, 5, "金额（元）", xf1);
                                ws.Cells.Add(4, 6, null, xf1);
                                ws.Cells.Add(5, 1, "油管及附件 合计", xf1);
                                ws.Cells.Add(5, 2, null, xf1);
                                ws.Cells.Add(5, 3, null, xf1);
                                ws.Cells.Add(5, 4, null, xf1);
                                ws.Cells.Add(5, 5, null, xf1);
                                ws.Cells.Add(5, 6, null, xf1);
                                ws.Cells.Add(6, 1, "套管及附件 合计", xf1);
                                ws.Cells.Add(6, 2, null, xf1);
                                ws.Cells.Add(6, 3, null, xf1);
                                ws.Cells.Add(6, 4, null, xf1);
                                ws.Cells.Add(6, 5, null, xf1);
                                ws.Cells.Add(6, 6, null, xf1);
                                ws.Cells.Add(7, 1, "井控 合计", xf1);
                                ws.Cells.Add(7, 2, null, xf1);
                                ws.Cells.Add(7, 3, null, xf1);
                                ws.Cells.Add(7, 4, null, xf1);
                                ws.Cells.Add(7, 5, null, xf1);
                                ws.Cells.Add(7, 6, null, xf1);
                                ws.Cells.Add(8, 1, "油建 合计", xf1);
                                ws.Cells.Add(8, 2, null, xf1);
                                ws.Cells.Add(8, 3, null, xf1);
                                ws.Cells.Add(8, 4, null, xf1);
                                ws.Cells.Add(8, 5, null, xf1);
                                ws.Cells.Add(8, 6, null, xf1);
                                ws.Cells.Add(9, 1, "设备 合计", xf1);
                                ws.Cells.Add(9, 2, null, xf1);
                                ws.Cells.Add(9, 3, null, xf1);
                                ws.Cells.Add(9, 4, null, xf1);
                                ws.Cells.Add(9, 5, null, xf1);
                                ws.Cells.Add(9, 6, null, xf1);
                                ws.Cells.Add(10, 1, "专用工具 合计", xf1);
                                ws.Cells.Add(10, 2, null, xf1);
                                ws.Cells.Add(10, 3, null, xf1);
                                ws.Cells.Add(10, 4, null, xf1);
                                ws.Cells.Add(10, 5, null, xf1);
                                ws.Cells.Add(10, 6, null, xf1);
                                ws.Cells.Add(11, 1, "总计", xf1);
                                ws.Cells.Add(11, 2, null, xf1);
                                ws.Cells.Add(11, 3, null, xf1);
                                ws.Cells.Add(11, 4, null, xf1);
                                ws.Cells.Add(11, 5, null, xf1);
                                ws.Cells.Add(11, 6, null, xf1);
                                ws.AddMergeArea(new MergeArea(1, 1, 1, 6));
                                ws.AddMergeArea(new MergeArea(2, 2, 1, 3));
                                ws.AddMergeArea(new MergeArea(3, 4, 1, 1));
                                ws.AddMergeArea(new MergeArea(3, 3, 2, 5));
                                ws.AddMergeArea(new MergeArea(3, 4, 6, 6));
                                #endregion
                                #endregion
                                break;
                                */
                            #endregion
                            case ReportTypeOut.单井工作统计表:
                                #region 单井工作统计表 --完成 等待测试数据
                                ws = xd.Workbook.Worksheets.Add("单井工作统计表");
                                #region Creat by Bot
                                ws.Cells.Add(1, 1, "物资供应处川西物资配送中心单井统计报表", xf3);
                                ws.Cells.Add(2, 1, "业主单位：");
                                ws.Cells.Add(2, 4, "井号：");
                                ws.Cells.Add(2, 10, "项目性质：");
                                ws.Cells.Add(2, 14, "截止日期：");
                                ws.Cells.Add(2, 16, "报出日期：");
                                ws.Cells.Add(3, 1, "序号", xf1);
                                ws.Cells.Add(3, 2, "名称", xf1);
                                ws.Cells.Add(3, 3, "规格型号", xf1);
                                ws.Cells.Add(3, 4, "物料编码", xf1);
                                ws.Cells.Add(3, 5, "财务编码", xf1);
                                ws.Cells.Add(3, 6, "出库单据编号", xf1);
                                ws.Cells.Add(3, 7, "出库时间", xf1);
                                ws.Cells.Add(3, 8, "生产厂家", xf1);
                                ws.Cells.Add(3, 9, "发料凭证", xf1);
                                ws.Cells.Add(3, 10, "计量单位", xf1);
                                ws.Cells.Add(3, 11, "发货数量", xf1);
                                ws.Cells.Add(3, 12, null, xf1);
                                ws.Cells.Add(3, 13, null, xf1);
                                ws.Cells.Add(3, 14, "单价（元）", xf1);
                                ws.Cells.Add(3, 15, "金额（元）", xf1);
                                ws.Cells.Add(3, 16, "施工单位", xf1);
                                ws.Cells.Add(3, 17, "项目阶段", xf1);
                                ws.Cells.Add(3, 18, "备注", xf1);
                                ws.Cells.Add(4, 1, null, xf1);
                                ws.Cells.Add(4, 2, null, xf1);
                                ws.Cells.Add(4, 3, null, xf1);
                                ws.Cells.Add(4, 4, null, xf1);
                                ws.Cells.Add(4, 5, null, xf1);
                                ws.Cells.Add(4, 6, null, xf1);
                                ws.Cells.Add(4, 7, null, xf1);
                                ws.Cells.Add(4, 8, null, xf1);
                                ws.Cells.Add(4, 9, null, xf1);
                                ws.Cells.Add(4, 10, null, xf1);
                                ws.Cells.Add(4, 11, "根 /套/只", xf1);
                                ws.Cells.Add(4, 12, "米", xf1);
                                ws.Cells.Add(4, 13, "吨", xf1);
                                ws.Cells.Add(4, 14, null, xf1);
                                ws.Cells.Add(4, 15, null, xf1);
                                ws.Cells.Add(4, 16, null, xf1);
                                ws.Cells.Add(4, 17, null, xf1);
                                ws.Cells.Add(4, 18, null, xf1);
                                ws.Cells.Add(5, 1, null, xf1);
                                ws.Cells.Add(5, 2, null, xf1);
                                ws.Cells.Add(5, 3, null, xf1);
                                ws.Cells.Add(5, 4, null, xf1);
                                ws.Cells.Add(5, 5, null, xf1);
                                ws.Cells.Add(5, 6, null, xf1);
                                ws.Cells.Add(5, 7, null, xf1);
                                ws.Cells.Add(5, 8, null, xf1);
                                ws.Cells.Add(5, 9, null, xf1);
                                ws.Cells.Add(5, 10, null, xf1);
                                ws.Cells.Add(5, 11, null, xf1);
                                ws.Cells.Add(5, 12, null, xf1);
                                ws.Cells.Add(5, 13, null, xf1);
                                ws.Cells.Add(5, 14, null, xf1);
                                ws.Cells.Add(5, 15, null, xf1);
                                ws.Cells.Add(5, 16, null, xf1);
                                ws.Cells.Add(5, 17, null, xf1);
                                ws.Cells.Add(5, 18, null, xf1);
                                ws.AddMergeArea(new MergeArea(1, 1, 1, 18));
                                ws.AddMergeArea(new MergeArea(2, 2, 1, 3));
                                ws.AddMergeArea(new MergeArea(3, 5, 1, 1));
                                ws.AddMergeArea(new MergeArea(3, 5, 2, 2));
                                ws.AddMergeArea(new MergeArea(3, 5, 3, 3));
                                ws.AddMergeArea(new MergeArea(3, 5, 4, 4));
                                ws.AddMergeArea(new MergeArea(3, 5, 5, 5));
                                ws.AddMergeArea(new MergeArea(3, 5, 6, 6));
                                ws.AddMergeArea(new MergeArea(3, 5, 7, 7));
                                ws.AddMergeArea(new MergeArea(3, 5, 8, 8));
                                ws.AddMergeArea(new MergeArea(3, 5, 9, 9));
                                ws.AddMergeArea(new MergeArea(3, 5, 10, 10));
                                ws.AddMergeArea(new MergeArea(3, 3, 11, 13));
                                ws.AddMergeArea(new MergeArea(3, 5, 14, 14));
                                ws.AddMergeArea(new MergeArea(3, 5, 15, 15));
                                ws.AddMergeArea(new MergeArea(3, 5, 16, 16));
                                ws.AddMergeArea(new MergeArea(3, 5, 17, 17));
                                ws.AddMergeArea(new MergeArea(3, 5, 18, 18));
                                ws.AddMergeArea(new MergeArea(4, 5, 11, 11));
                                ws.AddMergeArea(new MergeArea(4, 5, 12, 12));
                                ws.AddMergeArea(new MergeArea(4, 5, 13, 13));
                                #endregion
                                #region create table
                                var sout = from a in db.NormalOut

                                           select new
                                           {
                                               index = "",
                                               a.MaterialName,
                                               a.SpecificationModel,
                                               a.MaterialCode,
                                               a.FinanceCode,
                                               a.StorageOutNoticeCode,
                                               a.ConfirmTime,
                                               a.ManufacturerName,
                                               发料凭证 = "",
                                               a.CurUnit,
                                               a.RealGentaojian,
                                               a.RealMetre,
                                               a.RealTon,
                                               a.UnitPrice,
                                               a.RealAmount,
                                               a.BusinessUnitName,
                                               a.ProjectStage,
                                               a.ProjectName,
                                           };
                                sout.OrderBy(a => a.ProjectName);
                                dt = ReporterHelper.LinqQueryToDataTable(sout);
                                if (dt.Rows.Count == 0)
                                    break;


                                if (dt.Rows.Count > 0)
                                {
                                    i = 0;
                                    foreach (DataRow drtos in dt.Rows)
                                    {
                                        i++;
                                        drtos["index"] = i.ToString();


                                    }
                                }
                                ReporterHelper.ReadFromDataTable(ref dt, ref ws, 5, 1, xf2);

                                #endregion
                                #endregion
                                break;
                            case ReportTypeOut.委外出库报表:
                                #region 委外出库报表 --完工
                                ws = xd.Workbook.Worksheets.Add("委外出库报表");
                                #region Creat by Bot
                                ws.Cells.Add(1, 1, "物资供应处川西物资配送中心委外出库统计表", xf3);
                                ws.Cells.Add(2, 1, "填报单位：物资供应处川西物资配送中心");
                                ws.Cells.Add(2, 10, "报出日期：");
                                ws.Cells.Add(2, 17, "截止日期：");
                                ws.Cells.Add(3, 1, "序号", xf1);
                                ws.Cells.Add(3, 2, "名称", xf1);
                                ws.Cells.Add(3, 3, "规格型号", xf1);
                                ws.Cells.Add(3, 4, "物料编码", xf1);
                                ws.Cells.Add(3, 5, "财务编码", xf1);
                                ws.Cells.Add(3, 6, "出库单编号", xf1);
                                ws.Cells.Add(3, 7, "出库时间", xf1);
                                ws.Cells.Add(3, 8, "生产厂家", xf1);
                                ws.Cells.Add(3, 9, "发料凭证", xf1);
                                ws.Cells.Add(3, 10, "计量单位", xf1);
                                ws.Cells.Add(3, 11, "发货数量", xf1);
                                ws.Cells.Add(3, 12, null, xf1);
                                ws.Cells.Add(3, 13, null, xf1);
                                ws.Cells.Add(3, 14, "单价（元）", xf1);
                                ws.Cells.Add(3, 15, "金额（元）", xf1);
                                ws.Cells.Add(3, 16, "业主单位", xf1);
                                ws.Cells.Add(3, 17, "施工单位", xf1);
                                ws.Cells.Add(3, 18, "井号", xf1);
                                ws.Cells.Add(3, 19, "项目性质", xf1);
                                ws.Cells.Add(3, 20, "项目阶段", xf1);
                                ws.Cells.Add(3, 21, "备注", xf1);
                                ws.Cells.Add(4, 1, null, xf1);
                                ws.Cells.Add(4, 2, null, xf1);
                                ws.Cells.Add(4, 3, null, xf1);
                                ws.Cells.Add(4, 4, null, xf1);
                                ws.Cells.Add(4, 5, null, xf1);
                                ws.Cells.Add(4, 6, null, xf1);
                                ws.Cells.Add(4, 7, null, xf1);
                                ws.Cells.Add(4, 8, null, xf1);
                                ws.Cells.Add(4, 9, null, xf1);
                                ws.Cells.Add(4, 10, null, xf1);
                                ws.Cells.Add(4, 11, "根 /套/只", xf1);
                                ws.Cells.Add(4, 12, "米", xf1);
                                ws.Cells.Add(4, 13, "吨", xf1);
                                ws.Cells.Add(4, 14, null, xf1);
                                ws.Cells.Add(4, 15, null, xf1);
                                ws.Cells.Add(4, 16, null, xf1);
                                ws.Cells.Add(4, 17, null, xf1);
                                ws.Cells.Add(4, 18, null, xf1);
                                ws.Cells.Add(4, 19, null, xf1);
                                ws.Cells.Add(4, 20, null, xf1);
                                ws.Cells.Add(4, 21, null, xf1);
                                ws.Cells.Add(5, 1, null, xf1);
                                ws.Cells.Add(5, 2, null, xf1);
                                ws.Cells.Add(5, 3, null, xf1);
                                ws.Cells.Add(5, 4, null, xf1);
                                ws.Cells.Add(5, 5, null, xf1);
                                ws.Cells.Add(5, 6, null, xf1);
                                ws.Cells.Add(5, 7, null, xf1);
                                ws.Cells.Add(5, 8, null, xf1);
                                ws.Cells.Add(5, 9, null, xf1);
                                ws.Cells.Add(5, 10, null, xf1);
                                ws.Cells.Add(5, 11, null, xf1);
                                ws.Cells.Add(5, 12, null, xf1);
                                ws.Cells.Add(5, 13, null, xf1);
                                ws.Cells.Add(5, 14, null, xf1);
                                ws.Cells.Add(5, 15, null, xf1);
                                ws.Cells.Add(5, 16, null, xf1);
                                ws.Cells.Add(5, 17, null, xf1);
                                ws.Cells.Add(5, 18, null, xf1);
                                ws.Cells.Add(5, 19, null, xf1);
                                ws.Cells.Add(5, 20, null, xf1);
                                ws.Cells.Add(5, 21, null, xf1);
                                ws.AddMergeArea(new MergeArea(1, 1, 1, 21));
                                ws.AddMergeArea(new MergeArea(2, 2, 1, 3));
                                ws.AddMergeArea(new MergeArea(3, 5, 1, 1));
                                ws.AddMergeArea(new MergeArea(3, 5, 2, 2));
                                ws.AddMergeArea(new MergeArea(3, 5, 3, 3));
                                ws.AddMergeArea(new MergeArea(3, 5, 4, 4));
                                ws.AddMergeArea(new MergeArea(3, 5, 5, 5));
                                ws.AddMergeArea(new MergeArea(3, 5, 6, 6));
                                ws.AddMergeArea(new MergeArea(3, 5, 7, 7));
                                ws.AddMergeArea(new MergeArea(3, 5, 8, 8));
                                ws.AddMergeArea(new MergeArea(3, 5, 9, 9));
                                ws.AddMergeArea(new MergeArea(3, 5, 10, 10));
                                ws.AddMergeArea(new MergeArea(3, 3, 11, 13));
                                ws.AddMergeArea(new MergeArea(3, 5, 14, 14));
                                ws.AddMergeArea(new MergeArea(3, 5, 15, 15));
                                ws.AddMergeArea(new MergeArea(3, 5, 16, 16));
                                ws.AddMergeArea(new MergeArea(3, 5, 17, 17));
                                ws.AddMergeArea(new MergeArea(3, 5, 18, 18));
                                ws.AddMergeArea(new MergeArea(3, 5, 19, 19));
                                ws.AddMergeArea(new MergeArea(3, 5, 20, 20));
                                ws.AddMergeArea(new MergeArea(3, 5, 21, 21));
                                ws.AddMergeArea(new MergeArea(4, 5, 11, 11));
                                ws.AddMergeArea(new MergeArea(4, 5, 12, 12));
                                ws.AddMergeArea(new MergeArea(4, 5, 13, 13));
                                #endregion
                                #region create table
                                var cout = from a in db.ViewCommitOut
                                           select new
                                           {
                                               index = "",
                                               a.MaterialName,
                                               a.SpecificationModel,
                                               a.MaterialCode,
                                               a.FinanceCode,
                                               a.StorageCommitOutNoticeCode,
                                               a.ConfirmTime,
                                               a.ManufacturerName,
                                               发料凭证 = "",
                                               a.CurUnit,
                                               a.RealGentaojian,
                                               a.RealMetre,
                                               a.RealTon,
                                               a.UnitPrice,
                                               a.RealAmount,
                                               业主单位 = "",
                                               施工单位 = "",
                                               项目名称 = "",
                                               项目性质 = "",
                                               项目阶段 = "",
                                               a.Remark,
                                           };
                                dt = ReporterHelper.LinqQueryToDataTable(cout);
                                if (dt.Rows.Count == 0)
                                    break;


                                if (dt.Rows.Count > 0)
                                {
                                    i = 0;
                                    foreach (DataRow drtos in dt.Rows)
                                    {
                                        i++;
                                        drtos["index"] = i.ToString();


                                    }
                                }
                                ReporterHelper.ReadFromDataTable(ref dt, ref ws, 5, 1, xf2);

                                #endregion
                                #endregion
                                break;
                            case ReportTypeOut.项目发料出库报表:
                                #region 项目发料出库报表   --完成 等待测试数据
                                ws = xd.Workbook.Worksheets.Add("项目发料出库报表");
                                #region Creat by Bot
                                ws.Cells.Add(1, 1, "物资供应处川西物资配送中心项目发料出库统计表", xf3);
                                ws.Cells.Add(2, 1, "填报单位：物资供应处川西物资配送中心");
                                ws.Cells.Add(2, 10, "报出日期：");
                                ws.Cells.Add(2, 17, "截止日期：");
                                ws.Cells.Add(3, 1, "序号", xf1);
                                ws.Cells.Add(3, 2, "名称", xf1);
                                ws.Cells.Add(3, 3, "规格型号", xf1);
                                ws.Cells.Add(3, 4, "物料编码", xf1);
                                ws.Cells.Add(3, 5, "财务编码", xf1);
                                ws.Cells.Add(3, 6, "出库单编号", xf1);
                                ws.Cells.Add(3, 7, "出库时间", xf1);
                                ws.Cells.Add(3, 8, "生产厂家", xf1);
                                ws.Cells.Add(3, 9, "发料凭证", xf1);
                                ws.Cells.Add(3, 10, "计量单位", xf1);
                                ws.Cells.Add(3, 11, "发货数量", xf1);
                                ws.Cells.Add(3, 12, null, xf1);
                                ws.Cells.Add(3, 13, null, xf1);
                                ws.Cells.Add(3, 14, "单价（元）", xf1);
                                ws.Cells.Add(3, 15, "金额（元）", xf1);
                                ws.Cells.Add(3, 16, "业主单位", xf1);
                                ws.Cells.Add(3, 17, "施工单位", xf1);
                                ws.Cells.Add(3, 18, "井号", xf1);
                                ws.Cells.Add(3, 19, "项目性质", xf1);
                                ws.Cells.Add(3, 20, "项目阶段", xf1);
                                ws.Cells.Add(3, 21, "备注", xf1);
                                ws.Cells.Add(4, 1, null, xf1);
                                ws.Cells.Add(4, 2, null, xf1);
                                ws.Cells.Add(4, 3, null, xf1);
                                ws.Cells.Add(4, 4, null, xf1);
                                ws.Cells.Add(4, 5, null, xf1);
                                ws.Cells.Add(4, 6, null, xf1);
                                ws.Cells.Add(4, 7, null, xf1);
                                ws.Cells.Add(4, 8, null, xf1);
                                ws.Cells.Add(4, 9, null, xf1);
                                ws.Cells.Add(4, 10, null, xf1);
                                ws.Cells.Add(4, 11, "根 /套/只", xf1);
                                ws.Cells.Add(4, 12, "米", xf1);
                                ws.Cells.Add(4, 13, "吨", xf1);
                                ws.Cells.Add(4, 14, null, xf1);
                                ws.Cells.Add(4, 15, null, xf1);
                                ws.Cells.Add(4, 16, null, xf1);
                                ws.Cells.Add(4, 17, null, xf1);
                                ws.Cells.Add(4, 18, null, xf1);
                                ws.Cells.Add(4, 19, null, xf1);
                                ws.Cells.Add(4, 20, null, xf1);
                                ws.Cells.Add(4, 21, null, xf1);
                                ws.Cells.Add(5, 1, null, xf1);
                                ws.Cells.Add(5, 2, null, xf1);
                                ws.Cells.Add(5, 3, null, xf1);
                                ws.Cells.Add(5, 4, null, xf1);
                                ws.Cells.Add(5, 5, null, xf1);
                                ws.Cells.Add(5, 6, null, xf1);
                                ws.Cells.Add(5, 7, null, xf1);
                                ws.Cells.Add(5, 8, null, xf1);
                                ws.Cells.Add(5, 9, null, xf1);
                                ws.Cells.Add(5, 10, null, xf1);
                                ws.Cells.Add(5, 11, null, xf1);
                                ws.Cells.Add(5, 12, null, xf1);
                                ws.Cells.Add(5, 13, null, xf1);
                                ws.Cells.Add(5, 14, null, xf1);
                                ws.Cells.Add(5, 15, null, xf1);
                                ws.Cells.Add(5, 16, null, xf1);
                                ws.Cells.Add(5, 17, null, xf1);
                                ws.Cells.Add(5, 18, null, xf1);
                                ws.Cells.Add(5, 19, null, xf1);
                                ws.Cells.Add(5, 20, null, xf1);
                                ws.Cells.Add(5, 21, null, xf1);
                                ws.AddMergeArea(new MergeArea(1, 1, 1, 21));
                                ws.AddMergeArea(new MergeArea(2, 2, 1, 3));
                                ws.AddMergeArea(new MergeArea(3, 5, 1, 1));
                                ws.AddMergeArea(new MergeArea(3, 5, 2, 2));
                                ws.AddMergeArea(new MergeArea(3, 5, 3, 3));
                                ws.AddMergeArea(new MergeArea(3, 5, 4, 4));
                                ws.AddMergeArea(new MergeArea(3, 5, 5, 5));
                                ws.AddMergeArea(new MergeArea(3, 5, 6, 6));
                                ws.AddMergeArea(new MergeArea(3, 5, 7, 7));
                                ws.AddMergeArea(new MergeArea(3, 5, 8, 8));
                                ws.AddMergeArea(new MergeArea(3, 5, 9, 9));
                                ws.AddMergeArea(new MergeArea(3, 5, 10, 10));
                                ws.AddMergeArea(new MergeArea(3, 3, 11, 13));
                                ws.AddMergeArea(new MergeArea(3, 5, 14, 14));
                                ws.AddMergeArea(new MergeArea(3, 5, 15, 15));
                                ws.AddMergeArea(new MergeArea(3, 5, 16, 16));
                                ws.AddMergeArea(new MergeArea(3, 5, 17, 17));
                                ws.AddMergeArea(new MergeArea(3, 5, 18, 18));
                                ws.AddMergeArea(new MergeArea(3, 5, 19, 19));
                                ws.AddMergeArea(new MergeArea(3, 5, 20, 20));
                                ws.AddMergeArea(new MergeArea(3, 5, 21, 21));
                                ws.AddMergeArea(new MergeArea(4, 5, 11, 11));
                                ws.AddMergeArea(new MergeArea(4, 5, 12, 12));
                                ws.AddMergeArea(new MergeArea(4, 5, 13, 13));
                                #endregion

                                #region create table
                                var nout = from a in db.NormalOut
                                           select new
                                           {
                                               index = "",
                                               a.MaterialName,
                                               a.SpecificationModel,
                                               a.MaterialCode,
                                               a.FinanceCode,
                                               a.StorageOutNoticeCode,
                                               a.ConfirmTime,
                                               a.ManufacturerName,
                                               发料凭证 = "",
                                               a.CurUnit,
                                               a.RealGentaojian,
                                               a.RealMetre,
                                               a.RealTon,
                                               a.UnitPrice,
                                               a.RealAmount,
                                               a.own,
                                               a.BusinessUnitName,
                                               a.ProjectName,
                                               a.ProjectProperty,
                                               a.ProjectStage,
                                               a.Remark,
                                           };
                                dt = ReporterHelper.LinqQueryToDataTable(nout);
                                if (dt.Rows.Count == 0)
                                    break;


                                if (dt.Rows.Count > 0)
                                {
                                    i = 0;
                                    foreach (DataRow drtos in dt.Rows)
                                    {
                                        i++;
                                        drtos["index"] = i.ToString();


                                    }
                                }
                                ReporterHelper.ReadFromDataTable(ref dt, ref ws, 5, 1, xf2);

                                #endregion
                                #endregion
                                break;
                            case ReportTypeOut.销售出库报表:
                                #region 销售出库报表  --完成 等待测试数据
                                ws = xd.Workbook.Worksheets.Add("销售出库报表");

                                #region Creat by Bot
                                ws.Cells.Add(1, 1, "物资供应处川西物资配送中心销售出库统计表", xf3);
                                ws.Cells.Add(2, 1, "填报单位：物资供应处川西物资配送中心");
                                ws.Cells.Add(2, 10, "报出日期：");
                                ws.Cells.Add(2, 17, "截止日期：");
                                ws.Cells.Add(3, 1, "序号", xf1);
                                ws.Cells.Add(3, 2, "名称", xf1);
                                ws.Cells.Add(3, 3, "规格型号", xf1);
                                ws.Cells.Add(3, 4, "物料编码", xf1);
                                ws.Cells.Add(3, 5, "财务编码", xf1);
                                ws.Cells.Add(3, 6, "出库单编号", xf1);
                                ws.Cells.Add(3, 7, "出库时间", xf1);
                                ws.Cells.Add(3, 8, "生产厂家", xf1);
                                ws.Cells.Add(3, 9, "发料凭证", xf1);
                                ws.Cells.Add(3, 10, "计量单位", xf1);
                                ws.Cells.Add(3, 11, "发货数量", xf1);
                                ws.Cells.Add(3, 12, null, xf1);
                                ws.Cells.Add(3, 13, null, xf1);
                                ws.Cells.Add(3, 14, "单价（元）", xf1);
                                ws.Cells.Add(3, 15, "金额（元）", xf1);
                                ws.Cells.Add(3, 16, "业主单位", xf1);
                                ws.Cells.Add(3, 17, "施工单位", xf1);
                                ws.Cells.Add(3, 18, "井号", xf1);
                                ws.Cells.Add(3, 19, "项目性质", xf1);
                                ws.Cells.Add(3, 20, "项目阶段", xf1);
                                ws.Cells.Add(3, 21, "备注", xf1);
                                ws.Cells.Add(4, 1, null, xf1);
                                ws.Cells.Add(4, 2, null, xf1);
                                ws.Cells.Add(4, 3, null, xf1);
                                ws.Cells.Add(4, 4, null, xf1);
                                ws.Cells.Add(4, 5, null, xf1);
                                ws.Cells.Add(4, 6, null, xf1);
                                ws.Cells.Add(4, 7, null, xf1);
                                ws.Cells.Add(4, 8, null, xf1);
                                ws.Cells.Add(4, 9, null, xf1);
                                ws.Cells.Add(4, 10, null, xf1);
                                ws.Cells.Add(4, 11, "根 /套/只", xf1);
                                ws.Cells.Add(4, 12, "米", xf1);
                                ws.Cells.Add(4, 13, "吨", xf1);
                                ws.Cells.Add(4, 14, null, xf1);
                                ws.Cells.Add(4, 15, null, xf1);
                                ws.Cells.Add(4, 16, null, xf1);
                                ws.Cells.Add(4, 17, null, xf1);
                                ws.Cells.Add(4, 18, null, xf1);
                                ws.Cells.Add(4, 19, null, xf1);
                                ws.Cells.Add(4, 20, null, xf1);
                                ws.Cells.Add(4, 21, null, xf1);
                                ws.Cells.Add(5, 1, null, xf1);
                                ws.Cells.Add(5, 2, null, xf1);
                                ws.Cells.Add(5, 3, null, xf1);
                                ws.Cells.Add(5, 4, null, xf1);
                                ws.Cells.Add(5, 5, null, xf1);
                                ws.Cells.Add(5, 6, null, xf1);
                                ws.Cells.Add(5, 7, null, xf1);
                                ws.Cells.Add(5, 8, null, xf1);
                                ws.Cells.Add(5, 9, null, xf1);
                                ws.Cells.Add(5, 10, null, xf1);
                                ws.Cells.Add(5, 11, null, xf1);
                                ws.Cells.Add(5, 12, null, xf1);
                                ws.Cells.Add(5, 13, null, xf1);
                                ws.Cells.Add(5, 14, null, xf1);
                                ws.Cells.Add(5, 15, null, xf1);
                                ws.Cells.Add(5, 16, null, xf1);
                                ws.Cells.Add(5, 17, null, xf1);
                                ws.Cells.Add(5, 18, null, xf1);
                                ws.Cells.Add(5, 19, null, xf1);
                                ws.Cells.Add(5, 20, null, xf1);
                                ws.Cells.Add(5, 21, null, xf1);
                                ws.AddMergeArea(new MergeArea(1, 1, 1, 21));
                                ws.AddMergeArea(new MergeArea(2, 2, 1, 3));
                                ws.AddMergeArea(new MergeArea(3, 5, 1, 1));
                                ws.AddMergeArea(new MergeArea(3, 5, 2, 2));
                                ws.AddMergeArea(new MergeArea(3, 5, 3, 3));
                                ws.AddMergeArea(new MergeArea(3, 5, 4, 4));
                                ws.AddMergeArea(new MergeArea(3, 5, 5, 5));
                                ws.AddMergeArea(new MergeArea(3, 5, 6, 6));
                                ws.AddMergeArea(new MergeArea(3, 5, 7, 7));
                                ws.AddMergeArea(new MergeArea(3, 5, 8, 8));
                                ws.AddMergeArea(new MergeArea(3, 5, 9, 9));
                                ws.AddMergeArea(new MergeArea(3, 5, 10, 10));
                                ws.AddMergeArea(new MergeArea(3, 3, 11, 13));
                                ws.AddMergeArea(new MergeArea(3, 5, 14, 14));
                                ws.AddMergeArea(new MergeArea(3, 5, 15, 15));
                                ws.AddMergeArea(new MergeArea(3, 5, 16, 16));
                                ws.AddMergeArea(new MergeArea(3, 5, 17, 17));
                                ws.AddMergeArea(new MergeArea(3, 5, 18, 18));
                                ws.AddMergeArea(new MergeArea(3, 5, 19, 19));
                                ws.AddMergeArea(new MergeArea(3, 5, 20, 20));
                                ws.AddMergeArea(new MergeArea(3, 5, 21, 21));
                                ws.AddMergeArea(new MergeArea(4, 5, 11, 11));
                                ws.AddMergeArea(new MergeArea(4, 5, 12, 12));
                                ws.AddMergeArea(new MergeArea(4, 5, 13, 13));
                                #endregion
                                #region create table
                                nout = from a in db.NormalOut
                                       where a.ProjectName.Contains("销售")
                                       select new
                                       {
                                           index = "",
                                           a.MaterialName,
                                           a.SpecificationModel,
                                           a.MaterialCode,
                                           a.FinanceCode,
                                           a.StorageOutNoticeCode,
                                           a.ConfirmTime,
                                           a.ManufacturerName,
                                           发料凭证 = "",
                                           a.CurUnit,
                                           a.RealGentaojian,
                                           a.RealMetre,
                                           a.RealTon,
                                           a.UnitPrice,
                                           a.RealAmount,
                                           a.own,
                                           a.BusinessUnitName,
                                           a.ProjectName,
                                           a.ProjectProperty,
                                           a.ProjectStage,
                                           a.Remark,
                                       };
                                dt = ReporterHelper.LinqQueryToDataTable(nout);
                                if (dt.Rows.Count == 0)
                                    break;


                                if (dt.Rows.Count > 0)
                                {
                                    i = 0;
                                    foreach (DataRow drtos in dt.Rows)
                                    {
                                        i++;
                                        drtos["index"] = i.ToString();


                                    }
                                }
                                ReporterHelper.ReadFromDataTable(ref dt, ref ws, 5, 1, xf2);

                                #endregion
                                #endregion
                                break;
                            case ReportTypeOut.移库出库报表:
                                #region 移库出库报表  --完成 等待测试数据
                                ws = xd.Workbook.Worksheets.Add("移库出库报表");
                                #region Creat by Bot
                                ws.Cells.Add(1, 1, "物资供应处川西物资配送中心移库出库统计表", xf3);

                                ws.Cells.Add(2, 1, "填报单位：物资供应处川西物资配送中心");
                                ws.Cells.Add(2, 10, "报出日期：");
                                ws.Cells.Add(2, 17, "截止日期：");
                                ws.Cells.Add(3, 1, "序号", xf1);
                                ws.Cells.Add(3, 2, "名称", xf1);
                                ws.Cells.Add(3, 3, "规格型号", xf1);
                                ws.Cells.Add(3, 4, "物料编码", xf1);
                                ws.Cells.Add(3, 5, "财务编码", xf1);
                                ws.Cells.Add(3, 6, "出库单编号", xf1);
                                ws.Cells.Add(3, 7, "出库时间", xf1);
                                ws.Cells.Add(3, 8, "生产厂家", xf1);
                                ws.Cells.Add(3, 9, "发料凭证", xf1);
                                ws.Cells.Add(3, 10, "计量单位", xf1);
                                ws.Cells.Add(3, 11, "发货数量", xf1);
                                ws.Cells.Add(3, 12, null, xf1);
                                ws.Cells.Add(3, 13, null, xf1);
                                ws.Cells.Add(3, 14, "单价（元）", xf1);
                                ws.Cells.Add(3, 15, "金额（元）", xf1);
                                ws.Cells.Add(3, 16, "业主单位", xf1);
                                ws.Cells.Add(3, 17, "施工单位", xf1);
                                ws.Cells.Add(3, 18, "井号", xf1);
                                ws.Cells.Add(3, 19, "项目性质", xf1);
                                ws.Cells.Add(3, 20, "项目阶段", xf1);
                                ws.Cells.Add(3, 21, "备注", xf1);
                                ws.Cells.Add(4, 1, null, xf1);
                                ws.Cells.Add(4, 2, null, xf1);
                                ws.Cells.Add(4, 3, null, xf1);
                                ws.Cells.Add(4, 4, null, xf1);
                                ws.Cells.Add(4, 5, null, xf1);
                                ws.Cells.Add(4, 6, null, xf1);
                                ws.Cells.Add(4, 7, null, xf1);
                                ws.Cells.Add(4, 8, null, xf1);
                                ws.Cells.Add(4, 9, null, xf1);
                                ws.Cells.Add(4, 10, null, xf1);
                                ws.Cells.Add(4, 11, "根 /套/只", xf1);
                                ws.Cells.Add(4, 12, "米", xf1);
                                ws.Cells.Add(4, 13, "吨", xf1);
                                ws.Cells.Add(4, 14, null, xf1);
                                ws.Cells.Add(4, 15, null, xf1);
                                ws.Cells.Add(4, 16, null, xf1);
                                ws.Cells.Add(4, 17, null, xf1);
                                ws.Cells.Add(4, 18, null, xf1);
                                ws.Cells.Add(4, 19, null, xf1);
                                ws.Cells.Add(4, 20, null, xf1);
                                ws.Cells.Add(4, 21, null, xf1);
                                ws.Cells.Add(5, 1, null, xf1);
                                ws.Cells.Add(5, 2, null, xf1);
                                ws.Cells.Add(5, 3, null, xf1);
                                ws.Cells.Add(5, 4, null, xf1);
                                ws.Cells.Add(5, 5, null, xf1);
                                ws.Cells.Add(5, 6, null, xf1);
                                ws.Cells.Add(5, 7, null, xf1);
                                ws.Cells.Add(5, 8, null, xf1);
                                ws.Cells.Add(5, 9, null, xf1);
                                ws.Cells.Add(5, 10, null, xf1);
                                ws.Cells.Add(5, 11, null, xf1);
                                ws.Cells.Add(5, 12, null, xf1);
                                ws.Cells.Add(5, 13, null, xf1);
                                ws.Cells.Add(5, 14, null, xf1);
                                ws.Cells.Add(5, 15, null, xf1);
                                ws.Cells.Add(5, 16, null, xf1);
                                ws.Cells.Add(5, 17, null, xf1);
                                ws.Cells.Add(5, 18, null, xf1);
                                ws.Cells.Add(5, 19, null, xf1);
                                ws.Cells.Add(5, 20, null, xf1);
                                ws.Cells.Add(5, 21, null, xf1);
                                ws.AddMergeArea(new MergeArea(1, 1, 1, 21));
                                ws.AddMergeArea(new MergeArea(2, 2, 1, 3));
                                ws.AddMergeArea(new MergeArea(3, 5, 1, 1));
                                ws.AddMergeArea(new MergeArea(3, 5, 2, 2));
                                ws.AddMergeArea(new MergeArea(3, 5, 3, 3));
                                ws.AddMergeArea(new MergeArea(3, 5, 4, 4));
                                ws.AddMergeArea(new MergeArea(3, 5, 5, 5));
                                ws.AddMergeArea(new MergeArea(3, 5, 6, 6));
                                ws.AddMergeArea(new MergeArea(3, 5, 7, 7));
                                ws.AddMergeArea(new MergeArea(3, 5, 8, 8));
                                ws.AddMergeArea(new MergeArea(3, 5, 9, 9));
                                ws.AddMergeArea(new MergeArea(3, 5, 10, 10));
                                ws.AddMergeArea(new MergeArea(3, 3, 11, 13));
                                ws.AddMergeArea(new MergeArea(3, 5, 14, 14));
                                ws.AddMergeArea(new MergeArea(3, 5, 15, 15));
                                ws.AddMergeArea(new MergeArea(3, 5, 16, 16));
                                ws.AddMergeArea(new MergeArea(3, 5, 17, 17));
                                ws.AddMergeArea(new MergeArea(3, 5, 18, 18));
                                ws.AddMergeArea(new MergeArea(3, 5, 19, 19));
                                ws.AddMergeArea(new MergeArea(3, 5, 20, 20));
                                ws.AddMergeArea(new MergeArea(3, 5, 21, 21));
                                ws.AddMergeArea(new MergeArea(4, 5, 11, 11));
                                ws.AddMergeArea(new MergeArea(4, 5, 12, 12));
                                ws.AddMergeArea(new MergeArea(4, 5, 13, 13));
                                #endregion

                                #region create table
                                nout = from a in db.NormalOut
                                       where a.ProjectName.Contains("移库")
                                       select new
                                       {
                                           index = "",
                                           a.MaterialName,
                                           a.SpecificationModel,
                                           a.MaterialCode,
                                           a.FinanceCode,
                                           a.StorageOutNoticeCode,
                                           a.ConfirmTime,
                                           a.ManufacturerName,
                                           发料凭证 = "",
                                           a.CurUnit,
                                           a.RealGentaojian,
                                           a.RealMetre,
                                           a.RealTon,
                                           a.UnitPrice,
                                           a.RealAmount,
                                           a.own,
                                           a.BusinessUnitName,
                                           a.ProjectName,
                                           a.ProjectProperty,
                                           a.ProjectStage,
                                           a.Remark,
                                       };
                                dt = ReporterHelper.LinqQueryToDataTable(nout);
                                if (dt.Rows.Count == 0)
                                    break;


                                if (dt.Rows.Count > 0)
                                {
                                    i = 0;
                                    foreach (DataRow drtos in dt.Rows)
                                    {
                                        i++;
                                        drtos["index"] = i.ToString();


                                    }
                                }
                                ReporterHelper.ReadFromDataTable(ref dt, ref ws, 5, 1, xf2);

                                #endregion
                                #endregion
                                break;
                        }
                        #endregion
                        break;
                }
                
            }
            xd.Send();
            Response.Flush();
            Response.End();
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
